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

        string g_sourceLanguage = "";
        string g_sourceText = "";
        string g_transaltedText = "";

        List<IDictionary<string, string>> g_lines = new List<IDictionary<string, string>>();

        double g_OriginWidth = 0.0;
        double g_OriginHeight = 0.0;

        double CROP_KOEF_W = 0.0;
        double CROP_KOEF_H = 0.0;
        public OcrRecognitionPage()
        {
            this.Error = null;
            InitializeComponent();
            computerVision = new CognitiveService(Utils.GenerateRandomKey(Data.computerVisionKeys));
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
                if (img.Source is FileImageSource)
                {
                    FileImageSource objFileImageSource = (FileImageSource)img.Source;
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
                {
                    return;
                }
                originImage.IsVisible = true;
                croppedImage.IsVisible = false;
                backgroundImage.IsVisible = false;

                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;

                ImageSource photoStream = null;
                photoStream = ImageSource.FromStream(() => file.GetStream());

                originImage.Source = croppedImage.Source = photoStream;

                g_OriginWidth = originImage.Bounds.Size.Width != 0 ? originImage.Bounds.Size.Width : 240; // default size
                g_OriginHeight = originImage.Bounds.Size.Height != 0 ? originImage.Bounds.Size.Height : 340; // default size

                if (g_OriginWidth * 1.5 > croppedImage.WidthRequest)
                {
                    croppedImage.WidthRequest = g_OriginWidth;
                }

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
                    g_sourceLanguage = "en";
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
                    GettedLanguage.IsVisible = true;
                    PopulateUIWithRegions(ocrResult);
                }
                // добавити хендлер на копіювання тексту після обпрацювання зображення
                var tgr = new TapGestureRecognizer();
                tgr.Tapped += (s, ev) => ClipboardFunc(s, ev);
                tgr.NumberOfTapsRequired = 2;
                TranslatedText.GestureRecognizers.Add(tgr);
            }
            catch (Exception ex)
            {
                this.Error = ex;
                ClearView();
                return;
            }
            BoxesLayout.IsVisible = false;
            TranslatedText.IsVisible = true;
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
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
                    IDictionary<string, string> coordinates = new Dictionary<string, string>
                    {
                        ["height"] = line.Rectangle.Height.ToString(),
                        ["width"] = line.Rectangle.Width.ToString(),
                        ["left"] = line.Rectangle.Left.ToString(),
                        ["top"] = line.Rectangle.Top.ToString(),
                        ["words"] = wordsInLine
                    };
                    g_lines.Add(coordinates);
                }
            }
            // Відправка обробленого тексту на переклад
            Translate_Txt(Utils.LanguageEnumToIdentifier(navBar.GetDestinationLanguage()), g_lines);
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
                IDictionary<string, string> coordinates = new Dictionary<string, string>
                {
                    ["height"] = "0",
                    ["width"] = "0",
                    ["left"] = "0",
                    ["top"] = "0",
                    ["words"] = wordsInLine
                };
                g_lines.Add(coordinates);
            }
            // Відправка обробленого тексту на переклад
            Translate_Txt(Utils.LanguageEnumToIdentifier(navBar.GetDestinationLanguage()), g_lines);
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

                    string translatedwords = DependencyService.Get<PCL_Translator>().Translate(
                        words, 
                        g_sourceLanguage, 
                        destLang, 
                        Utils.GenerateRandomKey(Data.translationKeys)) 
                        + " ";

                    if (translatedwords == null || translatedwords == " ")
                    {
                        DisplayAlert("Translation error", "We can't translate your text", "Try again");
                        ClearView();
                        return;
                    }
                    var textLabel = new Label
                    {
                        TextColor = Xamarin.Forms.Color.Black,
                        Text = translatedwords
                    };
                    TranslatedText.Children.Add(textLabel);
                    g_transaltedText += translatedwords;
                    GenerateBoxes(h, w, t, l, translatedwords, 1, 1);
                }
            }
            else
            {
                DisplayAlert("Language don't recognized", "We can't recognize your language", "Try again");
                ClearView();
            }
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, ev) => OnImageTapped(s, ev);
            tgr.NumberOfTapsRequired = 2;
            BoxesLayout.GestureRecognizers.Add(tgr);
        }
        private void GenerateBoxes(int height, int width, int left, int top, string text, double koefW, double koefH)
        {
            Label label = new Label { BackgroundColor = Xamarin.Forms.Color.DarkGray, WidthRequest = width, HeightRequest = height };

            label.FontSize = Utils.FontSizeGenerator(height * koefH);

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

            var boxTgr = new TapGestureRecognizer();
            boxTgr.Tapped += (s, e) => OnImageTapped(s, e);
            boxTgr.NumberOfTapsRequired = 2;
            label.GestureRecognizers.Add(boxTgr);

            label.Text = text;
            Content = container;
        }
        protected override void OnSizeAllocated(double deviceW, double deviceH)
        {
            base.OnSizeAllocated(deviceW, deviceH);
            int bottomOffset = 65;

            croppedImage.WidthRequest = deviceW;
            croppedImage.HeightRequest = deviceH - bottomOffset;

            // (deviceW / originW) * api_returns_scale 
            CROP_KOEF_W = (croppedImage.Width / (240 * 2));
            // ((deviceH - bottomOffset) / originH) * api_returns_scale
            CROP_KOEF_H = (croppedImage.Height / (340 * 2));

            if (DeviceInfo.IsOrientationPortrait() && deviceW < deviceH || !DeviceInfo.IsOrientationPortrait() && deviceW > deviceH)
            {
                // Horizontal
            }
            else
            {
                // Vertical
            }
            if (Application.Current.Properties.ContainsKey("FirstUse"))
            {
                //Do things when it's NOT the first use...
            }
            else
            {
                Application.Current.Properties["FirstUse"] = false;
                DisplayAlert(Data.Settings_info_Title, Data.Settings_info_Data, "Got it");
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

        void SourceLanguageTapped(object sender, EventArgs args)
        {
            navBar.SourceLanguageTapped();
        }
        void PickerLanguage_Clicked(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            TranslatedText.Children.Clear();
            if (g_lines.Any())
            {
                BoxesLayout.Children.Clear();
                Translate_Txt(Utils.LanguageEnumToIdentifier((string)picker.SelectedItem), g_lines);
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
