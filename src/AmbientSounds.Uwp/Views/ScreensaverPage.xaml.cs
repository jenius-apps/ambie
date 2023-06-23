using AmbientSounds.Constants;
using AmbientSounds.Effects;
using AmbientSounds.Services;
using AmbientSounds.Shaders;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using JeniusApps.Common.Telemetry;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class ScreensaverPage : Page
{
    private readonly DisplayRequest _displayRequest;
    private readonly DispatcherQueue _dispatcherQueue;
    //private AnimatedWallpaperEffect? _animatedWallpaperEffect;
    //private double _resolutionScale;

    public ScreensaverPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ScreensaverPageViewModel>();
        IsButtonsHidden = false;
        Queue = DispatcherQueue.GetForCurrentThread();
        SetTimer();
        ViewModel.Loaded += OnViewModelLoaded;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        _displayRequest = new DisplayRequest();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Set the wallpapers to run at 24fps to save resources (the animations are very slow, so not noticeable)
        //WallpaperCanvasControl.TargetElapsedTime = TimeSpan.FromSeconds(1 / 24.0f);

        Unloaded += ScreensaverPage_Unloaded;
    }

    public ScreensaverPageViewModel ViewModel => (ScreensaverPageViewModel)this.DataContext;

    private bool IsFullscreen { get; set; }

    private bool IsButtonsHidden { get; set; }

    private DispatcherQueueTimer? InactiveTimer { get; set; }

    private DispatcherQueue Queue { get; set; }

    private const int SecondsToHide = 5;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = App.Services.GetRequiredService<IUserSettings>();
        await ViewModel.InitializeAsync(settings.Get<string>(UserSettingsConstants.LastUsedScreensaverKey));

        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
        {
            { "name", "screensaver" }
        });

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

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.Loaded -= OnViewModelLoaded;
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;

        var coreWindow = CoreWindow.GetForCurrentThread();
        coreWindow.KeyDown -= CoreWindow_KeyDown;
        coreWindow.SizeChanged -= CoreWindow_SizeChanged;
        var navigator = SystemNavigationManager.GetForCurrentView();
        navigator.BackRequested -= OnBackRequested;
        
        InactiveTimer?.Stop();
        CoreWindow.GetForCurrentThread().PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);

        SettingsFlyout?.Items?.Clear();
        _displayRequest.RequestRelease();
    }

    private void ScreensaverPage_Unloaded(object sender, RoutedEventArgs e)
    {
        // Remove the canvas from the visual tree manually to avoid memory leaks.
        // See: https://microsoft.github.io/Win2D/WinUI2/html/RefCycles.htm.
        //WallpaperCanvasControl.Draw -= CanvasAnimatedControl_Draw;
        //WallpaperCanvasControl.RemoveFromVisualTree();
        //WallpaperCanvasControl = null;

        //// Also dispose the effect to remove pressure from the GC
        //_animatedWallpaperEffect?.Dispose();
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.VideoSource))
        {
            VideoPlayer.MediaPlayer.IsLoopingEnabled = true;
            VideoPlayer.MediaPlayer.Source = MediaSource.CreateFromUri(ViewModel.VideoSource);
        }
        //else if (e.PropertyName == nameof(ViewModel.AnimatedBackgroundName))
        //{
        //    SetupAnimatedShaderProperties();
        //}
    }

    private void OnViewModelLoaded(object sender, EventArgs e)
    {
        if (!ViewModel.SettingsButtonVisible)
        {
            return;
        }

        SettingsFlyout.Items.Clear();

        foreach (var item in ViewModel.MenuItems)
        {
            MenuFlyoutItem menuItem;

            if (item.IsToggle)
            {
                menuItem = new ToggleMenuFlyoutItem()
                {
                    IsChecked = item == ViewModel.CurrentSelection
                };
            }
            else
            {
                menuItem = new MenuFlyoutItem();
            }

            menuItem.DataContext = item;
            menuItem.Text = item.Text;
            menuItem.Click += OnMenuItemClicked;

            SettingsFlyout.Items.Add(menuItem);
        }

        //SetupAnimatedShaderProperties();
    }

    private void OnMenuItemClicked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem flyoutItem &&
            flyoutItem.DataContext is FlyoutMenuItem dc)
        {

            if (flyoutItem is ToggleMenuFlyoutItem)
            {
                foreach (var item in SettingsFlyout.Items)
                {
                    if (item is ToggleMenuFlyoutItem menuItem)
                    {
                        menuItem.IsChecked = menuItem == flyoutItem;
                    }
                }
            }

            dc.Command.Execute(dc.CommandParameter);
        }
    }

    private void OnBackRequested(object sender, BackRequestedEventArgs e)
    {
        e.Handled = true;
        GoBack();
    }

    private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
    {
        if (args.VirtualKey == VirtualKey.Escape)
        {
            GoBack();
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
            telemetry.TrackEvent(TelemetryConstants.ScreensaverFullscreen, new Dictionary<string, string>
            {
                { "id", ViewModel.CurrentSelection?.Id ?? "null" },
                { "name", ViewModel.CurrentSelection?.Text ?? string.Empty }
            });
        }
    }

    private void SetTimer()
    {
        InactiveTimer = Queue.CreateTimer();
        InactiveTimer.Interval = new TimeSpan(0, 0, SecondsToHide);
        InactiveTimer.IsRepeating = false;
        InactiveTimer.Tick += OnInactive;
    }

    private void OnInactive(DispatcherQueueTimer t, object sender)
    {
        if (!IsButtonsHidden)
        {
            GoBackButton.Visibility = Visibility.Collapsed;
            ActionButtons.Visibility = Visibility.Collapsed;
            CoreWindow.GetForCurrentThread().PointerCursor = null;
            IsButtonsHidden = true;
        }

        InactiveTimer?.Stop();
    }

    private void RootPage_OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (IsButtonsHidden)
        {
            GoBackButton.Visibility = Visibility.Visible;
            ActionButtons.Visibility = Visibility.Visible;
            CoreWindow.GetForCurrentThread().PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            IsButtonsHidden = false;
        }

        InactiveTimer?.Start();
    }

    /// <summary>
    /// Configures the shader runner and resolution scale when the control is loaded or the selected shader changes.
    /// </summary>
    //private void SetupAnimatedShaderProperties()
    //{
    //    string? animatedBackgroundName = ViewModel.AnimatedBackgroundName;

    //    // Dispose the existing effect, if there is one
    //    _animatedWallpaperEffect?.Dispose();

    //    // We need explicit references to all type to help the .NET Native linker resolve all type dependencies
    //    _animatedWallpaperEffect = animatedBackgroundName switch
    //    {
    //        nameof(ColorfulInfinity) => new AnimatedWallpaperEffect.For<ColorfulInfinity>((width, height, time) => new ColorfulInfinity((float)time.TotalSeconds / 16f, new int2(width, height))),
    //        nameof(ProteanClouds) => new AnimatedWallpaperEffect.For<ProteanClouds>((width, height, time) => new ProteanClouds((float)time.TotalSeconds / 16f, new int2(width, height))),
    //        _ => null
    //    };

    //    // Configure the resolution scale, to save GPU computation. The scale is picked to
    //    // maintain enough visual quality, so it depends on the visuals of each shader.
    //    // In general, shaders with more fine grained details need a higher resolution.
    //    _resolutionScale = animatedBackgroundName switch
    //    {
    //        nameof(ColorfulInfinity) => 0.5,
    //        nameof(ProteanClouds) => 0.4,
    //        _ => 1.0
    //    };
    //}

    //private void CanvasAnimatedControl_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    //{
    //    // We do need an effect to draw (which should be available)
    //    if (_animatedWallpaperEffect is not { } animatedWallpaperEffect)
    //    {
    //        return;
    //    }

    //    try
    //    {
    //        double resolutionScale = _resolutionScale;
    //        Size canvasSize = sender.Size;
    //        Size renderSize = new(canvasSize.Width * resolutionScale, canvasSize.Height * resolutionScale);

    //        // Set the constant buffer
    //        animatedWallpaperEffect.ElapsedTime = args.Timing.TotalTime;
    //        animatedWallpaperEffect.ScreenWidth = sender.ConvertDipsToPixels((float)renderSize.Width, CanvasDpiRounding.Round);
    //        animatedWallpaperEffect.ScreenHeight = sender.ConvertDipsToPixels((float)renderSize.Height, CanvasDpiRounding.Round);

    //        // Draw the shader with the requested resolution scale
    //        args.DrawingSession.DrawImage(
    //            image: animatedWallpaperEffect,
    //            destinationRectangle: new Rect(0, 0, canvasSize.Width, canvasSize.Height),
    //            sourceRectangle: new Rect(0, 0, renderSize.Width, renderSize.Height));
    //    }
    //    catch (Exception e)
    //    {
    //        // Pause rendering
    //        sender.Paused = true;

    //        // Log the error to telemetry
    //        App.Services.GetRequiredService<ITelemetry>().TrackError(e, new Dictionary<string, string>()
    //        {
    //            { "name", animatedWallpaperEffect.EffectName },
    //            { "deviceLostReason", $"0x{args.DrawingSession.Device.GetDeviceLostReason():X8}" }
    //        });

    //        // Show the error banner. For this we need to first move back to the UI thread,
    //        // as the drawing handlers are invoked by a render thread spun up by Win2D.
    //        _ = _dispatcherQueue.TryEnqueue(() =>
    //        {
    //            ((InfoBar)FindName(nameof(RenderingErrorInfoBar))).IsOpen = true;
    //        });
    //    }
    //}
}
