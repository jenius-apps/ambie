using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Repositories;
using AmbientSounds.Services;
using AmbientSounds.Services.Uwp;
using AmbientSounds.Tools;
using AmbientSounds.Tools.Uwp;
using AmbientSounds.ViewModels;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Extensions.DependencyInjection;
using JeniusApps.Common.PushNotifications;
using JeniusApps.Common.PushNotifications.Uwp;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Settings.Uwp;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using JeniusApps.Common.Tools.Uwp;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Net.Http;
using Windows.Storage;
using Windows.System.Profile;

#nullable enable

namespace AmbientSounds;

partial class App
{
    private IServiceProvider? _serviceProvider;

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

    /// <summary>
    /// Builds the <see cref="IServiceProvider"/> instance with the required services.
    /// </summary>
    /// <param name="appsettings">The <see cref="IAppSettings"/> instance to use, if available.</param>
    /// <returns>The resulting service provider.</returns>
    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection collection = new();

        ConfigureServices(collection);

        // Manually register additional services requiring more customization
        collection.AddSingleton<IUserSettings, LocalSettings>(s => new LocalSettings(UserSettingsConstants.Defaults));
        collection.AddSingleton<IExperimentationService, LocalExperimentationService>(s => new LocalExperimentationService(ExperimentConstants.AllKeys, s.GetRequiredService<IUserSettings>()));
        collection.AddSingleton<ITelemetry, AppInsightsTelemetry>(s =>
        {
            var apiKey = s.GetRequiredService<IAppSettings>().TelemetryApiKey;
            var isEnabled = s.GetRequiredService<IUserSettings>().Get<bool>(UserSettingsConstants.TelemetryOn);
            var context = GetContext();
            foreach (var experiment in s.GetRequiredService<IExperimentationService>().GetAllExperiments())
            {
                context?.GlobalProperties.Add(experiment.Key, experiment.Value.ToString());
            }
            return new AppInsightsTelemetry(apiKey, isEnabled: isEnabled, context: context);
        });

        collection.AddSingleton<IPushNotificationStorage, AzureServiceBusPushNotificationStorage>(s =>
        {
            var connectionString = s.GetRequiredService<IAppSettings>().NotificationHubConnectionString;
            var queueName = s.GetRequiredService<IAppSettings>().NotificationHubName;
            return new AzureServiceBusPushNotificationStorage(connectionString, queueName);
        });

        IServiceProvider provider = collection.BuildServiceProvider();

        // Preload telemetry so it's configured as soon as possible.
        provider.GetService<ITelemetry>();

        // preload appservice controller to ensure its
        // dispatcher queue loads properly on the ui thread.
        provider.GetService<AppServiceController>();
        provider.GetService<ProtocolLaunchController>();
        provider.GetService<PlaybackModeObserver>();
        _playerTracker = provider.GetRequiredService<PlayerTelemetryTracker>();

