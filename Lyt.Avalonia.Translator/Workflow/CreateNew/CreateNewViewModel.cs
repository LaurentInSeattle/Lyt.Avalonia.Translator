namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

public sealed class CreateNewViewModel : Bindable<CreateNewView>
{
    private readonly TranslatorModel translatorModel;
    private readonly TranslatorService translatorService;
    private readonly IToaster toaster;
    private readonly List<LanguageInfoViewModel> languages;
    private readonly List<ClickableLanguageInfoViewModel> clickableLanguages;

    private bool isInitializing;
    private Language? selectedSourceLanguage;
    private ResourceFormat selectedFileFormat;
    private Project? project;

    public CreateNewViewModel(
        TranslatorModel translatorModel,
        TranslatorService translatorService,
        IToaster toaster)
    {
        this.translatorModel = translatorModel;
        this.translatorService = translatorService;
        this.toaster = toaster;
        this.languages = [];
        this.clickableLanguages = [];
        this.PopulateLanguageAndFormats();
        this.Messenger.Subscribe<ToolbarCommandMessage>(this.OnToolbarCommand);
        this.Messenger.Subscribe<DropFileMessage>(this.OnDropFile);
    }

    private void OnDropFile(DropFileMessage message)
    {
        if (message.Success)
        {
            this.ProcessSourceLanguageFile(path: message.Data);
        }
        else
        {
            this.ErrorMessage = this.Localizer.Lookup(message.Data);
        }
    }

    private void OnToolbarCommand(ToolbarCommandMessage message)
    {
        switch (message.Command)
        {
            case ToolbarCommandMessage.ToolbarCommand.CreateNewAddAllLanguages:
                this.AddAllLanguages();
                break;

            case ToolbarCommandMessage.ToolbarCommand.CreateNewClearAllLanguages:
                this.ClearAllLanguages();
                break;

            case ToolbarCommandMessage.ToolbarCommand.CreateNewSaveProject:
                this.SaveProject();
                break;

            // Ignore all other commands 
            default:
                break;
        }
    }

    private void AddAllLanguages()
    {
        var available = this.AvailableLanguages.ToList();
        foreach (var language in available)
        {
            language.ToggleAvailability();
            this.SelectedLanguages.Add(language);
        }

        this.AvailableLanguages.Clear();
    }

    private void ClearAllLanguages()
    {
        var selected = this.SelectedLanguages.ToList();
        foreach (var language in selected)
        {
            language.ToggleAvailability();
            this.AvailableLanguages.Add(language);
        }

        this.SelectedLanguages.Clear();
    }

    private void SaveProject()
    {
        if ( this.project is null )
        {
            this.toaster.Show(
                this.Localizer.Lookup("CreateNew.CantSaveProjectTitle"),
                this.Localizer.Lookup("CreateNew.CantSaveProjectText"), 
                3_000, InformationLevel.Warning);
            return ;
        }

        this.ErrorMessage = string.Empty;
        if (!this.project.Validate( out string errorMessageKey))
        {
            this.ErrorMessage = this.Localizer.Lookup(errorMessageKey);
            return;
        }

        if (!this.translatorModel.AddNewProject(this.project, out errorMessageKey))
        {
            this.ErrorMessage = this.Localizer.Lookup(errorMessageKey);
            return;
        }

        this.toaster.Show(
            this.Localizer.Lookup("CreateNew.ProjectSavedTitle"),
            this.Localizer.Lookup("CreateNew.ProjectSavedText"), 
            3_000, InformationLevel.Success);
    }


