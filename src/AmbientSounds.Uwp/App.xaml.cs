using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Services.Uwp;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
sealed partial class App : Application
{
    private static readonly bool _isTenFootPc = false;
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
        this.Resuming += OnResuming;
        this.UnhandledException += OnUnhandledException;
        NetworkHelper.Instance.NetworkChanged += OnNetworkChanged;

        if (IsTenFoot)
        {
            // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/how-to-disable-mouse-mode
            this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
        }
    }

    private void OnNetworkChanged(object sender, EventArgs e)
    {
        //var presence = _serviceProvider?.GetService<IPresenceService>();
        //if (presence is null)
        //{
        //    return;
        //}

        //if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        //{
        //    await presence.EnsureInitializedAsync();
        //}
        //else
        //{
        //    await presence.DisconnectAsync();
        //}
    }

    private void OnResuming(object sender, object e)
    {
        //if (_serviceProvider?.GetService<IPresenceService>() is IPresenceService presenceService)
        //{
        //    await presenceService.EnsureInitializedAsync();
        //}
    }

    private async void OnSuspension(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();
        _playerTracker?.TrackDuration(DateTimeOffset.Now);
        if (_serviceProvider is { } serviceProvider)
        {
            var flushTask = serviceProvider.GetRequiredService<ITelemetry>().FlushAsync();

            if (serviceProvider.GetRequiredService<IFocusService>() is { } focusService &&
                focusService.CurrentState == AmbientSounds.Services.FocusState.Active)
            {
                // We don't support focus sessions when ambie is suspended,
                // and we want to make sure notifications are cancelled.
                // Note: If music is playing, then ambie won't suspend on minimize.
                focusService.PauseTimer();
            }

            await serviceProvider.GetRequiredService<IFocusNotesService>().SaveNotesToStorageAsync();
            //await serviceProvider.GetRequiredService<IPresenceService>().DisconnectAsync();
            await flushTask;
        }

        deferral.Complete();
    }

    public static bool IsDesktop => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop" && !IsTenFoot;

    public static bool IsTenFoot => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" || _isTenFootPc;

    public static bool IsRightToLeftLanguage
    {
        get
        {
            string flowDirectionSetting = ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
            return flowDirectionSetting == "RTL";
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
                if (IsTenFoot)
                {
                    rootFrame.Navigate(typeof(Views.XboxShellPage));
                }
                else
                {
                    ContentPageType? firstPageOverride = LaunchConstants.ToPageType(launchArguments);

                    if (firstPageOverride is null &&
                        _userSettings is { } settings &&
                        settings.Get<string>(UserSettingsConstants.LastUsedContentPageKey) is { Length: > 0 } contentPage &&
                        Enum.TryParse(contentPage, out ContentPageType pageType))
                    {
                        firstPageOverride = pageType;
                    }

                    rootFrame.Navigate(typeof(Views.ShellPage), new ShellPageNavigationArgs
                    {
                        FirstPageOverride = firstPageOverride,
                        LaunchArguments = launchArguments
                    });
                }
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

        // Clear stale toasts
        ToastNotificationManager.History.Clear();

        var resumeService = Services.GetRequiredService<IResumeOnLaunchService>();
        await resumeService.LoadSoundsFromPreviousSessionAsync();
        resumeService.TryResumePlayback(force: launchArguments is LaunchConstants.QuickResumeArgument or LaunchConstants.StreakReminderArgument);

        // Reset tasks on launch
        var bgServices = Services.GetRequiredService<IBackgroundTaskService>();
        bgServices.UnregisterAllTasks();

        if (await bgServices.RequestPermissionAsync() && _userSettings is { } userSettings)
        {
            if (userSettings.Get<bool>(UserSettingsConstants.QuickResumeKey))
            {
                bgServices.ToggleQuickResumeStartupTask(true);
            }

            if (userSettings.Get<bool>(UserSettingsConstants.StreaksReminderEnabledKey))
            {
                bgServices.ToggleStreakReminderTask(true);
            }
        }
    }

    private async void HandleProtocolLaunch(IProtocolActivatedEventArgs protocolArgs)
    {
        try
        {
            var uri = protocolArgs.Uri;
            var arg = protocolArgs.Uri.Query.Replace("?", string.Empty);

            if (Services.GetService<ProtocolLaunchController>() is { } controller)
            {
                if (uri.Host is "share")
                {
                    controller.ProcessShareProtocolArguments(arg);
                }
                else if (uri.Segments.LastOrDefault()?.Contains("autoplay", StringComparison.OrdinalIgnoreCase) == true)
                {
                    await controller.ProcessAutoPlayProtocolArgumentsAsync(arg);
                }
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

    private async void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        if (_serviceProvider?.GetRequiredService<ITelemetry>() is { } telemetry)
        {
            telemetry.TrackError(e.Exception);
            await telemetry.FlushAsync();
        }
    }
}
