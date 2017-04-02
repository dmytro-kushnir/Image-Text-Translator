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
using ComputerVisionSample.ClipBoard;
using System.Drawing;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace ComputerVisionSample
{
      //<x:String >Destination language</x:String>
    public partial class OcrRecognitionPage : ContentPage
    {
        public Exception Error
        {
            get;
            private set;
        }
        // Початкове налаштування
        private int count = 0;
        private readonly VisionServiceClient visionClient;
        string sourceLanguage = "en"; // english by default
        string sourceText = "";
        string destinationLanguage = "";
        string bufferSourceText1 = "";
        string bufferSourceText2 = "";
        string bufferSourceText3 = "";
        string bufferSourceText4 = "";
        string bufferSourceText5 = "";
        string bufferSourceText6 = "";
        // визначимо координати лінії тексту
        int Top = 0;
        int Left = 0;
        int width = 0;
        int height = 0;
        //
        string transaltedText = "";
        bool flag = false; // прапорець для делегування зміною стану кнопок Камери та Галереї
        public OcrRecognitionPage()
        {
            this.Error = null;
            InitializeComponent();
            this.visionClient = new VisionServiceClient("cf3b45431cc14c799696821dd9668990");
            DestinationLangPicker.IsVisible = false;
            GettedLanguage.IsVisible = false;
            BackButton.IsVisible = false;
            BackButton.Text = "<- Back";
            Title = "Simple Circle";
            UploadPictureButton.IsVisible = false;
            TakePictureButton.IsVisible = false;
            SKCanvasView canvasView = new SKCanvasView();
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
            try
            {
               
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", "No camera available.", "OK");
                    return;
                }
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = false,
                    Name = "test.jpg",
                    CompressionQuality = 75
                   
                });
               
                UploadPictureButton.IsVisible = false;
                TakePictureButton.IsVisible = false;
                BackButton.IsVisible = true;
                Image1.IsVisible = true;
                if (file == null)
                    return;
                if (backgroundImage.Opacity != 0)
                {
                    backgroundImage.Opacity = 0;
                }
                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;


                Image1.Source = ImageSource.FromStream(() => file.GetStream());


                var ocrResult = await AnalyzePictureAsync(file.GetStream());
               
                this.BindingContext = null;
                this.BindingContext = ocrResult;
                sourceLanguage = ocrResult.Language;

                PopulateUIWithRegions(ocrResult);
                TranslatedText.IsVisible = true;
                Image1.IsVisible = true;
                this.Indicator1.IsRunning = false;
                this.Indicator1.IsVisible = false;
                //  DestinationLangPicker.SelectedIndex = 0;
                //  DestinationLangPicker.Title = "Destination language";
                flag = true;
                Image1.IsVisible = true;
               // this.TranslatedText.Children.Clear();
                DestinationLangPicker.IsVisible = true;
                GettedLanguage.IsVisible = true;

               
            }
            catch (Exception ex)
            {
                this.Error = ex;
            }
        }

        private void PopulateUIWithRegions(OcrResults ocrResult)
        {
          destinationLanguage = 
                DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];


                TranslatedText.Text = "";
         //   if (DestinationLangPicker.Title == "Destination language") destinationLanguage = "Ukrainian";

                //Iterate the regions
                foreach (var region in ocrResult.Regions)
            {
                //Iterate lines per region
                foreach (var line in region.Lines)
                {
                    //    For each line, add a panel to present words horizontally
                    var lineStack = new StackLayout
                    { Orientation = StackOrientation.Horizontal };

                    //Iterate words per line and add the word
                    //to the StackLayout
                    foreach (var word in line.Words)
                    {
                        var textLabel = new Label
                        {
                            TextColor = Xamarin.Forms.Color.Black,
                            Text = word.Text,
                            
                        };
                        sourceText += textLabel.Text + " ";

                       
                        if(bufferSourceText1.Length < 400)
                        {
                            bufferSourceText1 += textLabel.Text + " ";
                        }
                        else if(bufferSourceText2.Length < 400)
                        {
                            bufferSourceText2 += textLabel.Text + " ";
                        }
                        else if (bufferSourceText3.Length < 400)
                        {
                            bufferSourceText3 += textLabel.Text + " ";
                        }
                        else if (bufferSourceText4.Length < 400)
                        {
                            bufferSourceText4 += textLabel.Text + " ";
                        }
                        else if (bufferSourceText5.Length < 400)
                        {
                            bufferSourceText5 += textLabel.Text + " ";
                        }
                        else
                        {
                            bufferSourceText6 += textLabel.Text + " ";
                        }
                        lineStack.Children.Add(textLabel);
                    }

                    height = line.Rectangle.Height;
                    width = line.Rectangle.Width;
                    Left = line.Rectangle.Left;
                    Top = line.Rectangle.Top;

                    // Відправка обробленого тексту на переклад
                    Incapsulated_Picker(sourceText, destinationLanguage);
                    sourceText = "";
                }
                 
            }
        }
        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
           
        }
        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {         
            try
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                    return;
                }
                var file = await CrossMedia.Current.PickPhotoAsync();
                if (file == null)
                    return;
                if (backgroundImage.Opacity !=0)
                {
                    backgroundImage.Opacity = 0;
                }
                UploadPictureButton.IsVisible = false;
                TakePictureButton.IsVisible = false;
                Image1.IsVisible = true;
                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;
                Image1.Source = ImageSource.FromStream(() => file.GetStream());

                var ocrResult = await AnalyzePictureAsync(file.GetStream());

                this.BindingContext = ocrResult;
                sourceLanguage = ocrResult.Language;
                
                PopulateUIWithRegions(ocrResult);
                TranslatedText.IsVisible = true;

             
            }
            catch (Exception ex)
            {
                this.Error = ex;
            }
            BackButton.IsVisible = true;
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
            flag = true;
            //  DestinationLangPicker.SelectedIndex = 0;
            //    DestinationLangPicker.Title = "Destination language";

            Image1.IsVisible = true;
          //  this.TranslatedText.Children.Clear();
            DestinationLangPicker.IsVisible = true;
            GettedLanguage.IsVisible = true;
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            if (count == 0)
            {
                DestinationLangPicker.Focus();
                count++;
               
            }
            base.OnSizeAllocated(width, height);

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
            TakePictureButton.IsVisible = true;
            UploadPictureButton.IsVisible = true;
            DestinationLangPicker.Focus();
            BackButton.IsVisible = false;
            Image1.IsVisible = false;
            TranslatedText.IsVisible = false;
            GettedLanguage.IsVisible = false;
            DestinationLangPicker.IsVisible = false;
            TranslatedText.IsVisible = false;
             backgroundImage.Opacity = 0.4;
             bufferSourceText1 = "";
             bufferSourceText2 = "";
             bufferSourceText3 = "";
             bufferSourceText4 = "";
             bufferSourceText5 = "";
             bufferSourceText6 = "";
            UploadPictureButton.IsVisible = false;
            TakePictureButton.IsVisible = false;
            flag = false;
        }
        /// //////////////// TRANSLATION///////////////////////
        void Incapsulated_Picker(string sourceTxt, string destLang)
        {
            if (sourceLanguage != "unk")
            {
                    this.TranslatedText.Text += DependencyService.Get<PCL_Translator>().
                            Translate(sourceTxt, sourceLanguage, destLang) + " ";

                transaltedText = TranslatedText.Text;       
            }
            else
            {
                var Error = "unknown language! Please try again";
                this.TranslatedText.Text = Error;
            } 

        }

        public void YourFunctionToHandleMadTaps(object sender, EventArgs e)
        {
            DependencyService.Get<PCL_ClipBoard>().GetTextFromClipBoard(transaltedText);
            DisplayAlert("", "Successfully copied to the clipboard", "OK");
            // clipboardText = TranslatedText.Text;
        }

        void picker_language_choose(object sender, EventArgs e)
        {
            DestinationLangPicker.Title = DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
            DestinationLangPicker.WidthRequest = DestinationLangPicker.Title.Length*11.2;
            TranslatedText.Text = "";
            if (bufferSourceText1.Length > 1)
                Incapsulated_Picker(bufferSourceText1, DestinationLangPicker.Title);
            if(bufferSourceText2.Length > 1)
                Incapsulated_Picker(bufferSourceText2, DestinationLangPicker.Title);
            if (bufferSourceText3.Length > 1)
                Incapsulated_Picker(bufferSourceText3, DestinationLangPicker.Title);
            if (bufferSourceText4.Length > 1)
                Incapsulated_Picker(bufferSourceText3, DestinationLangPicker.Title);
            if (bufferSourceText5.Length > 1)
                Incapsulated_Picker(bufferSourceText3, DestinationLangPicker.Title);
            if (bufferSourceText6.Length > 1)
                Incapsulated_Picker(bufferSourceText3, DestinationLangPicker.Title);

            if (flag == false)
            {
                UploadPictureButton.IsVisible = true;
                TakePictureButton.IsVisible = true;
            }
        }
      
    }
}
