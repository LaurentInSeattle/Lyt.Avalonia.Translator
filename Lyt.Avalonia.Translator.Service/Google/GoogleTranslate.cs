using System.Text;

namespace Lyt.Avalonia.Translator.Service.Google;

internal class GoogleTranslate
{
    private const string RequestUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:91.0) Gecko/20100101 Firefox/91.0";
    private const string RequestGoogleTranslatorUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&hl=en&dt=t&dt=bd&dj=1&source=icon&tk=467103.467103&q={2}";

    private readonly Dictionary<string, string> Languages =
        new Dictionary<string, string>
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

    public static async Task<Tuple<bool, string>> Translate ( 
        string sourceText, string sourceLanguageKey, string destinationLanguageKey)
    {
        return new Tuple<bool, string>(false, string.Empty);
    }

    // TODO: Modernize to HTTP Client 
    // https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-migrate-from-httpwebrequest 

    private static WebRequest CreateWebRequest(
        string sourceText, string sourceLanguageKey, string destinationLanguageKey)
    {
        sourceText = HttpUtility.UrlEncode(sourceText);
        string url = string.Format(RequestGoogleTranslatorUrl, sourceLanguageKey, destinationLanguageKey, sourceText);
        var create = (HttpWebRequest)WebRequest.Create(url);
        create.UserAgent = RequestUserAgent;
        create.Timeout = 50 * 1000;
        return create;
    }

    public static bool Translate(
        string text,
        string sourceLng,
        string destLng,
        string textTranslatorUrlKey,
        out string result)
    {
        var request = CreateWebRequest(text, sourceLng, destLng);
        try
        {
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                result = "Response is failed with code: " + response.StatusCode;
                return false;
            }

            using (var stream = response.GetResponseStream())
            {
                var succeed = ReadGoogleTranslatedResult(stream, out var output);
                result = output;
                return succeed;
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            return false;
        }
    }

    public delegate void TranslateCallBack(bool succeed, string result);

    private static void TranslateRequestCallBack(IAsyncResult ar)
    {
        var pair = (KeyValuePair<WebRequest, TranslateCallBack>)ar.AsyncState;
        var request = pair.Key;
        var callback = pair.Value;
        HttpWebResponse response = null;
        try
        {
            response = (HttpWebResponse)request.EndGetResponse(ar);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                callback(false, "Response is failed with code: " + response.StatusCode);
                return;
            }

            using (var stream = response.GetResponseStream())
            {
                string output;
                var succeed = ReadGoogleTranslatedResult(stream, out output);

                callback(succeed, output);
            }
        }
        catch (Exception ex)
        {
            callback(false, "Request failed.\r\n" + ex.Message);
        }
        finally
        {
            response?.Close();
        }
    }

    /// <summary>
    ///  the main trick :)
    /// </summary>
    static bool ReadGoogleTranslatedResult(Stream rawdata, out string result)
    {
        result= string.Empty;
        string text;
        using (var reader = new StreamReader(rawdata, Encoding.UTF8))
        {
            text = reader.ReadToEnd();
        }

        try
        {
            //dynamic obj = SimpleJson.DeserializeObject(text);

            //var final = "";

            //// the number of lines
            //int lines = obj[0].Count;
            //for (int i = 0; i < lines; i++)
            //{
            //    // the translated text.
            //    final += (obj[0][i][0]).ToString();
            //}
            //result = final;
            return true;
        }
        catch (Exception ex)
        {
            result = ex.Message;
            return false;
        }
    }

}
