using Microsoft.ProjectOxford.Vision.Contract;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Linq;
using Xamarin.Forms;
using ComputerVisionSample.Translator;
using ComputerVisionSample.ClipBoard;
using ComputerVisionSample.services;
using ComputerVisionSample.helpers;
using System.Diagnostics;

namespace ComputerVisionSample
{
    public partial class OcrRecognitionPage : ContentPage
    {
        public Exception Error
        {
            get;
            private set;
        }

        // Початкове налаштування
        private int count = 0;
        protected static readonly TimeSpan QueryWaitTimeInSecond = TimeSpan.FromSeconds(3);
        protected static readonly int MaxRetryTimes = 3;
        CognitiveService computerVision;

        string sourceLanguage = "en"; // english by default
        string sourceText = "";
        string destinationLanguage = "";
        // визначимо координати лінії тексту
        int g_Top = 0;
        int g_Left = 0;
        int g_Width = 0;
        int g_Height = 0;
        //
        double g_screen_width = 0.0;
        double g_screen_height = 0.0;

        string[] subscriptionKeys = new String[] { "cf3b45431cc14c799696821dd9668990", "81e68751a466446c80076b8f82fd1adc", "81e68751a466446c80076b8f82fd1adc" }; // 7c45fc48ba8f42e993a1cd173e1b59a7 
        string[] hardwrittenLanguageSupports = new String[] {"blab", "English", "English1"};

        string transaltedText = "";
        bool flag = false; // прапорець для делегування зміною стану кнопок Камери та Галереї
        bool imageInverseFlag = false; // прапорець для делегування зумування зображенням
        public OcrRecognitionPage()
        {
            this.Error = null;
            InitializeComponent();

            Random rand = new Random();
            int randomIndex = rand.Next(0, subscriptionKeys.Length);

            computerVision = new CognitiveService(subscriptionKeys[randomIndex]);

            Debug.WriteLine("randomIndex -> {0} ", randomIndex);
            Debug.WriteLine("VisionServiceClient is created");

            DestinationLangPicker.IsVisible = false;
            GettedLanguage.IsVisible = false;
            BackButton.IsVisible = false;
            BackButton.Text = "<- Back";
            UploadPictureButton.IsVisible = false;
            TakePictureButton.IsVisible = false;
            this.countryFlag.InputTransparent = true;
        }

        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                var file = (dynamic)null;

