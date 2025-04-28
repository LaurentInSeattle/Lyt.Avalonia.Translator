namespace Lyt.Avalonia.Translator.Service;

public class TranslatorService
{
    private readonly ILogger logger;
    private readonly GoogleTranslate googleTranslate;

    public TranslatorService(ILogger logger)
    {
        this.logger = logger;
        this.googleTranslate = new GoogleTranslate();
    }

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
            throw;
        }
    }
}
