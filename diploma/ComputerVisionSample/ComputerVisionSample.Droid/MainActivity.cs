using Android.App;
using System;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms.Platform.Android;

namespace ComputerVisionSample.Droid
{
    [Activity(Label = "ComputerVisionSample", Icon = "@drawable/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.Window.RequestFeature(WindowFeatures.ActionBar);
             //Name of the MainActivity theme you had there before.
           //  Or you can use global::Android.Resource.Style.ThemeHoloLight
                base.SetTheme(Resource.Style.MainTheme_Base);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);

            //CachedImageRenderer.Init(true);

            //var config = new FFImageLoading.Config.Configuration()
            //{
            //    VerboseLogging = false,
            //    VerbosePerformanceLogging = false,
            //    VerboseMemoryCacheLogging = false,
            //    VerboseLoadingCancelledLogging = false,
            //    Logger = new CustomLogger(),
            //};
            //FFImageLoading.ImageService.Instance.Initialize(config);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            
            LoadApplication(new App());
        }

        //public class CustomLogger : FFImageLoading.Helpers.IMiniLogger
        //{
        //    public void Debug(string message)
        //    {
        //        Console.WriteLine(message);
        //    }

        //    public void Error(string errorMessage)
        //    {
        //        Console.WriteLine(errorMessage);
        //    }

        //    public void Error(string errorMessage, Exception ex)
        //    {
        //        Error(errorMessage + System.Environment.NewLine + ex.ToString());
        //    }
        //}
    }
}

