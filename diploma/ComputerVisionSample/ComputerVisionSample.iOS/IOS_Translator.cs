using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using ComputerVisionSample.Translator;
using Xamarin.Forms;

[assembly: Dependency(typeof(ComputerVisionSample.IOS.IOS_Translator))]

namespace ComputerVisionSample.IOS
{
    /// <summary>
    /// Translates text using Google's online language tools.
    /// </summary>
    ///
    public class IOS_Translator : PCL_Translator
    {
        #region Properties
        /// <summary>
        /// Gets the supported languages.
        /// </summary>
        public static IEnumerable<string> Languages
        {
            get
            {
                IOS_Translator.EnsureInitialized();
                return IOS_Translator._languageModeMap.Keys.OrderBy(p => p);
            }
        }

        /// <summary>
        /// Gets the time taken to perform the translation.
        /// </summary>
        public TimeSpan TranslationTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the url used to speak the translation.
        /// </summary>
        /// <value>The url used to speak the translation.</value>
        public string TranslationSpeechUrl
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error
        {
            get;
            private set;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Translates the specified source text.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="sourceLanguage">The source language.</param>
        /// <param name="targetLanguage">The target language.</param>
        /// <returns>The translation.</returns>
        public string Translate
            (string sourceText,
             string sourceLanguage,
             string targetLanguage)
        {
            // Initialize
            this.Error = null;
            this.TranslationSpeechUrl = null;
            this.TranslationTime = TimeSpan.Zero;
            DateTime tmStart = DateTime.Now;
            string translation = string.Empty;

            try
            {
                // Download translation
                string url = string.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
                                            sourceLanguage,
                                            IOS_Translator.LanguageEnumToIdentifier(targetLanguage),
                                            HttpUtility.UrlEncode(sourceText));
                string outputFile = Path.GetTempFileName();
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                    wc.DownloadFile(url, outputFile);
                }

                // Get translated text
                if (File.Exists(outputFile))
                {

                    // Get phrase collection
                    string text = File.ReadAllText(outputFile);
                    int index = text.IndexOf(string.Format(",,\"{0}\"", IOS_Translator.LanguageEnumToIdentifier(sourceLanguage)));
                    if (index == -1)
                    {
                        // Translation of single word
                        int startQuote = text.IndexOf('\"');
                        if (startQuote != -1)
                        {
                            int endQuote = text.IndexOf('\"', startQuote + 1);
                            if (endQuote != -1)
                            {
                                translation = text.Substring(startQuote + 1, endQuote - startQuote - 1);
                            }
                        }
                    }
                    else
                    {
                        // Translation of phrase
                        text = text.Substring(0, index);
                        text = text.Replace("],[", ",");
                        text = text.Replace("]", string.Empty);
                        text = text.Replace("[", string.Empty);
                        text = text.Replace("\",\"", "\"");

                        // Get translated phrases
                        string[] phrases = text.Split(new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; (i < phrases.Count()); i += 2)
                        {
                            string translatedPhrase = phrases[i];
                            if (translatedPhrase.StartsWith(",,"))
                            {
                                i--;
                                continue;
                            }
                            translation += translatedPhrase + "  ";
                        }
                    }

                    // Fix up translation
                    translation = translation.Trim();
                    translation = translation.Replace(" ?", "?");
                    translation = translation.Replace(" !", "!");
                    translation = translation.Replace(" ,", ",");
                    translation = translation.Replace(" .", ".");
                    translation = translation.Replace(" ;", ";");

                    // And translation speech URL
                    this.TranslationSpeechUrl = string.Format("https://translate.googleapis.com/translate_tts?ie=UTF-8&q={0}&tl={1}&total=1&idx=0&textlen={2}&client=gtx",
                                                               HttpUtility.UrlEncode(translation), IOS_Translator.LanguageEnumToIdentifier(targetLanguage), translation.Length);
                }
            }
            catch (Exception ex)
            {
                translation = "Вибачте, забагато слів :-(";
                this.Error = ex;
                return translation;
            }

            // Return result
            this.TranslationTime = DateTime.Now - tmStart;
            return translation;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Converts a language to its identifier.
        /// </summary>
        /// <param name="language">The language."</param>
        /// <returns>The identifier or <see cref="string.Empty"/> if none.</returns>
        private static string LanguageEnumToIdentifier
            (string language)
        {
            string mode = string.Empty;
            IOS_Translator.EnsureInitialized();
            IOS_Translator._languageModeMap.TryGetValue(language, out mode);
            return mode;
        }

