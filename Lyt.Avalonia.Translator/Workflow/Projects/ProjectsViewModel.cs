namespace Lyt.Avalonia.Translator.Workflow.Projects;

public sealed class ProjectsViewModel : Bindable<ProjectsView>
{
    private readonly TranslatorModel translatorModel;

    public ProjectsViewModel(TranslatorModel translatorModel)
    {
        this.translatorModel = translatorModel;
        this.ProjectTileViews = [];
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);
        this.Populate();
    }

    protected override void OnViewLoaded()
    {
        base.OnViewLoaded();
        this.Populate();
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

    public string? NoData { get => this.Get<string>(); set => this.Set(value); }

    public ObservableCollection<ProjectTileViewModel> ProjectTileViews
    {
        get => this.Get<ObservableCollection<ProjectTileViewModel>>()!;
        set => this.Set(value);
    }
}
