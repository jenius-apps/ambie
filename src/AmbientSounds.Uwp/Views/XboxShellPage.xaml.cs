using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
    }

    public XboxShellPageViewModel ViewModel => (XboxShellPageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _ = TrackList.InitializeAsync();
        _ = SlideshowControl.LoadAsync();

        await ViewModel.InitializeAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.Uninitialize();
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