        /// <summary>
        /// Ensures the IOS_Translator has been initialized.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (IOS_Translator._languageModeMap == null)
            {
                IOS_Translator._languageModeMap = new Dictionary<string, string>();
                IOS_Translator._languageModeMap.Add("English", "en");
                IOS_Translator._languageModeMap.Add("Russian", "ru");
                IOS_Translator._languageModeMap.Add("Ukrainian", "uk");
                IOS_Translator._languageModeMap.Add("French", "fr");
                IOS_Translator._languageModeMap.Add("German", "de");
                IOS_Translator._languageModeMap.Add("Afrikaans", "af");
                IOS_Translator._languageModeMap.Add("Albanian", "sq");
                IOS_Translator._languageModeMap.Add("Arabic", "ar");
                IOS_Translator._languageModeMap.Add("Armenian", "hy");
                IOS_Translator._languageModeMap.Add("Azerbaijani", "az");
                IOS_Translator._languageModeMap.Add("Basque", "eu");
                IOS_Translator._languageModeMap.Add("Belarusian", "be");
                IOS_Translator._languageModeMap.Add("Bengali", "bn");
                IOS_Translator._languageModeMap.Add("Bulgarian", "bg");
                IOS_Translator._languageModeMap.Add("Catalan", "ca");
                IOS_Translator._languageModeMap.Add("Chinese", "zh-CN");
                IOS_Translator._languageModeMap.Add("Croatian", "hr");
                IOS_Translator._languageModeMap.Add("Czech", "cs");
                IOS_Translator._languageModeMap.Add("Danish", "da");
                IOS_Translator._languageModeMap.Add("Dutch", "nl");
                IOS_Translator._languageModeMap.Add("Esperanto", "eo");
                IOS_Translator._languageModeMap.Add("Estonian", "et");
                IOS_Translator._languageModeMap.Add("Filipino", "tl");
                IOS_Translator._languageModeMap.Add("Finnish", "fi");
                IOS_Translator._languageModeMap.Add("Galician", "gl");
                IOS_Translator._languageModeMap.Add("Georgian", "ka");
                IOS_Translator._languageModeMap.Add("Greek", "el");
                IOS_Translator._languageModeMap.Add("Haitian Creole", "ht");
                IOS_Translator._languageModeMap.Add("Hebrew", "iw");
                IOS_Translator._languageModeMap.Add("Hindi", "hi");
                IOS_Translator._languageModeMap.Add("Hungarian", "hu");
                IOS_Translator._languageModeMap.Add("Icelandic", "is");
                IOS_Translator._languageModeMap.Add("Indonesian", "id");
                IOS_Translator._languageModeMap.Add("Irish", "ga");
                IOS_Translator._languageModeMap.Add("Italian", "it");
                IOS_Translator._languageModeMap.Add("Japanese", "ja");
                IOS_Translator._languageModeMap.Add("Korean", "ko");
                IOS_Translator._languageModeMap.Add("Lao", "lo");
                IOS_Translator._languageModeMap.Add("Latin", "la");
                IOS_Translator._languageModeMap.Add("Latvian", "lv");
                IOS_Translator._languageModeMap.Add("Lithuanian", "lt");
                IOS_Translator._languageModeMap.Add("Macedonian", "mk");
                IOS_Translator._languageModeMap.Add("Malay", "ms");
                IOS_Translator._languageModeMap.Add("Maltese", "mt");
                IOS_Translator._languageModeMap.Add("Norwegian", "no");
                IOS_Translator._languageModeMap.Add("Persian", "fa");
                IOS_Translator._languageModeMap.Add("Polish", "pl");
                IOS_Translator._languageModeMap.Add("Portuguese", "pt");
                IOS_Translator._languageModeMap.Add("Romanian", "ro");
                IOS_Translator._languageModeMap.Add("Serbian", "sr");
                IOS_Translator._languageModeMap.Add("Slovak", "sk");
                IOS_Translator._languageModeMap.Add("Slovenian", "sl");
                IOS_Translator._languageModeMap.Add("Spanish", "es");
                IOS_Translator._languageModeMap.Add("Swahili", "sw");
                IOS_Translator._languageModeMap.Add("Swedish", "sv");
                IOS_Translator._languageModeMap.Add("Tamil", "ta");
                IOS_Translator._languageModeMap.Add("Telugu", "te");
                IOS_Translator._languageModeMap.Add("Thai", "th");
                IOS_Translator._languageModeMap.Add("Turkish", "tr");
                IOS_Translator._languageModeMap.Add("Urdu", "ur");
                IOS_Translator._languageModeMap.Add("Vietnamese", "vi");
                IOS_Translator._languageModeMap.Add("Welsh", "cy");
                IOS_Translator._languageModeMap.Add("Yiddish", "yi");
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The language to translation mode map.
        /// </summary>
        private static Dictionary<string, string> _languageModeMap;

        #endregion
    }
}
