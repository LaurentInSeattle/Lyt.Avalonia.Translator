namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static ApplicationMessagingExtensions;
using static ToolbarCommandMessage;

public sealed partial class RunProjectToolbarViewModel : ViewModel<RunProjectToolbarView>
{
    public RunProjectToolbarViewModel() => this.IsRunning = false;

    [ObservableProperty]
    private bool isRunning; 

#pragma warning disable IDE0079 
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnStart() => Command(ToolbarCommand.StartProject);

    [RelayCommand]
    public void OnStop() => Command(ToolbarCommand.StopProject);

#pragma warning restore CA1822
#pragma warning restore IDE0079
}
