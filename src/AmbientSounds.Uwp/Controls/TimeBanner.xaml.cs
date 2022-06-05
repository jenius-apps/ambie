using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class TimeBanner : UserControl
    {
        public TimeBanner()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<TimeBannerViewModel>();
        }

        public TimeBannerViewModel ViewModel => (TimeBannerViewModel)this.DataContext;
    }
}
