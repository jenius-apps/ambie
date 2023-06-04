using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class TitleBarLogo : UserControl
{
    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(
            nameof(DisplayText),
            typeof(string),
            typeof(TitleBarLogo),
            new PropertyMetadata(Strings.Resources.AppDisplayName));

    public static readonly DependencyProperty IsWindowFocusedProperty =
        DependencyProperty.Register(
            nameof(IsWindowFocused),
            typeof(bool), 
            typeof(TitleBarLogo),
            new PropertyMetadata(false));

    public TitleBarLogo()
    {
        this.InitializeComponent();
        Window.Current.Activated += (_, e) =>
        {
            IsWindowFocused = e.WindowActivationState != CoreWindowActivationState.Deactivated;
        };
    }

    public bool IsWindowFocused
    {
        get => (bool)GetValue(IsWindowFocusedProperty);
        set => SetValue(IsWindowFocusedProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }
}
