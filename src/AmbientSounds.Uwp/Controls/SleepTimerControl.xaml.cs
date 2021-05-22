using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SleepTimerControl : UserControl
    {
        public SleepTimerControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SleepTimerViewModel>();
            this.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Dispose(); 
            this.DataContext = null;
            this.Unloaded -= OnUnloaded;
            this.Bindings.StopTracking();
        }

        public SleepTimerViewModel ViewModel => (SleepTimerViewModel)this.DataContext;
    }
}
