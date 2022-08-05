﻿using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using ComputeSharp;
using ComputeSharp.Uwp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Media.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    public sealed partial class ScreensaverPage : Page
    {
        public ScreensaverPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ScreensaverPageViewModel>();
            ViewModel.Loaded += OnViewModelLoaded;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        public ScreensaverPageViewModel ViewModel => (ScreensaverPageViewModel)this.DataContext;

        private bool IsFullscreen { get; set; }

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
            var device = GraphicsDevice.GetDefault();
            device.DeviceLost += Device_DeviceLost;

            var view = ApplicationView.GetForCurrentView();
            IsFullscreen = view.IsFullScreenMode;

            if (App.IsTenFoot)
            {
                GoBackButton.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            }
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
            var device = GraphicsDevice.GetDefault();
            device.DeviceLost -= Device_DeviceLost;

            SettingsFlyout?.Items?.Clear();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.VideoSource))
            {
                VideoPlayer.MediaPlayer.IsLoopingEnabled = true;
                VideoPlayer.MediaPlayer.Source = MediaSource.CreateFromUri(ViewModel.VideoSource);
            }
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
            this.Bindings.Update();
        }

        private void Device_DeviceLost(object sender, DeviceLostEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();

            telemetry.TrackEvent(TelemetryConstants.ShaderDeviceLost, new Dictionary<string, string>()
            {
                { "reason", e.Reason.ToString() }
            });
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

        private void AnimatedComputeShaderPanel_RenderingFailed(AnimatedComputeShaderPanel sender, RenderingFailedEventArgs args)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();

            telemetry.TrackError(args.Exception, new Dictionary<string, string>()
            {
                { "name", ViewModel.AnimatedBackgroundName ?? string.Empty },
            });

            InfoBar infoBar = (InfoBar)FindName(nameof(RenderingErrorInfoBar));

            infoBar.IsOpen = true;
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
    }
}
