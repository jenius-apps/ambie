using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class UploadedSoundsList : UserControl
    {
        private Flyout? _activeFlyout;

        public UploadedSoundsList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<UploadedSoundsListViewModel>();
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public UploadedSoundsListViewModel ViewModel => (UploadedSoundsListViewModel)this.DataContext;

        public async void Refresh()
        {
            // Exposed to allow parent
            // controls to trigger refresh.
            await ViewModel.LoadCommand.ExecuteAsync(null);
        }

        private void CloseDeleteFlyout(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _activeFlyout?.Hide();
            _activeFlyout = null;
        }

        private void DeleteFlyout_Opened(object sender, object e)
        {
            if (sender is Flyout f)
            {
                _activeFlyout = f;
            }
        }

        private void DeleteFlyout_Closed(object sender, object e)
        {
            _activeFlyout = null;
        }
    }
}
