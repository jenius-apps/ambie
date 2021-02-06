using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class ToggleCompactButton : UserControl
    {
        private readonly INavigator _navigator;

        public ToggleCompactButton()
        {
            this.InitializeComponent();
            _navigator = App.Services.GetRequiredService<INavigator>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _navigator.ToCompact();
        }
    }
}
