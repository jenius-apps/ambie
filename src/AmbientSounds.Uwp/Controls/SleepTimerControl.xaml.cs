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
            this.Loaded += (_, _) =>
            {
                ViewModel.Initialize();
            };
            this.Unloaded += (_, _) =>
            {
                ViewModel.Dispose();
            };
        }

        public SleepTimerViewModel ViewModel => (SleepTimerViewModel)this.DataContext;
    }
}
