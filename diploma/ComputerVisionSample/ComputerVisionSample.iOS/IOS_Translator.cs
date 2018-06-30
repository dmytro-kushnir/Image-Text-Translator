// БКР : модуль "Перекладач" IOS
// розробник: Кушнір Дмитро (c) 2017
// Призначення: перклад тексту отриманого з зображення та повернення його у вигляді стрічки

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using ComputerVisionSample.Translator;
using Xamarin.Forms;


[assembly: Dependency(typeof(ComputerVisionSample.Droid.IOS_Translator))]

namespace ComputerVisionSample.Droid
{

    public class IOS_Translator : PCL_Translator
    {
        #region Properties
        /// <summary>
        /// Отримання доступних мов
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
        /// Помилки.
        /// </summary>
        public Exception Error
        {
            get;
            private set;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Трансюємо необідний текст
        /// </summary>
        /// <param name="sourceText">Вхідний текст.</param>
        /// <param name="sourceLanguage">Вхідна мова</param>
        /// <param name="targetLanguage">Мова призначення</param>
        /// <returns>Переклад</returns>
        public string Translate
            (string sourceText,
             string sourceLanguage,
             string targetLanguage)
        {
            // Ініціалізація
            this.Error = null;
            DateTime tmStart = DateTime.Now;
            string translation = string.Empty;

            try
            {
                // Завантажити переклад
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

                // Отримуємо мову тексту та парсимо її в потрібний формат
                if (File.Exists(outputFile))
                {
                    if (sourceLanguage == "en")
                        sourceLanguage = "English";
                    else if (sourceLanguage == "ru")
                        sourceLanguage = "Russian";
                    else if (sourceLanguage == "fr")
                        sourceLanguage = "French";
                    else if (sourceLanguage == "gr")
                        sourceLanguage = "German";
                    else if (sourceLanguage == "it")
                        sourceLanguage = "Italian";
                    else if (sourceLanguage == "sr-Cyrl")
                        sourceLanguage = "Russian";
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

                    // Отримуємо колекцію фраз
                    string text = File.ReadAllText(outputFile);
                    int index = text.IndexOf(string.Format(",,\"{0}\"", IOS_Translator.LanguageEnumToIdentifier(sourceLanguage)));

                    if (index == -1)
                    {
                        // ділимо отриманйи текст з севреру на стрічки таким чином : непарні - вхідний текст , парні - трансльований текст
                        string[] phrases = text.Split(new[] { "\"," }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < phrases.Count(); i += 2)
                        {
                            if (i == phrases.Count() - 1) break;
                            int startQuote2 = phrases[i].IndexOf('\"'); // початок стрічки
                            if (startQuote2 != -1)
                            {
                                int endQuote2 = phrases[i].Length; // кінець стрічки
                                if (endQuote2 != -1)
                                {
                                    // записуємо це в стрічку результату
                                    translation += phrases[i].Substring(startQuote2 + 1, endQuote2 - startQuote2 - 1);
                                }
                            }
                        }

                    }

                    // Корегуємо можливі помилки в відображенні тексту
                    translation = translation.Trim();
                    translation = translation.Replace(" ?", "?");
                    translation = translation.Replace(" !", "!");
                    translation = translation.Replace(" ,", ",");
                    translation = translation.Replace(" .", ".");
                    translation = translation.Replace(" ;", ";");
                }
            }
            catch (Exception ex)
            {
                translation = "Sorry, to much words :-(";
                this.Error = ex;
                return translation;
            }

            // Повертаємо результат
            return translation;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Конвертування мова-ідентифікатор (наприклад Ukrainian -> ua)
        /// </summary>
        /// <param name="language">Иова."</param>
        /// <returns>Ідентифікатор <see cref="string.Empty"/> якщо нема співпадынь</returns>
        private static string LanguageEnumToIdentifier
            (string language)
        {
            string mode = string.Empty;
            IOS_Translator.EnsureInitialized();
            IOS_Translator._languageModeMap.TryGetValue(language, out mode);
            return mode;
        }

        /// <summary>
        /// Перевірка ініціалізації 
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
        /// мода для пересування по словнику
        /// </summary>
        private static Dictionary<string, string> _languageModeMap;

        #endregion
    }
}

