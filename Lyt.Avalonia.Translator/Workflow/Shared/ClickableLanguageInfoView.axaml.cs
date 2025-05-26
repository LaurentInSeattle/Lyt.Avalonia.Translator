namespace Lyt.Avalonia.Translator.Workflow.Shared;

using static Avalonia.Controls.Utilities;

public partial class ClickableLanguageInfoView : UserControl, IView
{
    private static readonly SolidColorBrush insideBrush;
    private static readonly SolidColorBrush pressedBrush;

    private bool isInside;
    private bool isPressed;

    static ClickableLanguageInfoView()
    {
        insideBrush = FindResource<SolidColorBrush>("OrangePeel_0_100");
        pressedBrush = FindResource<SolidColorBrush>("OrangePeel_1_100");
    }

    public ClickableLanguageInfoView()
    {
        this.InitializeComponent();
        this.PointerEntered += this.OnPointerEnter;
        this.PointerExited += this.OnPointerLeave;
        this.PointerPressed += this.OnPointerPressed;
        this.PointerReleased += this.OnPointerReleased;
        this.PointerMoved += this.OnPointerMoved;
        this.DataContextChanged += this.OnDataContextChanged;
        this.SetVisualState();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (this.DataContext is ClickableLanguageInfoViewModel thumbnailViewModel)
        {
            thumbnailViewModel.BindOnDataContextChanged(this);
        }
    }

    ~ClickableLanguageInfoView()
    {
        this.PointerEntered -= this.OnPointerEnter;
        this.PointerExited -= this.OnPointerLeave;
        this.PointerPressed -= this.OnPointerPressed;
        this.PointerReleased -= this.OnPointerReleased;
        this.DataContextChanged -= this.OnDataContextChanged;
    }

    private void OnPointerEnter(object? sender, PointerEventArgs args)
    {
        if ((sender is ClickableLanguageInfoView view) && (this == view))
        {
            this.isInside = true;
            this.SetVisualState();
        }
    }

    private void OnPointerLeave(object? sender, PointerEventArgs args)
    {
        if ((sender is ClickableLanguageInfoView view) && (this == view))
        {
            this.isInside = false;
            this.SetVisualState();
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs args)
    {
        if (!this.isInside)
        {
            return;
        }

        this.isInside = this.outerBorder.IsPointerInside(args);
        this.SetVisualState();
    }

    private void OnPointerPressed(object? sender, PointerEventArgs args)
    {
        if ((sender is ClickableLanguageInfoView view) && (this == view))
        {
            this.isPressed = true;
            this.SetVisualState();
        }
    }

    private void OnPointerReleased(object? sender, PointerEventArgs args)
    {
        bool wasInside = this.isInside;
        this.isPressed = false;
        if ((sender is ClickableLanguageInfoView view) && (this == view))
        {
            if (wasInside && this.DataContext is ClickableLanguageInfoViewModel viewModel)
            {
                this.isInside = false;
                this.SetVisualState();
                viewModel.OnSelect();
            }
        }
    }

    private void SetVisualState()
    {
        bool visible = this.isInside;
        this.outerBorder.BorderThickness = new Thickness(visible ? 1.0 : 0.0);
        this.outerBorder.BorderBrush = this.isPressed ? pressedBrush : insideBrush;
    }
}