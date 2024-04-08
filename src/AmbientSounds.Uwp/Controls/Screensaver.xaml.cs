using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class Screensaver : UserControl
{
    public Screensaver()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ScreensaverViewModel>();
        this.SizeChanged += OnSizeChanged;
    }

    public ScreensaverViewModel ViewModel => (ScreensaverViewModel)this.DataContext;

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width >= e.NewSize.Height)
        {
            Image1.Width = e.NewSize.Width * 1.3;
            Image2.Width = e.NewSize.Width * 1.3;
            Image1.Height = double.NaN;
            Image2.Height = double.NaN;
        }
        else
        {
            Image1.Height = e.NewSize.Height * 1.3;
            Image2.Height = e.NewSize.Height * 1.3;
            Image1.Width = double.NaN;
            Image2.Width = double.NaN;
        }
    }

    private async void PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.ImageVisible1))
        {
            if (ViewModel.ImageVisible1)
            {
                Image1.Visibility = Visibility.Visible;
                Image1Slide.Start();
                Image1FadeIn.Start();
            }
            else
            {
                await Image1FadeOut.StartAsync();
                Image1.Visibility = Visibility.Collapsed;
            }
        }
        else if (e.PropertyName == nameof(ViewModel.ImageVisible2))
        {
            if (ViewModel.ImageVisible2)
            {
                Image2.Visibility = Visibility.Visible;
                Image2Slide.Start();
                Image2FadeIn.Start();
            }
            else
            {
                await Image2FadeOut.StartAsync();
                Image2.Visibility = Visibility.Collapsed;
            }
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Initialize();
        ViewModel.PropertyChanged += PropertyChanged;
        await ViewModel.LoadAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Dispose();
        ViewModel.PropertyChanged -= PropertyChanged;
    }
}
