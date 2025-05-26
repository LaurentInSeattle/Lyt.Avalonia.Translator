namespace Lyt.Avalonia.Translator.Workflow.Projects;

public partial class ProjectTileView : UserControl, IView
{
    public ProjectTileView()
    {
        this.InitializeComponent();
        this.Height = 92.0;
        this.PointerEntered += this.OnPointerEnter;
        this.PointerExited += this.OnPointerLeave;
        this.SetVisible(visible: false);
    }

    ~ProjectTileView()
    {
        this.PointerEntered -= this.OnPointerEnter;
        this.PointerExited -= this.OnPointerLeave;
    }

    private void OnPointerEnter(object? sender, PointerEventArgs args)
    {
        if ((sender is ProjectTileView view) && (this == view))
        {
            this.SetVisible();
        }
    }

    private void OnPointerLeave(object? sender, PointerEventArgs args)
    {
        if ((sender is ProjectTileView view) && (this == view))
        {
            this.SetVisible(visible: false);
        }
    }

    private void SetVisible(bool visible = true)
    {
        this.outerBorder.BorderThickness = new Thickness(visible ? 1.0 : 0.0);
        this.openButton.IsVisible = visible;
        this.deleteButton.IsVisible = visible;
    }
}