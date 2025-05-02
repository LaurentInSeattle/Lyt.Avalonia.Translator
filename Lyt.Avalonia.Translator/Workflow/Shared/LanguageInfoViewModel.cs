namespace Lyt.Avalonia.Translator.Workflow.Shared;

public sealed class LanguageInfoViewModel : Bindable<LanguageInfoView>
{
    private const string UriPath = "avares://Lyt.Avalonia.Translator/Assets/Images/Flags/";
    private const string Extension = ".png";
    private readonly Language language;

    public LanguageInfoViewModel(Language language)
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
        if (flagTwo is null )
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

    public Language Language => this.language;

    public string Key { get => this.Get<string>()!; set => this.Set(value); }

    public string Name { get => this.Get<string>()!; set => this.Set(value); }

    public Bitmap? FlagOne { get => this.Get<Bitmap>(); set => this.Set(value); }

    public Bitmap? FlagTwo { get => this.Get<Bitmap>(); set => this.Set(value); }
}
