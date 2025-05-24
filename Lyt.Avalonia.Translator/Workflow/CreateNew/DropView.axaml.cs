namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

using static MessagingExtensions;
using static Lyt.Avalonia.Controls.Utilities; 

public partial class DropView : UserControl
{
    private static readonly SolidColorBrush? normalBrush; 
    private static readonly SolidColorBrush? hotBrush;

    static DropView ()
    {
        TryFindResource<SolidColorBrush>("LightAqua_0_120", out SolidColorBrush? brush);
        if (brush is not null) 
        {
            normalBrush = brush; 
        }
        
        TryFindResource<SolidColorBrush>("OrangePeel_0_100", out brush);
        if (brush is not null)
        {
            hotBrush = brush;
        }        
    }

    public DropView()
    {
        this.InitializeComponent();
        if (normalBrush is not null)
        {
            this.DropRectangle.Stroke = normalBrush;
        }

        DragDrop.SetAllowDrop(this.DropBorder, true);
        this.DropBorder.AddHandler(DragDrop.DropEvent, this.OnDrop);
        this.DropBorder.AddHandler(DragDrop.DragEnterEvent, this.OnDragEnter);
        this.DropBorder.AddHandler(DragDrop.DragLeaveEvent, this.OnDragLeave);
    }

    ~DropView()
    {
        DragDrop.SetAllowDrop(this.DropBorder, false);
        this.DropBorder.RemoveHandler(DragDrop.DropEvent, this.OnDrop);
        this.DropBorder.RemoveHandler(DragDrop.DragEnterEvent, this.OnDragEnter);
        this.DropBorder.RemoveHandler(DragDrop.DragLeaveEvent, this.OnDragLeave);
    }

    private void OnDragEnter(object? _, DragEventArgs e)
    {
        if (hotBrush is not null)
        {
            this.DropRectangle.Stroke = hotBrush;
        }
    }

    private void OnDragLeave(object? _, DragEventArgs e)
    {
        if (normalBrush is not null)
        {
            this.DropRectangle.Stroke = normalBrush;
        }
    }

    private void OnDrop(object? _, DragEventArgs dragEventArgs)
    {
        try
        {
            if (normalBrush is not null)
            {
                this.DropRectangle.Stroke = normalBrush;
            }

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