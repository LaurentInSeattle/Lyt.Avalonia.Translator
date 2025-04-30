namespace Lyt.Avalonia.Translator.Workflow.Interactive;

public sealed class InteractiveViewModel : Bindable<InteractiveView>
{
    private readonly TranslatorModel translatorModel;
    private readonly List<LanguageInfoViewModel> languages;
    private bool isInitializing;

    public InteractiveViewModel(TranslatorModel translatorModel)
    {
        this.translatorModel = translatorModel;
        this.languages = [];
        this.PopulateLanguages();
    }

    private void PopulateLanguages()
    {
        this.isInitializing = true;
        foreach (Language language in Language.Languages.Values)
        {
            LanguageInfoViewModel languageInfoViewModel =
                new(language.CultureKey, language.LocalName, language.PrimaryFlag, language.SecondaryFlag);
            this.languages.Add(languageInfoViewModel);
        }

        this.SourceLanguages = [.. this.languages];
        this.TargetLanguages = [.. this.languages];
        this.isInitializing = false;
    }

    public int SelectedSourceLanguageIndex
    {
        get => this.Get<int>();
        set
        {
            // Update the UI...
            this.Set(value);

            // ... But do not change the language when initializing 
            if (this.isInitializing)
            {
                return;
            }

            // string languageKey = this.Languages[value].Key;
            // Debug.WriteLine("Selected language: " + languageKey);
        }
    }

    public ObservableCollection<LanguageInfoViewModel> SourceLanguages
    {
        get => this.Get<ObservableCollection<LanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }

    public int SelectedTargetLanguageIndex
    {
        get => this.Get<int>();
        set
        {
            // Update the UI...
            this.Set(value);

            // ... But do not change the language when initializing 
            if (this.isInitializing)
            {
                return;
            }

            // string languageKey = this.Languages[value].Key;
            // Debug.WriteLine("Selected language: " + languageKey);
        }
    }

    public ObservableCollection<LanguageInfoViewModel> TargetLanguages
    {
        get => this.Get<ObservableCollection<LanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }
}
