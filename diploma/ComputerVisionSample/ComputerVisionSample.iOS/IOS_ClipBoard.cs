using Xamarin.Forms;
using UIKit;
using ComputerVisionSample.ClipBoard;

[assembly: Dependency(typeof(ComputerVisionSample.IOS.IOS_clipBoard))]

namespace ComputerVisionSample.IOS
{
    public class IOS_clipBoard  : PCL_ClipBoard
    {
        public void GetTextFromClipBoard(string text)
        {
           // var pb = UIPasteboard.General.GetValue("public.utf8-plain-text");
           //return pb.ToString();
            UIPasteboard clipboard = UIPasteboard.General;
            clipboard.String = text;
        }
    }
}