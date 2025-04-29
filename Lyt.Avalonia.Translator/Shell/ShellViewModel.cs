using System;

namespace Lyt.Avalonia.Translator.Shell;

//using static MessagingExtensions;
//using static ViewActivationMessage;

// https://stackoverflow.com/questions/385793/programmatically-start-application-on-login 

public sealed partial class ShellViewModel : Bindable<ShellView>
{
    private const int MinutesToMillisecs = 60 * 1_000;

    private readonly TranslatorModel translatorModel;
    private readonly TranslatorService translatorService;
    private readonly IToaster toaster;
    private readonly IMessenger messenger;
    private readonly ILocalizer localizer;

    #region To please the XAML viewer 

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Should never be executed 
    public ShellViewModel()
    {
    }
#pragma warning restore CS8618 

    #endregion To please the XAML viewer 

    public ShellViewModel(
        TranslatorModel translatorModel,
        TranslatorService translatorService, 
        ILocalizer localizer, IToaster toaster, IMessenger messenger)
    {
        this.translatorModel = translatorModel;
        this.translatorService = translatorService; 
        this.localizer = localizer;
        this.toaster = toaster;
        this.messenger = messenger;

        //this.Messenger.Subscribe<ViewActivationMessage>(this.OnViewActivation);
        //this.Messenger.Subscribe<ToolbarCommandMessage>(this.OnToolbarCommand);
        this.Messenger.Subscribe<LanguageChangedMessage>(this.OnLanguageChanged);
    }

    private void OnLanguageChanged(LanguageChangedMessage message)
    {
    }

    // private void OnToolbarCommand(ToolbarCommandMessage _) => this.rotatorTimer.Reset();

    protected override void OnViewLoaded()
    {
        this.Logger.Debug("OnViewLoaded begins");

        base.OnViewLoaded();
        if (this.View is null)
        {
            throw new Exception("Failed to startup...");
        }

        // Select default language 
        string preferredLanguage = this.translatorModel.Language;
        this.Logger.Debug("Language: " + preferredLanguage);
        this.localizer.SelectLanguage(preferredLanguage);

        this.Logger.Debug("OnViewLoaded language loaded");

        // Create all statics views and bind them 
        ShellViewModel.SetupWorkflow();
        this.Logger.Debug("OnViewLoaded SetupWorkflow complete");


        this.Logger.Debug("OnViewLoaded SetupTrayIcon complete");

        // Ready 
        this.toaster.Host = this.View.ToasterHost;
        if (true)
        {
            this.toaster.Show(
                this.localizer.Lookup("Shell.Ready"), this.localizer.Lookup("Shell.Greetings"),
                3_000, InformationLevel.Info);
        }

        // Delay a bit the launch of the gallery so that there is time to ping 
        this.Logger.Debug("OnViewLoaded: Internet connected: " + this.translatorModel.IsInternetConnected);
        Schedule.OnUiThread(100, this.ActivateInitialView, DispatcherPriority.Background);

        this.Logger.Debug("OnViewLoaded complete");
    }

    private async void ActivateInitialView()
    {
        var result = await this.translatorService.Translate(
            ProviderKey.Google, "Hello! Have you been able to complete the translation?", "en", "es");
        if (result is not null && result.Item1)
        {
            Debug.WriteLine(result.Item2);
        }

        Dictionary<string, string> dictionary = new Dictionary<string, string>()
        {
            { "key1" , "Hello! Have you been able to complete the translation?"},
            { "key2" , "Not yet! But it will be finished tonight."},
        }; 

        var results = await this.translatorService.BatchTranslate(ProviderKey.Google, dictionary, "en", "it");
        if (results is not null && results.Item1 && results.Item2.Count > 0)
        {
            foreach (var item in results.Item2)
            {
                Debug.WriteLine(item.Key + ":  " + item.Value);
            }
        }

        string sourcePath =
            @"C:\Users\Laurent\source\repos\Lyt.Avalonia.Translator\Lang_en-US - Copy.axaml";
        var fileResults = await this.translatorModel.TranslateAxamlResourceFile(
            ProviderKey.Google, sourcePath, "en", "fr");

        Debugger.Break();

        //if (this.translatorModel.IsFirstRun)
        //{
        //    this.OnViewActivation(ActivatedView.Intro, parameter: null, isFirstActivation: true);
        //}
        //else
        //{
        //    int retries = 3;
        //    while (retries > 0)
        //    {
        //        this.Logger.Debug("ActivateInitialView: Internet connected: " + this.translatorModel.IsInternetConnected);
        //        if (this.translatorModel.IsInternetConnected)
        //        {
        //            this.OnViewActivation(ActivatedView.Gallery, parameter: null, isFirstActivation: true);
        //            this.Logger.Debug("OnViewLoaded OnViewActivation complete");
        //            return;
        //        }

        //        await Task.Delay(100);
        //        --retries;
        //    }
        //}

        //this.Logger.Debug("OnViewLoaded OnViewActivation complete");
    }

