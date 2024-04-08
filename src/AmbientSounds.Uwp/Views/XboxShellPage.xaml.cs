using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace AmbientSounds.Views;

public sealed partial class XboxShellPage : Page
{
    public XboxShellPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<XboxShellPageViewModel>();
    }

    public XboxShellPageViewModel ViewModel => (XboxShellPageViewModel)this.DataContext;

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
