// МКР : модуль "Перекладач" Android
// розробник: Кушнір Дмитро (c) 2018
// Призначення: переклад тексту отриманого з зображення та повернення його у вигляді стрічки
using System;
using System.Net;
using ComputerVisionSample.Translator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

[assembly: Dependency(typeof(ComputerVisionSample.Droid.Android_Translator))]
namespace ComputerVisionSample.Droid
{
    public class Android_Translator : PCL_Translator
    {
        private static readonly string host = "https://api.cognitive.microsofttranslator.com";
        private static readonly string path = "/translate?api-version=3.0";
        private static readonly string params_ = "&from={0}&to={1}";
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
        public string Translate(string sourceText, string sourceLanguage, string targetLanguage, string key)
        {
            this.Error = null;
            try
            {
                System.Object[] body = new System.Object[] { new { Text = sourceText } };
                var requestBody = JsonConvert.SerializeObject(body);
                string filledParams = string.Format(params_, sourceLanguage, targetLanguage);
                string uri = host + path + filledParams;
                string response = "";
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Ocp-Apim-Subscription-Key", key);
                    wc.Headers.Add("Accept", "application/json");
                    wc.Headers.Add("Content-Type", "application/json");
                    response = wc.UploadString(uri, "POST", requestBody);
                }
                return RecursivelyParseRspString(response);
            }
            catch (Exception ex)
            {
                this.Error = ex;
                return null;
            }
        }
        public string RecursivelyParseRspString(string json)
        {
            JArray a = JArray.Parse(json);
            foreach (JObject o in a.Children<JObject>())
            {
                foreach (JProperty p in o.Properties())
                {
                    var name = p.Name;
                    var value = p.Value;
                    return value.HasValues ? RecursivelyParseRspString(value.ToString()) : value.ToString();
                }
            }
            throw Error;
        }
        #endregion
    }
}

