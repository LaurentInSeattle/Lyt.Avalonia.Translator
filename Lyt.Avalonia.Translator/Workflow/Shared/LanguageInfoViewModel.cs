namespace Lyt.Avalonia.Translator.Workflow.Shared;

public sealed class LanguageInfoViewModel : Bindable<LanguageInfoView>
{
    private const string UriPath = "avares://Lyt.Avalonia.Translator/Assets/Images/Flags/";
    private const string Extension = ".png";

    public LanguageInfoViewModel(string key, string name, string flagOne, string? flagTwo)
    {
        this.Key = key;
        this.Name = name;
        this.FlagOne = new Bitmap(AssetLoader.Open(new Uri(UriPath + flagOne + Extension)));
        this.FlagTwo =
            string.IsNullOrWhiteSpace(flagTwo) ?
                null :
                new Bitmap(AssetLoader.Open(new Uri(UriPath + flagTwo + Extension)));
    }

    public string Key { get => this.Get<string>()!; set => this.Set(value); }

    public string Name { get => this.Get<string>()!; set => this.Set(value); }

    public Bitmap FlagOne { get => this.Get<Bitmap>()!; set => this.Set(value); }

    public Bitmap? FlagTwo { get => this.Get<Bitmap>()!; set => this.Set(value); }
}
