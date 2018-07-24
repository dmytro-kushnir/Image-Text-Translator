using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerVisionSample.helpers
{
    class Data
    {
        public const string VERSION = "2.0";
        public const string Settings_defaultMode = "Settings";
        public const string Settings_info = "Info";
        public const string Settings_clrAll = "Clear all";
        public const string Settings_handwrittenMode = "Handwritten mode";

        public const string Settings_info_Title = "Image Text Translator v" + VERSION;
        public const string Settings_info_Data = "Application requires internet connection! \n " +
            "Functionality: \n " +
            "- To start translating you may press camera or gallery icon \n " +
            "- Before that you may choose translation destination language in right bottom corne \n" +
            "- You are able to change destination language in any time \n" +
            "- 'Clear all' option removes all recognized data, and returns to default app state \n " +
            "- Handwritten Mode. IMPORTANT. For now allows to recognize only english text \n " +
            "- Awailable languages to detect: English, Czech, Danish, Dutch, Finnish, French, German, Greek, Hungarian, ChineseSimplified, ChineseTraditional, Italian, Japanese, Korean, Norwegian, Polish, Portuguese, Russian, Spanish, Swedish, Turkish, Romanian, Slovac, Serbian \n " +
            "- To enter ZOOM mode double tap on image after recognition is complete. To exit double tap again on image\n " +
            "- To copy recognized and translated text into clipboard just use double click and popup will be triggered.";

        static public string[] settings = new String[]
        {
            Settings_defaultMode,
            Settings_info,
            Settings_clrAll,
            Settings_handwrittenMode
        };

        static public string[] destinationLanguages = new String[]
        {
            "Choose destination language",
            "English",
            "Ukrainian",
            "Russian",
            "French",
            "Polish",
            "Spanish",
            "German",
            "Italian",
            "Latvian",
            "Chinese",
            "Japanese",
            "Korean",
            "Portuguese",
            "Arabic",
            "Hindi",
            "Hebrew",
            "Swedish",
            "Danish",
            "Norwegian",
            "Finnish",
            "Georgian",
            "Turkish",
            "Czech",
            "Greek"
        };
        public static Dictionary<string, string> _languageModeMap = new Dictionary<string, string>
        {
            ["Russian"] = "ru",
            ["Ukrainian"] = "uk",
            ["French"] = "fr",
            ["German"] = "de",
            ["Afrikaans"] = "af",
            ["Albanian"] = "sq",
            ["Arabic"] = "ar",
            ["Armenian"] = "hy",
            ["Azerbaijani"] = "az",
            ["Basque"] = "eu",
            ["Belarusian"] = "be",
            ["Bengali"] = "bn",
            ["Catalan"] = "ca",
            ["Chinese"] = "zh-CN",
            ["Croatian"] = "hr",
            ["Czech"] = "cs",
            ["Danish"] = "da",
            ["Dutch"] = "nl",
            ["Esperanto"] = "eo",
            ["Estonian"] = "et",
            ["Filipino"] = "tl",
            ["Finnish"] = "fi",
            ["Galician"] = "gl",
            ["Georgian"] = "ka",
            ["Greek"] = "el",
            ["Haitian Creole"] = "ht",
            ["Hebrew"] = "iw",
            ["Hindi"] = "hi",
            ["Hungarian"] = "hu",
            ["Icelandic"] = "is",
            ["Indonesian"] = "id",
            ["Irish"] = "ga",
            ["Italian"] = "it",
            ["Japanese"] = "ja",
            ["Korean"] = "ko",
            ["Lao"] = "lo",
            ["Latvian"] = "lv",
            ["Lithuanian"] = "lt",
            ["Macedonian"] = "mk",
            ["Malay"] = "ms",
            ["Maltese"] = "mt",
            ["Norwegian"] = "no",
            ["Persian"] = "fa",
            ["Polish"] = "pl",
            ["Portuguese"] = "pt",
            ["Romanian"] = "ro",
            ["Serbian"] = "sr",
            ["Slovak"] = "sk",
            ["Slovenian"] = "sl",
            ["Swedish"] = "sv",
            ["Tamil"] = "ta",
            ["Thai"] = "th",
            ["Urdu"] = "ur",
            ["Vietnamese"] = "vi",
            ["Welsh"] = "cy",
            ["Yiddish"] = "yi"
        };

        static public string[] hardwrittenLanguageSupports = new String[] 
        {
            "blab",
            "English",
            "English1"
        };
        static public string[] subscriptionKeys = new String[] 
        {
            "cf3b45431cc14c799696821dd9668990",
            "81e68751a466446c80076b8f82fd1adc"
        }; // 7c45fc48ba8f42e993a1cd173e1b59a7 
    }
}
