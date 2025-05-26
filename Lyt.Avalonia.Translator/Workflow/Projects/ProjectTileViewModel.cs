namespace Lyt.Avalonia.Translator.Workflow.Projects;

using static ToolbarCommandMessage;
using static MessagingExtensions;

public sealed partial class ProjectTileViewModel : ViewModel<ProjectTileView>
{
    private readonly Project project;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string opened;

    [ObservableProperty]
    private string description;

    public ProjectTileViewModel(Project project)
    {
        this.project = project;
        this.Name = project.Name;
        this.Opened =
            string.Format(
                "Created: {0} {1} - Updated: {2} {3}", 
                project.Created.ToShortDateString(), project.Created.ToShortTimeString(),
                project.LastUpdated.ToShortDateString(), project.LastUpdated.ToShortTimeString()); 
        this.Description = 
            string.Format ( 
                "Source file: {0} - {1} - {2} - {3} target languages.", 
                this.project.SourceFile, 
                this.project.SourceLanguageCultureKey, 
                this.project.Format.ToFriendlyName(), 
                this.project.TargetLanguagesCultureKeys.Count);
    }

    [RelayCommand]
    public void OnOpen() => Command(ToolbarCommand.RunProject, this.project);

    [RelayCommand]
    public void OnDelete() => Command(ToolbarCommand.DeleteProject, this.project);
}
