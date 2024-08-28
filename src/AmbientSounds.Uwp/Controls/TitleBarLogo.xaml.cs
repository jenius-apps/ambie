using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class TitleBarLogo : UserControl
{
    public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
        nameof(DisplayText),
        typeof(string),
        typeof(TitleBarLogo),
        new PropertyMetadata(Strings.Resources.AppDisplayName));

    public static readonly DependencyProperty IsWindowFocusedProperty = DependencyProperty.Register(
        nameof(IsWindowFocused),
        typeof(bool),
        typeof(TitleBarLogo),
        new PropertyMetadata(false));

    public static readonly DependencyProperty TextVisibleProperty = DependencyProperty.Register(
        nameof(TextVisible),
        typeof(Visibility),
        typeof(TitleBarLogo),
        new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty UsePremiumTitleProperty = DependencyProperty.Register(
        nameof(UsePremiumTitle),
        typeof(bool),
        typeof(TitleBarLogo),
        new PropertyMetadata(false));

    public TitleBarLogo()
    {
        this.InitializeComponent();
    }

    public bool UsePremiumTitle
    {
        get => (bool)GetValue(UsePremiumTitleProperty);
        set => SetValue(UsePremiumTitleProperty, value);
    }

    public bool IsWindowFocused
    {
        get => (bool)GetValue(IsWindowFocusedProperty);
        set => SetValue(IsWindowFocusedProperty, value);
    }

    public Visibility TextVisible
    {
        get => (Visibility)GetValue(TextVisibleProperty);
        set => SetValue(TextVisibleProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs e)
    {
        IsWindowFocused = e.WindowActivationState != CoreWindowActivationState.Deactivated;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Window.Current.Activated += Window_Activated;
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        Window.Current.Activated -= Window_Activated;
    }
}
