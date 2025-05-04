namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

using static MessagingExtensions;
using static ToolbarCommandMessage;

public sealed class CreateNewToolbarViewModel : Bindable<CreateNewToolbarView>
{
#pragma warning disable IDE0079 
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    private void OnAddAllLanguages(object? _) => Command(ToolbarCommand.CreateNewAddAllLanguages);
    private void OnClearAllLanguages(object? _) => Command(ToolbarCommand.CreateNewClearAllLanguages);
    private void OnSaveProject(object? _) => Command(ToolbarCommand.CreateNewSaveProject);

#pragma warning restore CA1822
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079

    public ICommand AddAllLanguagesCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand ClearAllLanguagesCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand SaveProjectCommand { get => this.Get<ICommand>()!; set => this.Set(value); }
}
