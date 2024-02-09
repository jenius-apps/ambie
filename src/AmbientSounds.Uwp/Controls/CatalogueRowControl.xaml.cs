using AmbientSounds.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class CatalogueRowControl : UserControl
{
    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(
            nameof(TitleText),
            typeof(string),
            typeof(CatalogueRowControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(CatalogueRowControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SoundsVisibleProperty =
        DependencyProperty.Register(
            nameof(SoundsVisible),
            typeof(bool),
            typeof(CatalogueRowControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SparkleIconVisibleProperty =
        DependencyProperty.Register(
            nameof(SparkleIconVisible),
            typeof(bool),
            typeof(CatalogueRowControl),
            new PropertyMetadata(false));

    public CatalogueRowControl()
    {
        this.InitializeComponent();
    }

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public bool SoundsVisible
    {
        get => (bool)GetValue(SoundsVisibleProperty);
        set => SetValue(SoundsVisibleProperty, value);
    }

    public bool SparkleIconVisible
    {
        get => (bool)GetValue(SparkleIconVisibleProperty);
        set => SetValue(SparkleIconVisibleProperty, value);
    }

    private void OnGridTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement g && g.DataContext is OnlineSoundViewModel vm)
        {
            _ = vm.PrimaryActionCommand.ExecuteAsync(null);
        }
    }
}
