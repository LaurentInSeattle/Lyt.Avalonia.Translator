namespace Lyt.Avalonia.Translator.Model.DataObjects;

public sealed record class Provider
(
    ProviderKey Key = ProviderKey.Unknown, 
    string Name = "" )
{
    public bool IsSelected { get; set; } = true ;

    public bool IsLoaded { get; set; }  
}
