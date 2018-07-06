using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerVisionSample.helpers
{
    class Data
    {
        // INFO There is auto detect, but we need this to determne handwritten OCR
        static public string[] sourceLanguages = new String[]
        {
            "English",
            "Russian",
            "French",
            "German",
            "Italian",
            "Russian",
            "Spanish",
            "Arabic",
            "Chinese",
            "ChineseSimplified",
            "ChineseTraditional",
            "Polish",
            "Turkish",
            "Portuguese",
            "Greek",
            "Japanese",
            "Hungarian",
            "Finnish",
            "Swedish",
            "Norwegian",
            "Korean",
            "Czech",
            "Danish",
            "Dutch",
            "Slovak",
            "Romanian",
            "SerbianCyrillic",
            "SerbianLatin"
        };
        static public string[] destinationLanguages = new String[]
        {
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
