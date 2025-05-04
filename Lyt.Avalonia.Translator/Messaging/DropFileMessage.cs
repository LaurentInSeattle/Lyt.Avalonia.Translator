namespace Lyt.Avalonia.Translator.Messaging; 

public sealed record class DropFileMessage (bool Success, string Data);
