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
                LanguageInfoViewModel languageInfoViewModel =
                    new(language.CultureKey, language.LocalName, language.PrimaryFlag, language.SecondaryFlag);
                this.languages.Add(languageInfoViewModel);
            }

            this.SourceLanguages = [.. this.languages];
            this.SelectedSourceLanguageIndex = 0;
            this.selectedSourceLanguage = Language.Languages[this.SourceLanguages[0].Key];

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
                this.selectedSourceLanguage = Language.Languages[this.SourceLanguages[value].Key];
                Debug.WriteLine("Selected Source language: " + this.selectedSourceLanguage);
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
}
