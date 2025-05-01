namespace Lyt.Avalonia.Translator.Workflow.Shared;

public sealed class LanguageInfoViewModel : Bindable<LanguageInfoView>
{
    private const string UriPath = "avares://Lyt.Avalonia.Translator/Assets/Images/Flags/";
    private const string Extension = ".png";

    public LanguageInfoViewModel(string key, string name, string flagOne, string? flagTwo)
    {
        static Bitmap? From(string? flag)
            => string.IsNullOrWhiteSpace(flag) ?
                    null :
                    new Bitmap(AssetLoader.Open(new Uri(string.Concat(UriPath, flag, Extension))));

        this.DisablePropertyChangedLogging = true;
        this.Key = key;
        this.Name = name;
        this.FlagOne = From(flagOne);
        this.FlagTwo = From(flagTwo);
    }

    public string Key { get => this.Get<string>()!; set => this.Set(value); }

    public string Name { get => this.Get<string>()!; set => this.Set(value); }

    public Bitmap? FlagOne { get => this.Get<Bitmap>(); set => this.Set(value); }

    public Bitmap? FlagTwo { get => this.Get<Bitmap>(); set => this.Set(value); }
}
