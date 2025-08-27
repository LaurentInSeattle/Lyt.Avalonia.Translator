namespace Lyt.Avalonia.Translator.Workflow.Projects;

using static ApplicationMessagingExtensions;
using static ToolbarCommandMessage;

public sealed partial class ProjectsViewModel : ViewModel<ProjectsView>, IRecipient<ToolbarCommandMessage>
{
    private readonly TranslatorModel translatorModel;
    private readonly IToaster toaster;

    [ObservableProperty]
    public string? noData;

    [ObservableProperty]
    private ObservableCollection<ProjectTileViewModel> projectTileViews;

    public ProjectsViewModel(TranslatorModel translatorModel, IToaster toaster)
    {
        this.translatorModel = translatorModel;
        this.toaster = toaster;
        this.ProjectTileViews = [];
        this.Subscribe<ToolbarCommandMessage>();
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);
        this.Populate();
    }

    public void Receive(ToolbarCommandMessage message)
    {
        if (message.CommandParameter is not Project project)
        {
            if ((message.Command == ToolbarCommand.RunProject) ||
                (message.Command == ToolbarCommand.DeleteProject))
            {
                throw new ArgumentException(null, nameof(message));
            }

            // Ignore all other commands 
            return;
        }

        switch (message.Command)
        {
            case ToolbarCommand.RunProject:
                this.RunProject(project);
                break;

            case ToolbarCommand.DeleteProject:
                this.DeleteProject(project);
                break;

            // Ignore all other commands 
            default:
                break;
        }
    }

    private void RunProject(Project project)
    {
        if (!this.translatorModel.CheckProjectExistence(project, out string errorMessageKey))
        {
            this.toaster.Show(
                this.Localizer.Lookup("Shell.Error"),
                this.Localizer.Lookup(errorMessageKey),
                3_000, InformationLevel.Error);
            return;
        }

        // Make it the active project and then navigate to RunProject 
        this.translatorModel.ActiveProject = project;   
        NavigateTo(ViewActivationMessage.ActivatedView.RunProject); 
    }

    private void DeleteProject(Project project)
    {
        if (!this.translatorModel.DeleteProject(project.Name, out string errorMessageKey))
        {
            this.toaster.Show(
                this.Localizer.Lookup("Shell.Error"),
                this.Localizer.Lookup(errorMessageKey),
                3_000, InformationLevel.Error);
            return;
        }

        this.Populate();
        this.toaster.Show(
            this.Localizer.Lookup("Shell.Success"),
            this.Localizer.Lookup("Projects.ProjectDeleted"),
            3_000, InformationLevel.Success);
    }

    private void Populate()
    {
        List<ProjectTileViewModel> tiles = [];
        var projects = this.translatorModel.Projects;
        if (projects.Count > 0)
        {
            this.NoData = string.Empty;
            var sortedProjects =
                from project in projects orderby project.Name select project;
            foreach (Project project in sortedProjects)
            {
                ProjectTileViewModel projectTileViewModel = new(project);
                tiles.Add(projectTileViewModel);
            }

            this.ProjectTileViews = [..tiles]; 
        }
        else
        {
            this.NoData = this.Localizer.Lookup("Projects.None");
        }
    }
}
