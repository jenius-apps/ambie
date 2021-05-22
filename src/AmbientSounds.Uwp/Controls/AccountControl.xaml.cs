using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class AccountControl : UserControl
    {
        public AccountControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<AccountControlViewModel>();
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public AccountControlViewModel ViewModel => (AccountControlViewModel)this.DataContext;

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadAsync();
        }
    }
}
