namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

using static MessagingExtensions; 
public partial class DropView : UserControl
{
    public DropView()
    {
        this.InitializeComponent();
        DragDrop.SetAllowDrop(this.DropBorder, true);
        this.AddHandler(DragDrop.DropEvent, this.OnDrop);
    }

    ~DropView()
    {
        DragDrop.SetAllowDrop(this.DropBorder, false);
        this.DropBorder.RemoveHandler(DragDrop.DropEvent, this.OnDrop);
    }

    private void OnDrop(object? sender, DragEventArgs dragEventArgs)
    {
        try
        {

            IDataObject data = dragEventArgs.Data;
            var files = data.GetFiles();
            if (files is not null)
            {
                bool success = false;
                foreach (IStorageItem file in files)
                {
                    string path = file.Path.LocalPath;
                    Debug.WriteLine("Dropped: " + path);
                    if (File.Exists(path))
                    {
                        success = true;
                        Publish(new DropFileMessage(Success: true, Data: path));
                        break;
                    }
                }

                if (!success)
                {
                    Publish(new DropFileMessage(Success: false, Data: "CreateNew.FileDoesNotExist"));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OnDrop: Exception: " + ex);
            Publish(new DropFileMessage(Success: false, Data: "CreateNew.Exception"));
        }

        dragEventArgs.Handled = true;
    }
}