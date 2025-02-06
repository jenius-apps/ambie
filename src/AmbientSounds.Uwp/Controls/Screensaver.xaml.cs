using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class Screensaver : UserControl
{
    public event EventHandler? ImageChanged;

    public Screensaver()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ScreensaverViewModel>();
        this.Loaded += (_, _) =>
        {
            ViewModel.Initialize();
            ViewModel.PropertyChanged += OnPropertyChanged;
        };
        this.Unloaded += (_, _) =>
        {
            ImageSb1.Stop();
            ImageSb2.Stop();
            ViewModel.Dispose();
            ViewModel.PropertyChanged -= OnPropertyChanged;
        };
        this.SizeChanged += OnSizeChanged;
    }

    public ScreensaverViewModel ViewModel => (ScreensaverViewModel)this.DataContext;

    public void Uninitialize()
    {
        ImageSb1.Stop();
        ImageSb2.Stop();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width >= e.NewSize.Height)
        {
            image.Width = e.NewSize.Width * 1.3;
            image2.Width = e.NewSize.Width * 1.3;
            image.Height = double.NaN;
            image2.Height = double.NaN;
        }
        else
        {
            image.Height = e.NewSize.Height * 1.3;
            image2.Height = e.NewSize.Height * 1.3;
            image.Width = double.NaN;
            image2.Width = double.NaN;
        }
    }

    private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.ImageVisible1))
        {
            if (ViewModel.ImageVisible1 == false)
            {
                await Image1Hide.StartAsync();
                image.Visibility = Visibility.Collapsed;
                ImageSb1.Stop();
            }
            else
            {
                image.Visibility = Visibility.Visible;
                ImageSb1.Begin();
            }

            ImageChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (e.PropertyName == nameof(ViewModel.ImageVisible2))
        {
            if (ViewModel.ImageVisible2 == false)
            {
                await Image2Hide.StartAsync();
                image2.Visibility = Visibility.Collapsed;
                ImageSb2.Stop();
            }
            else
            {
                image2.Visibility = Visibility.Visible;
                ImageSb2.Begin();
            }

            ImageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}