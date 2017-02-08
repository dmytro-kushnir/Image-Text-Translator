// Copyright (c) 2015 Ravi Bhavnani
// License: Code Project Open License
// http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using ComputerVisionSample.Translator;
using Xamarin.Forms;


[assembly: Dependency(typeof(ComputerVisionSample.Droid.Android_Translator))]

namespace ComputerVisionSample.Droid
{
    /// <summary>
    /// Translates text using Google's online language tools.
    /// </summary>
    ///
    public class Android_Translator : PCL_Translator
    {
        #region Properties
        /// <summary>
        /// Gets the supported languages.
        /// </summary>
        public static IEnumerable<string> Languages
        {
            get
            {
                Android_Translator.EnsureInitialized();
                return Android_Translator._languageModeMap.Keys.OrderBy(p => p);
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
        public  string Translate
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
                                            Android_Translator.LanguageEnumToIdentifier(targetLanguage),
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
                    if(sourceLanguage == "en")
                        sourceLanguage = "English";
                    else if(sourceLanguage == "ru")
                        sourceLanguage = "Russian";
                    else if (sourceLanguage == "fr")
                        sourceLanguage = "French";
                    else if (sourceLanguage == "gr")
                        sourceLanguage = "German";
                    else if (sourceLanguage == "it")
                    sourceLanguage = "Italian";
                    else if (sourceLanguage == "es")
                        sourceLanguage = "Spanish";
                    else if (sourceLanguage == "ar")
                        sourceLanguage = "Arabic";
                    else if (sourceLanguage == "zh-CN")
                        sourceLanguage = "Chinese";
                    else if (sourceLanguage == "ch")
                        sourceLanguage = "Chinese";
                    else if (sourceLanguage == "pl")
                        sourceLanguage = "Polish";
                    else if (sourceLanguage == "tr")
                        sourceLanguage = "Turkish";
                    else if (sourceLanguage == "pt")
                        sourceLanguage = "Portuguese";
                    else if (sourceLanguage == "el")
                        sourceLanguage = "Greek";
                    else if (sourceLanguage == "ja")
                        sourceLanguage = "Japanese";
                    else if (sourceLanguage == "hu")
                        sourceLanguage = "Hungarian";
                    else if (sourceLanguage == "fi")
                        sourceLanguage = "Finnish";
                    else if (sourceLanguage == "sv")
                        sourceLanguage = "Swedish";
                    else if (sourceLanguage == "no")
                        sourceLanguage = "Norwegian";
                    else if (sourceLanguage == "ko")
                        sourceLanguage = "Korean";
                    else if (sourceLanguage == "cs")
                        sourceLanguage = "Czech";
                    else if (sourceLanguage == "da")
                        sourceLanguage = "Danish";
                    else if (sourceLanguage == "nl")
                        sourceLanguage = "Dutch";
                  
                    // Get phrase collection
                    string text = File.ReadAllText(outputFile);
                    int index = text.IndexOf(string.Format(",,\"{0}\"", Android_Translator.LanguageEnumToIdentifier(sourceLanguage)));
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
                                                               HttpUtility.UrlEncode(translation), Android_Translator.LanguageEnumToIdentifier(targetLanguage), translation.Length);
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
            Android_Translator.EnsureInitialized();
            Android_Translator._languageModeMap.TryGetValue(language, out mode);
            return mode;
        }

