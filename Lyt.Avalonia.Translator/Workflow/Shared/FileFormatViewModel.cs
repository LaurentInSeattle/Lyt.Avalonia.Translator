namespace Lyt.Avalonia.Translator.Workflow.Shared;

public sealed class FileFormatViewModel : Bindable<FileFormatView>
{
    private readonly ResourceFormat resourceFormat; 

    public FileFormatViewModel(ResourceFormat resourceFormat)
    {
        this.resourceFormat = resourceFormat;
        this.Name = this.resourceFormat.ToFriendlyName();
    }

    public ResourceFormat ResourceFormat => this.resourceFormat;

    public string Name { get => this.Get<string>()!; set => this.Set(value); }
}
