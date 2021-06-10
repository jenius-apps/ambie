using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class ThemeSettings : UserControl
    {
        public ThemeSettings()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ThemeSettingsViewModel>();
        }

        public ThemeSettingsViewModel ViewModel => (ThemeSettingsViewModel)this.DataContext;
    }
}
