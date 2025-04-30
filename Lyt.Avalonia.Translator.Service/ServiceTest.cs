namespace Lyt.Avalonia.Translator.Service
{
    internal class ServiceTest(ILogger logger)
    {
        private readonly TranslatorService translatorService = new(logger);

        public async void Test()
        {
            var result = await this.translatorService.Translate(
                ProviderKey.Google, "Hello! Have you been able to complete the translation?", "en", "es");
            if (result is not null && result.Item1)
            {
                Debug.WriteLine(result.Item2);
            }

            Dictionary<string, string> dictionary = new ()
            {
                { "key1" , "Hello! Have you been able to complete the translation?"},
                { "key2" , "Not yet! But it will be finished tonight."},
            };

            var results = await this.translatorService.BatchTranslate(ProviderKey.Google, dictionary, "en", "it");
            if (results is not null && results.Item1 && results.Item2.Count > 0)
            {
                foreach (var item in results.Item2)
                {
                    Debug.WriteLine(item.Key + ":  " + item.Value);
                }
            }

            //string sourcePath =
            //    @"C:\Users\Laurent\source\repos\Lyt.Avalonia.Translator\Lang_en-US - Copy.axaml";
            //var fileResults = await this.translatorModel.TranslateAxamlResourceFile(
            //    ProviderKey.Google, sourcePath, "en", "fr");

        }

    }
}