    //private void OnModelUpdated(ModelUpdateMessage message)
    //{
    //    string msgProp = string.IsNullOrWhiteSpace(message.PropertyName) ? "<unknown>" : message.PropertyName;
    //    string msgMethod = string.IsNullOrWhiteSpace(message.MethodName) ? "<unknown>" : message.MethodName;
    //    this.Logger.Debug("Model update, property: " + msgProp + " method: " + msgMethod);
    //}

    //private void OnViewActivation(ViewActivationMessage message)
    //    => this.OnViewActivation(message.View, message.ActivationParameter, false);

    //private void OnViewActivation(ActivatedView activatedView, object? parameter = null, bool isFirstActivation = false)
    //{
    //    Bindable? CurrentViewModel()
    //    {
    //        object? currentView = this.View.ShellViewContent.Content;
    //        if (currentView is Control control &&
    //            control.DataContext is Bindable currentViewModel)
    //        {
    //            return currentViewModel;
    //        }

    //        return null;
    //    }

    //    // Navigation also reset the wallpaper rotation timer
    //    this.rotatorTimer.Reset();

    //    if (activatedView == ActivatedView.Exit)
    //    {
    //        OnExit();
    //    }

    //    if (activatedView == ActivatedView.GoBack)
    //    {
    //        // We always go back to the Intro View 
    //        activatedView = ActivatedView.Intro;
    //    }

    //    bool programmaticNavigation = false;
    //    ActivatedView hasBeenActivated = ActivatedView.Exit;
    //    Bindable? currentViewModel = null;
    //    if (parameter is bool navigationType)
    //    {
    //        programmaticNavigation = navigationType;
    //        currentViewModel = CurrentViewModel();
    //    }

    //    switch (activatedView)
    //    {
    //        default:
    //        case ActivatedView.Gallery:
    //            if (!(programmaticNavigation && currentViewModel is GalleryViewModel))
    //            {
    //                this.SetupToolbar<GalleryToolbarViewModel, GalleryToolbarView>();
    //                this.Activate<GalleryViewModel, GalleryView>(isFirstActivation, null);
    //                hasBeenActivated = ActivatedView.Gallery;
    //            }
    //            break;

    //        case ActivatedView.Collection:
    //            if (!(programmaticNavigation && currentViewModel is CollectionViewModel))
    //            {
    //                this.SetupToolbar<CollectionToolbarViewModel, CollectionToolbarView>();
    //                this.Activate<CollectionViewModel, CollectionView>(isFirstActivation, null);
    //                hasBeenActivated = ActivatedView.Collection;
    //            }
    //            break;

    //        case ActivatedView.Language:
    //            // No toolbar
    //            this.View.ShellViewToolbar.Content = null;
    //            this.Activate<LanguageViewModel, LanguageView>(isFirstActivation, null);
    //            break;

    //        case ActivatedView.Intro:
    //            this.SetupToolbar<IntroToolbarViewModel, IntroToolbarView>();
    //            this.Activate<IntroViewModel, IntroView>(isFirstActivation, null);
    //            break;

    //        case ActivatedView.Settings:
    //            if (!(programmaticNavigation && currentViewModel is SettingsViewModel))
    //            {
    //                this.SetupToolbar<SettingsToolbarViewModel, SettingsToolbarView>();
    //                this.Activate<SettingsViewModel, SettingsView>(isFirstActivation, parameter);
    //                hasBeenActivated = ActivatedView.Settings;
    //            }
    //            break;
    //    }

