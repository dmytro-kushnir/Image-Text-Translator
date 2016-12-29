namespace ComputerVisionSample.Translator
{

    public interface PCL_Translator
    {
        string Translate(string sourceText, string sourceLanguage, string targetLanguage);
    }
}