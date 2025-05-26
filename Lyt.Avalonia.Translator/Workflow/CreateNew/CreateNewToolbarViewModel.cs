namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

using static MessagingExtensions;
using static ToolbarCommandMessage;

public sealed partial class CreateNewToolbarViewModel : ViewModel<CreateNewToolbarView>
{
#pragma warning disable IDE0079
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnAddAllLanguages() => Command(ToolbarCommand.CreateNewAddAllLanguages);

    [RelayCommand]
    public void OnClearAllLanguages() => Command(ToolbarCommand.CreateNewClearAllLanguages);

    [RelayCommand]
    public void OnSaveProject() => Command(ToolbarCommand.CreateNewSaveProject);

#pragma warning restore CA1822
#pragma warning restore IDE0079
}
