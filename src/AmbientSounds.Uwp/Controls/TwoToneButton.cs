using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed class TwoToneButton : Button
{
    public static readonly DependencyProperty SecondaryContentProperty =
        DependencyProperty.Register(
            nameof(SecondaryContent),
            typeof(object),
            typeof(TwoToneButton),
            new PropertyMetadata(null));

    public TwoToneButton()
    {
        this.DefaultStyleKey = typeof(TwoToneButton);
    }

    public object? SecondaryContent
    {
        get => GetValue(SecondaryContentProperty);
        set => SetValue(SecondaryContentProperty, value);
    }
}
