using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ComputerVisionSample.helpers;

namespace ComputerVisionSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationBar : ContentView
    {
        public event EventHandler uploadPictureButton_Clicked;
        public event EventHandler openLanguagePicker_Clicked;
        public NavigationBar()
        {
            InitializeComponent();
            Image[] downloadImages = new Image[] { gallery, camera };
            Image[] pickImages = new Image[] { sourceLanguage, destinationLanguage };

            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => UploadPictureButton_Clicked(s, e);
            var tgr2 = new TapGestureRecognizer();
            tgr2.Tapped += (s, e) => OpenLanguagePicker_Clicked(s, e);

            Utils.generateImageGesture(downloadImages, tgr);
            Utils.generateImageGesture(pickImages, tgr2);
        }
        void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            uploadPictureButton_Clicked?.Invoke(sender, e);
        }

        void OpenLanguagePicker_Clicked(object sender, EventArgs e)
        {
            openLanguagePicker_Clicked?.Invoke(sender, e);
        }
    }
}