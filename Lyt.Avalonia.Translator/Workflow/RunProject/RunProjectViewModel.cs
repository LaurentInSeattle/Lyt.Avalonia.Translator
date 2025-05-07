namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static ToolbarCommandMessage;

public sealed class RunProjectViewModel : Bindable<RunProjectView>
{
    private readonly TranslatorModel translatorModel;
    private readonly IToaster toaster;

    public RunProjectViewModel(TranslatorModel translatorModel, IToaster toaster)
    {
        this.translatorModel = translatorModel;
        this.toaster = toaster;

        this.SelectedLanguages = [];
        this.Messenger.Subscribe<ToolbarCommandMessage>(this.OnToolbarCommand);
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

    private void OnToolbarCommand(ToolbarCommandMessage message)
    {
        switch (message.Command)
        {
            case ToolbarCommand.StartProject:
                this.StartProject();
                break;

            case ToolbarCommand.StopProject:
                this.StopProject();
                break;

            // Ignore all other commands 
            default:
                break;
        }
    }

    private void Populate() 
    {
        var currentProject = this.translatorModel.ActiveProject;
        if ((currentProject is null) || currentProject.IsInvalid) 
        {
            // Error: no active project 
            this.ProjectName = string.Empty;
            this.ProjectDetails = string.Empty;
            this.TranslationStatus = string.Empty; 
            this.ErrorMessage = this.Localizer.Lookup("RunProject.NoActiveProject");
            return; 
        }

        this.TranslationStatus = this.Localizer.Lookup("RunProject.Idle");
        this.ErrorMessage = string.Empty;
        this.ProjectName = currentProject.Name;
        this.ProjectDetails =
            string.Format(
                "Source file: {0} - Last updated: {1} {2}",
                currentProject.SourceFile,
                currentProject.LastUpdated.ToShortDateString(),
                currentProject.LastUpdated.ToShortTimeString());
        this.SourceLanguage = 
            new LanguageInfoViewModel(Language.Languages[currentProject.SourceLanguageCultureKey]);
        this.SelectedLanguages = [];
    }

    private void StartProject() { }

    private void StopProject() { }

    public string? ErrorMessage { get => this.Get<string?>(); set => this.Set(value); }

    public string? ProjectName { get => this.Get<string?>(); set => this.Set(value); }

    public string? ProjectDetails { get => this.Get<string?>(); set => this.Set(value); }

    public string? TranslationStatus { get => this.Get<string?>(); set => this.Set(value); }

    public ObservableCollection<ClickableLanguageInfoViewModel> SelectedLanguages
    {
        get => this.Get<ObservableCollection<ClickableLanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

    public LanguageInfoViewModel? SourceLanguage
    {
        get => this.Get<LanguageInfoViewModel?>();
        set => this.Set(value);
    }

}
