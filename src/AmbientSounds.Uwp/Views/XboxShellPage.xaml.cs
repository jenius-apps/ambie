﻿using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class XboxShellPage : Page
{
    private readonly IReadOnlyList<(SlideshowMode, UIElement, AnimationSet)> _fadeOutCombos;
    private readonly IReadOnlyList<(SlideshowMode, UIElement, AnimationSet)> _fadeInCombos;
    private CancellationTokenSource _slideshowTransitionCts = new();
    private SemaphoreSlim _slideshowTransitionLock = new(1, 1);

    public XboxShellPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<XboxShellPageViewModel>();

        if (App.IsTenFoot)
        {
            // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/turn-off-overscan
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
        }

        VideoPlayer.MediaPlayer.IsLoopingEnabled = true;

        _fadeOutCombos = 
        [
            (SlideshowMode.Video, VideoPlayer, VideoFadeOut),
            (SlideshowMode.Images, SlideshowControl, SlideshowFadeOut)
        ];

        _fadeInCombos =
        [
            (SlideshowMode.Video, VideoPlayer, VideoFadeIn),
            (SlideshowMode.Images, SlideshowControl, SlideshowFadeIn)
        ];
    }

    public XboxShellPageViewModel ViewModel => (XboxShellPageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        _ = TrackList.InitializeAsync();
        _ = SlideshowControl.LoadAsync();

        await ViewModel.InitializeAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.PropertyChanged -= OnPropertyChanged;
        ViewModel.Uninitialize();
    }

    private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewModel.VideoSource) &&
            ViewModel.VideoSource is { Length: > 0 } source)
        {
            try
            {
                VideoPlayer.Source = MediaSource.CreateFromUri(new Uri(source));
            }
            catch (UriFormatException) { }
        }
        else if (e.PropertyName is nameof(ViewModel.SlideshowMode))
        {
            _slideshowTransitionCts.Cancel();
            _slideshowTransitionCts = new();
            await _slideshowTransitionLock.WaitAsync();

            try
            {
                await UpdateSlideshowModeAsync(_slideshowTransitionCts.Token);
            }
            catch (OperationCanceledException) 
            { 
            }
            finally
            {
                _slideshowTransitionLock.Release();
            }
        }
    }

    private async Task UpdateSlideshowModeAsync(CancellationToken ct)
    {
        foreach (var combo in _fadeOutCombos)
        {
            await TryFadeOutAsync(combo.Item1, combo.Item2, combo.Item3, ct);
        }

        foreach (var combo in _fadeInCombos)
        {
            if (await TryTriggerFadeInAsync(combo.Item1, combo.Item2, combo.Item3, ct))
            {
                break;
            }
        }
    }

    private async Task TryFadeOutAsync(SlideshowMode mode, UIElement control, AnimationSet fadeOutAnimation, CancellationToken ct)
    {
        if (ViewModel.SlideshowMode != mode &&
            control.Visibility is Visibility.Visible)
        {
            try
            {
                await fadeOutAnimation.StartAsync(ct);
            }
            catch (OperationCanceledException) { }

            control.Visibility = Visibility.Collapsed;
            ct.ThrowIfCancellationRequested();
        }
    }

    private async Task<bool> TryTriggerFadeInAsync(SlideshowMode mode, UIElement control, AnimationSet fadeInAnimation, CancellationToken ct)
    {
        if (ViewModel.SlideshowMode == mode)
        {
            control.Visibility = Visibility.Visible;
            await fadeInAnimation.StartAsync(ct);

            return true;
        }

        return false;
    }

    private async void OnMoreSoundsClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (App.Services.GetRequiredService<INavigator>().RootFrame is Frame root)
        {
            _ = FadeOutAnimation.StartAsync();
            _ = ActionBarExitAnimation.StartAsync();
            await SoundGridExitAnimation.StartAsync();
            root.Navigate(typeof(XboxCataloguePage), null, new SuppressNavigationTransitionInfo());
        }
    }

    private void OnXboxSoundItemFocused(object sender, RoutedEventArgs e)
    {
        if (sender is GridViewItem { DataContext: SoundViewModel vm, FocusState: Windows.UI.Xaml.FocusState.Keyboard })
        {
            vm.IsKeyPadFocused = true;
        }
    }

    private void OnXboxSoundItemLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is GridViewItem { DataContext: SoundViewModel vm })
        {
            vm.IsKeyPadFocused = false;
        }
    }
}
