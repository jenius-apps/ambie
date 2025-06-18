using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class ScreensaverPage : Page
{
    private const int SecondsToHide = 3;
    private readonly DisplayRequest _displayRequest;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly SemaphoreSlim _blockPointerLock = new(1, 1);

    public ScreensaverPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ScreensaverPageViewModel>();
        Queue = DispatcherQueue.GetForCurrentThread();
        SetTimer();
        ViewModel.Loaded += OnViewModelLoaded;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        _displayRequest = new DisplayRequest();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        ScreensaverControl.ImageChanged += OnImageChanged;
    }

    public ScreensaverPageViewModel ViewModel => (ScreensaverPageViewModel)this.DataContext;

    private bool IsFullscreen { get; set; }

    private bool IsButtonsHidden { get; set; }

    private DispatcherQueueTimer? InactiveTimer { get; set; }

    private DispatcherQueue Queue { get; set; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _ = PlayConnectedAnimationAsync();
        ClockControl.Initialize();

        var settings = App.Services.GetRequiredService<IUserSettings>();
        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackPageView(nameof(ScreensaverPage));
        telemetry.TrackEvent(TelemetryConstants.NavigatedToChannelViewer, new Dictionary<string, string>
        {
            { "clockEnabled", settings.Get<bool>(UserSettingsConstants.ChannelClockEnabledKey).ToString() },
        });

        if (e.Parameter is ScreensaverArgs args)
        {
            await ViewModel.InitializeAsync(args);
        }
        else
        {
            await ViewModel.InitializeAsync(settings.Get<string>(UserSettingsConstants.LastUsedChannelKey));
        }
        await FocusTimerWidget.InitializeAsync(allowSoundPausing: false);

        var coreWindow = CoreWindow.GetForCurrentThread();
        coreWindow.KeyDown += CoreWindow_KeyDown;
        coreWindow.SizeChanged += CoreWindow_SizeChanged;
        var navigator = SystemNavigationManager.GetForCurrentView();
        navigator.BackRequested += OnBackRequested;

        var view = ApplicationView.GetForCurrentView();
        IsFullscreen = view.IsFullScreenMode;

        if (App.IsTenFoot)
        {
            GoBackButton.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        _displayRequest.RequestActive();
    }

    private async Task PlayConnectedAnimationAsync()
    {
        if (ConnectedAnimationService.GetForCurrentView().GetAnimation("channelVideoClicked") is ConnectedAnimation animation)
        {
            VideoPlaceholderImage.Visibility = Visibility.Visible;
            animation.TryStart(VideoPlaceholderImage);
            _ = VideoShow.StartAsync();
            await VideoPlaceholderHide.StartAsync();
            VideoPlaceholderImage.Visibility = Visibility.Collapsed;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (App.Services.GetRequiredService<IMixMediaPlayerService>() is { FeaturedSoundType: FeaturedSoundType.Channel } player)
        {
            // This ensures that channel sounds are always paused
            // when leaving the screensaver page, which is by design.
            player.StopFeaturedSound();
        }

        ClockControl.Uninitialize();
        ScreensaverControl?.Uninitialize();
        ViewModel.Loaded -= OnViewModelLoaded;
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        ViewModel.Uninitialize();

        var coreWindow = CoreWindow.GetForCurrentThread();
        coreWindow.KeyDown -= CoreWindow_KeyDown;
        coreWindow.SizeChanged -= CoreWindow_SizeChanged;
        var navigator = SystemNavigationManager.GetForCurrentView();
        navigator.BackRequested -= OnBackRequested;

        StopHideCursorTimer();

        _displayRequest.RequestRelease();
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.VideoSource))
        {
            VideoPlayer.MediaPlayer.IsLoopingEnabled = true;
            VideoPlayer.MediaPlayer.Source = MediaSource.CreateFromUri(ViewModel.VideoSource);
        }
        else if (e.PropertyName == nameof(ViewModel.DialogOpen))
        {
            if (ViewModel.DialogOpen)
            {
                StopHideCursorTimer();
                ShowButtonsAndCursor();
            }
        }
    }

    private void OnViewModelLoaded(object sender, EventArgs e)
    {
        if (!ViewModel.SettingsButtonVisible)
        {
            return;
        }
    }

    private void OnBackRequested(object sender, BackRequestedEventArgs e)
    {
        e.Handled = true;
        GoBack();
    }

    private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
    {
        if (args.VirtualKey == VirtualKey.Escape && !ViewModel.DialogOpen)
        {
            if (ApplicationView.GetForCurrentView() is { IsFullScreenMode: true } view)
            {
                view.ExitFullScreenMode();
            }

            args.Handled = true;
        }
    }

    private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
    {
        var view = ApplicationView.GetForCurrentView();

        IsFullscreen = view.IsFullScreenMode;

        // We hide the back button when in full screen mode
        // to avoid the opacity animation bug that occurs
        // when navigating back to home page while in full screen mode.
        GoBackButton.Visibility = view.IsFullScreenMode
            ? Visibility.Collapsed
            : Visibility.Visible;

        this.Bindings.Update();
    }

    private void GoBack()
    {
        var view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
        {
            view.ExitFullScreenMode();
        }

        var navigator = App.Services.GetRequiredService<INavigator>();
        navigator.GoBack(nameof(ScreensaverPage));
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        GoBack();
    }

    private void OnToggleFullscreen(object sender, RoutedEventArgs e)
    {
        var view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
        {
            view.ExitFullScreenMode();
        }
        else
        {
            view.TryEnterFullScreenMode();
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.ChannelViewerFullScreen);
        }
    }

    private void SetTimer()
    {
        InactiveTimer = Queue.CreateTimer();
        InactiveTimer.Interval = new TimeSpan(0, 0, SecondsToHide);
        InactiveTimer.IsRepeating = false;
        InactiveTimer.Tick += OnInactive;
    }

    private async void OnInactive(DispatcherQueueTimer t, object sender)
    {
        if (ViewModel.DialogOpen)
        {
            return;
        }

        StopHideCursorTimer();

        if (!IsButtonsHidden)
        {
            await _blockPointerLock.WaitAsync();
            await HideButtonsAndCursorAsync();
            await Task.Delay(1000);
            _blockPointerLock.Release();
        }
    }

    private async void OnImageChanged(object sender, EventArgs e)
    {
        await _blockPointerLock.WaitAsync();
        await Task.Delay(1000);
        _blockPointerLock.Release();
    }

    private void RootPage_OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.DialogOpen ||
            _blockPointerLock.CurrentCount == 0)
        {
            return;
        }

        if (IsButtonsHidden)
        {
            ShowButtonsAndCursor();
        }

        StartHideCursorTimer();
    }

    private void ShowButtonsAndCursor()
    {
        IsButtonsHidden = false;

        if (TopPanel.Visibility is Visibility.Collapsed)
        {
            TopPanel.Visibility = Visibility.Visible;
            _ = TopPanelShow.StartAsync();
        }

        if (VideosGrid.Visibility is Visibility.Collapsed)
        {
            VideosGrid.Visibility = Visibility.Visible;
            _ = BottomPanelShow.StartAsync();
        }
    }

    private async Task HideButtonsAndCursorAsync()
    {
        IsButtonsHidden = true;

        await Task.WhenAll(FadeOutAsync(TopPanelHide, TopPanel), FadeOutAsync(BottomPanelHide, VideosGrid));
    }

    private async Task FadeOutAsync(AnimationSet fadeOutAnimation, UIElement element)
    {
        if (element.Visibility is Visibility.Visible)
        {
            await fadeOutAnimation.StartAsync();
            element.Visibility = Visibility.Collapsed;
        }
    }

    private void StopHideCursorTimer() => InactiveTimer?.Stop();
    private void StartHideCursorTimer() => InactiveTimer?.Start();
}
