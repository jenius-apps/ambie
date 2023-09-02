using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Repositories;
using AmbientSounds.Services;
using AmbientSounds.Services.Uwp;
using AmbientSounds.Tools;
using AmbientSounds.Tools.Uwp;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Tools;
using JeniusApps.Common.Tools.Uwp;
using Microsoft.AppCenter;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
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
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Telemetry.Uwp;
using Windows.UI.Core.Preview;

#nullable enable

namespace AmbientSounds;

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
    //private XboxGameBarWidget? _widget;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        this.Suspending += OnSuspension;
        this.Resuming += OnResuming;
        NetworkHelper.Instance.NetworkChanged += OnNetworkChanged;

        if (IsTenFoot)
        {
            // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/how-to-disable-mouse-mode
            //this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;

            // Ref: https://docs.microsoft.com/en-us/windows/uwp/design/input/gamepad-and-remote-interactions#reveal-focus
            this.FocusVisualKind = FocusVisualKind.Reveal;
        }
    }

    private async void OnNetworkChanged(object sender, EventArgs e)
    {
        var presence = _serviceProvider?.GetService<IPresenceService>();
        if (presence is null)
        {
            return;
        }

        if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        {
            await presence.EnsureInitializedAsync();
        }
        else
        {
            await presence.DisconnectAsync();
        }
    }

    private async void OnResuming(object sender, object e)
    {
        if (_serviceProvider?.GetService<IPresenceService>() is IPresenceService presenceService)
        {
            await presenceService.EnsureInitializedAsync();
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

        if (_serviceProvider?.GetService<IPresenceService>() is IPresenceService presenceService)
        {
            await presenceService.DisconnectAsync();
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

        // Ensure previously scheduled toasts are closed on a fresh new launch.
        Services.GetRequiredService<IToastService>().ClearScheduledToasts();
    }

    /// <inheritdoc/>
    protected override async void OnActivated(IActivatedEventArgs args)
    {
        if (args is ToastNotificationActivatedEventArgs toastActivationArgs)
        {
            await ActivateAsync(false, launchArguments: toastActivationArgs.Argument);

            // Must be performed after activate async
            // because the services are setup in that method.
            Services.GetRequiredService<ITelemetry>().TrackEvent(
                TelemetryConstants.LaunchViaToast,
                new Dictionary<string, string>
                {
                    { "args", toastActivationArgs.Argument }
                });
        }
        else if (args is IProtocolActivatedEventArgs protocolActivatedEventArgs)
        {
            if (protocolActivatedEventArgs.Uri.AbsoluteUri.StartsWith("ms-gamebarwidget"))
            {
                //await ActivateAsync(false, widgetMode: true);
                //_widget = new XboxGameBarWidget(args as XboxGameBarWidgetActivatedEventArgs, Window.Current.CoreWindow, AppFrame);
                //Window.Current.Closed += OnWidgetClosed;
            }
            else
            {
                await ActivateAsync(false);
                HandleProtocolLaunch(protocolActivatedEventArgs);
            }
        }
    }

    //private void OnWidgetClosed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
    //{
    //    _widget = null;
    //    Window.Current.Closed -= OnWidgetClosed;
    //}

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

    private async Task ActivateAsync(
        bool prelaunched, 
        IAppSettings? appsettings = null,
        string launchArguments = "")
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
                rootFrame.Navigate(typeof(Views.ShellPage), new ShellPageNavigationArgs
                {
                    FirstPageOverride = LaunchConstants.ToPageType(launchArguments),
                    LaunchArguments = launchArguments
                });
            }

            Window.Current.Activate();
        }

        AppFrame = rootFrame;
        if (IsRightToLeftLanguage)
        {
            rootFrame.FlowDirection = FlowDirection.RightToLeft;
        }
        SetAppRequestedTheme();
        Services.GetRequiredService<Services.INavigator>().RootFrame = rootFrame;
        CustomizeTitleBar(rootFrame.ActualTheme == ElementTheme.Dark);
        await TryRegisterNotifications();

        try
        {
            await BackgroundDownloadService.Instance.DiscoverActiveDownloadsAsync();
        }
        catch (Exception ex)
        {
            Services.GetRequiredService<ITelemetry>().TrackError(ex);
        }

        // Register for close requested event here because
        // we need the event to be handled on all pages, not just from the shell page. 
        SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += async (sender, args) =>
        {
            var d = args.GetDeferral();

            if (Services.GetRequiredService<IUserSettings>().Get<bool>(UserSettingsConstants.ConfirmCloseKey) &&
                Services.GetRequiredService<IMixMediaPlayerService>().PlaybackState == MediaPlaybackState.Playing)
            {
                var telemetry = Services.GetRequiredService<ITelemetry>();
                telemetry.TrackEvent(TelemetryConstants.CloseRequestedWhilePlaying);
                bool closeConfirmed = await Services.GetRequiredService<IDialogService>().OpenConfirmCloseAsync();
                if (!closeConfirmed)
                {
                    // this cancels the close event.
                    args.Handled = true;
                    telemetry.TrackEvent(TelemetryConstants.CloseCanceled);
                }
                else
                {
                    telemetry.TrackEvent(TelemetryConstants.CloseConfirmed);
                }
            }

            d.Complete();
        };
    }

    private void HandleProtocolLaunch(IProtocolActivatedEventArgs protocolArgs)
    {
        try
        {
            var uri = protocolArgs.Uri;
            var arg = protocolArgs.Uri.Query.Replace("?", string.Empty);

            if (uri.Host is "launch")
            {
                Services.GetService<ProtocolLaunchController>()?.ProcessLaunchProtocolArguments(arg);
            }
            else if (uri.Host is "share" && Services.GetService<ProtocolLaunchController>() is { } controller)
            {
                controller.ProcessShareProtocolArguments(arg);
            }
        }
        catch (UriFormatException)
        {
            // An invalid Uri may have been passed in.
        }
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
    /// Builds the <see cref="IServiceProvider"/> instance with the required services.
    /// </summary>
    /// <param name="appsettings">The <see cref="IAppSettings"/> instance to use, if available.</param>
    /// <returns>The resulting service provider.</returns>
    private static IServiceProvider ConfigureServices(IAppSettings? appsettings = null)
    {
        ServiceCollection collection = new();

        ConfigureServices(collection);

        // Manually register additional services requiring more customization
        collection.AddSingleton(appsettings ?? new AppSettings());
        collection.AddSingleton<ITelemetry, AppCenterTelemetry>(s =>
        {
            var apiKey = s.GetRequiredService<IAppSettings>().TelemetryApiKey;
            return new AppCenterTelemetry(apiKey);
        });
        
        IServiceProvider provider = collection.BuildServiceProvider();

        // preload telemetry to ensure country code is set.
        provider.GetService<ITelemetry>();
        AppCenter.SetCountryCode(new GeographicRegion().CodeTwoLetter);

        // preload appservice controller to ensure its
        // dispatcher queue loads properly on the ui thread.
        provider.GetService<AppServiceController>();
        provider.GetService<ProtocolLaunchController>();
        provider.GetService<PlaybackModeObserver>();
        _playerTracker = provider.GetRequiredService<PlayerTelemetryTracker>();

        return provider;
    }

    /// <summary>
    /// Configures services used by the app.
    /// </summary>
    /// <param name="services">The target <see cref="IServiceCollection"/> instance to register services with.</param>
    [Singleton(typeof(HttpClient))]
    [Singleton(typeof(SoundListViewModel))] // shared in main and compact pages
    [Transient(typeof(ScreensaverViewModel))]
    [Singleton(typeof(ScreensaverPageViewModel))]
    [Singleton(typeof(SettingsViewModel))]
    [Singleton(typeof(CataloguePageViewModel))]
    [Singleton(typeof(FocusTaskModuleViewModel))]
    [Singleton(typeof(PremiumControlViewModel))]
    [Singleton(typeof(FocusTimerModuleViewModel))]
    [Singleton(typeof(ShellPageViewModel))]
    [Singleton(typeof(PlayerViewModel))] // shared in main and compact pages
    [Singleton(typeof(SleepTimerViewModel))] // shared in main and compact pages
    [Singleton(typeof(FocusHistoryModuleViewModel))]
    [Singleton(typeof(VideosMenuViewModel))]
    [Singleton(typeof(TimeBannerViewModel))]
    [Singleton(typeof(UpdatesViewModel))]
    [Singleton(typeof(InterruptionPageViewModel))]
    [Singleton(typeof(InterruptionInsightsViewModel))]
    [Singleton(typeof(DownloadMissingViewModel))]
    [Singleton(typeof(ShareViewModel))]
    [Singleton(typeof(MeditatePageViewModel))]
    [Singleton(typeof(FocusPageViewModel))]
    [Singleton(typeof(CompactPageViewModel))]
    [Transient(typeof(ActiveTrackListViewModel))]
    [Singleton(typeof(AppServiceController))]
    [Singleton(typeof(PlaybackModeObserver))]
    [Singleton(typeof(ProtocolLaunchController))]
    [Transient(typeof(PartnerCentreNotificationRegistrar), typeof(IStoreNotificationRegistrar))]
    [Transient(typeof(ImagePicker), typeof(IImagePicker))]
    [Singleton(typeof(WindowsClipboard), typeof(IClipboard))]
    [Singleton(typeof(MicrosoftStoreRatings), typeof(IAppStoreRatings))]
    [Transient(typeof(TimerService), typeof(ITimerService))] // Must be transient because this is basically a timer factory
    [Singleton(typeof(WindowsDispatcherQueue), typeof(IDispatcherQueue))]
    [Singleton(typeof(FocusNotesService), typeof(IFocusNotesService))]
    [Singleton(typeof(FocusService), typeof(IFocusService))]
    [Singleton(typeof(FocusHistoryService), typeof(IFocusHistoryService))]
    [Singleton(typeof(RecentFocusService), typeof(IRecentFocusService))]
    [Singleton(typeof(DialogService), typeof(IDialogService))]
    [Singleton(typeof(ShareService), typeof(IShareService))]
    [Singleton(typeof(PresenceService), typeof(IPresenceService))]
    [Singleton(typeof(FileDownloader), typeof(IFileDownloader))]
    [Singleton(typeof(SoundVmFactory), typeof(ISoundVmFactory))]
    [Singleton(typeof(GuideVmFactory), typeof(IGuideVmFactory))]
    [Singleton(typeof(CatalogueRowVmFactory))]
    [Singleton(typeof(CatalogueService), typeof(ICatalogueService))]
    [Singleton(typeof(VideoService), typeof(IVideoService))]
    [Singleton(typeof(FocusTaskCache), typeof(IFocusTaskCache))]
    [Singleton(typeof(FocusHistoryCache), typeof(IFocusHistoryCache))]
    [Singleton(typeof(VideoCache), typeof(IVideoCache))]
    [Singleton(typeof(PageCache), typeof(IPageCache))]
    [Singleton(typeof(PagesRepository), typeof(IPagesRepository))]
    [Singleton(typeof(AssetLocalizer), typeof(IAssetLocalizer))]
    [Singleton(typeof(ShareDetailCache), typeof(IShareDetailCache))]
    [Singleton(typeof(ShareDetailRepository), typeof(IShareDetailRepository))]
    [Singleton(typeof(FocusTaskRepository), typeof(IFocusTaskRepository))]
    [Singleton(typeof(OfflineVideoRepository), typeof(IOfflineVideoRepository))]
    [Singleton(typeof(OnlineVideoRepository), typeof(IOnlineVideoRepository))]
    [Singleton(typeof(OfflineSoundRepository), typeof(IOfflineSoundRepository))]
    [Singleton(typeof(OnlineSoundRepository), typeof(IOnlineSoundRepository))]
    [Singleton(typeof(OnlineGuideRepository), typeof(IOnlineGuideRepository))]
    [Singleton(typeof(OfflineGuideRepository), typeof(IOfflineGuideRepository))]
    [Singleton(typeof(SoundCache), typeof(ISoundCache))]
    [Singleton(typeof(GuideCache), typeof(IGuideCache))]
    [Singleton(typeof(SoundService), typeof(ISoundService))]
    [Singleton(typeof(GuideService), typeof(IGuideService))]
    [Singleton(typeof(FocusHistoryRepository), typeof(IFocusHistoryRepository))]
    [Singleton(typeof(LocalSettings), typeof(IUserSettings))]
    [Singleton(typeof(SoundMixService), typeof(ISoundMixService))]
    [Singleton(typeof(Renamer), typeof(IRenamer))]
    [Singleton(typeof(UpdateService), typeof(IUpdateService))]
    [Singleton(typeof(ReswLocalizer), typeof(ILocalizer))]
    [Singleton(typeof(FileWriter), typeof(IFileWriter))]
    [Singleton(typeof(FilePicker), typeof(IFilePicker))]
    [Singleton(typeof(FocusToastService), typeof(IFocusToastService))]
    [Singleton(typeof(ToastService), typeof(IToastService))]
    [Singleton(typeof(Services.Uwp.Navigator), typeof(Services.INavigator))]
    [Singleton(typeof(CompactNavigator), typeof(ICompactNavigator))]
    [Singleton(typeof(CloudFileWriter), typeof(ICloudFileWriter))]
    [Singleton(typeof(PlayerTelemetryTracker))]
    [Singleton(typeof(SoundEffectsService), typeof(ISoundEffectsService))]
    [Singleton(typeof(PreviewService), typeof(IPreviewService))]
    [Singleton(typeof(StoreService), typeof(IIapService))]
    [Singleton(typeof(WindowsDownloadManager), typeof(IDownloadManager))]
    [Singleton(typeof(ScreensaverService), typeof(IScreensaverService))]
    [Singleton(typeof(Services.Uwp.SystemInfoProvider), typeof(Services.ISystemInfoProvider))]
    [Singleton(typeof(AssetsReader), typeof(Tools.IAssetsReader))]
    [Singleton(typeof(MixMediaPlayerService), typeof(IMixMediaPlayerService))]
    [Singleton(typeof(WindowsSystemMediaControls), typeof(ISystemMediaControls))]
    [Singleton(typeof(AudioDeviceService), typeof(IAudioDeviceService))]
    [Singleton(typeof(WindowsMediaPlayerFactory), typeof(IMediaPlayerFactory))]
    private static partial void ConfigureServices(IServiceCollection services);
}
