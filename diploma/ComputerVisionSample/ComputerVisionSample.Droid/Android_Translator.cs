// МКР : модуль "Перекладач" Android
// розробник: Кушнір Дмитро (c) 2018
// Призначення: переклад тексту отриманого з зображення та повернення його у вигляді стрічки

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using ComputerVisionSample.Translator;
using Xamarin.Forms;
[assembly: Dependency(typeof(ComputerVisionSample.Droid.Android_Translator))]

namespace ComputerVisionSample.Droid
{
    public class Android_Translator : PCL_Translator
    {
        private const string googleUri = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}";
        /// <summary>
        /// Помилки.
        /// </summary>
        public Exception Error
        {
            get;
            private set;
        }
        #region Public methods
        /// <summary>
        /// Трансюємо необідний текст
        /// </summary>
        /// <param name="sourceText">Вхідний текст.</param>
        /// <param name="sourceLanguage">Вхідна мова</param>
        /// <param name="targetLanguage">Мова призначення</param>
        /// <returns>Переклад</returns>
        public  string Translate(string sourceText, string sourceLanguage, string targetLanguage)
        {
            // Ініціалізація
            this.Error = null;
            DateTime tmStart = DateTime.Now;
            string translation = string.Empty;
            try
            {
                // Завантажити переклад
                string url = string.Format(googleUri, sourceLanguage, targetLanguage, HttpUtility.UrlEncode(sourceText));
                string outputFile = Path.GetTempFileName();
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                    wc.DownloadFile(url, outputFile);
                }
                // Отримуємо мову тексту та парсимо її в потрібний формат
                if (File.Exists(outputFile))
                {
                    // Отримуємо колекцію фраз
                    string text = File.ReadAllText(outputFile);
                    int index = text.IndexOf(string.Format(",,\"{0}\"", sourceLanguage));
                    // ділимо отриманйи текст з севреру на стрічки таким чином : непарні - вхідний текст , парні - трансльований текст
                    string[] phrases = text.Split(new[] { "\"," }, StringSplitOptions.RemoveEmptyEntries); 
                    for (int i = 0; i < phrases.Count(); i+=2)
                    {
                        if (i == phrases.Count() -1 ) break;
                        int startQuote2 = phrases[i].IndexOf('\"'); // початок стрічки
                        if (startQuote2 != -1)
                        {
                            int endQuote2 = phrases[i].Length ; // кінець стрічки
                            if (endQuote2 != -1)
                            {
                                // записуємо це в стрічку результату
                                translation += phrases[i].Substring(startQuote2 + 1, endQuote2 - startQuote2 - 1); 
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
                this.Error = ex;
                return null;
            }
            return translation;
        }
    }
}
#endregion
