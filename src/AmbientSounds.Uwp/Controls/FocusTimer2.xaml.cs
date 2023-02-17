using AmbientSounds.Constants;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class FocusTimer2 : UserControl
{
    public static readonly DependencyProperty FocusLengthProgressProperty = DependencyProperty.Register(
        nameof(FocusLengthProgress),
        typeof(double),
        typeof(FocusTimer2),
        new PropertyMetadata(0d));

    public static readonly DependencyProperty RestLengthProgressProperty = DependencyProperty.Register(
        nameof(RestLengthProgress),
        typeof(double),
        typeof(FocusTimer2),
        new PropertyMetadata(0d));

    public static readonly DependencyProperty FocusLengthProperty = DependencyProperty.Register(
        nameof(FocusLength),
        typeof(double),
        typeof(FocusTimer2),
        new PropertyMetadata(FocusConstants.TimerLimit));

    public static readonly DependencyProperty RestLengthProperty = DependencyProperty.Register(
        nameof(RestLength),
        typeof(double),
        typeof(FocusTimer2),
        new PropertyMetadata(FocusConstants.TimerLimit));

    public static readonly DependencyProperty FocusRingVisibleProperty = DependencyProperty.Register(
        nameof(FocusRingVisible),
        typeof(Visibility),
        typeof(FocusTimer2),
        new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty RestRingVisibleProperty = DependencyProperty.Register(
        nameof(RestRingVisible),
        typeof(Visibility),
        typeof(FocusTimer2),
        new PropertyMetadata(Visibility.Collapsed));

    public FocusTimer2()
    {
        this.InitializeComponent();
        this.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Bindings.Update();
    }

    public Visibility FocusRingVisible
    {
        get => (Visibility)GetValue(FocusRingVisibleProperty);
        set => SetValue(FocusRingVisibleProperty, value);
    }

    public Visibility RestRingVisible
    {
        get => (Visibility)GetValue(RestRingVisibleProperty);
        set => SetValue(RestRingVisibleProperty, value);
    }

    public double FocusLengthProgress
    {
        get => (double)GetValue(FocusLengthProgressProperty);
        set => SetValue(FocusLengthProgressProperty, value);
    }

    public double RestLengthProgress
    {
        get => (double)GetValue(RestLengthProgressProperty);
        set => SetValue(RestLengthProgressProperty, value);
    }

    public double FocusLength
    {
        get => (double)GetValue(FocusLengthProperty);
        set => SetValue(FocusLengthProperty, value);
    }

    public double RestLength
    {
        get => (double)GetValue(RestLengthProperty);
        set => SetValue(RestLengthProperty, value);
    }
}