        /// <summary>
        /// Ensures the Android_Translator has been initialized.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (Android_Translator._languageModeMap == null)
            {
                Android_Translator._languageModeMap = new Dictionary<string, string>();
                Android_Translator._languageModeMap.Add("English", "en");
                Android_Translator._languageModeMap.Add("Russian", "ru");
                Android_Translator._languageModeMap.Add("Ukrainian", "uk");
                Android_Translator._languageModeMap.Add("French", "fr");
                Android_Translator._languageModeMap.Add("German", "de");
                Android_Translator._languageModeMap.Add("Afrikaans", "af");
                Android_Translator._languageModeMap.Add("Albanian", "sq");
                Android_Translator._languageModeMap.Add("Arabic", "ar");
                Android_Translator._languageModeMap.Add("Armenian", "hy");
                Android_Translator._languageModeMap.Add("Azerbaijani", "az");
                Android_Translator._languageModeMap.Add("Basque", "eu");
                Android_Translator._languageModeMap.Add("Belarusian", "be");
                Android_Translator._languageModeMap.Add("Bengali", "bn");
                Android_Translator._languageModeMap.Add("Bulgarian", "bg");
                Android_Translator._languageModeMap.Add("Catalan", "ca");
                Android_Translator._languageModeMap.Add("Chinese", "zh-CN");
                Android_Translator._languageModeMap.Add("Croatian", "hr");
                Android_Translator._languageModeMap.Add("Czech", "cs");
                Android_Translator._languageModeMap.Add("Danish", "da");
                Android_Translator._languageModeMap.Add("Dutch", "nl");
                Android_Translator._languageModeMap.Add("Esperanto", "eo");
                Android_Translator._languageModeMap.Add("Estonian", "et");
                Android_Translator._languageModeMap.Add("Filipino", "tl");
                Android_Translator._languageModeMap.Add("Finnish", "fi");
                Android_Translator._languageModeMap.Add("Galician", "gl");
                Android_Translator._languageModeMap.Add("Georgian", "ka");
                Android_Translator._languageModeMap.Add("Greek", "el");
                Android_Translator._languageModeMap.Add("Haitian Creole", "ht");
                Android_Translator._languageModeMap.Add("Hebrew", "iw");
                Android_Translator._languageModeMap.Add("Hindi", "hi");
                Android_Translator._languageModeMap.Add("Hungarian", "hu");
                Android_Translator._languageModeMap.Add("Icelandic", "is");
                Android_Translator._languageModeMap.Add("Indonesian", "id");
                Android_Translator._languageModeMap.Add("Irish", "ga");
                Android_Translator._languageModeMap.Add("Italian", "it");
                Android_Translator._languageModeMap.Add("Japanese", "ja");
                Android_Translator._languageModeMap.Add("Korean", "ko");
                Android_Translator._languageModeMap.Add("Lao", "lo");
                Android_Translator._languageModeMap.Add("Latin", "la");
                Android_Translator._languageModeMap.Add("Latvian", "lv");
                Android_Translator._languageModeMap.Add("Lithuanian", "lt");
                Android_Translator._languageModeMap.Add("Macedonian", "mk");
                Android_Translator._languageModeMap.Add("Malay", "ms");
                Android_Translator._languageModeMap.Add("Maltese", "mt");
                Android_Translator._languageModeMap.Add("Norwegian", "no");
                Android_Translator._languageModeMap.Add("Persian", "fa");
                Android_Translator._languageModeMap.Add("Polish", "pl");
                Android_Translator._languageModeMap.Add("Portuguese", "pt");
                Android_Translator._languageModeMap.Add("Romanian", "ro");
                Android_Translator._languageModeMap.Add("Serbian", "sr");
                Android_Translator._languageModeMap.Add("Slovak", "sk");
                Android_Translator._languageModeMap.Add("Slovenian", "sl");
                Android_Translator._languageModeMap.Add("Spanish", "es");
                Android_Translator._languageModeMap.Add("Swahili", "sw");
                Android_Translator._languageModeMap.Add("Swedish", "sv");
                Android_Translator._languageModeMap.Add("Tamil", "ta");
                Android_Translator._languageModeMap.Add("Telugu", "te");
                Android_Translator._languageModeMap.Add("Thai", "th");
                Android_Translator._languageModeMap.Add("Turkish", "tr");
                Android_Translator._languageModeMap.Add("Urdu", "ur");
                Android_Translator._languageModeMap.Add("Vietnamese", "vi");
                Android_Translator._languageModeMap.Add("Welsh", "cy");
                Android_Translator._languageModeMap.Add("Yiddish", "yi");
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
