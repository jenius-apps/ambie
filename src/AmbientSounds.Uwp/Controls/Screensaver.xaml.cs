using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class Screensaver : UserControl
    {
        public Screensaver()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ScreensaverViewModel>();
        }

        public ScreensaverViewModel ViewModel => (ScreensaverViewModel)this.DataContext;
    }
}
