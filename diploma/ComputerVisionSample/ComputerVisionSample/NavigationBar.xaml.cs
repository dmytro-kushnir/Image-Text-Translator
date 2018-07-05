using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ComputerVisionSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationBar : ContentView
    {
        public event EventHandler uploadPictureButton_Clicked;
        public NavigationBar()
        {
            InitializeComponent();
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => UploadPictureButton_Clicked(s, e);
            gallery.GestureRecognizers.Add(tgr);
            camera.GestureRecognizers.Add(tgr);
        }
        void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            uploadPictureButton_Clicked?.Invoke(sender, e);
        }
    }
}