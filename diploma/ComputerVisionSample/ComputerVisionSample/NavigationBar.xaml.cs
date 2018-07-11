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

        public string getCurrentSourceLanguage()
        {
            return (string)SourceLangPicker.SelectedItem != null ? (string)SourceLangPicker.SelectedItem : "English";
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
  
            Picker picker = (Picker)sender;

            if (DestinationLangPicker == picker)
            {
                destinationLanguage.Source = Utils.generateFlag((string)picker.SelectedItem);
            }
            else if (SourceLangPicker == picker)
            {
                sourceLanguage.Source = Utils.generateFlag((string)picker.SelectedItem);
            }
        }

        // TODO - handle if picker index is -1
        void UnfocusedPicker(object sender, EventArgs e)
        {
            //if (DestinationLangPicker.SelectedIndex < 0)
            //{
            //    DestinationLangPicker.SelectedIndex = 0;
            //}
        }
    }
}