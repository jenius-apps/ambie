using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using JeniusApps.Common.Telemetry;
using System.ComponentModel;

namespace AmbientSounds.Controls;

public sealed partial class SoundItemControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(SoundViewModel),
            typeof(SoundItemControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(SoundItemControl),
            new PropertyMetadata(false));

    public SoundItemControl()
    {
        this.InitializeComponent();
    }

    public SoundViewModel ViewModel
    {
        get => (SoundViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    private void BitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is BitmapImage img)
        {
            img.UriSource = new Uri("http://localhost");
        }
    }

    private async void OnEditHomePageClicked(object sender, RoutedEventArgs e)
    {
        await App.Services.GetRequiredService<IDialogService>().OpenTutorialAsync();
        App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.ReorderClicked);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SoundItemControl control)
        {
            control.RegisterEvents(e.OldValue, e.NewValue);
        }
    }

    private void RegisterEvents(object oldObject, object newObject)
    {
        if (oldObject is SoundViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (newObject is SoundViewModel newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SoundViewModel.IsCurrentlyPlaying))
        {
            if (ViewModel.IsCurrentlyPlaying)
            {
                SoundsStartingStoryboard.Begin();
            }
            else
            {
                SoundsStoppingStoryboard.Begin();
            }
        }
    }
}
