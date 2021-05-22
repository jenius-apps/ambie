using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class ToggleCompactButton : UserControl
    {

        public ToggleCompactButton()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Services.GetRequiredService<INavigator>().ToCompact();
        }
    }
}
