namespace Lyt.Avalonia.Translator.Workflow.CreateNew;

public partial class DropView : UserControl
{
    public DropView()
    {
        this.InitializeComponent();

        this.DataContextChanged += this.OnDataContextChanged;
        DragDrop.SetAllowDrop(this.DropBorder, true);
        this.AddHandler(DragDrop.DropEvent, this.OnDrop);
    }

    ~DropView()
    {
        DragDrop.SetAllowDrop(this.DropBorder, false);
        this.DropBorder.RemoveHandler(DragDrop.DropEvent, this.OnDrop);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (this.DataContext is DropViewModel dropViewModel)
        {
            dropViewModel.BindOnDataContextChanged(this);
        }
    }

    private void OnDrop(object? sender, DragEventArgs dragEventArgs)
    {
        IDataObject data = dragEventArgs.Data;
        var files = data.GetFiles();
        if (files is not null)
        {
            foreach (IStorageItem file in files)
            {
                string path = file.Path.LocalPath;
                Debug.WriteLine("Dropped: " + path);
                if (File.Exists(path))
                {
                    if (this.DataContext is DropViewModel dropViewModel)
                    {
                        if (dropViewModel.OnDrop(path))
                        {
                            break; 
                        } 
                    }
                }
            }
        }

        dragEventArgs.Handled = true;
    }
}