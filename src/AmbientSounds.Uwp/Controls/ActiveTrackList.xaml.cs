using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class ActiveTrackList : UserControl
    {
        public ActiveTrackList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
            this.Unloaded += OnUnloaded;
        }

        public ActiveTrackListViewModel ViewModel => (ActiveTrackListViewModel)this.DataContext;

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Dispose();
        }

        private void NameInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.SaveCommand.ExecuteAsync(NameInput.Text);
                e.Handled = true;
                SaveFlyout.Hide();
            }
        }

        private void SaveFlyout_Closed(object sender, object e)
        {
            NameInput.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFlyout.Hide();
        }

        private async void OnListLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadPreviousStateAsync();
        }
    }
}
