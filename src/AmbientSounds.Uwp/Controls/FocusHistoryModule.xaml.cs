using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusHistoryModule : UserControl
    {
        public FocusHistoryModule()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<FocusHistoryModuleViewModel>();
        }

        public FocusHistoryModuleViewModel ViewModel => (FocusHistoryModuleViewModel)this.DataContext;
    }
}
