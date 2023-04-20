﻿using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace AmbientSounds.Controls;

public sealed partial class FocusTimerModule : UserControl, ICanInitialize
{
    private static readonly Dictionary<string, string> CommonTelemetryContent = new()
    {
        { "page", "focus" }
    };

    public FocusTimerModule()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<FocusTimerModuleViewModel>();
    }

    public FocusTimerModuleViewModel ViewModel => (FocusTimerModuleViewModel)this.DataContext;

    private bool IsDesktop => App.IsDesktop;

    public async Task InitializeAsync()
    {
        await ViewModel.InitializeAsync();
    }

    public void Uninitialize() => ViewModel.Uninitialize();

    private void StartTutorial(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.CanStartTutorial())
        {
            CloseAll();
            return;
        }

        ViewModel.IsHelpMessageVisible = false;
        TeachingTip1.IsOpen = true;
        TeachingTip2.IsOpen = false;
        TeachingTip3.IsOpen = false;
        TeachingTip4.IsOpen = false;

        App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.FocusTutorialStarted);
    }

    private void ShowTip2(TeachingTip sender, object args)
    {
        if (!ViewModel.CanStartTutorial())
        {
            CloseAll();
            return;
        }

        TeachingTip1.IsOpen = false;
        TeachingTip2.IsOpen = true;
        TeachingTip3.IsOpen = false;
        TeachingTip4.IsOpen = false;
    }

    private void ShowTip3(TeachingTip sender, object args)
    {
        if (!ViewModel.CanStartTutorial())
        {
            CloseAll();
            return;
        }

        TeachingTip1.IsOpen = false;
        TeachingTip2.IsOpen = false;
        TeachingTip3.IsOpen = true;
        TeachingTip4.IsOpen = false;
    }

    private void ShowTip4(TeachingTip sender, object args)
    {
        if (!ViewModel.CanStartTutorial())
        {
            CloseAll();
            return;
        }

        TeachingTip1.IsOpen = false;
        TeachingTip2.IsOpen = false;
        TeachingTip3.IsOpen = false;
        TeachingTip4.IsOpen = true;
    }

    private void OnTutorialEnded(TeachingTip sender, object args)
    {
        CloseAll();
        App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.FocusTutorialEnded);
    }

    private void CloseAll()
    {
        TeachingTip1.IsOpen = false;
        TeachingTip2.IsOpen = false;
        TeachingTip3.IsOpen = false;
        TeachingTip4.IsOpen = false;
    }

    private void OnResetClicked(object sender, RoutedEventArgs e)
    {
        StartButton.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        ViewModel.Stop();
    }

    private void OnRecentClicked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is RecentFocusSettingsViewModel s)
        {
            ViewModel.LoadRecentSettings(s);
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.FocusRecentClicked, new Dictionary<string, string>
            {
                { "index", ViewModel.RecentSettings.IndexOf(s).ToString() }
            });
        }
    }

    private void OnSettingsClicked(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton hb)
        {
            FlyoutBase.ShowAttachedFlyout(hb);
        }
    }

    private void OnSettingsFlyoutItemToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleMenuFlyoutItem item && item.Tag is string tag)
        {
            if (tag == "compact")
            {
                App.Services.GetRequiredService<ITelemetry>().TrackEvent(
                    item.IsChecked ? TelemetryConstants.MiniAutoEnabled : TelemetryConstants.MiniAutoDisabled,
                    CommonTelemetryContent);
            }
            else if (tag == "playAfterFocus")
            {
                App.Services.GetRequiredService<ITelemetry>().TrackEvent(
                    item.IsChecked ? TelemetryConstants.PlayAfterFocusEnabled : TelemetryConstants.PlayAfterFocusDisabled,
                    CommonTelemetryContent);
            }
        }
    }
}
