using AmbientSounds.ViewModels;
using Autofac;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Container.Resolve<PlayerViewModel>();
        }

        public PlayerViewModel ViewModel => (PlayerViewModel)this.DataContext;
    }
}
