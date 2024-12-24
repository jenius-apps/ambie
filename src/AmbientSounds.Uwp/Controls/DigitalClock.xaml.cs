using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Timers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class DigitalClock : UserControl
{
    private readonly IDispatcherQueue _dispatcher;

    private readonly Timer _timer = new()
    {
        Interval = 1000 // milliseconds
    };

    private static readonly DependencyProperty TimeTextProperty = DependencyProperty.Register(
        nameof(TimeText),
        typeof(string),
        typeof(DigitalClock),
        new PropertyMetadata(string.Empty));

    public DigitalClock()
    {
        this.InitializeComponent();
        this.Unloaded += OnUnloaded;
        _dispatcher = App.Services.GetRequiredService<IDispatcherQueue>();
        UpdateTimeText();
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private string TimeText
    {
        get => (string)GetValue(TimeTextProperty);
        set => SetValue(TimeTextProperty, value);
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e) => UpdateTimeText();

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded -= OnUnloaded;
        _timer.Stop();
        _timer.Elapsed -= OnTimerElapsed;
    }

    private void UpdateTimeText()
    {
        _dispatcher.TryEnqueue(() =>
        {
            TimeText = DateTime.Now.ToLongTimeString();
        });
    }
}
