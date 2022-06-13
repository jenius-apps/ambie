using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Repositories;
using AmbientSounds.Services;
using AmbientSounds.Services.Uwp;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Tools;
using JeniusApps.Common.Tools.Uwp;
using Microsoft.AppCenter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client.Extensibility;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
        private AppServiceConnection? _appServiceConnection;
        private BackgroundTaskDeferral? _appServiceDeferral;
        private static PlayerTelemetryTracker? _playerTracker;
        private IUserSettings? _userSettings;
        private static Frame? AppFrame;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspension;

            if (IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/how-to-disable-mouse-mode
                //this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;

                // Ref: https://docs.microsoft.com/en-us/windows/uwp/design/input/gamepad-and-remote-interactions#reveal-focus
                this.FocusVisualKind = FocusVisualKind.Reveal;
            }
        }

        private async void OnSuspension(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            _playerTracker?.TrackDuration(DateTimeOffset.Now);
            if (_serviceProvider?.GetService<IFocusService>() is IFocusService focusService &&
                focusService.CurrentState == AmbientSounds.Services.FocusState.Active)
            {
                // We don't support focus sessions when ambie is suspended,
                // and we want to make sure notifications are cancelled.
                // Note: If music is playing, then ambie won't suspend on minimize.
                focusService.PauseTimer();
            }

            if (_serviceProvider?.GetService<IFocusNotesService>() is IFocusNotesService notesService)
            {
                await notesService.SaveNotesToStorageAsync();
            }

            deferral.Complete();
        }

        public static bool IsDesktop => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop";

        public static bool IsTenFoot => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" || _isTenFootPc;

        public static bool IsRightToLeftLanguage
        {
            get
            {
                string flowDirectionSetting = ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
                return flowDirectionSetting == "RTL";
            }
        }

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
            if (e is IActivatedEventArgs activatedEventArgs
                && activatedEventArgs is IProtocolActivatedEventArgs protocolArgs)
            {
                HandleProtocolLaunch(protocolArgs);
            }
        }

        /// <inheritdoc/>
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                new PartnerCentreNotificationRegistrar().TrackLaunch(toastActivationArgs.Argument);
                await ActivateAsync(false);
            }
            else if (args is IProtocolActivatedEventArgs protocolActivatedEventArgs)
            {
                await ActivateAsync(false);
                HandleProtocolLaunch(protocolActivatedEventArgs);
            }
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails appService)
            {
                _appServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += OnAppServicesCanceled;
                _appServiceConnection = appService.AppServiceConnection;
                _appServiceConnection.RequestReceived += OnAppServiceRequestReceived;
                _appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
            }
        }

        private async void OnAppServiceRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            AppServiceDeferral messageDeferral = args.GetDeferral();
            var controller = App.Services.GetService<AppServiceController>();
            if (controller is not null)
            {
                await controller.ProcessRequest(args.Request);
            }
            else
            {
                var message = new ValueSet();
                message.Add("result", "Fail. Launch Ambie in the foreground to use its app services.");
                await args.Request.SendResponseAsync(message);
            }

            messageDeferral.Complete();
        }

        private void OnAppServicesCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _appServiceDeferral?.Complete();
        }

        private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _appServiceDeferral?.Complete();
        }

        private async Task ActivateAsync(bool prelaunched, IAppSettings? appsettings = null)
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
                _serviceProvider = ConfigureServices(appsettings);
                rootFrame.ActualThemeChanged += OnActualThemeChanged;
                _userSettings = Services.GetRequiredService<IUserSettings>();
                _userSettings.SettingSet += OnSettingSet;
            }

            if (prelaunched == false)
            {
                CoreApplication.EnablePrelaunch(true);

                // Navigate to the root page if one isn't loaded already
                if (rootFrame.Content is null)
                {
                    rootFrame.Navigate(typeof(Views.ShellPage));
                }

                SetMinSize();
                Window.Current.Activate();
            }

            AppFrame = rootFrame;
            if (IsRightToLeftLanguage)
            {
                rootFrame.FlowDirection = FlowDirection.RightToLeft;
            }
            SetAppRequestedTheme();
            Services.GetRequiredService<INavigator>().RootFrame = rootFrame;
            CustomizeTitleBar(rootFrame.ActualTheme == ElementTheme.Dark);
            await TryRegisterNotifications();
            await BackgroundDownloadService.Instance.DiscoverActiveDownloadsAsync();
        }

        private void HandleProtocolLaunch(IProtocolActivatedEventArgs protocolArgs)
        {
            try
            {
                var uri = protocolArgs.Uri;

                if (uri.Host == "launch")
                {
                    var arg = protocolArgs.Uri.Query.Replace("?", string.Empty);
                    Services.GetService<ProtocolLaunchController>()?.ProcessLaunchProtocolArguments(arg);
                }
            }
            catch (UriFormatException)
            {
                // An invalid Uri may have been passed in.
            }
        }

        private void SetMinSize()
        {
            // Note: needs to be run sometime before Window.Current.Activate()
            var scale = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(
                new Windows.Foundation.Size(260 * scale, 500 * scale));
        }

        private void OnSettingSet(object sender, string key)
        {
            if (key == UserSettingsConstants.Theme)
            {
                SetAppRequestedTheme();
            }
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            CustomizeTitleBar(sender.ActualTheme == ElementTheme.Dark);
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
            // Note: this method must run after AppFrame has been assigned.

            object themeObject = ApplicationData.Current.LocalSettings.Values[UserSettingsConstants.Theme];
            if (themeObject is not null && AppFrame is not null)
            {
                string theme = themeObject.ToString();
                switch (theme)
                {
                    case "light":
                        AppFrame.RequestedTheme = ElementTheme.Light;
                        break;
                    case "dark":
                        AppFrame.RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        AppFrame.RequestedTheme = ElementTheme.Default;
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
        private static IServiceProvider ConfigureServices(IAppSettings? appsettings = null)
        {
            var client = new HttpClient();

            var provider = new ServiceCollection()
                .AddSingleton(client)
                // if viewmodel, then always transient unless otherwise stated
                .AddSingleton<SoundListViewModel>() // shared in main and compact pages
                .AddTransient<CatalogueListViewModel>()
                .AddTransient<ScreensaverViewModel>()
                .AddSingleton<ScreensaverPageViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<ThemeSettingsViewModel>()
                .AddTransient<CataloguePageViewModel>()
                .AddSingleton<ShellPageViewModel>()
                .AddTransient<ShareResultsViewModel>()
                .AddSingleton<PlayerViewModel>() // shared in main and compact pages
                .AddSingleton<SleepTimerViewModel>() // shared in main and compact pages
                .AddSingleton<VideosMenuViewModel>()
                .AddSingleton<TimeBannerViewModel>()
                .AddSingleton<FocusPageViewModel>()
                .AddTransient<ActiveTrackListViewModel>()
                .AddSingleton<AccountControlViewModel>() // singleton to avoid re-signing in every navigation
                .AddSingleton<AppServiceController>()
                .AddSingleton<ProtocolLaunchController>()
                // object tree is all transient
                .AddTransient<IStoreNotificationRegistrar, PartnerCentreNotificationRegistrar>()
                .AddTransient<IImagePicker, ImagePicker>()
                // Must be transient because this is basically
                // a timer factory.
                .AddTransient<ITimerService, TimerService>()
                // exposes events or object tree has singleton, so singleton.
                .AddSingleton<IFocusNotesService, FocusNotesService>()
                .AddSingleton<IFocusService, FocusService>()
                .AddSingleton<IRecentFocusService, RecentFocusService>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<IFileDownloader, FileDownloader>()
                .AddSingleton<ISoundVmFactory, SoundVmFactory>()
                .AddSingleton<IVideoService, VideoService>()
                .AddSingleton<IVideoCache, VideoCache>()
                .AddSingleton<IOfflineVideoRepository, OfflineVideoRepository>()
                .AddSingleton<IOnlineVideoRepository, OnlineVideoRepository>()
                .AddSingleton<IUserSettings, LocalSettings>()
                .AddSingleton<ISoundMixService, SoundMixService>()
                .AddSingleton<IRenamer, Renamer>()
                .AddSingleton<ILocalizer, ReswLocalizer>()
                .AddSingleton<IFileWriter, FileWriter>()
                .AddSingleton<IFilePicker, FilePicker>()
                .AddSingleton<IFocusToastService, FocusToastService>()
                .AddSingleton<ICustomWebUi, CustomAuthUiService>()
                .AddSingleton<IToastService, ToastService>()
                .AddSingleton<IMsaAuthClient, MsalClient>()
                .AddSingleton<INavigator, Navigator>()
                .AddSingleton<ICloudFileWriter, CloudFileWriter>()
                .AddSingleton<PlayerTelemetryTracker>()
                .AddSingleton<ISyncEngine, SyncEngine>()
                .AddSingleton<IAccountManager, AccountManager>()
                .AddSingleton<IPreviewService, PreviewService>()
                .AddSingleton<IIapService, StoreService>()
                .AddSingleton<IDownloadManager, WindowsDownloadManager>()
                .AddSingleton<IScreensaverService, ScreensaverService>()
                .AddSingleton<ITelemetry, AppCenterTelemetry>()
                .AddSingleton<IOnlineSoundDataProvider, OnlineSoundDataProvider>()
                .AddSingleton<ISystemInfoProvider, SystemInfoProvider>()
                .AddSingleton<IMixMediaPlayerService, MixMediaPlayerService>()
                .AddSingleton<ISoundDataProvider, SoundDataProvider>()
                .AddSingleton(appsettings ?? new AppSettings())
                .BuildServiceProvider(true);

            // preload telemetry to ensure country code is set.
            provider.GetService<ITelemetry>();
            AppCenter.SetCountryCode(new GeographicRegion().CodeTwoLetter);

            // preload appservice controller to ensure its
            // dispatcher queue loads properly on the ui thread.
            provider.GetService<AppServiceController>();
            provider.GetService<ProtocolLaunchController>();
            _playerTracker = provider.GetRequiredService<PlayerTelemetryTracker>();
            return provider;
        }
    }
}
