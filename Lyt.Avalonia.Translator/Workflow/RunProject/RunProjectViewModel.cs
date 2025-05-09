namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static ToolbarCommandMessage;

public sealed class RunProjectViewModel : Bindable<RunProjectView>
{
    private readonly TranslatorModel translatorModel;
    private readonly TranslatorService translatorService;
    private readonly RunProjectToolbarViewModel runProjectToolbarViewModel; 
    private readonly IToaster toaster;

    private readonly Dictionary<string, Dictionary<string, string>> targetDictionaries;
    private readonly Dictionary<string, Dictionary<string, string>> needTranslationDictionaries;
    private readonly Dictionary<string, int> missingEntries;
    private readonly Dictionary<string, ExtLanguageInfoViewModel> targetLanguageViewModels;

    private Dictionary<string, string> sourceDictionary;
    private bool isPopulated;
    private bool abortRequested;

    public RunProjectViewModel(
        TranslatorModel translatorModel, 
        TranslatorService translatorService,
        RunProjectToolbarViewModel runProjectToolbarViewModel, 
        IToaster toaster)
    {
        this.translatorModel = translatorModel;
        this.translatorService = translatorService;
        this.runProjectToolbarViewModel = runProjectToolbarViewModel;
        this.toaster = toaster;

        this.sourceDictionary = [];
        this.targetDictionaries = [];
        this.needTranslationDictionaries = [];
        this.missingEntries = [];
        this.SelectedLanguages = [];
        this.targetLanguageViewModels = [];
        this.Messenger.Subscribe<ToolbarCommandMessage>(this.OnToolbarCommand);
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);
        Dispatch.OnUiThread(this.Populate);
    }

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
                "Source file: {0} - Last updated: {1} {2}",
                currentProject.SourceFile,
                currentProject.LastUpdated.ToShortDateString(),
                currentProject.LastUpdated.ToShortTimeString());
        this.SourceLanguage =
            new LanguageInfoViewModel(Language.Languages[currentProject.SourceLanguageCultureKey]);
        this.SelectedLanguages = [];

        void LoadDictionaries()
        {
            string sourcePath = currentProject.SourceFilePath();

            // TODO: Abstract the storage format 
            var sourceResult = TranslatorModel.ParseAxamlResourceFile(sourcePath);
            bool sourceLoaded = sourceResult.Item1;
            if (!sourceLoaded)
            {
                this.ErrorMessage = this.Localizer.Lookup("RunProject.FailedLoadingSource");
                return;
            }

            this.sourceDictionary = sourceResult.Item2;
            this.targetDictionaries.Clear();

            // Loop through target languages 
            // Check whether of not we have existing translations, if we do load them 
            // if the file is not there create an empty dictionary 
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                string targetPath = currentProject.TargetFilePath(cultureKey);
                Dictionary<string, string> targetDictionary = [];
                if (File.Exists(targetPath))
                {
                    // TODO: Abstract the storage format 
                    var targetResult = TranslatorModel.ParseAxamlResourceFile(targetPath);
                    if (targetResult.Item1)
                    {
                        targetDictionary = targetResult.Item2;
                    }
                }

                this.targetDictionaries.Add(cultureKey, targetDictionary);
            }
        }

        void FindMissingTranslations()
        {
            this.needTranslationDictionaries.Clear();
            this.missingEntries.Clear();

            // Loop through the list of target languages to initialize the nested dictionary 
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                this.needTranslationDictionaries.Add(cultureKey, []);
            }

            // Loop again through the keys of the source language to populate 
            foreach (string languageKey in this.sourceDictionary.Keys)
            {
                // Loop through target languages 
                foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
                {
                    var dictionary = this.targetDictionaries[cultureKey];
                    var needed = this.needTranslationDictionaries[cultureKey];
                    if (dictionary.TryGetValue(languageKey, out string? value))
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            // Empty translation: Add key and empty string
                            needed.Add(languageKey, string.Empty);
                        }

                        // We have a translation: dont change it
                    }
                    else
                    {
                        // Missing translation: Add key and empty string
                        needed.Add(languageKey, string.Empty);
                    }
                }
            }

            // Loop through the list of target languages to initialize the count of missing entries 
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                int missingEntries = this.needTranslationDictionaries[cultureKey].Values.Count;
                this.missingEntries.Add(cultureKey, missingEntries);
            }
        }

        void PopulateTargetLanguages()
        {
            this.targetLanguageViewModels.Clear();

            // Loop through target languages 
            // Create a VM for each save in UI and in class data 
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                ExtLanguageInfoViewModel vm = new(Language.Languages[cultureKey]);
                int missingEntries = this.missingEntries[cultureKey];
                vm.SetComplete(missing: missingEntries);
                this.targetLanguageViewModels.Add(cultureKey, vm);
            }

            this.SelectedLanguages = [.. this.targetLanguageViewModels.Values];
        }

        try
        {
            LoadDictionaries();
            FindMissingTranslations();
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
        this.abortRequested = false;
        this.TranslationStatus = this.Localizer.Lookup("RunProject.ProjectInProgress");
        Task.Run(this.RunTranslation);
    }

    private void StopProject() => this.abortRequested = true;

    private void EndProject(bool aborted)
    {
        this.runProjectToolbarViewModel.IsRunning = false;
        this.TranslationStatus = 
            this.Localizer.Lookup(aborted ? "RunProject.Aborted" : "RunProject.Idle"); 

        if (aborted)
        {
            // May need to try to save if some translations completed 

            // May need to reload better than that 
            var currentProject = this.translatorModel.ActiveProject;
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                ExtLanguageInfoViewModel vm = this.targetLanguageViewModels[cultureKey];
                int missingEntries = this.needTranslationDictionaries.Keys.Count;
                vm.SetComplete(missingEntries);
            }
        } 
    }

    private async Task<bool> RunTranslation()
    {
        var currentProject = this.translatorModel.ActiveProject;
        if ((currentProject is null) || currentProject.IsInvalid)
        {
            // Error: no active project 
            Dispatch.OnUiThread(this.NoProject);
            return false;
        }

        // Loop through target languages 
        string sourceLanguageKey =
            Language.Languages[currentProject.SourceLanguageCultureKey].LanguageKey;
        foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
        {
            ExtLanguageInfoViewModel vm = this.targetLanguageViewModels[cultureKey];
            Dispatch.OnUiThread(() => { vm.InProgress(); });

            if (this.abortRequested)
            {
                Dispatch.OnUiThread(() => { this.EndProject(aborted: true); });
                return false;
            }

            // Loop through missing target language strings 
            string targetLanguageKey = Language.Languages[cultureKey].LanguageKey;
            var missingTranslations = this.needTranslationDictionaries[cultureKey];
            foreach (string targetKey in missingTranslations.Keys)
            {
                if (this.abortRequested)
                {
                    Dispatch.OnUiThread(() => { this.EndProject(aborted: true); });
                    return false;
                }

                string sourceText = this.sourceDictionary[targetKey];
                string translatedText = "Yolo - " + targetKey;

                // DONT Call the service until the UI is complete 
                //
                //var result = 
                //    await this.translatorService.Translate(
                //      this.translatorModel.ActiveProvider,
                //      sourceText, sourceLanguageKey, targetLanguageKey);
                //bool success = result.Item1;


                Debug.WriteLine(targetLanguageKey + " - " + sourceText);
                await Task.Delay(999);
                bool success = true;
                if (success)
                {
                    Dispatch.OnUiThread(() => 
                    { 
                        this.SourceText = sourceText;
                        this.TargetText = translatedText;
                    });

                    missingTranslations[targetKey] = translatedText;

                    await Task.Delay(50);
                    Dispatch.OnUiThread(() => { vm.TranslationAdded(); });
                    await Task.Delay(50);
                }
                else
                {
                    this.abortRequested = true;
                    Dispatch.OnUiThread(() => { this.EndProject(aborted: true); });
                    return false; 
                }
            }

            // Save translations in chosen format 
            if (this.MergeAndSaveTranslations(cultureKey, missingTranslations))
            {
                Dispatch.OnUiThread(() => { vm.SetComplete(0); });
            }
            else
            {
                Dispatch.OnUiThread(() => { this.EndProject(aborted: true); });
                return false;
            }
        }

        // Complete 
        if (!this.abortRequested)
        {
            Dispatch.OnUiThread(() => { this.EndProject(aborted: false); });
            return true;
        }

        return false; 
    }

    private bool MergeAndSaveTranslations (
        string cultureKey, Dictionary<string, string> missingTranslations )
    {
        // TODO: Abstract the storage format 
        try
        {

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        } 
    }

    private void NoProject()
    {
        this.ProjectName = string.Empty;
        this.ProjectDetails = string.Empty;
        this.TranslationStatus = string.Empty;
        this.ErrorMessage = this.Localizer.Lookup("RunProject.NoActiveProject");
    }

    public string? ErrorMessage { get => this.Get<string?>(); set => this.Set(value); }

    public string? ProjectName { get => this.Get<string?>(); set => this.Set(value); }

    public string? ProjectDetails { get => this.Get<string?>(); set => this.Set(value); }

    public string? TranslationStatus { get => this.Get<string?>(); set => this.Set(value); }

    public string? SourceText { get => this.Get<string?>(); set => this.Set(value); }

    public string? TargetText { get => this.Get<string?>(); set => this.Set(value); }

    public string? TargetLanguage { get => this.Get<string?>(); set => this.Set(value); }

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

}