    private void ProcessSourceLanguageFile(string path)
    {
        FileInfo fileInfo = new(path);
        if (!fileInfo.Exists)
        {
            this.ErrorMessage = this.Localizer.Lookup("CreateNew.FileDoesNotExist");
            return;
        }

        string? directoryName = fileInfo.DirectoryName;
        string? fileName = fileInfo.Name;
        string? extension = fileInfo.Extension;

        Debug.WriteLine(directoryName + "  -   " + fileName + "  -   " + extension);

        if (string.IsNullOrWhiteSpace(directoryName) ||
            string.IsNullOrWhiteSpace(fileName) ||
            string.IsNullOrWhiteSpace(extension))
        {
            this.ErrorMessage = this.Localizer.Lookup("CreateNew.UnsupportedFileFormat");
            return;
        }

        // Check for supported extension 
        bool hasFoundFormat = false;
        var foundResourceFormat = default(ResourceFormat);
        foreach (ResourceFormat resourceFormat in ResourceFormats.AvailableFormats)
        {
            if (resourceFormat.ToFileExtension() == extension)
            {
                foundResourceFormat = resourceFormat;
                hasFoundFormat = true;
                break;
            }
        }

        if (!hasFoundFormat)
        {
            this.ErrorMessage = this.Localizer.Lookup("CreateNew.UnsupportedFileFormat");
            return;
        }

        // Check if file name contains a supported language key 
        bool hasFoundKey = false;
        string foundKey = string.Empty;
        foreach (string cultureKey in Language.Languages.Keys.ToList())
        {
            int indexOfKey = fileName.IndexOf(cultureKey, StringComparison.OrdinalIgnoreCase);
            if (indexOfKey > 0)
            {
                hasFoundKey = true;
                foundKey = cultureKey;
                break;
            }
        }

        if (!hasFoundKey)
        {
            this.ErrorMessage = this.Localizer.Lookup("CreateNew.UnsupportedCulture");
            return;
        }

        if ((!Language.Languages.TryGetValue(foundKey, out Language? language)) ||
            (language is null))
        {
            this.ErrorMessage = this.Localizer.Lookup("CreateNew.UnsupportedCulture");
            return;
        }

        // Create the target file format 
        string targetFileFormat = fileName.Replace(foundKey, "{0}");
        string untitled = this.Localizer.Lookup("CreateNew.Untitled");
        string projectName =
            string.IsNullOrWhiteSpace(this.ProjectName) ? untitled : this.ProjectName;
        this.project = new Project()
        {
            Name = projectName,
            FolderPath = directoryName,
            Format = foundResourceFormat,
            SourceFile = fileName,
            TargetFileFormat = targetFileFormat,
            SourceLanguageCultureKey = language.CultureKey,
            TargetLanguagesCultureKeys = 
                [.. (from item in this.SelectedLanguages select item.Language.CultureKey)],
        };

        // Here it's ok to have errors...
        _ = this.project.Validate(out string errorMessageKey );
        this.ErrorMessage = this.Localizer.Lookup(errorMessageKey);

        // Update UI: Note that we do NOT set the initializing flag. 
        this.ProjectName = projectName;
        this.SourceFile = fileName;

        // Auto Select File format 
        this.SelectFileFormat(foundResourceFormat);

        // Auto Select Source Language
        this.SelectSourceLanguage(foundKey);
    }

    private void PopulateLanguageAndFormats()
    {
        this.isInitializing = true;
        {
            foreach (Language language in Language.Languages.Values)
            {
                this.languages.Add(new LanguageInfoViewModel(language));
                this.clickableLanguages.Add(
                    new ClickableLanguageInfoViewModel(this, language, isAvailable: true));
            }

            this.SourceLanguages = [.. this.languages];
            this.AvailableLanguages = [.. this.clickableLanguages];
            this.SelectedLanguages = [];
            this.SelectedSourceLanguageIndex = 0;
            LanguageInfoViewModel selected = this.SourceLanguages[0];
            this.selectedSourceLanguage = selected.Language;
            if (this.Available(selected) is ClickableLanguageInfoViewModel available)
            {
                this.AvailableLanguages.Remove(available);
            }

            this.FileFormats = [];
            foreach (ResourceFormat resourceFormat in ResourceFormats.AvailableFormats)
            {
                this.FileFormats.Add(new FileFormatViewModel(resourceFormat));
            }

            this.SelectedFileFormatIndex = 0;
            this.selectedFileFormat = this.FileFormats[0].ResourceFormat;

            this.ErrorMessage = string.Empty;
            this.SourceFile = string.Empty;
        }

        this.isInitializing = false;
    }

