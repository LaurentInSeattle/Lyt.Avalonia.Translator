using static Lyt.Avalonia.Translator.Messaging.ViewActivationMessage;

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
        this.SelectedSourceLanguageIndex = 0; 
        this.TargetLanguages = [.. this.languages];
        this.SelectedTargetLanguageIndex = 1;
        this.isInitializing = false;
    }

#pragma warning disable IDE0079 
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    private void OnClearSource(object? _) { }

    private void OnCopyTarget(object? _) { } 

#pragma warning restore CA1822
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079

    public ICommand ClearSourceCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand CopyTargetCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

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
