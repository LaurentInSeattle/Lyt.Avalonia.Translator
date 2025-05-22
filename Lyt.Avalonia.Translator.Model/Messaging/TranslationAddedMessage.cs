namespace Lyt.Avalonia.Translator.Model.Messaging;

public sealed record class TranslationAddedMessage(
    string SourceLanguageKey, string TargetLanguageKey,
    string SourceText, string TargetText);
