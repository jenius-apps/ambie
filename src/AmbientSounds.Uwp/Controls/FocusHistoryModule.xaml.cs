using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusHistoryModule : UserControl, ICanInitialize
    {
        public FocusHistoryModule()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<FocusHistoryModuleViewModel>();
        }

        public FocusHistoryModuleViewModel ViewModel => (FocusHistoryModuleViewModel)this.DataContext;

        public Task InitializeAsync() => ViewModel.InitializeAsync();

        public void Uninitialize()
        {
            ViewModel.Uninitialize();
        }

        private void OnHistoryClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is FocusHistoryViewModel vm)
            {
                ViewModel.ViewDetailsCommand.Execute(vm);
            }
        }
    }
}
