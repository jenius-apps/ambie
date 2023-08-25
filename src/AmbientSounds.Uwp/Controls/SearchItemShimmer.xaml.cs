using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class SearchItemShimmer : UserControl
{
    public static readonly DependencyProperty TextLengthProperty = DependencyProperty.Register(
        nameof(TextLength),
        typeof(double), 
        typeof(SearchItemShimmer),
        new PropertyMetadata(200));

    public SearchItemShimmer()
    {
        this.InitializeComponent();
    }

    public double TextLength
    {
        get => (double)GetValue(TextLengthProperty);
        set => SetValue(TextLengthProperty, value);
    }
}
