using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class SegmentItem : UserControl
{
    public event EventHandler<RoutedEventArgs> Clicked;

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(SegmentItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ItemContentProperty =
        DependencyProperty.Register(
            nameof(ItemContent),
            typeof(FrameworkElement),
            typeof(SegmentItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty AutomationNameProperty =
        DependencyProperty.Register(
            nameof(AutomationName),
            typeof(string),
            typeof(SegmentItem),
            new PropertyMetadata(""));

    public SegmentItem()
    {
        this.InitializeComponent();
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public FrameworkElement ItemContent
    {
        get => (FrameworkElement)GetValue(ItemContentProperty);
        set => SetValue(ItemContentProperty, value);
    }

    public string AutomationName
    {
        get => (string)GetValue(AutomationNameProperty);
        set => SetValue(AutomationNameProperty, value);
    }

    private void OnClicked(object sender, RoutedEventArgs e)
    {
        Clicked?.Invoke(this, e);
    }
}
