namespace Lyt.Avalonia.Translator.Shell;

using static MessagingExtensions;
using static ViewActivationMessage;

public sealed partial class ShellViewModel : ViewModel<ShellView>
{
    private const int MinutesToMillisecs = 60 * 1_000;

    private readonly TranslatorModel translatorModel;
    private readonly TranslatorService translatorService;
    private readonly IToaster toaster;

    [ObservableProperty]
    private bool isInternetConnected;

    [ObservableProperty]
    private bool mainToolbarIsVisible;

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
        IToaster toaster)
    {
        this.translatorModel = translatorModel;
        this.translatorService = translatorService; 
        this.toaster = toaster;

        this.toaster.BreakOnError = false; 
        this.Messenger.Subscribe<ToolbarCommandMessage>(this.OnToolbarCommand);
        this.Messenger.Subscribe<ViewActivationMessage>(this.OnViewActivation);
        this.Messenger.Subscribe<LanguageChangedMessage>(this.OnLanguageChanged);
        this.Messenger.Subscribe<ModelUpdateMessage>(this.OnModelUpdate);
        
    }

    private void OnModelUpdate(ModelUpdateMessage message) 
    {
        if (message.PropertyName == nameof(this.translatorModel.IsInternetConnected))
        {
            this.IsInternetConnected = this.translatorModel.IsInternetConnected; 
        }
    }

    private void OnLanguageChanged(LanguageChangedMessage _)
    {
    }

    
    private void OnToolbarCommand(ToolbarCommandMessage _)
    {
    }

    public override void OnViewLoaded()
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
        this.Localizer.SelectLanguage(preferredLanguage);

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
                this.Localizer.Lookup("Shell.Ready"), this.Localizer.Lookup("Shell.Greetings"),
                3_000, InformationLevel.Info);
        }

        // Delay a bit the launch of the gallery so that there is time to ping 
        this.Logger.Debug("OnViewLoaded: Internet connected: " + this.translatorModel.IsInternetConnected);
        Schedule.OnUiThread(100, this.ActivateInitialView, DispatcherPriority.Background);

        this.Logger.Debug("OnViewLoaded complete");
    }

    private /* async */ void ActivateInitialView()
    {
        this.Logger.Debug("ActivateInitialView: Internet connected: " + this.translatorModel.IsInternetConnected);
        this.OnViewActivation(ActivatedView.Interactive, parameter: null, isFirstActivation: true);
        this.Logger.Debug("OnViewLoaded OnViewActivation complete");

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

    private void OnViewActivation(ViewActivationMessage message)
        => this.OnViewActivation(message.View, message.ActivationParameter, false);

    private void OnViewActivation(ActivatedView activatedView, object? parameter = null, bool isFirstActivation = false)
    {
        ViewModel? CurrentViewModel()
        {
            object? currentView = this.View.ShellViewContent.Content;
            if (currentView is Control control &&
                control.DataContext is ViewModel currentViewModel)
            {
                return currentViewModel;
            }

            return null;
        }

        if (activatedView == ActivatedView.Exit)
        {
            OnExit();
        }

        if (activatedView == ActivatedView.GoBack)
        {
            // We always go back to the Intro View 
            activatedView = ActivatedView.Intro;
        }

        bool programmaticNavigation = false;
        ActivatedView hasBeenActivated = ActivatedView.Exit;
        ViewModel? currentViewModel = null;
        if (parameter is bool navigationType)
        {
            programmaticNavigation = navigationType;
            currentViewModel = CurrentViewModel();
        }

        void NoToolbar() => this.View.ShellViewToolbar.Content = null;

        void SetupToolbar<TViewModel, TControl>()
            where TViewModel : ViewModel<TControl>
            where TControl : Control, IView, new()
        {
            if (this.View is null)
            {
                throw new Exception("No view: Failed to startup...");
            }

            var newViewModel = App.GetRequiredService<TViewModel>();
            this.View.ShellViewToolbar.Content = newViewModel.View;
        }

        switch (activatedView)
        {
            default:
            case ActivatedView.Interactive:
                NoToolbar();
                this.Activate<InteractiveViewModel, InteractiveView>(isFirstActivation, null);
                hasBeenActivated = ActivatedView.Interactive;
                break;

            case ActivatedView.Projects:
                if (!(programmaticNavigation && currentViewModel is ProjectsViewModel))
                {
                    NoToolbar();
                    this.Activate<ProjectsViewModel, ProjectsView>(isFirstActivation, null);
                    hasBeenActivated = ActivatedView.Projects;
                }
                break;

            case ActivatedView.RunProject:
                if (!(programmaticNavigation && currentViewModel is RunProjectViewModel))
                {
                    SetupToolbar<RunProjectToolbarViewModel, RunProjectToolbarView>();
                    this.Activate<RunProjectViewModel, RunProjectView>(isFirstActivation, null);
                    hasBeenActivated = ActivatedView.RunProject;
                }
                break;

            case ActivatedView.CreateNew:
                SetupToolbar<CreateNewToolbarViewModel, CreateNewToolbarView>();
                this.Activate<CreateNewViewModel, CreateNewView>(isFirstActivation, null);
                hasBeenActivated = ActivatedView.CreateNew;
                break;

                //case ActivatedView.Intro:
                //    this.SetupToolbar<IntroToolbarViewModel, IntroToolbarView>();
                //    this.Activate<IntroViewModel, IntroView>(isFirstActivation, null);
                //    break;

                //case ActivatedView.Language:
                //    // No toolbar
                //    this.View.ShellViewToolbar.Content = null;
                //    this.Activate<LanguageViewModel, LanguageView>(isFirstActivation, null);
                //    break;

        }

        // Reflect in the navigation toolbar the programmatic change
        if (programmaticNavigation && (hasBeenActivated != ActivatedView.Exit))
        {
            if (this.View is not ShellView view)
            {
                throw new Exception("No view: Failed to startup...");
            }

            var selector = view.SelectionGroup;
            var button = hasBeenActivated switch
            {
                // ActivatedView.Intro => view.IntroButton,
                ActivatedView.Projects => view.ProjectsButton,
                ActivatedView.RunProject => view.RunProjectButton,
                // ActivatedView.Settings => view.SettingsButton,
                _ => view.ProjectsButton,
            };
            selector.Select(button);
        }

        this.MainToolbarIsVisible = true; // CurrentViewModel() is CreateNewViewModel;
    }

    private static async void OnExit()
    {
        var application = App.GetRequiredService<IApplicationBase>();
        await application.Shutdown();
    }

    private void Activate<TViewModel, TControl>(bool isFirstActivation, object? activationParameters)
        where TViewModel : ViewModel<TControl>
        where TControl : Control, IView, new()
    {
        if (this.View is null)
        {
            throw new Exception("No view: Failed to startup...");
        }

        var newViewModel = App.GetRequiredService<TViewModel>();
        object? currentView = this.View.ShellViewContent.Content;
        if (currentView is Control control && control.DataContext is ViewModel currentViewModel)
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
        App.GetRequiredService<InteractiveViewModel>().CreateViewAndBind();
        App.GetRequiredService<CreateNewToolbarViewModel>().CreateViewAndBind();
        App.GetRequiredService<CreateNewViewModel>().CreateViewAndBind();
        App.GetRequiredService<ProjectsViewModel>().CreateViewAndBind();
        App.GetRequiredService<RunProjectViewModel>().CreateViewAndBind();
        App.GetRequiredService<RunProjectToolbarViewModel>().CreateViewAndBind();
    }

    [RelayCommand]
    public void OnTranslate() => this.OnViewActivation(ActivatedView.Interactive);

    [RelayCommand]
    public void OnCreateNew() => this.OnViewActivation(ActivatedView.CreateNew);

    [RelayCommand]
    public void OnProjects() => this.OnViewActivation(ActivatedView.Projects);

    [RelayCommand]
    public void OnRunProject() => this.OnViewActivation(ActivatedView.RunProject);

    //private void OnSettings(object? _) => this.OnViewActivation(ActivatedView.Settings);

    //private void OnInfo(object? _) => this.OnViewActivation(ActivatedView.Intro);

    //private void OnLanguage(object? _) => this.OnViewActivation(ActivatedView.Language);

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822
    // Mark members as static
    // Relay commands cannot be static
    [RelayCommand]
    public void OnClose() => OnExit();
#pragma warning restore CA1822 
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
