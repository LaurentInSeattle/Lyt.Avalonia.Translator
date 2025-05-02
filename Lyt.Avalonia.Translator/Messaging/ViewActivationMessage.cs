namespace Lyt.Avalonia.Translator.Messaging;

public sealed record class ViewActivationMessage(
    ViewActivationMessage.ActivatedView View, object? ActivationParameter = null)
{
    public enum ActivatedView
    {
        Intro,
        Interactive,
        CreateNew,
        Language,
        Project, 

        GoBack,
        Exit,
    }
}
