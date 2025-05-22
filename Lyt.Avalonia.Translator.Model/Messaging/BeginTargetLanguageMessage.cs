namespace Lyt.Avalonia.Translator.Model.Messaging; 

public sealed record class BeginTargetLanguageMessage(
    string CultureKey , string EnglishName, string LocalName ); 
