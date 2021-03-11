using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class UploadedSoundsList : UserControl
    {
        public UploadedSoundsList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<UploadedSoundsListViewModel>();
        }

        public UploadedSoundsListViewModel ViewModel => (UploadedSoundsListViewModel)this.DataContext;

        public async void Refresh()
        {
            // Exposed to allow parent
            // controls to trigger refresh.
            await ViewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
