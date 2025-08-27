namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static ToolbarCommandMessage;

public sealed partial class RunProjectViewModel : 
    ViewModel<RunProjectView>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<BeginSourceLanguageMessage>,
    IRecipient<BeginTargetLanguageMessage>,
    IRecipient<TranslationAddedMessage>,
    IRecipient<TranslationCompleteMessage>
{
    private readonly TranslatorModel translatorModel;
    private readonly RunProjectToolbarViewModel runProjectToolbarViewModel;
    private readonly IToaster toaster;

    private readonly Dictionary<string, ExtLanguageInfoViewModel> targetLanguageViewModels;
    private bool isPopulated;

    public RunProjectViewModel(
        TranslatorModel translatorModel,
        RunProjectToolbarViewModel runProjectToolbarViewModel,
        IToaster toaster)
    {
        this.translatorModel = translatorModel;
        this.runProjectToolbarViewModel = runProjectToolbarViewModel;
        this.toaster = toaster;
        this.SelectedLanguages = [];
        this.targetLanguageViewModels = [];

        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<BeginSourceLanguageMessage>();
        this.Subscribe<BeginTargetLanguageMessage>();
        this.Subscribe<TranslationAddedMessage>();
        this.Subscribe<TranslationCompleteMessage>();
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);
        Dispatch.OnUiThread(this.Populate);
    }

    public void Receive(BeginSourceLanguageMessage message)
    {
        Dispatch.OnUiThread(()=>
        {
            this.SourceLanguageLabel =
                string.Concat(message.EnglishName, "  ~  ", message.LocalName);
        });
    }

    public void Receive(BeginTargetLanguageMessage message)
    {
        Dispatch.OnUiThread(() =>
        {
            ExtLanguageInfoViewModel vm = this.targetLanguageViewModels[message.CultureKey];
            vm.InProgress();
            this.TargetLanguageLabel = string.Concat(message.EnglishName, "  ~  ", message.LocalName);
        });
    }

    public void Receive(TranslationAddedMessage message)
    {
        Dispatch.OnUiThread(() =>
        {
            // Update the right side of the view 
            this.SourceText = message.SourceText;
            this.SourceLanguageKey = message.TargetLanguageKey;
            this.TargetText = message.TargetText;

            // Update the counts and status in the View on the left.
            ExtLanguageInfoViewModel vm = this.targetLanguageViewModels[message.TargetLanguageKey];
            vm.TranslationAdded();
        });
    }

    public void Receive(TranslationCompleteMessage message)
        => Dispatch.OnUiThread(()=>
        {
            this.EndProject(message.Aborted);
        });

    public void Receive(ToolbarCommandMessage message)
    {
        var currentProject = this.translatorModel.ActiveProject;
        if ((currentProject is null) || currentProject.IsInvalid)
        {
            // Error: no active project 
            this.NoProject();
            return;
        }

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
            this.NoProject();
            return;
        }

        this.TranslationStatus = this.Localizer.Lookup("RunProject.Idle");
        this.ErrorMessage = string.Empty;
        this.ProjectName = currentProject.Name;
        this.ProjectDetails =
            string.Format(
                "Source: {0}  -  Updated: {1} {2}",
                currentProject.SourceFile,
                currentProject.LastUpdated.ToShortDateString(),
                currentProject.LastUpdated.ToShortTimeString());
        this.SourceLanguage =
            new LanguageInfoViewModel(Language.Languages[currentProject.SourceLanguageCultureKey]);
        this.FileFormat = new FileFormatViewModel(currentProject.Format);
        this.SelectedLanguages = [];
        this.IsInProgress = false;
        this.SourceLanguageLabel = string.Empty;
        this.SourceLanguageKey = string.Empty;
        this.TargetLanguageLabel = string.Empty;

        void PopulateTargetLanguages()
        {
            this.targetLanguageViewModels.Clear();

            // Loop through target languages 
            // Create a VM for each save in UI and in class data 
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                ExtLanguageInfoViewModel vm = new(Language.Languages[cultureKey]);
                int missingEntries = this.translatorModel.MissingEntriesCount(cultureKey);
                vm.SetComplete(missing: missingEntries);
                this.targetLanguageViewModels.Add(cultureKey, vm);
            }

            this.SelectedLanguages = [.. this.targetLanguageViewModels.Values];
        }

        try
        {
            if ( !this.translatorModel.PrepareForRunningProject(out string errorMessage))
            {
                this.ErrorMessage = this.Localizer.Lookup("RunProject.FailedLoadingSource");
                return ;
            }

            PopulateTargetLanguages();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            this.IsInProgress = false;
            this.ProjectName = string.Empty;
            this.ProjectDetails = string.Empty;
            this.SourceLanguage = null;
            this.FileFormat = null;
            this.SourceLanguageLabel = string.Empty;
            this.SourceLanguageKey = string.Empty;
            this.TargetLanguageLabel = string.Empty;
            this.ErrorMessage = this.Localizer.Lookup("RunProject.FileSystemError");
            return;
        }

        this.isPopulated = true;
    }

    private void StartProject()
    {
        if (!this.isPopulated)
        {
            return;
        }

        var currentProject = this.translatorModel.ActiveProject;
        if ((currentProject is null) || currentProject.IsInvalid)
        {
            // Error: no active project 
            this.NoProject();
            return;
        }

        this.runProjectToolbarViewModel.IsRunning = true;
        this.TranslationStatus = this.Localizer.Lookup("RunProject.ProjectInProgress");
        if ((currentProject is null) || currentProject.IsInvalid)
        {
            // Error: no active project 
            this.NoProject() ;
            return ;
        }

        this.IsInProgress = true;
        this.SourceLanguageLabel = string.Empty;
        this.SourceLanguageKey = string.Empty;
        this.TargetLanguageLabel = string.Empty;

        Task.Run(this.RunTranslation);
    }

    private void StopProject() => this.translatorModel.AbortProject() ;

    private void EndProject(bool aborted)
    {
        this.IsInProgress = false;
        this.SourceLanguageLabel = string.Empty;
        this.SourceLanguageKey = string.Empty;
        this.TargetLanguageLabel = string.Empty;
        this.runProjectToolbarViewModel.IsRunning = false;
        this.TranslationStatus =
            this.Localizer.Lookup(aborted ? "RunProject.Aborted" : "RunProject.Idle");

        if (aborted)
        {
            this.toaster.Show(
                this.Localizer.Lookup("Shell.Error"),
                this.Localizer.Lookup("RunProject.Aborted"),
                3_000, InformationLevel.Error);

            // Reload to update current state
            this.Populate();
        }
    }

    private async Task<bool> RunTranslation()
        => _ = await this.translatorModel.RunProject(); 

    private void NoProject()
    {
        this.ProjectName = string.Empty;
        this.ProjectDetails = string.Empty;
        this.TranslationStatus = string.Empty;
        this.ErrorMessage = this.Localizer.Lookup("RunProject.NoActiveProject");
    }

    #region Bound Properties 

    [ObservableProperty]
    private string? errorMessage ;

    [ObservableProperty]
    private string? projectName ;

    [ObservableProperty]
    private string? projectDetails ;

    [ObservableProperty]
    private string? translationStatus ;

    [ObservableProperty]
    private string? sourceText ;

    [ObservableProperty]
    private string? targetText ;

    [ObservableProperty]
    private string? sourceLanguageLabel ;

    [ObservableProperty]
    private string? sourceLanguageKey ;

    [ObservableProperty]
    private string? targetLanguageLabel ;

    [ObservableProperty]
    private bool isInProgress ;

    [ObservableProperty]
    private ObservableCollection<ExtLanguageInfoViewModel> selectedLanguages;

    [ObservableProperty]
    private LanguageInfoViewModel? sourceLanguage;

    [ObservableProperty]
    private FileFormatViewModel? fileFormat; 

    #endregion Bound Properties 
}