    //    // Reflect in the navigation toolbar the programmatic change 
    //    if (programmaticNavigation && (hasBeenActivated != ActivatedView.Exit))
    //    {
    //        if (this.View is not ShellView view)
    //        {
    //            throw new Exception("No view: Failed to startup...");
    //        }

    //        var selector = view.SelectionGroup;
    //        var button = hasBeenActivated switch
    //        {
    //            ActivatedView.Intro => view.IntroButton,
    //            ActivatedView.Collection => view.CollectionButton,
    //            ActivatedView.Settings => view.SettingsButton,
    //            _ => view.TodayButton,
    //        };
    //        selector.Select(button);
    //    }

    //    this.MainToolbarIsVisible = CurrentViewModel() is not IntroViewModel;
    //}

    private static async void OnExit()
    {
        var application = App.GetRequiredService<IApplicationBase>();
        await application.Shutdown();
    }

    private void SetupToolbar<TViewModel, TControl>()
        where TViewModel : Bindable<TControl>
        where TControl : Control, new()
    {
        if (this.View is null)
        {
            throw new Exception("No view: Failed to startup...");
        }

        var newViewModel = App.GetRequiredService<TViewModel>();
        this.View.ShellViewToolbar.Content = newViewModel.View;
    }

    private void Activate<TViewModel, TControl>(bool isFirstActivation, object? activationParameters)
        where TViewModel : Bindable<TControl>
        where TControl : Control, new()
    {
        if (this.View is null)
        {
            throw new Exception("No view: Failed to startup...");
        }

        var newViewModel = App.GetRequiredService<TViewModel>();
        object? currentView = this.View.ShellViewContent.Content;
        if (currentView is Control control && control.DataContext is Bindable currentViewModel)
        {
            if (newViewModel == currentViewModel)
            {
                return;
            }

            currentViewModel.Deactivate();
        }


        newViewModel.Activate(activationParameters);
        this.View.ShellViewContent.Content = newViewModel.View;
        if (!isFirstActivation)
        {
            this.Profiler.MemorySnapshot(newViewModel.View.GetType().Name + ":  Activated");
        }
    }

    private static void SetupWorkflow()
    {
        static void CreateAndBind<TViewModel, TControl>()
             where TViewModel : Bindable<TControl>
             where TControl : Control, new()
        {
            var vm = App.GetRequiredService<TViewModel>();
            vm.CreateViewAndBind();
        }

        //CreateAndBind<GalleryViewModel, GalleryView>();
        //CreateAndBind<GalleryToolbarViewModel, GalleryToolbarView>();
        //CreateAndBind<CollectionViewModel, CollectionView>();
        //CreateAndBind<CollectionToolbarViewModel, CollectionToolbarView>();
        //CreateAndBind<IntroViewModel, IntroView>();
        //CreateAndBind<IntroToolbarViewModel, IntroToolbarView>();
        //CreateAndBind<LanguageViewModel, LanguageView>();
        //CreateAndBind<SettingsViewModel, SettingsView>();
        //CreateAndBind<SettingsToolbarViewModel, SettingsToolbarView>();
    }

#pragma warning disable IDE0079 
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

    //private void OnToday(object? _) => this.OnViewActivation(ActivatedView.Gallery);

    //private void OnCollection(object? _) => this.OnViewActivation(ActivatedView.Collection);

    //private void OnSettings(object? _) => this.OnViewActivation(ActivatedView.Settings);

    //private void OnInfo(object? _) => this.OnViewActivation(ActivatedView.Intro);

    //private void OnLanguage(object? _) => this.OnViewActivation(ActivatedView.Language);

    private void OnClose(object? _) => OnExit();

#pragma warning restore CA1822
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079

    //public ICommand TodayCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    //public ICommand CollectionCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    //public ICommand SettingsCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    //public ICommand InfoCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    //public ICommand LanguageCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public ICommand CloseCommand { get => this.Get<ICommand>()!; set => this.Set(value); }

    public bool MainToolbarIsVisible { get => this.Get<bool>()!; set => this.Set(value); }
}
