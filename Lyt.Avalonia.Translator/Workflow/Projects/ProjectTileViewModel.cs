namespace Lyt.Avalonia.Translator.Workflow.Projects;

using static ToolbarCommandMessage;
using static ViewActivationMessage;

public sealed class ProjectTileViewModel : Bindable<ProjectTileView>
{
    private readonly Project project; 

    public ProjectTileViewModel(Project project)
    {
        this.project = project;
        this.Name = project.Name;
    }

    #region Methods invoked by the Framework using reflection 
#pragma warning disable IDE0051 // Remove unused private members

    private void OnOpen(object? _) { } 
        //=> ActivateView(
        //    ActivatedView.RunProject,
        //    new ComputerActivationParameter(
        //        ComputerActivationParameter.Kind.Document, string.Empty, this.QuComputer));

    private void OnDelete(object? _)
    { } //    => Command(ToolbarCommand.DeleteDocument, this);

    #endregion Methods invoked by the Framework using reflection 
#pragma warning restore IDE0051 // Remove unused private members

    public ICommand OpenCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand DeleteCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public string Name { get => this.Get<string>()!; set => this.Set(value); }

    public string Opened { get => this.Get<string>()!; set => this.Set(value); }

    public string Description { get => this.Get<string>()!; set => this.Set(value); }
}
