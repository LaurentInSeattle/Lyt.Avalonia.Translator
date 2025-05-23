namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static ToolbarCommandMessage;

public sealed class RunProjectViewModel : Bindable<RunProjectView>
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

        this.DisablePropertyChangedLogging = true;
        this.SelectedLanguages = [];
        this.targetLanguageViewModels = [];
        this.Messenger.Subscribe<ToolbarCommandMessage>(this.OnToolbarCommand);
        this.Messenger.Subscribe<BeginSourceLanguageMessage>(this.OnBeginSourceLanguage, withUiDispatch: true);
        this.Messenger.Subscribe<BeginTargetLanguageMessage>(this.OnTargetSourceLanguage, withUiDispatch: true);
        this.Messenger.Subscribe<TranslationAddedMessage>(this.OnTranslationAdded, withUiDispatch: true);
        this.Messenger.Subscribe<TranslationCompleteMessage>(this.OnTranslationComplete, withUiDispatch: true);
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);
        Dispatch.OnUiThread(this.Populate);
    }

    private void OnBeginSourceLanguage(BeginSourceLanguageMessage message)
    {
        this.SourceLanguageLabel = 
            string.Concat(message.EnglishName, "  ~  ", message.LocalName);
    }

    private void OnTargetSourceLanguage(BeginTargetLanguageMessage message)
    {
        ExtLanguageInfoViewModel vm = this.targetLanguageViewModels[message.CultureKey];
        vm.InProgress();
        this.TargetLanguageLabel = string.Concat(message.EnglishName, "  ~  ", message.LocalName);
    }

    private void OnTranslationAdded(TranslationAddedMessage message)
    {
        // Update the right side of the view 
        this.SourceText = message.SourceText;
        this.SourceLanguageKey = message.TargetLanguageKey;
        this.TargetText = message.TargetText;

        // Update the counts and status in the View on the left.
        ExtLanguageInfoViewModel vm = this.targetLanguageViewModels[message.TargetLanguageKey];
        vm.TranslationAdded();
    }

    private void OnTranslationComplete(TranslationCompleteMessage message)
        => this.EndProject(message.Aborted); 

    private void OnToolbarCommand(ToolbarCommandMessage message)
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

    public string? ErrorMessage { get => this.Get<string?>(); set => this.Set(value); }

    public string? ProjectName { get => this.Get<string?>(); set => this.Set(value); }

    public string? ProjectDetails { get => this.Get<string?>(); set => this.Set(value); }

    public string? TranslationStatus { get => this.Get<string?>(); set => this.Set(value); }

    public string? SourceText { get => this.Get<string?>(); set => this.Set(value); }

    public string? TargetText { get => this.Get<string?>(); set => this.Set(value); }

    public string? SourceLanguageLabel { get => this.Get<string?>(); set => this.Set(value); }

    public string? SourceLanguageKey { get => this.Get<string?>(); set => this.Set(value); }

    public string? TargetLanguageLabel { get => this.Get<string?>(); set => this.Set(value); }

    public bool IsInProgress { get => this.Get<bool>(); set => this.Set(value); }

    public ObservableCollection<ExtLanguageInfoViewModel> SelectedLanguages
    {
        get => this.Get<ObservableCollection<ExtLanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

    public LanguageInfoViewModel? SourceLanguage
    {
        get => this.Get<LanguageInfoViewModel?>();
        set => this.Set(value);
    }

    public FileFormatViewModel? FileFormat
    {
        get => this.Get<FileFormatViewModel?>();
        set => this.Set(value);
    }

    #endregion Bound Properties 
}
