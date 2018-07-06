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
        public event EventHandler pickLanguage_Clicked;
        public Exception Error
        {
            get;
            private set;
        }
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

            Utils.generatePicker(DestinationLangPicker, Data.destinationLanguages);
            Utils.generatePicker(SourceLangPicker, Data.sourceLanguages);
        }
        void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            uploadPictureButton_Clicked?.Invoke(sender, e);
        }

        void OpenLanguagePicker_Clicked(object sender, EventArgs e)
        {
            DestinationLangPicker.Focus();
        }

        public void PickLanguage_Clicked(object sender, EventArgs e)
        {
            pickLanguage_Clicked?.Invoke(sender, e);
            DestinationLangPicker.Title = DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
            destinationLanguage.Source = Utils.generateFlag(DestinationLangPicker.Title, destinationLanguage.Source);
            if (Device.OS == TargetPlatform.Android)
            {
                DestinationLangPicker.WidthRequest = DestinationLangPicker.Title.Length * 12;
            }
        }

        void UnfocusedPicker(object sender, EventArgs e)
        {
            if (DestinationLangPicker.SelectedIndex < 0)
                DestinationLangPicker.SelectedIndex = 0;
        }
    }
}