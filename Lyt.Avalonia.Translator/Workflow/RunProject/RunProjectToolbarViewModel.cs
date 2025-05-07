namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static MessagingExtensions;
using static ToolbarCommandMessage;

public sealed class RunProjectToolbarViewModel : Bindable<RunProjectToolbarView>
{
    public RunProjectToolbarViewModel() => this.IsRunning = false;

#pragma warning disable IDE0079 
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    private void OnStart(object? _) => Command(ToolbarCommand.StartProject);

    private void OnStop(object? _) => Command(ToolbarCommand.StopProject);

#pragma warning restore CA1822
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079

    public ICommand StartCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand StopCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public bool IsRunning { get => this.Get<bool>(); set => this.Set(value); }
}
