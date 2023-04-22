using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AmbientSounds.Controls;

public sealed partial class TitleBarLogo : UserControl
{
    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(
            nameof(DisplayText),
            typeof(string),
            typeof(TitleBarLogo),
            new PropertyMetadata(Strings.Resources.AppDisplayName));

    public static readonly DependencyProperty DisplayTextForegroundBrushProperty =
        DependencyProperty.Register(nameof(DisplayTextForegroundBrush),
            typeof(Brush),
            typeof(TitleBarLogo),
            null);

    public TitleBarLogo()
    {
        this.InitializeComponent();
        Window.Current.Activated += Window_Activated;
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public Brush DisplayTextForegroundBrush
    {
        get => (Brush)GetValue(DisplayTextForegroundBrushProperty);
        set => SetValue(DisplayTextForegroundBrushProperty, value);
    }

    private void SetDisplayTextForeground(bool isWindowFocused)
    {
        DisplayTextForegroundBrush = App.Current.Resources[isWindowFocused ?
            "TextFillColorPrimaryBrush" : "TextFillColorTertiaryBrush"] as Brush;
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs e)
    {
        SetDisplayTextForeground(e.WindowActivationState != CoreWindowActivationState.Deactivated);
    }

    private void UserControl_ActualThemeChanged(FrameworkElement sender, object args)
    {
        SetDisplayTextForeground(Window.Current.CoreWindow.ActivationMode != CoreWindowActivationMode.Deactivated);
    }
}
