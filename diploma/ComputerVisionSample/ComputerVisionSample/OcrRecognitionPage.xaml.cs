using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ComputerVisionSample.Translator;

namespace ComputerVisionSample
{
    public partial class OcrRecognitionPage : ContentPage
    {

        private readonly VisionServiceClient visionClient;
        string sourceLanguage = "en"; // english by default
        string sourceText = "";
  
        public OcrRecognitionPage()
        {
            InitializeComponent();
            this.visionClient = new VisionServiceClient("4d8f3dcfe9f346028b5b3d36c082c7d9");
            DestinationLangPicker.IsVisible = false;
            GettedLanguage.IsVisible = false;
          

        }

        private async Task<OcrResults> AnalyzePictureAsync(Stream inputFile)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Network error", "Please check your network connection and retry.", "OK");
                return null;
            }

            OcrResults ocrResult = await visionClient.RecognizeTextAsync(inputFile);
            return ocrResult;
        }
        
        private async void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            Image1.IsVisible = true;
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                SaveToAlbum = false,
                Name = "test.jpg"
            });
        
            if (file == null)
                return;
           
            this.Indicator1.IsVisible = true;
            this.Indicator1.IsRunning = true;
        

            Image1.Source = ImageSource.FromStream(() => file.GetStream());
            

            var ocrResult = await AnalyzePictureAsync(file.GetStream());

            this.BindingContext = null;           
            this.BindingContext = ocrResult;
            sourceLanguage = ocrResult.Language;

            PopulateUIWithRegions(ocrResult);

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
            DestinationLangPicker.SelectedIndex = 0;
            DestinationLangPicker.Title = "Destination language";

            Image1.IsVisible = true;
            this.TranslatedText.Children.Clear();
            DestinationLangPicker.IsVisible = true;
            GettedLanguage.IsVisible = true;

        }

        private void PopulateUIWithRegions(OcrResults ocrResult)
        {
            this.DetectedText.Children.Clear();
            this.sourceText = "";
            // Iterate the regions
            foreach (var region in ocrResult.Regions)
            {
                // Iterate lines per region
                foreach (var line in region.Lines)
                {
                    // For each line, add a panel
                    // to present words horizontally
                    var lineStack = new StackLayout
                    { Orientation = StackOrientation.Horizontal };

                    // Iterate words per line and add the word
                    // to the StackLayout
                    foreach (var word in line.Words)
                    {
                        var textLabel = new Label { TextColor = Xamarin.Forms.Color.Black,
                            Text = word.Text };

                        sourceText += textLabel.Text + " "; 

                        lineStack.Children.Add(textLabel);
                        
                    }
                    // Add the StackLayout to the UI
                    this.DetectedText.Children.Add(lineStack);
                }
            }
        }

        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
        
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync();
            if (file == null)
                return;

            this.Indicator1.IsVisible = true;
            this.Indicator1.IsRunning = true;
            Image1.Source = ImageSource.FromStream(() => file.GetStream());

            var ocrResult = await AnalyzePictureAsync(file.GetStream());

            this.BindingContext = ocrResult;
            sourceLanguage = ocrResult.Language;

            PopulateUIWithRegions(ocrResult);

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;

            DestinationLangPicker.SelectedIndex = 0;
            DestinationLangPicker.Title = "Destination language";

            Image1.IsVisible = true;
            this.TranslatedText.Children.Clear();
            DestinationLangPicker.IsVisible = true;
            GettedLanguage.IsVisible = true;
            
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (DeviceInfo.IsOrientationPortrait() && width > height || !DeviceInfo.IsOrientationPortrait() && width < height)
            {

                // Orientation got changed! Do your changes here
              //  Image1.IsVisible = false;
            }
        }


        /// //////////////// TRANSLATION///////////////////////
        void picker_language_choose(object sender, EventArgs e)
        {
            Image1.IsVisible = false;
            if (DestinationLangPicker.SelectedIndex != 0)  //  запобігаємо зміни бекграунду при ініціалізції 
            {
                backgroundImage.Source = Image1.Source;
                backgroundImage.Opacity = 0.1;
                backgroundImage.HeightRequest = 240;
            }
            this.TranslatedText.Children.Clear();
            DestinationLangPicker.Title = DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
                if (sourceLanguage != "unk") {
                    if(DestinationLangPicker.SelectedIndex == 0) {
                    this.TranslatedText.Children.Clear();
                    }
                    else {
                    var translatedLabel = new Label {
                        TextColor = Xamarin.Forms.Color.Black,
                        Text = DependencyService.Get<PCL_Translator>().Translate(sourceText, sourceLanguage, DestinationLangPicker.Title)};
                    this.TranslatedText.Children.Add(translatedLabel);
                    
                    }
                }
                else {
                   var Error = new Label { Text = "unknown language! Please try again" };
                  this.TranslatedText.Children.Add(Error); 
                }
            }
        }
}
