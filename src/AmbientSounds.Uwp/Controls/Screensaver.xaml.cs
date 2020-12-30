using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class Screensaver : UserControl
    {
        public Screensaver()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ScreensaverViewModel>();
            ViewModel.PropertyChanging += PropertyChanging;
            this.SizeChanged += OnSizeChanged;
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

        private void PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ImageVisible1))
            {
                if (ViewModel.ImageVisible1 == false)
                {
                    ImageSb1.Stop();
                    ImageSb1.Begin();
                }
            }
            else if (e.PropertyName == nameof(ViewModel.ImageVisible2))
            {
                if (ViewModel.ImageVisible2 == false)
                {
                    ImageSb2.Stop();
                    ImageSb2.Begin();
                }
            }
        }

        public ScreensaverViewModel ViewModel => (ScreensaverViewModel)this.DataContext;
    }
}
