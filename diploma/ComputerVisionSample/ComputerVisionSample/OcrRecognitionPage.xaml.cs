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
        const int DEFAULT_CROPPED_IMAGE_WIDHT = 480;
        const int DEFAULT_CROPPED_IMAGE_HEIGHT = 480;

        double CROP_KOEF_W = 0.0;
        double CROP_KOEF_H = 0.0;

        string transaltedText = "";
        public OcrRecognitionPage()
        {
            this.Error = null;
            InitializeComponent();

            Random rand = new Random();
            int randomIndex = rand.Next(0, Data.subscriptionKeys.Length);

            computerVision = new CognitiveService(Data.subscriptionKeys[randomIndex]);

            Debug.WriteLine("randomIndex -> {0} ", randomIndex);
            Debug.WriteLine("VisionServiceClient is created");
            GettedLanguage.IsVisible = false;
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
                        PhotoSize = PhotoSize.Full,
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
                    file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Full
                    });
                }

                if (file == null)
                    return;
                originImage.IsVisible = true;
                croppedImage.IsVisible = false;
                backgroundImage.IsVisible = false;

                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;

                originImage.Source = ImageSource.FromStream(() => file.GetStream());
                croppedImage.Source = ImageSource.FromStream(() => file.GetStream());

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
            GettedLanguage.IsVisible = true;
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
        private void generateBoxes(int height, int width, int left, int top, string text, double koefW, double koefH)
        {
            Label label = new Label { BackgroundColor = Xamarin.Forms.Color.Gray, WidthRequest = width, HeightRequest = height };
            label.FontSize = 10;
            container.Children.Add(label,
                Constraint.RelativeToParent((parent) =>
                {
                    return left * koefW;  // встановлення координати X
                }),
                Constraint.RelativeToParent((parent) =>
                {
                    return top * koefH; // встановлення координати Y
                }),
                Constraint.Constant(width * koefW), // встановлення ширини
                Constraint.Constant(height * koefH)  // встановлення висоти
            );
            label.Text = text;
            Content = container;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            croppedImage.WidthRequest = width;
            croppedImage.HeightRequest = height;
            CROP_KOEF_W = (width / DEFAULT_CROPPED_IMAGE_WIDHT);
            CROP_KOEF_H = (height / DEFAULT_CROPPED_IMAGE_HEIGHT);

            if (DeviceInfo.IsOrientationPortrait() && width < height || !DeviceInfo.IsOrientationPortrait() && width > height)
            {
                //Image1.IsVisible = false;
            }
            else
            {
                // Image1.IsVisible = true;
            }
        }

        void Translate_Txt(string sourceTxt, string destLang)
        {
            if (sourceLanguage != "unk")
            {
                string buffer = DependencyService.Get<PCL_Translator>().Translate(sourceTxt, sourceLanguage, destLang) + " ";

                var textLabel = new Label
                {
                    TextColor = Xamarin.Forms.Color.Black,
                    Text = buffer
                };

                TranslatedText.Children.Add(textLabel);

                // this.TranslatedText.Text += buffer;

                transaltedText += buffer;

                generateBoxes(g_Height, g_Width, g_Left, g_Top, buffer, CROP_KOEF_W, CROP_KOEF_H);
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
        void OnImageTapped(object sender, EventArgs args)
        {
            if (originImage.IsVisible) // збільшений режим
            {
                originImage.IsVisible = false;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
                croppedImage.IsVisible = true;
            }
            else // звичайний режим
            {
                originImage.IsVisible = true;
                TranslatedText.IsVisible = true;
                GettedLanguage.IsVisible = true;
                croppedImage.IsVisible = false;
            }
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
                originImage.Source = null;
                croppedImage.Source = null;
                sourceText = sourceText.Length > 0 ? "" : sourceText;
            }
        }
    }
}
