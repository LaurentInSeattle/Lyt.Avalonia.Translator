namespace Lyt.Avalonia.Translator.Model.DataObjects; 

public sealed class Project
{
    public Project() { /* Required for serialization */ }

    [JsonRequired]
    public required string Name { get; set; }

    [JsonRequired]
    public List<ResourceFile> ResourcesFiles { get; set; } = [];

    public bool IsEmpty => this.ResourcesFiles.Count == 0;
}
