using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<PlayerViewModel>();
            this.Loaded += (_, _) => { ViewModel.Initialize(); };
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public PlayerViewModel ViewModel => (PlayerViewModel)this.DataContext;
    }
}
