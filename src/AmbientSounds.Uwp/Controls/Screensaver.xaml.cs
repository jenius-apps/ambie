using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
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
