using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class AssetRowControl : UserControl
{
    public static readonly DependencyProperty TitleMarginProperty =
        DependencyProperty.Register(
            nameof(TitleMargin),
            typeof(Thickness),
            typeof(AssetRowControl),
            new PropertyMetadata(new Thickness(0)));

    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(
            nameof(TitleText),
            typeof(string),
            typeof(AssetRowControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(AssetRowControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ItemsVisibleProperty =
        DependencyProperty.Register(
            nameof(ItemsVisible),
            typeof(bool),
            typeof(AssetRowControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SparkleIconVisibleProperty =
        DependencyProperty.Register(
            nameof(SparkleIconVisible),
            typeof(bool),
            typeof(AssetRowControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(AssetRowControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ItemsPanelProperty =
        DependencyProperty.Register(
            nameof(ItemsPanel),
            typeof(ItemsPanelTemplate),
            typeof(AssetRowControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShimmerHeightProperty =
        DependencyProperty.Register(
            nameof(ShimmerHeight),
            typeof(double),
            typeof(AssetRowControl),
            new PropertyMetadata(256d));

    public AssetRowControl()
    {
        this.InitializeComponent();
    }

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public Thickness TitleMargin
    {
        get => (Thickness)GetValue(TitleMarginProperty);
        set => SetValue(TitleMarginProperty, value);
    }

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public bool ItemsVisible
    {
        get => (bool)GetValue(ItemsVisibleProperty);
        set => SetValue(ItemsVisibleProperty, value);
    }

    public bool SparkleIconVisible
    {
        get => (bool)GetValue(SparkleIconVisibleProperty);
        set => SetValue(SparkleIconVisibleProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public ItemsPanelTemplate? ItemsPanel
    {
        get => (ItemsPanelTemplate)GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }

    public double ShimmerHeight
    {
        get => (double)GetValue(ShimmerHeightProperty);
        set => SetValue(ShimmerHeightProperty, value);
    }
}
