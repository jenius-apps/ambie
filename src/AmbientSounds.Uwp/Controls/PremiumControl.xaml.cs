using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class PremiumControl : UserControl
    {
        public event EventHandler CloseRequested;

        public PremiumControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<PremiumControlViewModel>();
            this.Loaded += async (s, e) => { await this.ViewModel.InitializeAsync(); };
        }

        public PremiumControlViewModel ViewModel => (PremiumControlViewModel)this.DataContext;

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
