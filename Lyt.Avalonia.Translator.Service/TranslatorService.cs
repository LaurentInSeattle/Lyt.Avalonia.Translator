namespace Lyt.Avalonia.Translator.Service;

public class TranslatorService(ILogger logger)
{
    private readonly ILogger logger = logger;
    private readonly GoogleTranslate googleTranslate = new();

    public async Task<Tuple<bool, string>> Translate(
        ProviderKey provider, 
        string sourceText, string sourceLanguageKey, string destinationLanguageKey)
    {
        try
        {
            switch (provider)
            {
                case ProviderKey.Google:
                    await Task.Delay(100); 
                    return await this.googleTranslate.Translate(sourceText, sourceLanguageKey, destinationLanguageKey);

                default:
                    break;
            }

            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            string msg = "Exception thrown: " + ex.Message + "\n" + ex;
            this.logger.Error(msg);
            return Tuple.Create(false, msg);
        }
    }

    public async Task<Tuple<bool, Dictionary<string, string>>> BatchTranslate(
        ProviderKey provider, 
        Dictionary<string,string> sourceTexts, string sourceLanguageKey, string destinationLanguageKey,
        int throttleDelayMillisecs = 2_000)
    {
        bool success = true ;
        try
        {
            Dictionary<string, string> translatedTexts = new ((int)(sourceTexts.Count* 1.25));
            bool isFirst = true ;
            foreach (var item in sourceTexts)
            {
                if (!isFirst)
                {
                    // Do not delay on first and last service call 
                    if (throttleDelayMillisecs > 50)
                    {
                        await Task.Delay(throttleDelayMillisecs);
                    }
                }
                else
                {
                    isFirst = false ;
                } 

                string key = item.Key;
                string sourceText = item.Value;
                Tuple<bool, string> translation = await this.Translate(provider, sourceText, sourceLanguageKey, destinationLanguageKey);
                if (translation.Item1)
                {
                    translatedTexts.Add(key, translation.Item2);
                }
                else
                {
                    success = false;
                    break ;
                }
            }

            return Tuple.Create(success, translatedTexts);
        }
        catch (Exception ex)
        {
            string msg = "Exception thrown: " + ex.Message + "\n" + ex;
            this.logger.Error(msg);
            return Tuple.Create(success, new Dictionary<string, string>());
        }
    }
}
