namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static MessagingExtensions;
using static ToolbarCommandMessage;

public sealed class RunProjectToolbarViewModel : Bindable<RunProjectToolbarView>
{
#pragma warning disable IDE0079 
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    private void OnRun(object? _) => Command(ToolbarCommand.RunProject);

    private void OnStop(object? _) => Command(ToolbarCommand.StopProject);

#pragma warning restore CA1822
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079

    public ICommand RunCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand StopCommand { get => this.Get<ICommand>()!; set => this.Set(value); }
}
