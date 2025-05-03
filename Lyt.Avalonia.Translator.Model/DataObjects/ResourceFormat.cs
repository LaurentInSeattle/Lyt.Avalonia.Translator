namespace Lyt.Avalonia.Translator.Model.DataObjects;

public enum ResourceFormat
{
    Unknown = 0, 
    Axaml , // Avalonia Xaml resource file format 
    Resx,   // Good old WinForms 
}

public static class ResourceFormats
{
    public static string ToFriendlyName(this ResourceFormat resourceFormat)
        => resourceFormat switch
        {
            // No need to localize 
            ResourceFormat.Axaml => "Avalonia  .axaml",
            ResourceFormat.Resx => "Microsoft  .resx",
            _ => throw new ArgumentException(nameof(resourceFormat)),
        };

    public static List<ResourceFormat> Available()
        => [ResourceFormat.Axaml, ResourceFormat.Resx]; 
}