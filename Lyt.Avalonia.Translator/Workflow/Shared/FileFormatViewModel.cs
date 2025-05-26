namespace Lyt.Avalonia.Translator.Workflow.Shared;

public sealed partial class FileFormatViewModel : ViewModel<FileFormatView>
{
    private readonly ResourceFormat resourceFormat;

    [ObservableProperty]
    private string name;

    public FileFormatViewModel(ResourceFormat resourceFormat)
    {
        this.resourceFormat = resourceFormat;
        this.Name = this.resourceFormat.ToFriendlyName();
    }

    public ResourceFormat ResourceFormat => this.resourceFormat;

}
