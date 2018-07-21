using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace ComputerVisionSample.Droid
{
    [Activity(Label = "ComputerVisionSample", Icon = "@drawable/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            base.Window.RequestFeature(WindowFeatures.ActionBar);
             //Name of the MainActivity theme you had there before.
           //  Or you can use global::Android.Resource.Style.ThemeHoloLight
                base.SetTheme(Resource.Style.MainTheme_Base);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            
            LoadApplication(new App());
        }
    }
}

