namespace Lyt.Avalonia.Translator.Model.DataObjects;

public sealed class Project
{
    public Project() { /* Required for serialization */ }

    [JsonRequired]
    public required string Name { get; set; } = string.Empty;

    [JsonRequired]
    public required ResourceFormat Format { get; set; } = ResourceFormat.Unknown;

    [JsonRequired]
    public required string FolderPath { get; set; } = string.Empty;

    [JsonRequired]
    public required string SourceFile { get; set; } = string.Empty;

    [JsonRequired]
    public required string TargetFileFormat { get; set; } = string.Empty;

    [JsonRequired]
    public Language SourceLanguage { get; set; } = Language.Default;

    [JsonRequired]
    public List<Language> TargetLanguages { get; set; } = [];

    public bool IsInvalid
        =>
            this.Format == ResourceFormat.Unknown ||
            string.IsNullOrWhiteSpace(this.Name) ||
            string.IsNullOrWhiteSpace(this.FolderPath) ||
            string.IsNullOrWhiteSpace(this.SourceFile) ||
            string.IsNullOrWhiteSpace(this.TargetFileFormat) ||
            this.TargetLanguages.Count == 0;

    public string TargetFilePath(string cultureKey)
    {
        string fileName = string.Format(this.TargetFileFormat, cultureKey);
        return
            Path.Combine(this.FolderPath, string.Concat(fileName, this.Format.ToFileExtension()));
    }
}
