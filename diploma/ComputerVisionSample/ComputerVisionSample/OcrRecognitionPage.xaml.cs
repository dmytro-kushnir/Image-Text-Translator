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

     //       OcrResults ocrResult =
  //await visionClient.RecognizeTextAsync(inputFile,
  //new RecognizeLanguage() { ShortCode = "it", LongName = "Italian" }

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
            this.translatedText.Text = "";
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
                        var textLabel = new Label { Text = word.Text };

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
            this.translatedText.Text = "";
            DestinationLangPicker.IsVisible = true;
            GettedLanguage.IsVisible = true;
        }
        /// <summary>
        /// //////////////// TRANSLATION///////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       
        void picker_language_choose(object sender, EventArgs e)
        {
        
            Image1.IsVisible = false;
            DestinationLangPicker.Title = DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
            //if (Device.OS == TargetPlatform.Android)
          //  {
                if (sourceLanguage != "unk")
                {
                    if(DestinationLangPicker.SelectedIndex == 0)
                    {
                    this.translatedText.Text = "";
                 //   DestinationLangPicker.WidthRequest = 200;
                }
                else {
                   
                    var translationResult =
                DependencyService.Get<PCL_Translator>().Translate(sourceText, sourceLanguage, DestinationLangPicker.Title);
                    this.translatedText.Text = translationResult;
                }
                   
                //int tempSource = DestinationLangPicker.SelectedIndex;
                //int tempDest = DestinationLangPicker.SelectedIndex + 1;

                //if (tempSource >= DestinationLangPicker.Items.Count-1) { // if choosen is in end of list
                //     tempDest = DestinationLangPicker.SelectedIndex-1; // swap with previous, not with next
                //}

                //    string temp = DestinationLangPicker.Items[tempSource];
                //    DestinationLangPicker.Items[tempSource] = DestinationLangPicker.Items[tempDest];
                //    DestinationLangPicker.Items[tempDest] = temp;

            }
                else
                {
                    this.translatedText.Text = "unknown language! Please try again";
                }
                
           // }
        }
        
    }

}
