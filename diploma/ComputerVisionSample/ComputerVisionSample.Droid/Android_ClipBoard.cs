using Android.Content;
using Xamarin.Forms;
using ComputerVisionSample.ClipBoard;
[assembly: Dependency(typeof(ComputerVisionSample.Droid.Android_ClipBoard))]
namespace ComputerVisionSample.Droid
{
    public class Android_ClipBoard : PCL_ClipBoard
    {
        public void GetTextFromClipBoard(string text)
        {
            // Get the Clipboard Manager
            var clipboardManager = (ClipboardManager)Forms.Context.GetSystemService(Context.ClipboardService);

            // Create a new Clip
            ClipData clip = ClipData.NewPlainText("xxx_title", text);

            // Copy the text
            clipboardManager.PrimaryClip = clip;
        }
    }
}