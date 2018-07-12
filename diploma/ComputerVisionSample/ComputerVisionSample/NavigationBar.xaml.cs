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
        public event EventHandler picker_Clicked;
        public Exception Error
        {
            get;
            private set;
        }
        public NavigationBar()
        {
            InitializeComponent();
            Image[] downloadImages = new Image[] { gallery, camera };
            Image[] pickLanguage = new Image[] { destinationLanguage };
            Image[] pickSettings = new Image[] { settings };

            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => UploadPictureButton_Clicked(s, e);
            var tgr2 = new TapGestureRecognizer();
            tgr2.Tapped += (s, e) => OpenLanguagePicker_Clicked(s, e);
            var tgr3 = new TapGestureRecognizer();
            tgr3.Tapped += (s, e) => OpenSettingsPicker_Clicked(s, e);

            Utils.generateImageGesture(downloadImages, tgr);
            Utils.generateImageGesture(pickLanguage, tgr2);
            Utils.generateImageGesture(pickSettings, tgr3);

            Utils.generatePicker(DestinationLangPicker, Data.destinationLanguages);
            Utils.generatePicker(SettingsPicker, Data.settings);
        }

        public string getCurrentSourceLanguage()
        {
            return (string)SettingsPicker.SelectedItem != null ? (string)SettingsPicker.SelectedItem : "if";
        }

        void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            uploadPictureButton_Clicked?.Invoke(sender, e);
        }

        void OpenLanguagePicker_Clicked(object sender, EventArgs e)
        {
            DestinationLangPicker.Focus();
        }

        void OpenSettingsPicker_Clicked(object sender, EventArgs e)
        {
            SettingsPicker.Focus();
        }

        public void Picker_Clicked(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            if (DestinationLangPicker == picker)
            {
                if (DestinationLangPicker.SelectedIndex == 0)
                {
                    DestinationLangPicker.SelectedIndex = 1;
                };
                destinationLanguage.Source = Utils.generateFlag((string)picker.SelectedItem);
                picker_Clicked?.Invoke(sender, e);
            }
            else if (SettingsPicker == picker)
            {
                settings.Source = Utils.generateFlag((string)picker.SelectedItem);
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