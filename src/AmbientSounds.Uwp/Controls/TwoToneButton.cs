using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed class TwoToneButton : Button
{
    public static readonly DependencyProperty PrimaryTextProperty =
        DependencyProperty.Register(
            "PrimaryText",
            typeof(string),
            typeof(TwoToneButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SecondaryTextProperty =
        DependencyProperty.Register(
            "SecondaryText",
            typeof(string),
            typeof(TwoToneButton),
            new PropertyMetadata(string.Empty));

    public TwoToneButton()
    {
        this.DefaultStyleKey = typeof(TwoToneButton);
    }

    public string PrimaryText
    {
        get => (string)GetValue(PrimaryTextProperty);
        set => SetValue(PrimaryTextProperty, value);
    }

    public string SecondaryText
    {
        get => (string)GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }
}