    private void SelectFileFormat(ResourceFormat fileFormat)
    {
        int index = 0;
        foreach (var resourceFormat in this.FileFormats)
        {
            if (fileFormat == resourceFormat.ResourceFormat)
            {
                this.SelectedFileFormatIndex = index;
                return;
            }

            ++index;
        }
    }

    private void SelectSourceLanguage(string cultureKey)
    {
        int index = 0;
        foreach (var item in this.SourceLanguages)
        {
            if (cultureKey == item.Language.CultureKey)
            {
                this.SelectedSourceLanguageIndex = index;
                return;
            }

            ++index;
        }
    }

    private ClickableLanguageInfoViewModel? Available(LanguageInfoViewModel vm)
        => (from item in this.AvailableLanguages
            where item.Language.LanguageKey == vm.Language.LanguageKey
            select item)
             .FirstOrDefault();

    private ClickableLanguageInfoViewModel? Selected(LanguageInfoViewModel vm)
        => (from item in this.SelectedLanguages
            where item.Language.LanguageKey == vm.Language.LanguageKey
            select item)
             .FirstOrDefault();

    internal void OnClicked(ClickableLanguageInfoViewModel viewModel)
    {
        if (viewModel.IsAvailable)
        {
            this.AvailableLanguages.Remove(viewModel);
            this.SelectedLanguages.Add(viewModel);
        }
        else
        {
            this.SelectedLanguages.Remove(viewModel);
            this.AvailableLanguages.Add(viewModel);
        }

        viewModel.ToggleAvailability();
        if (this.project is not null)
        {
            this.project.TargetLanguagesCultureKeys = 
                [.. from language in this.SelectedLanguages select language.Language.CultureKey];
        } 
    }

    public string? ErrorMessage { get => this.Get<string?>(); set => this.Set(value); }

    public string? ProjectName
    {
        get => this.Get<string?>();
        set
        {
            this.Set(value);
            if ((this.project is not null) && !string.IsNullOrWhiteSpace(value))
            {
                this.project.Name = value.Trim();
            }
        }
    }

    public string? SourceFile { get => this.Get<string?>(); set => this.Set(value); }

    public int SelectedSourceLanguageIndex
    {
        get => this.Get<int>();
        set
        {
            // Update the UI...
            int oldValue = this.Get<int>();
            bool changed = this.Set(value);

            // ... But do not change the language when initializing 
            if (this.isInitializing)
            {
                return;
            }

            if (changed)
            {
                LanguageInfoViewModel selectedVm = this.SourceLanguages[value];
                this.selectedSourceLanguage = selectedVm.Language;
                var available = this.Available(selectedVm);
                if (available is not null)
                {
                    this.AvailableLanguages.Remove(available);
                }

                var selected = this.Selected(selectedVm);
                if (selected is not null)
                {
                    this.SelectedLanguages.Remove(selected);
                }

                if (this.project is not null)
                {
                    this.project.SourceLanguageCultureKey = this.selectedSourceLanguage.CultureKey;
                }

                // Old value becomes available 
                Language language = this.SourceLanguages[oldValue].Language;
                this.AvailableLanguages.Add(
                    new ClickableLanguageInfoViewModel(this, language, isAvailable: true));

                Debug.WriteLine("Selected Source language: " + this.selectedSourceLanguage.LocalName);
            }
        }
    }

    public ObservableCollection<LanguageInfoViewModel> SourceLanguages
    {
        get => this.Get<ObservableCollection<LanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

    public int SelectedFileFormatIndex
    {
        get => this.Get<int>();
        set
        {
            // Update the UI...
            bool changed = this.Set(value);

            // ... But do not change the language when initializing 
            if (this.isInitializing)
            {
                return;
            }

            if (changed)
            {
                this.selectedFileFormat = this.FileFormats[value].ResourceFormat;
            }
        }
    }

    public ObservableCollection<FileFormatViewModel> FileFormats
    {
        get => this.Get<ObservableCollection<FileFormatViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }


    public ObservableCollection<ClickableLanguageInfoViewModel> AvailableLanguages
    {
        get => this.Get<ObservableCollection<ClickableLanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

    public ObservableCollection<ClickableLanguageInfoViewModel> SelectedLanguages
    {
        get => this.Get<ObservableCollection<ClickableLanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

}
