namespace Lyt.Avalonia.Translator.Model.Messaging; 

public sealed record class BeginSourceLanguageMessage(
    string CultureKey, string EnglishName, string LocalName);
