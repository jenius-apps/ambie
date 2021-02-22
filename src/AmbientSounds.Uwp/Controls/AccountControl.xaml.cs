using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class AccountControl : UserControl
    {
        public AccountControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<AccountControlViewModel>();
        }

        public AccountControlViewModel ViewModel => (AccountControlViewModel)this.DataContext;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Load();
        }
    }
}
