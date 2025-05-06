namespace Lyt.Avalonia.Translator.Messaging;

public sealed record class ToolbarCommandMessage(
    ToolbarCommandMessage.ToolbarCommand Command, object? CommandParameter = null)
{
    public enum ToolbarCommand
    {
        // Left - Main toolbar in Shell view 

        // Right - Main toolbar in Shell view  
        Close,

        // CreateNew toolbar
        CreateNewAddAllLanguages,
        CreateNewClearAllLanguages,
        CreateNewSaveProject,
        RunProject,
        StopProject,

        // Etc... Settings toolbars 
    }
}
