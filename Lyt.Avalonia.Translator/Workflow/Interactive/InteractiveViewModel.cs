namespace Lyt.Avalonia.Translator.Workflow.Interactive;

public sealed partial class InteractiveViewModel : ViewModel<InteractiveView>
{
    private readonly TranslatorModel translatorModel;
    private readonly TranslatorService translatorService;
    private readonly List<LanguageInfoViewModel> languages;

    private bool isInitializing;
    private Language? selectedSourceLanguage;
    private Language? selectedTargetLanguage;
    private string? lastTranslatedText;

    public InteractiveViewModel(
        TranslatorModel translatorModel, TranslatorService translatorService)
    {
        this.translatorModel = translatorModel;
        this.translatorService = translatorService;
        this.languages = [];
        this.SourceLanguages = [];
        this.TargetLanguages = [];
        this.PopulateLanguages();
    }

    private void PopulateLanguages()
    {
        this.isInitializing = true;
        this.ProgressRingIsActive = false;
        this.SourceText = string.Empty;
        foreach (Language language in Language.Languages.Values)
        {
            LanguageInfoViewModel languageInfoViewModel = new(language);
            this.languages.Add(languageInfoViewModel);
        }

        this.SourceLanguages = [.. this.languages];
        this.SelectedSourceLanguageIndex = 0;
        this.selectedSourceLanguage = this.SourceLanguages[0].Language;

        this.TargetLanguages = [.. this.languages];
        this.SelectedTargetLanguageIndex = 1;
        this.selectedTargetLanguage = this.TargetLanguages[1].Language;
        this.isInitializing = false;
    }

    [RelayCommand]
    public async Task OnCopyTarget()
    {
        string? maybeTranslation = this.TargetText;
        if (string.IsNullOrWhiteSpace(maybeTranslation))
        {
            return;
        }

        maybeTranslation = maybeTranslation.Trim();
        if (string.IsNullOrWhiteSpace(maybeTranslation))
        {
            return;
        }

        var clipboard = App.MainWindow.Clipboard;
        if (clipboard is null)
        {
            return;
        }

        await clipboard.SetTextAsync(maybeTranslation);
    }

    [RelayCommand]
    public void OnClearSource()
    {
        this.SourceText = string.Empty;
        this.TargetText = string.Empty;
    }

    [RelayCommand]
    public void OnGo() => this.OnEnter();

    [RelayCommand]
    public void OnEnter()
    {
        // Nothing to translate
        string? maybeSourceText = this.SourceText;
        if (string.IsNullOrWhiteSpace(maybeSourceText))
        {
            return;
        }

        maybeSourceText = maybeSourceText.Trim();
        if (string.IsNullOrWhiteSpace(maybeSourceText))
        {
            return;
        }

        // Dont translate again same text 
        if (this.lastTranslatedText is not null && (this.lastTranslatedText == maybeSourceText))
        {
            return;
        }

        async Task<bool> TryTranslate()
        {
            if ((this.selectedSourceLanguage is null) || (this.selectedTargetLanguage is null))
            {
                return false;
            }

            this.lastTranslatedText = maybeSourceText;
            var result = await this.translatorService.Translate(
                this.translatorModel.ActiveProvider,
                maybeSourceText,
                this.selectedSourceLanguage.LanguageKey,
                this.selectedTargetLanguage.LanguageKey);
            bool success = result.Item1;
            await Task.Delay(50);
            Dispatch.OnUiThread(() =>
            {
                this.TargetText = success ? result.Item2 : string.Empty;
            });
            await Task.Delay(200);

            // If successful translation, try to translate back 
            if (success)
            {
                var secondResult = await this.translatorService.Translate(
                    this.translatorModel.ActiveProvider,
                    result.Item2,
                    this.selectedTargetLanguage.LanguageKey,
                    this.selectedSourceLanguage.LanguageKey);
                bool secondSuccess = secondResult.Item1;
                Dispatch.OnUiThread(() =>
                {
                    this.TranslatedBackText = secondSuccess ? secondResult.Item2 : string.Empty;
                });
            }

            // Regarless of success, complete operation 
            Dispatch.OnUiThread(this.CompleteOnlineOperation);

            return success;
        }

        this.TryOnlineOperation(TryTranslate);
    }

    private void TryOnlineOperation(Func<Task<bool>> action)
    {
        try
        {
            // Check internet 
            if (!this.translatorModel.IsInternetConnected)
            {
                this.UpdateInternetConnectionStatus();
                return;
            }

            var view = this.View;
            this.Logger.Debug(view.GetType().FullName!);

            // Block the UI
            this.View.Opacity = 0.666;
            this.View.IsEnabled = false;
            this.View.IsHitTestVisible = false;

            // Launch spinner and show new message 
            this.ProgressRingIsActive = true;

            // Now onto the job 
            _ = action();
        }
        catch (Exception ex)
        {
            // Logging 
            this.Logger.Error(ex.ToString());
        }
    }

    private void CompleteOnlineOperation()
    {
        Dispatch.OnUiThread(() =>
        {
            // Unlock the UI, clear spinner, etc...
            this.View.Opacity = 1.0;
            this.View.IsEnabled = true;
            this.View.IsHitTestVisible = true;
            this.ProgressRingIsActive = false;
        });
    }

    private void UpdateInternetConnectionStatus()
        // Could be redundant... but better twice than never.
        => new ModelUpdateMessage(
                this.translatorModel, nameof(this.translatorModel.IsInternetConnected)).Publish();

    [ObservableProperty]
    private string? sourceText;

    [ObservableProperty]
    private string? targetText;

    [ObservableProperty]
    private string? translatedBackText;

    [ObservableProperty]
    private bool progressRingIsActive;

    [ObservableProperty]
    private int selectedSourceLanguageIndex;

    partial void OnSelectedSourceLanguageIndexChanged(int value)
    {
        // Do not change the language when initializing 
        if (this.isInitializing)
        {
            return;
        }

        this.selectedSourceLanguage = Language.Languages[this.SourceLanguages[value].Key];
        this.OnClearSource();
        Debug.WriteLine("Selected Source language: " + this.selectedSourceLanguage);
    }

    [ObservableProperty]
    private ObservableCollection<LanguageInfoViewModel> sourceLanguages;

    [ObservableProperty]
    private int selectedTargetLanguageIndex;

    partial void OnSelectedTargetLanguageIndexChanged(int value)
    {
        // Do not change the language when initializing 
        if (this.isInitializing)
        {
            return;
        }

        this.selectedTargetLanguage = Language.Languages[this.TargetLanguages[value].Key];
        Debug.WriteLine("Selected Target language: " + this.selectedTargetLanguage);

        // Force a new translation
        this.lastTranslatedText = string.Empty;
        this.OnEnter();
    }

    [ObservableProperty]
    private ObservableCollection<LanguageInfoViewModel> targetLanguages;
}
