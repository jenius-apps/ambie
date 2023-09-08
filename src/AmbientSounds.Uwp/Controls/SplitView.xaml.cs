using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class SplitView : UserControl
{
    public static readonly DependencyProperty PaneProperty = DependencyProperty.Register(
        nameof(Pane),
        typeof(UIElement),
        typeof(SplitView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register(
        nameof(MainContent),
        typeof(UIElement),
        typeof(SplitView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty PaneBackgroundProperty = DependencyProperty.Register(
        nameof(PaneBackground),
        typeof(Brush),
        typeof(SplitView),
        new PropertyMetadata(null));


    public static readonly DependencyProperty OpenPaneLengthProperty = DependencyProperty.Register(
        nameof(OpenPaneLength), 
        typeof(GridLength), 
        typeof(SplitView), 
        new PropertyMetadata(GridLength.Auto));

    public SplitView()
    {
        this.InitializeComponent();
    }

    public GridLength OpenPaneLength
    {
        get => (GridLength)GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public Brush? PaneBackground
    {
        get => (Brush)GetValue(PaneBackgroundProperty);
        set => SetValue(PaneBackgroundProperty, value);
    }

    public UIElement? Pane
    {
        get => (UIElement)GetValue(PaneProperty);
        set => SetValue(PaneProperty, value);
    }

    public UIElement? MainContent
    {
        get => (UIElement)GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }
}
