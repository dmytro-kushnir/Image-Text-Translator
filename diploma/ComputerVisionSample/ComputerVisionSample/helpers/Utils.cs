using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ComputerVisionSample.helpers
{
    class Utils
    {
        public static IEnumerable<string> SplitBy(string str, int chunkLength)
        {
            if (String.IsNullOrEmpty(str)) yield return "";

            for (int i = 0; i < str.Length; i += chunkLength)
            {
                if (chunkLength + i > str.Length)
                    chunkLength = str.Length - i;

                yield return str.Substring(i, chunkLength);
            }
         }
        public static ImageSource generateFlag(string language)
        {
            ImageSource imgSrc;
            switch (language)
            {
                case "French": imgSrc = "fr.png"; break;
                case "English": imgSrc = "gb.png"; break;
                case "Russian": imgSrc = "ru.png"; break;
                case "Ukrainian": imgSrc = "ua.png"; break;
                case "Latvian": imgSrc = "lv.png"; break;
                case "German": imgSrc = "gr.png"; break;
                case "Polish": imgSrc = "pl.png"; break;
                case "Spanish": imgSrc = "sp.png"; break;
                case "Italian": imgSrc = "it.png"; break;
                case "Chinese": imgSrc = "china.png"; break;
                case "ChineseTraditional": imgSrc = "china.png"; break;
                case "ChineseSimplified": imgSrc = "china.png"; break;
                case "Korean": imgSrc = "korea.png"; break;
                case "Japanese": imgSrc = "ja.png"; break;
                case "Portuguese": imgSrc = "po.png"; break;
                case "Arabic": imgSrc = "arabic.png"; break;
                case "Hindi": imgSrc = "india.png"; break;
                case "Hebrew": imgSrc = "isr.png"; break;
                case "Swedish": imgSrc = "sw.png"; break;
                case "Norwegian": imgSrc = "norway.png"; break;
                case "Danish": imgSrc = "denmark.png"; break;
                case "Finnish": imgSrc = "finland.png"; break;
                case "Georgian": imgSrc = "ge.png"; break;
                case "Greek": imgSrc = "gre.png"; break;
                case "Turkish": imgSrc = "turkey.png"; break;
                case "Czech": imgSrc = "cz.png"; break;
                case "Slovak": imgSrc = "slo.png"; break;
                case "Romanian": imgSrc = "ro.png"; break;
                case "SerbianLatin": imgSrc = "sk.png"; break;
                case "SerbianCyrillic": imgSrc = "sk.png"; break;
                case Data.Settings_defaultMode: imgSrc = "settings.png"; break;
                case Data.Settings_handwrittenMode: imgSrc = "handwritten.png"; break;
                case Data.Settings_info: imgSrc = "info.png"; break;
                case Data.Settings_clrAll: imgSrc = "clear.png"; break;
                default: imgSrc = "gb.png"; break;
            }
            return imgSrc;
        }
        public static void generateImageGesture(Image[] images, TapGestureRecognizer tgr)
        {
            foreach (var image in images)
            {
                image.GestureRecognizers.Add(tgr);
            }
        }
        public static void generatePicker(Picker picker, string[] pickers)
        {
            foreach (var pr in pickers)
            {
                picker.Items.Add(pr);
            }
        }
    }
}
