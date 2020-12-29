using AmbientSounds.Services;
using AmbientSounds.Services.Uwp;
using AmbientSounds.ViewModels;
using AmbientSounds.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Diagnostics;
using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Net.Http;
using AmbientSounds.Factories;

#nullable enable

namespace AmbientSounds
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static readonly bool _isTenFootPc = false;
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            if (IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/how-to-disable-mouse-mode
                this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
            }

            SetAppRequestedTheme();
        }

        public static bool IsTenFoot => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" || _isTenFootPc;

        public static Frame? AppFrame { get; private set; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance for the current application instance.
        /// </summary>
        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider? serviceProvider = ((App)Current)._serviceProvider;

                if (serviceProvider is null)
                {
                    ThrowHelper.ThrowInvalidOperationException("The service provider is not initialized");
                }

                return serviceProvider;
            }
        }

        /// <inheritdoc/>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await ActivateAsync(e.PrelaunchActivated);
        }

        /// <inheritdoc/>
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                new PartnerCentreNotificationRegistrar().TrackLaunch(toastActivationArgs.Argument);
            }

            await ActivateAsync(false);
        }

        private async Task ActivateAsync(bool prelaunched)
        {
            // Do not repeat app initialization when the Window already has content
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                // Configure the services for later use
                _serviceProvider = ConfigureServices();
                var navigator = App.Services.GetRequiredService<INavigator>();
                navigator.Frame = rootFrame;
            }

            if (prelaunched == false)
            {
                CoreApplication.EnablePrelaunch(true);

                // Navigate to the root page if one isn't loaded already
                if (rootFrame.Content is null)
                {
                    rootFrame.Navigate(typeof(Views.MainPage));
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }

            AppFrame = rootFrame;
            CustomizeTitleBar(rootFrame.ActualTheme == ElementTheme.Dark);
            await TryRegisterNotifications();
        }

        private Task TryRegisterNotifications()
        {
            var settingsService = App.Services.GetRequiredService<IUserSettings>();

            if (settingsService.Get<bool>(UserSettingsConstants.Notifications))
            {
                return new PartnerCentreNotificationRegistrar().Register();
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Invoked when navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load Page {e.SourcePageType.FullName}.");
        }

        /// <summary>
        /// Removes title bar and sets title bar button backgrounds to transparent.
        /// </summary>
        private void CustomizeTitleBar(bool darkTheme)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = darkTheme ? Colors.LightGray : Colors.Black;
        }

        /// <summary>
        /// Method for setting requested app theme based on user's local settings.
        /// </summary>
        private void SetAppRequestedTheme()
        {
            object themeObject = ApplicationData.Current.LocalSettings.Values[UserSettingsConstants.Theme];
            if (themeObject != null)
            {
                string theme = themeObject.ToString();
                switch (theme)
                {
                    case "light":
                        App.Current.RequestedTheme = ApplicationTheme.Light;
                        break;
                    case "dark":
                        App.Current.RequestedTheme = ApplicationTheme.Dark;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[UserSettingsConstants.Theme] = "default";
            }
        }

        /// <summary>
        /// Configures a new <see cref="IServiceProvider"/> instance with the required services.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var client = new HttpClient();

            return new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton<SoundListViewModel>()
                .AddSingleton<CatalogueListViewModel>()
                .AddTransient<SoundSuggestionViewModel>()
                .AddTransient<ScreensaverViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<MainPageViewModel>()
                .AddTransient<IStoreNotificationRegistrar, PartnerCentreNotificationRegistrar>()
                .AddTransient<IDialogService, DialogService>()
                .AddTransient<IFileDownloader, FileDownloader>()
                .AddTransient<ISoundVmFactory, SoundVmFactory>()
                .AddTransient<IFileWriter, FileWriter>()
                .AddTransient<IUserSettings, LocalSettings>()
                .AddTransient<ITimerService, TimerService>()
                .AddSingleton<INavigator, Navigator>()
                .AddSingleton<PlayerViewModel>()
                .AddSingleton<SleepTimerViewModel>()
                .AddSingleton<IDownloadManager, DownloadManager>()
                .AddSingleton<IScreensaverService, ScreensaverService>()
                .AddSingleton<ITelemetry, AppCentreTelemetry>()
                .AddSingleton<IOnlineSoundDataProvider, OnlineSoundDataProvider>()
                .AddSingleton<IMediaPlayerService, MediaPlayerService>()
                .AddSingleton<ISoundDataProvider, SoundDataProvider>()
                .BuildServiceProvider();
        }
    }
}
