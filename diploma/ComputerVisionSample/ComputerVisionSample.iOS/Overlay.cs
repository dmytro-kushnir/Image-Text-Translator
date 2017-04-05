using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace ComputerVisionSample.iOS
{
   public class Overlay
    {
        Func<object> overlay = () =>
        {
            var imageView = new UIImageView(UIImage.FromBundle("face-template.png"));
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            var screen = UIScreen.MainScreen.Bounds;
            imageView.Frame = screen;

            return imageView;
        };
    }
}
