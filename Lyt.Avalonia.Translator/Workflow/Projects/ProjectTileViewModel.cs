namespace Lyt.Avalonia.Translator.Workflow.Projects;

using static ToolbarCommandMessage;
using static MessagingExtensions;

public sealed class ProjectTileViewModel : Bindable<ProjectTileView>
{
    private readonly Project project; 

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

    #region Methods invoked by the Framework using reflection 
#pragma warning disable IDE0051 // Remove unused private members

    private void OnOpen(object? _) => Command(ToolbarCommand.RunProject, this.project);

    private void OnDelete(object? _) => Command(ToolbarCommand.DeleteProject, this.project);

#pragma warning restore IDE0051 // Remove unused private members
    #endregion Methods invoked by the Framework using reflection 

    public ICommand OpenCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand DeleteCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public string Name { get => this.Get<string>()!; set => this.Set(value); }

    public string Opened { get => this.Get<string>()!; set => this.Set(value); }

    public string Description { get => this.Get<string>()!; set => this.Set(value); }
}
