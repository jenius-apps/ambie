using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompactPage : Page
    {
        public CompactPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<MainPageViewModel>();
        }

        public MainPageViewModel ViewModel => (MainPageViewModel)this.DataContext;

        private async void CloseCompactClicked()
        {
            var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.Default);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default, preferences);
            App.AppFrame.GoBack();
        }
    }
}
