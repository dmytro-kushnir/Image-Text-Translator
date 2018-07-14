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
        CognitiveService computerVision;

        string sourceLanguage = "en"; // english by default
        string sourceText = "";
        // визначимо координати лінії тексту
        int g_Top = 0;
        int g_Left = 0;
        int g_Width = 0;
        int g_Height = 0;
        //
        double g_screen_width = 0.0;
        double g_screen_height = 0.0;

        string transaltedText = "";
        bool flag = false; // прапорець для делегування зміною стану кнопок Камери та Галереї
        bool imageInverseFlag = false; // прапорець для делегування зумування зображенням
        public OcrRecognitionPage()
        {
            this.Error = null;
            InitializeComponent();

            Random rand = new Random();
            int randomIndex = rand.Next(0, Data.subscriptionKeys.Length);

            computerVision = new CognitiveService(Data.subscriptionKeys[randomIndex]);

            Debug.WriteLine("randomIndex -> {0} ", randomIndex);
            Debug.WriteLine("VisionServiceClient is created");

         //   DestinationLangPicker.IsVisible = false;
            GettedLanguage.IsVisible = false;
            //this.countryFlag.InputTransparent = true;

        }

        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Image img = (Image)sender;
                var file = (dynamic)null;
                string imageName = "";
                if (img.Source is Xamarin.Forms.FileImageSource)
                {
                    Xamarin.Forms.FileImageSource objFileImageSource = (Xamarin.Forms.FileImageSource)img.Source;
                    //
                    // Access the file that was specified:-
                    imageName = objFileImageSource.File;
                }
                ClearView();
                if (imageName == "camera.png")
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
                else if (imageName == "gallery.png")
                {
                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        Debug.WriteLine("Picking a photo is not supported");
                        await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                        return;
                    }
                    file = await CrossMedia.Current.PickPhotoAsync();
                }
                if (file == null)
                    return;
                flag = true;
                Image1.IsVisible = true;
                backgroundImage.IsVisible = false;

                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;

                Image1.Source = ImageSource.FromStream(() => file.GetStream());
                
                // INFO - for future modifications
                //if (Data.hardwrittenLanguageSupports.Any(s => navBar.checkHandwrittenMode().Contains(s)))
                if (navBar.CheckHandwrittenMode() == Data.Settings_handwrittenMode)
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
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;

            Image1.IsVisible = true;
         //   DestinationLangPicker.IsVisible = true;
            GettedLanguage.IsVisible = true;

            //  DestinationLangPicker.SelectedIndex = 0;
            //  DestinationLangPicker.Title = "Destination language";
            // this.TranslatedText.Children.Clear();
        }

        private void PopulateUIWithRegions(OcrResults ocrResult)
        {
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
                    Translate_Txt(innerChunkOfText, navBar.getDestinationLanguage());
                    innerChunkOfText = "";
                }
            }
        }

        private void PopulateUIWithHardwirttenLines(HandwritingRecognitionOperationResult ocrResult)
        {
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
                    Translate_Txt(innerChunkOfText, navBar.getDestinationLanguage());
                    innerChunkOfText = "";
                }    
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

        protected override void OnSizeAllocated(double width, double height)
        {
            //DestinationLangPicker.Focus();
            if (Device.OS == TargetPlatform.iOS) // 
            {
                /*
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
                */
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
                Image1.IsVisible = false;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
              //  DestinationLangPicker.IsVisible = false;
                TranslatedText.IsVisible = false;
                // ImageBackButton.IsVisible = false;
                backgroundImage.Opacity = 0.4;
                sourceText = "";
                backgroundImage.IsVisible = true;
                flag = false;
              //  DestinationLangPicker.Focus();
               // picker_func();
            }
            else // інакше, для виходу з режиму збільшеного зображення
            {
                TapGesture(true);
            }
        }
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
                TranslatedText.IsVisible = true;
                GettedLanguage.IsVisible = true;
             //   DestinationLangPicker.IsVisible = true;
                Image1.HeightRequest = 240;
                Image1.WidthRequest = 240;
                imageInverseFlag = false;
            }
            else
            {
                Image1.HeightRequest = g_screen_height - 100;
                Image1.WidthRequest = g_screen_width - 50;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
              //  DestinationLangPicker.IsVisible = false;
                imageInverseFlag = true;
            }
        }
        void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            //if (TakePictureButton.IsVisible == false)
            //{
            //    if (imageInverseFlag == false)
            //        TapGesture(false);
            //    else
            //        TapGesture(true);
            //}
        }
        void PickerLanguage_Clicked(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            int splitChunkSize = 399;

            TranslatedText.Children.Clear();
            var splittedText = Utils.SplitBy(sourceText, splitChunkSize);

            foreach (string item in splittedText)
            {
                if (item.Length > 0)
                {
                    Translate_Txt(item, (string)picker.SelectedItem);
                }
            }
        }
        void PickerSettings_Clicked(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            switch ((string)picker.SelectedItem)
            {
                case Data.Settings_defaultMode:
                    break;
                case Data.Settings_info:
                    DisplayAlert(Data.Settings_info_Title, Data.Settings_info_Data, "Got it");
                    break;
                case Data.Settings_clrAll:
                    ClearView();
                    navBar.SetPickersToDefault();
                    break;
                case Data.Settings_handwrittenMode:
                    break;
                default:
                    break;
            }
        }

        void ClearView()
        {
            if (backgroundImage.IsVisible == false)
            {
                backgroundImage.IsVisible = true;
                GettedLanguage.IsVisible = false;
                TranslatedText.Children.Clear();
                Image1.Source = null;
                sourceText = sourceText.Length > 0 ? "" : sourceText;
            }
        }
    }
}
