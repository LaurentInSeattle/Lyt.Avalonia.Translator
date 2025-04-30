namespace Lyt.Avalonia.Translator.Model.DataObjects; 

public sealed class ResourceFile
{
    public ResourceFile() { /* Required for serialization */ }

    [JsonRequired]
    public required string Path { get; set; }

    [JsonRequired]
    public required string CultureKey { get; set; }

    [JsonRequired]
    ResourceFormat Format { get; set; }

    /// <summary> True if this file is used as the source language for translations. </summary>
    [JsonRequired]
    public bool IsSource { get; set; }
}
