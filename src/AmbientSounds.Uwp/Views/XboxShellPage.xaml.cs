using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using Windows.Media.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Views;

public sealed partial class XboxShellPage : Page
{
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

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
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
