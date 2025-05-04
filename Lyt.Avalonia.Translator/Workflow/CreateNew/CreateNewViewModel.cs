namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

public sealed class CreateNewViewModel : Bindable<CreateNewView>
{
    private readonly TranslatorModel translatorModel;
    private readonly TranslatorService translatorService;
    private readonly List<LanguageInfoViewModel> languages;
    private readonly List<ClickableLanguageInfoViewModel> clickableLanguages;

    private bool isInitializing;
    private Language? selectedSourceLanguage;
    private ResourceFormat selectedFileFormat;

    public CreateNewViewModel(
        TranslatorModel translatorModel, TranslatorService translatorService )
    {
        this.translatorModel = translatorModel;
        this.translatorService = translatorService;
        this.languages = [];
        this.clickableLanguages = [];
        this.PopulateLanguageAndFormats();
        this.Messenger.Subscribe<ToolbarCommandMessage>(this.OnToolbarCommand);
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

    }

    private void ClearAllLanguages()
    {

    }

    private void SaveProject()
    {

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

            this.FileFormats =[];
            foreach (ResourceFormat resourceFormat in ResourceFormats.Available())
            {
                this.FileFormats.Add(new FileFormatViewModel(resourceFormat));
            }

            this.SelectedFileFormatIndex = 0;
            this.selectedFileFormat = this.FileFormats[0].ResourceFormat;

            this.SourcePath = string.Empty;
        }

        this.isInitializing = false;
    }

    private ClickableLanguageInfoViewModel? Available (LanguageInfoViewModel vm )
        =>  (from item in this.AvailableLanguages
             where item.Language.LanguageKey == vm.Language.LanguageKey
             select item)
             .FirstOrDefault() ;

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
    }

#pragma warning disable IDE0079
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    // TODO : Save, Add All buttons, etc 

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
