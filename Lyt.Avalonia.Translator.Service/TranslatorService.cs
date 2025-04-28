namespace Lyt.Avalonia.Translator.Service;

public class TranslatorService(ILogger logger, IRandomizer randomizer)
{
    private readonly ILogger logger = logger;
    private readonly IRandomizer randomizer = randomizer;

    public async Task<List<string>> Translate(ProviderKey provider)
    {
        try
        {
            switch (provider)
            {
                case ProviderKey.Google:
                    await Task.Delay(100); 
                    return []; //  await GoogleService.Translate(....);

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