        return provider;
    }

    private static TelemetryContext? GetContext()
    {
        var context = new TelemetryContext();
        context.Session.Id = Guid.NewGuid().ToString();
        context.Component.Version = SystemInformation.Instance.ApplicationVersion.ToFormattedString();
        context.GlobalProperties.Add("isFirstRun", SystemInformation.Instance.IsFirstRun.ToString());

        if (ApplicationData.Current.LocalSettings.Values[UserSettingsConstants.LocalUserIdKey] is string { Length: > 0 } id)
        {
            context.User.Id = id;
        }
        else
        {
            string userId = Guid.NewGuid().ToString();
            ApplicationData.Current.LocalSettings.Values[UserSettingsConstants.LocalUserIdKey] = userId;
            context.User.Id = userId;
        }

        // Ref: https://learn.microsoft.com/en-us/answers/questions/1563897/uwp-and-winui-how-to-check-my-os-version-through-c
        ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
        ulong major = (version & 0xFFFF000000000000L) >> 48;
        ulong minor = (version & 0x0000FFFF00000000L) >> 32;
        ulong build = (version & 0x00000000FFFF0000L) >> 16;
        context.Device.OperatingSystem = $"Windows {major}.{minor}.{build}";

        return context;
    }

    /// <summary>
    /// Configures services used by the app.
    /// </summary>
    /// <param name="services">The target <see cref="IServiceCollection"/> instance to register services with.</param>
    [Singleton(typeof(AppSettings), typeof(IAppSettings))]
    [Singleton(typeof(HttpClient))]
    [Singleton(typeof(SoundListViewModel))] // shared in main and compact pages
    [Transient(typeof(ScreensaverViewModel))]
    [Singleton(typeof(ScreensaverPageViewModel))]
    [Transient(typeof(SettingsViewModel))]
    [Singleton(typeof(CataloguePageViewModel))]
    [Singleton(typeof(FocusTaskModuleViewModel))]
    [Singleton(typeof(PremiumControlViewModel))]
    [Transient(typeof(ChannelsPageViewModel))]
    [Singleton(typeof(FocusTimerModuleViewModel))]
    [Transient(typeof(ShellPageViewModel))]
    [Singleton(typeof(PlayerViewModel))] // shared in main and compact pages
    [Singleton(typeof(SleepTimerViewModel))] // shared in main and compact pages
    [Singleton(typeof(DigitalClockViewModel))] // timer needs to persist across pages
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
    [Transient(typeof(PushNotificationService), typeof(IPushNotificationService))]
    [Transient(typeof(WindowsPushNotificationSource), typeof(IPushNotificationSource))]
    [Transient(typeof(ImagePicker), typeof(IImagePicker))]
    [Singleton(typeof(WindowsClipboard), typeof(IClipboard))]
    [Singleton(typeof(MicrosoftStoreRatings), typeof(IAppStoreRatings))]
    [Transient(typeof(TimerService), typeof(ITimerService))] // Must be transient because this is basically a timer factory
    [Singleton(typeof(WindowsDispatcherQueue), typeof(IDispatcherQueue))]
    [Singleton(typeof(FocusNotesService), typeof(IFocusNotesService))]
    [Singleton(typeof(FocusService), typeof(IFocusService))]
    [Singleton(typeof(FocusHistoryService), typeof(IFocusHistoryService))]
    [Singleton(typeof(FocusTaskService), typeof(IFocusTaskService))]
    [Singleton(typeof(RecentFocusService), typeof(IRecentFocusService))]
    [Singleton(typeof(DialogService), typeof(IDialogService))]
    [Singleton(typeof(ShareService), typeof(IShareService))]
    [Singleton(typeof(PresenceService), typeof(IPresenceService))]
    [Singleton(typeof(FileDownloader), typeof(IFileDownloader))]
    [Singleton(typeof(SoundVmFactory), typeof(ISoundVmFactory))]
    [Singleton(typeof(GuideVmFactory), typeof(IGuideVmFactory))]
    [Singleton(typeof(CatalogueRowVmFactory), typeof(ICatalogueRowVmFactory))]
    [Singleton(typeof(ChannelVmFactory))]
    [Singleton(typeof(SoundVolumeService), typeof(ISoundVolumeService))]
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
    [Singleton(typeof(SoundMixService), typeof(ISoundMixService))]
    [Singleton(typeof(Renamer), typeof(IRenamer))]
    [Singleton(typeof(UpdateService), typeof(IUpdateService))]
    [Singleton(typeof(ReswLocalizer), typeof(ILocalizer))]
    [Singleton(typeof(FileWriter), typeof(IFileWriter))]
    [Singleton(typeof(FilePicker), typeof(IFilePicker))]
    [Singleton(typeof(FocusToastService), typeof(IFocusToastService))]
    [Singleton(typeof(Services.Uwp.ToastService), typeof(Services.IToastService))]
    [Singleton(typeof(Services.Uwp.Navigator), typeof(Services.INavigator))]
    [Singleton(typeof(CompactNavigator), typeof(ICompactNavigator))]
    [Singleton(typeof(CloudFileWriter), typeof(ICloudFileWriter))]
    [Singleton(typeof(PlayerTelemetryTracker))]
    [Singleton(typeof(SoundEffectsService), typeof(ISoundEffectsService))]
    [Singleton(typeof(PreviewService), typeof(IPreviewService))]
    [Singleton(typeof(StoreService), typeof(IIapService))]
    [Singleton(typeof(WindowsDownloadManager), typeof(IDownloadManager))]
    [Singleton(typeof(ScreensaverService), typeof(IScreensaverService))]
    [Singleton(typeof(SystemInfoProvider), typeof(ISystemInfoProvider))]
    [Singleton(typeof(AssetsReader), typeof(Tools.IAssetsReader))]
    [Singleton(typeof(MixMediaPlayerService), typeof(IMixMediaPlayerService))]
    [Singleton(typeof(WindowsSystemMediaControls), typeof(ISystemMediaControls))]
    [Singleton(typeof(WindowsMediaPlayerFactory), typeof(IMediaPlayerFactory))]
    [Singleton(typeof(SearchService), typeof(ISearchService))]
    [Singleton(typeof(StartupService), typeof(IStartupService))]
    [Singleton(typeof(UriLauncher), typeof(IUriLauncher))]
    [Transient(typeof(SearchPageViewModel))]
    [Singleton(typeof(ResumeOnLaunchService), typeof(IResumeOnLaunchService))]
    [Singleton(typeof(QuickResumeService), typeof(IQuickResumeService))]
    [Singleton(typeof(StreakReminderService), typeof(IStreakReminderService))]
    [Singleton(typeof(BackgroundTaskService), typeof(IBackgroundTaskService))]
    [Singleton(typeof(StatService), typeof(IStatService))]
    [Singleton(typeof(StreakHistoryCache), typeof(IStreakHistoryCache))]
    [Singleton(typeof(StreakHistoryRepository), typeof(IStreakHistoryRepository))]
    [Singleton(typeof(MicrosoftStoreUpdater), typeof(IAppStoreUpdater))]
    [Singleton(typeof(SleepTimerService), typeof(ISleepTimerService))]
    [Transient(typeof(XboxShellPageViewModel))]
    [Singleton(typeof(XboxSlideshowService), typeof(IXboxSlideshowService))]
    [Singleton(typeof(ChannelService), typeof(IChannelService))]
    [Singleton(typeof(ChannelCache), typeof(IChannelCache))]
    [Singleton(typeof(ChannelRepository), typeof(IChannelRepository))]
    [Singleton(typeof(PromoCodeService), typeof(IPromoCodeService))]
    [Singleton(typeof(TimerFactory), typeof(ITimerFactory))]
    [Singleton(typeof(PushNotificationRegistrationService), typeof(IPushNotificationRegistrationService))]
    [Transient(typeof(StatsPageViewModel))]
    private static partial void ConfigureServices(IServiceCollection services);
}
