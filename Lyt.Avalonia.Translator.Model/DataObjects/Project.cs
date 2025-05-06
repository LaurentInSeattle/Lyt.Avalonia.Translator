namespace Lyt.Avalonia.Translator.Model.DataObjects;

public sealed class Project
{
    public Project() { /* Required for serialization */ }

    [JsonRequired]
    public required string Name { get; set; } = string.Empty;

    [JsonRequired]
    public required DateTime Created { get; set; } = DateTime.Now;

    [JsonRequired]
    public required DateTime LastUpdated { get; set; } = DateTime.Now;

    [JsonRequired]
    public required ResourceFormat Format { get; set; } = ResourceFormat.Unknown;

    [JsonRequired]
    public required string FolderPath { get; set; } = string.Empty;

    [JsonRequired]
    public required string SourceFile { get; set; } = string.Empty;

    [JsonRequired]
    public required string TargetFileFormat { get; set; } = string.Empty;

    [JsonRequired]
    public string SourceLanguageCultureKey { get; set; } = string.Empty;

    [JsonRequired]
    public List<string> TargetLanguagesCultureKeys { get; set; } = [];

    public bool IsInvalid
        =>
            this.Format == ResourceFormat.Unknown ||
            string.IsNullOrWhiteSpace(this.Name) ||
            string.IsNullOrWhiteSpace(this.FolderPath) ||
            string.IsNullOrWhiteSpace(this.SourceFile) ||
            string.IsNullOrWhiteSpace(this.SourceLanguageCultureKey) ||
            string.IsNullOrWhiteSpace(this.TargetFileFormat) ||
            this.TargetLanguagesCultureKeys.Count == 0;

    public string TargetFilePath(string cultureKey)
    {
        string fileName = string.Format(this.TargetFileFormat, cultureKey);
        return
            Path.Combine(this.FolderPath, string.Concat(fileName, this.Format.ToFileExtension()));
    }

    public bool Validate(out string errorMessageKey)
    {
        errorMessageKey = string.Empty;
        if (this.Format == ResourceFormat.Unknown)
        {
            errorMessageKey = "Model.Project.Format";
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.Name))
        {
            errorMessageKey = "Model.Project.Name";
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.FolderPath))
        {
            errorMessageKey = "Model.Project.FolderPath";
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.SourceFile))
        {
            errorMessageKey = "Model.Project.SourceFile";
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.SourceLanguageCultureKey))
        {
            errorMessageKey = "Model.Project.SourceFile";
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.TargetFileFormat))
        {
            errorMessageKey = "Model.Project.TargetFileFormat";
            return false;
        }

        if (this.TargetLanguagesCultureKeys.Count == 0)
        {
            errorMessageKey = "Model.Project.ZeroTargetLanguages";
            return false;
        }

        DirectoryInfo directoryInfo = new(this.FolderPath);
        if (!directoryInfo.Exists)
        {
            errorMessageKey = "Model.Project.FolderPath";
        }

        FileInfo fileInfo = new(Path.Combine(this.FolderPath, this.SourceFile));
        if (!fileInfo.Exists)
        {
            errorMessageKey = "Model.Project.SourceFile";
        }

        // CONSIDER ~ LATER
        // Try to write a dummy file in the target directory to ensure write access is granted
        return true;
    }
}
