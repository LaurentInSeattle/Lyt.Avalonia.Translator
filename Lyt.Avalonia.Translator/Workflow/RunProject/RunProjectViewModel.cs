namespace Lyt.Avalonia.Translator.Workflow.RunProject;

using static ToolbarCommandMessage;

public sealed class RunProjectViewModel : Bindable<RunProjectView>
{
    private readonly TranslatorModel translatorModel;
    private readonly IToaster toaster;

    private bool isPopulated;
    private Dictionary<string, string> sourceDictionary;
    private readonly Dictionary<string, Dictionary<string, string>> targetDictionaries;
    private readonly Dictionary<string, Dictionary<string, string>> needTranslationDictionaries;
    private readonly Dictionary<string, ExtLanguageInfoViewModel> targetLanguageViewModels;

    public RunProjectViewModel(TranslatorModel translatorModel, IToaster toaster)
    {
        this.translatorModel = translatorModel;
        this.toaster = toaster;

        this.sourceDictionary = [];
        this.targetDictionaries = [];
        this.needTranslationDictionaries = [];

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

        void LoadDictionaries()
        {
            string sourcePath = currentProject.SourceFilePath();
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
        }

        void PopulateTargetLanguages ()
        {
            this.targetLanguageViewModels.Clear();
            // Loop through target languages 
            // Create a VM for each save in UI and in class data 
            foreach (string cultureKey in currentProject.TargetLanguagesCultureKeys)
            {
                ExtLanguageInfoViewModel vm = new (Language.Languages[cultureKey]);
                vm.SetComplete(missing: 1);
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
    }

    private void StopProject() { }

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
