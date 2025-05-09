namespace Lyt.Avalonia.Translator.Workflow.RunProject;

public sealed class ExtLanguageInfoViewModel : Bindable<ExtLanguageInfoView>
{
    private const string UriPath = "avares://Lyt.Avalonia.Translator/Assets/Images/Flags/";
    private const string Extension = ".png";
    private readonly Language language;

    private int missing; 

    public ExtLanguageInfoViewModel(Language language)
    {
        this.language = language;
        string key = language.CultureKey;
        string name = language.LocalName;
        string flagOne = language.PrimaryFlag;
        string? flagTwo = language.SecondaryFlag;

        static Bitmap? From(string? flag)
            => string.IsNullOrWhiteSpace(flag) ?
                    null :
                    new Bitmap(AssetLoader.Open(new Uri(string.Concat(UriPath, flag, Extension))));

        this.DisablePropertyChangedLogging = true;
        this.Key = key;
        this.Name = name;

        // If we have only one flag we place it on the right 
        if (flagTwo is null)
        {
            this.FlagOne = null;
            this.FlagTwo = From(flagOne);
        }
        else
        {
            this.FlagOne = From(flagOne);
            this.FlagTwo = From(flagTwo);
        }
    }

    public void SetComplete(int missing)
    {
        this.missing = missing;
        this.IsInProgress = false;
        this.UpdateComplete();
    }

    public void InProgress()
    {
        this.IsComplete = false;
        this.IsInProgress = true; 
        this.Status = this.Localizer.Lookup("RunProject.LanguageInProgress");
    }

    public void TranslationAdded ()
    {
        if ( this.missing <= 0)
        {
            return; 
        }

        --this.missing;
        this.UpdateComplete();
    }

    private void UpdateComplete()
    {
        bool complete = this.missing == 0;
        this.IsComplete = complete;
        if (complete)
        {
            this.IsInProgress = false;
        } 

        string missingFormat = this.Localizer.Lookup("RunProject.Incomplete");
        this.Status =
            complete ?
                this.Localizer.Lookup("RunProject.Complete") :
                string.Format(missingFormat, this.missing);
    }

    public Language Language => this.language;

    public string Key { get => this.Get<string>()!; set => this.Set(value); }

    public string Name { get => this.Get<string>()!; set => this.Set(value); }

    public Bitmap? FlagOne { get => this.Get<Bitmap>(); set => this.Set(value); }

    public Bitmap? FlagTwo { get => this.Get<Bitmap>(); set => this.Set(value); }

    public string Status { get => this.Get<string>()!; set => this.Set(value); }

    public bool IsComplete { get => this.Get<bool>(); set => this.Set(value); }

    public bool IsInProgress { get => this.Get<bool>(); set => this.Set(value); }
}
