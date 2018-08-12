using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.Diagnostics;
using System.IO;
using Plugin.Connectivity;
using Xamarin.Forms;

namespace ComputerVisionSample.services
{
    public partial class CognitiveService : ContentPage
    {
        private readonly VisionServiceClient visionClient;
        protected static readonly TimeSpan QueryWaitTimeInSecond = TimeSpan.FromSeconds(3);
        protected static readonly int MaxRetryTimes = 3;

        public CognitiveService(string subscriptionKey)
        {
            this.visionClient = new VisionServiceClient(subscriptionKey);
        }

        public async Task<OcrResults> AnalyzePictureAsync(Stream inputFile)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                Debug.WriteLine("Network error. Please check your network connection and retry");
                await App.Current.MainPage.DisplayAlert("Network error", "Please check your network connection and retry.", "Got it");
                return null;
            }
            try
            {
                OcrResults ocrResult = await this.visionClient.RecognizeTextAsync(inputFile);
                return ocrResult;
            }
            catch (ClientException ex)
            {
                Debug.WriteLine(ex.Error.Message);
                await App.Current.MainPage.DisplayAlert("Application error","API retuned code -> 500. Internal server Error", "Try later");
                return null;
            }
        }

        /// <summary>
        /// Uploads the image to Project Oxford and performs Handwriting Recognition
        /// </summary>
        /// <param name="inputFile">The image stream.</param>
        /// <returns></returns>
        public async Task<HandwritingRecognitionOperationResult> RecognizeUrl(Stream inputFile)
        {
            return await RecognizeAsync(async (VisionServiceClient VisionServiceClient) => await VisionServiceClient.CreateHandwritingRecognitionOperationAsync(inputFile));
        }

        public async Task<HandwritingRecognitionOperationResult> RecognizeAsync(Func<VisionServiceClient, Task<HandwritingRecognitionOperation>> Func)
        {
            HandwritingRecognitionOperationResult result;
            if (!CrossConnectivity.Current.IsConnected)
            {
                Debug.WriteLine("Network error. Please check your network connection and retry");
                await App.Current.MainPage.DisplayAlert("Network error", "Please check your network connection and retry.", "OK");
                return null;
            }
            try
            {
                Debug.WriteLine("Calling VisionServiceClient.CreateHandwritingRecognitionOperationAsync()...");
                HandwritingRecognitionOperation operation = await Func(this.visionClient);

                Debug.WriteLine("Calling VisionServiceClient.GetHandwritingRecognitionOperationResultAsync()...");
                result = await this.visionClient.GetHandwritingRecognitionOperationResultAsync(operation);
                int i = 0;
                while ((result.Status == HandwritingRecognitionOperationStatus.Running || result.Status == HandwritingRecognitionOperationStatus.NotStarted) && i++ < MaxRetryTimes)
                {
                    Debug.WriteLine(string.Format("Server status: {0}, wait {1} seconds...", result.Status, QueryWaitTimeInSecond));
                    await Task.Delay(QueryWaitTimeInSecond);

                    Debug.WriteLine("Calling VisionServiceClient.GetHandwritingRecognitionOperationResultAsync()...");
                    result = await this.visionClient.GetHandwritingRecognitionOperationResultAsync(operation);
                }
                return result;
            }
            catch (ClientException ex)
            {
                await App.Current.MainPage.DisplayAlert("Application error", "API retuned code -> 500. Internal server Error", "Try later");
                result = new HandwritingRecognitionOperationResult() { Status = HandwritingRecognitionOperationStatus.Failed };
                Debug.WriteLine(ex.Error.Message);
            }
            return result;
        }
    }
}
