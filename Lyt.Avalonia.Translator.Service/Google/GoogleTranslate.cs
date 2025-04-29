namespace Lyt.Avalonia.Translator.Service.Google;

internal class GoogleTranslate
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:91.0) Gecko/20100101 Firefox/91.0";
    private const string GoogleTranslatorUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&hl=en&dt=t&dt=bd&dj=1&source=icon&tk=467103.467103&q={2}";

    private readonly Dictionary<string, string> Languages =
        new()
        {
                {"auto", "(Detect)"},
                {"af", "Afrikaans"},
                {"sq", "Albanian"},
                {"ar", "Arabic"},
                {"hy", "Armenian"},
                {"az", "Azerbaijani"},
                {"eu", "Basque"},
                {"be", "Belarusian"},
                {"bn", "Bengali"},
                {"bg", "Bulgarian"},
                {"ca", "Catalan"},
                {"zh-CN", "Chinese (Simplified)"},
                {"zh-TW", "Chinese (Traditional)"},
                {"hr", "Croatian"},
                {"cs", "Czech"},
                {"da", "Danish"},
                {"nl", "Dutch"},
                {"en", "English"},
                {"eo", "Esperanto"},
                {"et", "Estonian"},
                {"tl", "Filipino"},
                {"fi", "Finnish"},
                {"fr", "French"},
                {"gl", "Galician"},
                {"ka", "Georgian"},
                {"de", "German"},
                {"el", "Greek"},
                {"gu", "Gujarati"},
                {"ht", "Haitian Creole"},
                {"iw", "Hebrew"},
                {"hi", "Hindi"},
                {"hu", "Hungarian"},
                {"is", "Icelandic"},
                {"id", "Indonesian"},
                {"ga", "Irish"},
                {"it", "Italian"},
                {"ja", "Japanese"},
                {"kn", "Kannada"},
                {"km", "Khmer"},
                {"ko", "Korean"},
                {"lo", "Lao"},
                {"la", "Latin"},
                {"lv", "Latvian"},
                {"lt", "Lithuanian"},
                {"mk", "Macedonian"},
                {"ms", "Malay"},
                {"mt", "Maltese"},
                {"no", "Norwegian"},
                {"fa", "Persian"},
                {"pl", "Polish"},
                {"pt-PT", "Portuguese - Portugal"},
                {"pt-BR", "Portuguese - Brazil"},
                {"ro", "Romanian"},
                {"ru", "Russian"},
                {"sr", "Serbian"},
                {"sk", "Slovak"},
                {"sl", "Slovenian"},
                {"es", "Spanish"},
                {"sw", "Swahili"},
                {"sv", "Swedish"},
                {"ta", "Tamil"},
                {"te", "Telugu"},
                {"th", "Thai"},
                {"tr", "Turkish"},
                {"uk", "Ukrainian"},
                {"ur", "Urdu"},
                {"vi", "Vietnamese"},
                {"cy", "Welsh"},
                {"yi", "Yiddish"}
        };

    private readonly HttpClient client;

    public GoogleTranslate()
    {
        this.client = new();
        this.client.DefaultRequestHeaders.Add("UserAgent", UserAgent);
        this.client.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<Tuple<bool, string>> Translate(
        string sourceText, string sourceLanguageKey, string destinationLanguageKey)
    {
        try
        {
            sourceText = HttpUtility.UrlEncode(sourceText);
            string url = string.Format(GoogleTranslatorUrl, sourceLanguageKey, destinationLanguageKey, sourceText);
            using var response = await this.client.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string jsonText = reader.ReadToEnd();
            var responseObject = JsonSerializer.Deserialize<TranslationResponse>(jsonText);
            if (responseObject is TranslationResponse translationResponse)
            {
                var sentences = translationResponse.Sentences;
                if (sentences is not null && sentences.Count > 0)
                {
                    string translation = string.Empty;
                    foreach (Sentence sentence in sentences)
                    {
                        string? maybeTranslation = sentence.Translation;
                        if (!string.IsNullOrWhiteSpace(maybeTranslation))
                        {
                            translation = string.Concat(translation, " ", maybeTranslation);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(translation))
                    {
                        Debug.WriteLine(sourceText + " :  " + translation.Trim());
                        return new Tuple<bool, string>(true, translation.Trim());
                    }
                }
            }

            throw new Exception("Deserialization Error");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, ex.Message);
        }
    }
}
