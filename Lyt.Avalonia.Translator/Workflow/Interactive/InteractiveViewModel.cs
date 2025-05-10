namespace Lyt.Avalonia.Translator.Workflow.Interactive;

public sealed class InteractiveViewModel : Bindable<InteractiveView>
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
        this.DisablePropertyChangedLogging = true;
        this.translatorModel = translatorModel;
        this.translatorService = translatorService;
        this.languages = [];
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

#pragma warning disable IDE0079 
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    private void OnClearSource(object? _)
    {
        this.SourceText = string.Empty;
        this.TargetText = string.Empty;
    }

    private async void OnCopyTarget(object? _)
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

    private void OnGo(object? discard) => this.OnEnter(discard);

    private void OnEnter(object? _)
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


#pragma warning restore CA1822
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079

    private void TryOnlineOperation(Func<Task<bool>> action)
    {
        // string? wasWelcomeMessage = this.Welcome;
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
        => this.Messenger.Publish(
            new ModelUpdateMessage(
                this.translatorModel, nameof(this.translatorModel.IsInternetConnected)));

    public string? SourceText { get => this.Get<string?>(); set => this.Set(value); }

    public string? TargetText { get => this.Get<string?>(); set => this.Set(value); }

    public string? TranslatedBackText { get => this.Get<string?>(); set => this.Set(value); }

    public bool ProgressRingIsActive { get => this.Get<bool>(); set => this.Set(value); }

    public ICommand EnterCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand GoCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand ClearSourceCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand CopyTargetCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

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

                this.OnClearSource(null);
            }
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
            bool changed = this.Set(value);

            // ... But do not change the language when initializing 
            if (this.isInitializing)
            {
                return;
            }

            if (changed)
            {
                this.selectedTargetLanguage = Language.Languages[this.TargetLanguages[value].Key];
                Debug.WriteLine("Selected Target language: " + this.selectedTargetLanguage);

                // Force a new translation
                this.lastTranslatedText = string.Empty;
                this.OnEnter(null);
            }
        }
    }

    public ObservableCollection<LanguageInfoViewModel> TargetLanguages
    {
        get => this.Get<ObservableCollection<LanguageInfoViewModel>?>() ?? throw new ArgumentNullException("Languages");
        set => this.Set(value);
    }
}
