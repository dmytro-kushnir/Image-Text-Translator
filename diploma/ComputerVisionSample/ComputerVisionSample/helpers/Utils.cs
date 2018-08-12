using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static ImageSource GenerateFlag(string language)
        {
            ImageSource imgSrc = null;
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
                case "Croatian": imgSrc = "croatia.png"; break;
                case "Estonian": imgSrc = "estonia.png"; break;
                case "Bulgarian": imgSrc = "bulgaria.png"; break;
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
        public static void GenerateImageGesture(Image[] images, TapGestureRecognizer tgr)
        {
            foreach (var image in images)
            {
                image.GestureRecognizers.Add(tgr);
            }
        }
        public static void GeneratePicker(Picker picker, string[] pickers)
        {
            foreach (var pr in pickers)
            {
                picker.Items.Add(pr);
            }
        }
        /// <summary>
        /// Генерує розмір шрифта, відповідно до висоти прямокутника
        /// </summary>
        /// <param name="boxHeight">Висота прямокутника</param>
        /// <returns>[Int] - розмір шрифта</returns>
        public static int FontSizeGenerator(double boxHeight)
        {
            const int SMALL_BOX_HEIGHT = 60;
            const int MEDIUM_BOX_HEIGHT = 80;
            const int BIX_BOX_HEIGHT = 120;

            const int SMALL_FONT_SIZE = 16; 
            const int MEDIUM_FONT_SIZE = 20; 
            const int BIX_FONT_SIZE = 24; 
            const int EXTRA_BIX_FONT_SIZE = 28;

            if (boxHeight <= SMALL_BOX_HEIGHT)
            {
                return SMALL_FONT_SIZE;
            }
            else if (boxHeight <= MEDIUM_BOX_HEIGHT)
            {
                return MEDIUM_FONT_SIZE;
            }
            else
            {
                return boxHeight <= BIX_BOX_HEIGHT ? BIX_FONT_SIZE : EXTRA_BIX_FONT_SIZE;
            }
        }

        /// <summary> 
        /// Конвертування "мова -> ідентифікатор" (наприклад Ukrainian -> ua)
        /// </summary>
        /// <param name="language">Мова."</param>
        /// <returns>Ідентифікатор <see cref="string.Empty"/> якщо нема співпадінь</returns>
        public static string LanguageEnumToIdentifier(string language)
        {
            string mode = string.Empty;
            Data._languageModeMap.TryGetValue(language, out mode);
            return mode;
        }

        public static string GenerateRandomKey(string[] keys)
        {
            Random rand = new Random();
            Debug.WriteLine("randomIndex -> {0} ", rand.Next(0, keys.Length));
            return keys[rand.Next(0, keys.Length)];
        }
    }
}
