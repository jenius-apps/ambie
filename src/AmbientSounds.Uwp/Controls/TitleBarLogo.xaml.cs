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

    public TitleBarLogo()
    {
        this.InitializeComponent();
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }
}
