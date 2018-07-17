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
using System.Collections.Generic;

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

        string g_sourceLanguage = "en"; // english by default
        string g_sourceText = "";
        string g_transaltedText = "";

        List<IDictionary<string, string>> g_lines = new List<IDictionary<string, string>>();

        const int DEFAULT_CROPPED_IMAGE_WIDHT = 480;
        const int DEFAULT_CROPPED_IMAGE_HEIGHT = 480;

        double CROP_KOEF_W = 0.0;
        double CROP_KOEF_H = 0.0;
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
                    HandwritingRecognitionOperationResult hwResult;
                    hwResult = await computerVision.RecognizeUrl(file.GetStream());
                    if (hwResult == null)
                    {
                        ClearView();
                        return;
                    }
                    PopulateUIWithHardwirttenLines(hwResult);
                }
                else
                {
                    var ocrResult = await computerVision.AnalyzePictureAsync(file.GetStream());
                    if (ocrResult == null)
                    {
                        ClearView();
                        return;
                    }
                    this.BindingContext = null;
                    this.BindingContext = ocrResult;
                    g_sourceLanguage = ocrResult.Language;
                    PopulateUIWithRegions(ocrResult);
                }
            }
            catch (Exception ex)
            {
                this.Error = ex;
            }
            BoxesLayout.IsVisible = false;
            TranslatedText.IsVisible = true;
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
            GettedLanguage.IsVisible = true;
        }
        private void PopulateUIWithRegions(OcrResults ocrResult)
        {
            TranslatedText.Children.Clear();
            //Ітерація по регіонах
            foreach (var region in ocrResult.Regions)
            {
                //Ітерація по лініях в регіоні
                foreach (var line in region.Lines)
                {
                    string wordsInLine = "";
                    //Ітерація по словах в лінії
                    foreach (var word in line.Words)
                    {
                        // конкатенація слів в лінію
                        wordsInLine += word.Text + " ";
                    }
                    // генерація словника, з якого формуємо список ліній з координатами та текстом кожної з них
                    IDictionary<string, string> coordinates = new Dictionary<string, string>();
                    coordinates["height"] = line.Rectangle.Height.ToString();
                    coordinates["width"] = line.Rectangle.Width.ToString();
                    coordinates["left"] = line.Rectangle.Left.ToString();
                    coordinates["top"] = line.Rectangle.Top.ToString();
                    coordinates["words"] = wordsInLine;
                    g_lines.Add(coordinates);
                }
            }
            // Відправка обробленого тексту на переклад
            Translate_Txt(navBar.getDestinationLanguage(), g_lines);
        }
        private void PopulateUIWithHardwirttenLines(HandwritingRecognitionOperationResult ocrResult)
        {
            TranslatedText.Children.Clear();

                //Ітерація по лініях в регіоні
                foreach (var line in ocrResult.RecognitionResult.Lines)
                {
                    string wordsInLine = "";
                    //Ітерація по словах в лінії
                    foreach (var word in line.Words)
                    {
                        wordsInLine += word.Text + " ";
                    }
                // TODO - handwritten returns 8 positions instead of 4
                // [height, width, xy, xy, xy, xy, xy, xy]

                // генерація словника, з якого формуємо список ліній з координатами та текстом кожної з них
                IDictionary<string, string> coordinates = new Dictionary<string, string>();
                coordinates["height"] = line.BoundingBox[0].ToString();
                coordinates["width"] = line.BoundingBox[1].ToString();
                coordinates["left"] = line.BoundingBox[2].ToString();
                coordinates["top"] = line.BoundingBox[3].ToString();
                coordinates["words"] = wordsInLine;
                g_lines.Add(coordinates);
            }
            // Відправка обробленого тексту на переклад
            Translate_Txt(navBar.getDestinationLanguage(), g_lines);
        }
        void Translate_Txt(string destLang, List<IDictionary<string, string>> lines)
        {
            if (g_sourceLanguage != "unk")
            {
                if(g_transaltedText.Length > 0)
                {
                    g_transaltedText = string.Empty;
                }
                foreach (var line in lines)
                {
                    int h = Int32.Parse(line["height"]);
                    int w = Int32.Parse(line["width"]);
                    int t = Int32.Parse(line["left"]);
                    int l = Int32.Parse(line["top"]);
                    string words = line["words"];

                    string translatedwords = DependencyService.Get<PCL_Translator>().Translate(words, g_sourceLanguage, destLang) + " ";
                    var textLabel = new Label
                    {
                        TextColor = Xamarin.Forms.Color.Black,
                        Text = translatedwords
                    };
                    TranslatedText.Children.Add(textLabel);
                    g_transaltedText += translatedwords;
                    GenerateBoxes(h, w, t, l, translatedwords, CROP_KOEF_W, CROP_KOEF_H);
                }
            }
            else
            {
                DisplayAlert("Language don't recognized", "We can't recognize your language", "Try again");
                TranslatedText.Children.Clear();
            }
        }
        private void GenerateBoxes(int height, int width, int left, int top, string text, double koefW, double koefH)
        {
            Label label = new Label { BackgroundColor = Xamarin.Forms.Color.Gray, WidthRequest = width, HeightRequest = height };
            label.FontSize = 10;
            BoxesLayout.Children.Add(label,
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
        protected override void OnSizeAllocated(double deviceW, double deviceH)
        {
            base.OnSizeAllocated(deviceW, deviceH);
            croppedImage.WidthRequest = deviceW;
            croppedImage.HeightRequest = deviceH;
            CROP_KOEF_W = (deviceW / DEFAULT_CROPPED_IMAGE_WIDHT);
            CROP_KOEF_H = (deviceH / DEFAULT_CROPPED_IMAGE_HEIGHT);
            if (DeviceInfo.IsOrientationPortrait() && deviceW < deviceH || !DeviceInfo.IsOrientationPortrait() && deviceW > deviceH)
            {
                // Horizontal
            }
            else
            {
                // Vertical
            }
        }
        public void ClipboardFunc(object sender, EventArgs e)
        {
            DependencyService.Get<PCL_ClipBoard>().GetTextFromClipBoard(g_transaltedText);
            DisplayAlert("Clipboard", "Successfully copied to the clipboard", "OK");
        }
        void OnImageTapped(object sender, EventArgs args)
        {
            if (originImage.IsVisible) // збільшений режим
            {
                originImage.IsVisible = false;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
                croppedImage.IsVisible = true;
                BoxesLayout.IsVisible = true;
            }
            else // звичайний режим
            {
                originImage.IsVisible = true;
                TranslatedText.IsVisible = true;
                GettedLanguage.IsVisible = true;
                croppedImage.IsVisible = false;
                BoxesLayout.IsVisible = false;
            }
        }
        void PickerLanguage_Clicked(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            TranslatedText.Children.Clear();
            if (g_lines.Any())
            {
                BoxesLayout.Children.Clear();
                Translate_Txt((string)picker.SelectedItem, g_lines);
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
                Indicator1.IsRunning = false;
                Indicator1.IsVisible = false;
                g_sourceText = g_sourceText.Length > 0 ? "" : g_sourceText;
                BoxesLayout.Children.Clear();
                g_lines.Clear();
            }
        }
    }
}
