using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class ShareResultsControl : UserControl
    {
        public ShareResultsControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ShareResultsViewModel>();
        }

        public ShareResultsViewModel ViewModel => (ShareResultsViewModel)this.DataContext;

        public async void LoadSoundsAsync(IList<string> soundIds)
        {
            await ViewModel.LoadAsync(soundIds);
        }
    }
}
