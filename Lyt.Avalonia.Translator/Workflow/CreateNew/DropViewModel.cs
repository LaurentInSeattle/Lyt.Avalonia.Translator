namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

public sealed class DropViewModel : Bindable<DropView>
{
    /// <summary> Returns true if the path is a valid language file. </summary>
    internal bool OnDrop(string path)
    {
        try             
        {

            throw new Exception("Failed to load language file: " + path); 
        }
        catch (Exception ex) 
        { 
            this.Logger.Warning(ex.ToString());
            return false;
        }
    }
}
