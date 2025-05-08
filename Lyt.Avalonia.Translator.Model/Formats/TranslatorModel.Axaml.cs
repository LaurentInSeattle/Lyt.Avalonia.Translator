
namespace Lyt.Avalonia.Translator.Model;

public sealed partial class TranslatorModel : ModelBase
{
    private const string ResourceDictionaryHeader =
@"
<ResourceDictionary 
    xmlns=""https://github.com/avaloniaui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:system=""clr-namespace:System;assembly=System.Runtime""
    >
";

    private const string ResourceDictionaryFooter =
@"
</ResourceDictionary>
";

    private const string ResourceDictionaryEntryFormat =
@"
\t<system:String x:Key=""{0}"">{1}</system:String>
";

    private static List<string> ResourceDictionaryEntryTokens =
        [
            "<system:String x:Key=\"",
            "\">",
            "</system:String>"
        ];

    private static int ResourceDictionaryMinimumLength = 
        ResourceDictionaryHeader.Length + 
        ResourceDictionaryFooter.Length + 
        ResourceDictionaryEntryFormat.Length;

    public async Task<bool> TranslateAxamlResourceFile(
        ProviderKey provider,
        string sourcePath,
        string sourceLanguageKey, string destinationLanguageKey)
    {
        FileInfo fileInfo = new(sourcePath);
        if (!fileInfo.Exists || (fileInfo.Length < ResourceDictionaryMinimumLength))
        {
            return false;
        }

        Tuple<bool, Dictionary<string, string>> parseResult = 
            TranslatorModel.ParseAxamlResourceFile(sourcePath);
        if (!parseResult.Item1)
        {
            return false;
        }

        Dictionary<string, string> sourceDictionary = parseResult.Item2;
        var result = await this.translatorService.BatchTranslate(
             ProviderKey.Google,
             sourceDictionary,
             sourceLanguageKey, destinationLanguageKey,
             throttleDelayMillisecs: 2_000);
        if (result is null || !result.Item1)
        {
            return false;
        }

        var translatedDictionary = result.Item2;
        string extension = fileInfo.Extension;
        string destinationPath = sourcePath[..^extension.Length];
        destinationPath = string.Concat(destinationPath, "_", destinationLanguageKey, extension);
        TranslatorModel.CreateAxamlResourceFile(destinationPath, translatedDictionary); 
        return true;
    }

    public static Tuple<bool, Dictionary<string, string>> ParseAxamlResourceFile(string sourcePath)
    {
        Dictionary<string, string> dictionary = [];
        string lineStartsWith = ResourceDictionaryEntryTokens[0];
        string lineSpliter = ResourceDictionaryEntryTokens[1];
        string lineEndsWith = ResourceDictionaryEntryTokens[2];
        string[] lines = File.ReadAllLines(sourcePath);
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if ((trimmedLine.Length == 0) ||
                (!trimmedLine.StartsWith(lineStartsWith)) ||
                (!trimmedLine.EndsWith(lineEndsWith)))
            {
                continue;
            }

            trimmedLine = trimmedLine.Replace(lineStartsWith, string.Empty);
            trimmedLine = trimmedLine.Replace(lineEndsWith, string.Empty);
            string[] tokens =
                trimmedLine.Split(lineSpliter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tokens.Length != 2)
            {
                continue;
            }

            string key = tokens[0];
            string value = tokens[1];
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            dictionary.Add(key, value);
        }

        return Tuple.Create(true, dictionary);
    }

    public static bool CreateAxamlResourceFile(string destinationPath, Dictionary<string, string> dictionary)
    {
        try
        {
            StringBuilder stringBuilder = new();
            foreach (var item in dictionary)
            {
                string key = item.Key;
                string value = item.Value;
                string line = string.Format(ResourceDictionaryEntryFormat, key, value);
                stringBuilder.Append(line);
            }

            File.WriteAllText(destinationPath, stringBuilder.ToString());

            return true;
        }
        catch
        {
            return false;
        } 
    }
}
