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
        public event EventHandler pickerLanguage_Clicked;
        public event EventHandler pickerSettings_Clicked;
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

            Utils.GenerateImageGesture(downloadImages, tgr);
            Utils.GenerateImageGesture(pickLanguage, tgr2);
            Utils.GenerateImageGesture(pickSettings, tgr3);

            Utils.GeneratePicker(DestinationLangPicker, Data.destinationLanguages);
            Utils.GeneratePicker(SettingsPicker, Data.settings);
        }
        public string CheckHandwrittenMode()
        {
            return (string)SettingsPicker.SelectedItem ?? Data.Settings_defaultMode;
        }
        public string GetDestinationLanguage()
        {
            return (string)DestinationLangPicker.SelectedItem ?? "English";
        }
        public void SetPickersToDefault()
        {
            SettingsPicker.SelectedIndex = 0;
            DestinationLangPicker.SelectedIndex = 0;
        }
        public void SourceLanguageTapped() => DestinationLangPicker.Focus();

        void UploadPictureButton_Clicked(object sender, EventArgs e) => uploadPictureButton_Clicked?.Invoke(sender, e);

        void OpenLanguagePicker_Clicked(object sender, EventArgs e) => DestinationLangPicker.Focus();

        void OpenSettingsPicker_Clicked(object sender, EventArgs e) => SettingsPicker.Focus();

        void Picker_Clicked(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            if (DestinationLangPicker == picker)
            {
                if (DestinationLangPicker.SelectedIndex == 0)
                {
                    DestinationLangPicker.SelectedIndex = 1;
                    picker.SelectedItem = "English";
                };
                destinationLanguage.Source = Utils.GenerateFlag((string)picker.SelectedItem);
                pickerLanguage_Clicked?.Invoke(sender, e);
            }
            else if (SettingsPicker == picker)
            {
                settings.Source = Utils.GenerateFlag((string)picker.SelectedItem);
                pickerSettings_Clicked?.Invoke(sender, e);
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