                if (btn.Text == "Camera")
                {
                    await CrossMedia.Current.Initialize();
                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        Debug.WriteLine("No camera available");
                        await DisplayAlert("No Camera", "No camera available.", "OK");
                        return;
                    }
                    file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        SaveToAlbum = false,
                        Name = "test.jpg",
                        CompressionQuality = 75,
                        AllowCropping = true
                    });
                }
                else if (btn.Text == "Gallery")
                {
                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        Debug.WriteLine("Picking a photo is not supported");
                        await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                        return;
                    }
                    file = await CrossMedia.Current.PickPhotoAsync();
                }
                flag = true;
                UploadPictureButton.IsVisible = false;
                TakePictureButton.IsVisible = false;
                BackButton.IsVisible = true;
                Image1.IsVisible = true;
                if (file == null)
                    return;
                if (backgroundImage.Opacity != 0)
                    backgroundImage.Opacity = 0;

                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;

               
                Image1.Source = ImageSource.FromStream(() => file.GetStream());

                if (hardwrittenLanguageSupports.Any(s => "English".Contains(s)))
                {
                    HandwritingRecognitionOperationResult result;
                    result = await computerVision.RecognizeUrl(file.GetStream());
                    PopulateUIWithHardwirttenLines(result);
                }
                else
                {
                    var ocrResult = await computerVision.AnalyzePictureAsync(file.GetStream());
                    this.BindingContext = null;
                    this.BindingContext = ocrResult;
                    sourceLanguage = ocrResult.Language;
                    PopulateUIWithRegions(ocrResult);
                }
            }
            catch (Exception ex)
            {
                this.Error = ex;
            }
            TranslatedText.IsVisible = true;
            BackButton.IsVisible = true;
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;

            Image1.IsVisible = true;
            DestinationLangPicker.IsVisible = true;
            GettedLanguage.IsVisible = true;

            //  DestinationLangPicker.SelectedIndex = 0;
            //  DestinationLangPicker.Title = "Destination language";
            // this.TranslatedText.Children.Clear();
        }

        private void PopulateUIWithRegions(OcrResults ocrResult)
        {
            destinationLanguage =
                  DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
            TranslatedText.Children.Clear();
            string innerChunkOfText = "";

            //Ітерація по регіонах
            foreach (var region in ocrResult.Regions)
            {
                //Ітерація по лініях в регіоні
                foreach (var line in region.Lines)
                {
                    //   Для кожної лінії згенерувати горизонтальну панель
                    var lineStack = new StackLayout
                    { Orientation = StackOrientation.Horizontal };

                    //Ітерація по словах в лінії
                    foreach (var word in line.Words)
                    {
                        var textLabel = new Label
                        {
                            TextColor = Xamarin.Forms.Color.Black,
                            Text = word.Text,
                        };
                        innerChunkOfText += textLabel.Text + " ";
                        sourceText += textLabel.Text + " "; // save to global var 

                        lineStack.Children.Add(textLabel);
                    }

                    g_Height = line.Rectangle.Height;
                    g_Width = line.Rectangle.Width;
                    g_Left = line.Rectangle.Left;
                    g_Top = line.Rectangle.Top;

                    //Xamarin.Forms.Rectangle rec = new Xamarin.Forms.Rectangle(Top, Left, width, height);
                    // Відправка обробленого тексту на переклад
                    Translate_Txt(innerChunkOfText, destinationLanguage);
                    innerChunkOfText = "";
                }
            }
        }

        private void PopulateUIWithHardwirttenLines(HandwritingRecognitionOperationResult ocrResult)
        {
            destinationLanguage =
                  DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
            TranslatedText.Children.Clear();
            string innerChunkOfText = "";
       
                //Ітерація по лініях в регіоні
                foreach (var line in ocrResult.RecognitionResult.Lines)
                {
                    //   Для кожної лінії згенерувати горизонтальну панель
                    var lineStack = new StackLayout
                    { Orientation = StackOrientation.Horizontal };

                    //Ітерація по словах в лінії
                    foreach (var word in line.Words)
                    {
                        var textLabel = new Label
                        {
                            TextColor = Xamarin.Forms.Color.Black,
                            Text = word.Text,
                        };
                        innerChunkOfText += textLabel.Text + " ";
                        sourceText += textLabel.Text + " "; // save to global var 

                        lineStack.Children.Add(textLabel);
                    }

                    // Відправка обробленого тексту на переклад
                    Translate_Txt(innerChunkOfText, destinationLanguage);
                    innerChunkOfText = "";
                }    
        }

        void onTapFlagSelect(object sender, EventArgs args)
        {
            Debug.WriteLine("CLIKCKED !!!!!!!!!!!!!!!");
            DestinationLangPicker.Focus();
        }
        private void generateBoxes(int height, int width, int left, int top, string text)
        {
            Label label = new Label { BackgroundColor = Xamarin.Forms.Color.Gray, WidthRequest = width, HeightRequest = height };
            label.FontSize = 10;
            container.Children.Add(label,
                Constraint.RelativeToParent((parent) =>
                {
                    return left;  // встановлення координати X
                }),
                Constraint.RelativeToParent((parent) =>
                {
                    return top; // встановлення координати Y
                }),
                Constraint.Constant(width), // встановлення ширини
                Constraint.Constant(height)  // встановлення высоти

            );
            label.Text = text;
            Content = container;
        }

        private void generateFlag(string destLng)
        {
            switch (destLng)
            {
                case "French": countryFlag.Source = "fr.png"; break;
                case "English": countryFlag.Source = "gb.png"; break;
                case "Russian": countryFlag.Source = "ru.png"; break;
                case "Ukrainian": countryFlag.Source = "ua.png"; break;
                case "Latvian": countryFlag.Source = "lv.png"; break;
                case "German": countryFlag.Source = "gr.png"; break;
                case "Polish": countryFlag.Source = "pl.png"; break;
                case "Spanish": countryFlag.Source = "sp.png"; break;
                case "Italian": countryFlag.Source = "it.png"; break;
                case "Chinese": countryFlag.Source = "china.png"; break;
                case "Korean": countryFlag.Source = "korea.png"; break;
                case "Japanese": countryFlag.Source = "ja.png"; break;
                case "Portuguese": countryFlag.Source = "po.png"; break;
                case "Arabic": countryFlag.Source = "arabic.png"; break;
                case "Hindi": countryFlag.Source = "india.png"; break;
                case "Hebrew": countryFlag.Source = "isr.png"; break;
                case "Swedish": countryFlag.Source = "sw.png"; break;
                case "Norwegian": countryFlag.Source = "norway.png"; break;
                case "Danish": countryFlag.Source = "denmark.png"; break;
                case "Finnish": countryFlag.Source = "finland.png"; break;
                case "Georgian": countryFlag.Source = "ge.png"; break;
                case "Greek": countryFlag.Source = "gre.png"; break;
                case "Turkish": countryFlag.Source = "turkey.png"; break;
                case "Czech": countryFlag.Source = "cz.png"; break;
                default: countryFlag.Source = "gb.png"; break;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (count == 0)
            {
                DestinationLangPicker.Focus();
                count++;
                if (Device.OS == TargetPlatform.iOS) // 
                {
                    DestinationLangPicker.Items.Clear();
                    DestinationLangPicker.Items.Add("Destination language");

                    DestinationLangPicker.Items.Add("English");
                    DestinationLangPicker.Items.Add("Ukrainian");
                    DestinationLangPicker.Items.Add("French");
                    DestinationLangPicker.Items.Add("Polish");
                    DestinationLangPicker.Items.Add("Spanish");
                    DestinationLangPicker.Items.Add("German");
                    DestinationLangPicker.Items.Add("Italian");
                    DestinationLangPicker.Items.Add("Latvian");
                    DestinationLangPicker.Items.Add("Chinese");
                    DestinationLangPicker.Items.Add("Japanese");
                    DestinationLangPicker.Items.Add("Korean");
                    DestinationLangPicker.Items.Add("Portuguese");
                    DestinationLangPicker.Items.Add("Arabic");
                    DestinationLangPicker.Items.Add("Hindi");
                    DestinationLangPicker.Items.Add("Hebrew");
                    DestinationLangPicker.Items.Add("Swedish");
                    DestinationLangPicker.Items.Add("Danish");
                    DestinationLangPicker.Items.Add("Norwegian");
                    DestinationLangPicker.Items.Add("Finnish");
                    DestinationLangPicker.Items.Add("Georgian");
                    DestinationLangPicker.Items.Add("Turkish");
                    DestinationLangPicker.Items.Add("Russian");
                    DestinationLangPicker.Items.Add("Czech");
                    DestinationLangPicker.Items.Add("Greek");
                    DestinationLangPicker.SelectedIndexChanged += (sender, e) =>
                    {
                        if (DestinationLangPicker.SelectedIndex == 0)
                            DestinationLangPicker.SelectedIndex = 1;
                    };
                }
            }
            base.OnSizeAllocated(width, height);
            g_screen_height = height;
            g_screen_width = width;


            if (DeviceInfo.IsOrientationPortrait() && width < height || !DeviceInfo.IsOrientationPortrait() && width > height)
            {
                //Image1.IsVisible = false;
            }
            else
            {
                // Image1.IsVisible = true;
            }
        }

        void BackButton_Clicked(object sender, EventArgs e)
        {
            if (imageInverseFlag == false) // якщо кнопка використовується для виходу в голове меню
            {
                TakePictureButton.IsVisible = true;
                UploadPictureButton.IsVisible = true;
                BackButton.IsVisible = false;
                Image1.IsVisible = false;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
                DestinationLangPicker.IsVisible = false;
                TranslatedText.IsVisible = false;
                // ImageBackButton.IsVisible = false;
                backgroundImage.Opacity = 0.4;
                sourceText = "";
                UploadPictureButton.IsVisible = false;
                TakePictureButton.IsVisible = false;
                backgroundImage.IsVisible = true;
                flag = false;
                DestinationLangPicker.Focus();
                picker_func();
            }
            else // інакше, для виходу з режиму збільшеного зображення
            {
                TapGesture(true);
            }
        }
        /// //////////////// TRANSLATION///////////////////////
        void Translate_Txt(string sourceTxt, string destLang)
        {
            if (sourceLanguage != "unk")
            {
                string buffer = DependencyService.Get<PCL_Translator>().
                            Translate(sourceTxt, sourceLanguage, destLang) + " ";


                var textLabel = new Label
                {
                    TextColor = Xamarin.Forms.Color.Black,
                    Text = buffer
                };

                TranslatedText.Children.Add(textLabel);

                // this.TranslatedText.Text += buffer;

                transaltedText += buffer;

                //generateBoxes(g_Height, g_Width, g_Left, g_Top, buffer);
            }
            else
            {
                //var Error = "unknown language! Please try again";
                TranslatedText.Children.Clear();
            }
        }
        /// //////////////// TRANSLATION END///////////////////////
        public void ClipboardFunc(object sender, EventArgs e)
        {
            DependencyService.Get<PCL_ClipBoard>().GetTextFromClipBoard(transaltedText);
            DisplayAlert("", "Successfully copied to the clipboard", "OK");
            // clipboardText = TranslatedText.Text;
        }
        void TapGesture(bool move_to_default)
        {
            if (move_to_default == true)
            {
                BackButton.Text = " < -Back";
                TranslatedText.IsVisible = true;
                GettedLanguage.IsVisible = true;
                DestinationLangPicker.IsVisible = true;
                Image1.HeightRequest = 240;
                Image1.WidthRequest = 240;
                imageInverseFlag = false;
            }
            else
            {
                BackButton.Text = "Resize";
                TakePictureButton.IsVisible = false;
                UploadPictureButton.IsVisible = false;
                Image1.HeightRequest = g_screen_height - 100;
                Image1.WidthRequest = g_screen_width - 50;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
                DestinationLangPicker.IsVisible = false;
                imageInverseFlag = true;
            }
        }
        void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            if (TakePictureButton.IsVisible == false)
            {
                if (imageInverseFlag == false)
                    TapGesture(false);
                else
                    TapGesture(true);
            }
        }
        void UnfocusedPicker(object sender, EventArgs e)
        {
            if (DestinationLangPicker.SelectedIndex < 0)
                DestinationLangPicker.SelectedIndex = 0;
        }
        void picker_language_choose(object sender, EventArgs e)
        {
            picker_func();
        }
        void picker_func()
        {
            DestinationLangPicker.Title = DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];

            if (Device.OS == TargetPlatform.Android)
            {
                DestinationLangPicker.WidthRequest = DestinationLangPicker.Title.Length * 12;
            }

            int splitChunkSize = 399;

            TranslatedText.Children.Clear();
            var splittedText = Utils.SplitBy(sourceText, splitChunkSize);

            foreach (string item in splittedText)
            {
                Translate_Txt(item, DestinationLangPicker.Title);
            }

            generateFlag(DestinationLangPicker.Title);

            if (flag == false)
            {
                UploadPictureButton.IsVisible = true;
                TakePictureButton.IsVisible = true;
            }
        }
    }
}
