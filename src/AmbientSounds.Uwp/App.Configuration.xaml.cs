﻿using AmbientSounds.Services.Uwp;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Telemetry.Uwp;
using JeniusApps.Common.Telemetry;
using Microsoft.AppCenter;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.Globalization;
using AmbientSounds.ViewModels;
using CommunityToolkit.Extensions.DependencyInjection;
using System.Net.Http;
using JeniusApps.Common.Tools.Uwp;
using AmbientSounds.Tools.Uwp;
using AmbientSounds.Tools;
using JeniusApps.Common.Tools;
using AmbientSounds.Factories;
using AmbientSounds.Cache;
using AmbientSounds.Repositories;

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
    [Transient(typeof(ShellPageViewModel))]
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
    [Singleton(typeof(FocusTaskService), typeof(IFocusTaskService))]
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
    [Singleton(typeof(BackgroundTaskService), typeof(IBackgroundTaskService))]
    [Singleton(typeof(StatService), typeof(IStatService))]
    [Singleton(typeof(StreakHistoryCache), typeof(IStreakHistoryCache))]
    [Singleton(typeof(StreakHistoryRepository), typeof(IStreakHistoryRepository))]
    private static partial void ConfigureServices(IServiceCollection services);
}
