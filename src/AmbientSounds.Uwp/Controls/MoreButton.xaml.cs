using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Services.Store;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using AmbientSounds.Views.Dialogs;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class MoreButton : UserControl
    {
        private const string Translations = "https://github.com/jenius-apps/ambie#translation";
        private const string Github = "https://github.com/jenius-apps/ambie";
        private const string Contact = "mailto:jenius_apps@outlook.com";
        private const string StorePage = "https://www.microsoft.com/store/productId/9P07XNM5CHP0";
        private const string UwpDiscord = "https://discord.gg/b9z3BeXk3D";

        public MoreButton()
        {
            this.InitializeComponent();

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (DataTransferManager sender, DataRequestedEventArgs args) =>
            {
                DataRequest request = args.Request;
                request.Data.SetWebLink(new Uri(StorePage));
                request.Data.Properties.Title = StorePage;
                request.Data.Properties.Description = "Ambie";
            };
        }

        /// <summary>
        /// If true, the compact mode button is visible.
        /// Default is true.
        /// </summary>
        public bool ShowCompactMode
        {
            get => (bool)GetValue(ShowCompactModeProperty);
            set => SetValue(ShowCompactModeProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="ShowCompactMode"/>.
        /// Default is true.
        /// </summary>
        public static readonly DependencyProperty ShowCompactModeProperty = DependencyProperty.Register(
            nameof(ShowCompactMode),
            typeof(bool),
            typeof(MoreButton),
            new PropertyMetadata(true));

        private bool CompactButtonVisible => !App.IsTenFoot && ShowCompactMode;

        private void ShareClicked()
        {
            DataTransferManager.ShowShareUI();
        }

        private async void CompactOverlayClicked()
        {
            // Ref: https://programmer.group/uwp-use-compact-overlay-mode-to-always-display-on-the-front-end.html
            var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            preferences.CustomSize = new Windows.Foundation.Size(360, 500);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);
            App.AppFrame.Navigate(typeof(Views.CompactPage), null, new SuppressNavigationTransitionInfo());
        }

        private async void RateUsClicked(object sender, RoutedEventArgs e)
        {
            var storeContext = StoreContext.GetDefault();
            await storeContext.RequestRateAndReviewAppAsync();
        }

        private async void DiscordClicked()
        {
            await LaunchAsync(UwpDiscord);
        }

        private async void ContactClicked()
        {
            await LaunchAsync(Contact);
        }

        private async void GithubClicked()
        {
            await LaunchAsync(Github);
        }

        private async void TranslationClicked()
        {
            await LaunchAsync(Translations);
        }

        private async Task LaunchAsync(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) 
                return;

            try
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
            catch { }
        }

        private async void SettingsClicked()
        {
            var dialogService = App.Services.GetRequiredService<IDialogService>();
            await dialogService.OpenSettingsAsync();
        }
    }
}
