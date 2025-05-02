namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

public sealed class CreateNewViewModel : Bindable<CreateNewView>
{
    private readonly TranslatorModel translatorModel;
    private readonly TranslatorService translatorService; 
    private readonly List<LanguageInfoViewModel> languages;

    private bool isInitializing;
    private Language? selectedSourceLanguage;
    private ResourceFormat selectedFileFormat;

    public CreateNewViewModel(
        TranslatorModel translatorModel, TranslatorService translatorService )
    {
        this.translatorModel = translatorModel;
        this.translatorService = translatorService;
        this.languages = [];
        this.PopulateLanguageAndFormats();
    }

    private void PopulateLanguageAndFormats()
    {
        this.isInitializing = true;
        {
            foreach (Language language in Language.Languages.Values)
            {
                LanguageInfoViewModel languageInfoViewModel = new(language);
                this.languages.Add(languageInfoViewModel);
            }

            this.SourceLanguages = [.. this.languages];
            this.AvailableLanguages = [.. this.languages];
            this.SelectedLanguages = [];
            this.SelectedSourceLanguageIndex = 0;
            LanguageInfoViewModel selected = this.SourceLanguages[0];
            this.selectedSourceLanguage = selected.Language;
            this.AvailableLanguages.Remove(selected);

            this.FileFormats = [.. ResourceFormats.Available()];
            this.SelectedFileFormatIndex = 0;
            this.selectedFileFormat = ResourceFormat.Axaml;

            this.SourcePath = string.Empty;
        }

        this.isInitializing = false;
    }

#pragma warning disable IDE0079 
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    // TODO : Save button 

#pragma warning restore CA1822
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079

    public string? ProjectName { get => this.Get<string?>(); set => this.Set(value); }

    public string? SourcePath { get => this.Get<string?>(); set => this.Set(value); }

    public int SelectedSourceLanguageIndex
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
                LanguageInfoViewModel selectedVm = this.SourceLanguages[value]; 
                this.selectedSourceLanguage = selectedVm.Language;
                this.AvailableLanguages.Remove(selectedVm);
                this.SelectedLanguages.Remove(selectedVm);
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
                this.selectedFileFormat = this.FileFormats[value]; 
            }
        }
    }

    public ObservableCollection<ResourceFormat> FileFormats
    {
        get => this.Get<ObservableCollection<ResourceFormat>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }


    public ObservableCollection<LanguageInfoViewModel> AvailableLanguages
    {
        get => this.Get<ObservableCollection<LanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

    public ObservableCollection<LanguageInfoViewModel> SelectedLanguages
    {
        get => this.Get<ObservableCollection<LanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

}
