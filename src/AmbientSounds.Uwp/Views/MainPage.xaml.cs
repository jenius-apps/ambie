using AmbientSounds.Animations;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.ComponentModel;
using Windows.Foundation.Metadata;
using Windows.UI.Shell;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<MainPageViewModel>();

            if (App.IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/turn-off-overscan
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.MaxTeachingTipOpen))
            {
                FlyoutBase.ShowAttachedFlyout(ActiveList);
            }
        }

        public bool IsNotTenFoot => !App.IsTenFoot;

        public MainPageViewModel ViewModel => (MainPageViewModel)this.DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.StartTimer();
            ViewModel.PropertyChanged += OnPropertyChanged;

            if (e.NavigationMode == NavigationMode.New)
            {
                TryShowPinTeachingTip();
            }

            TryStartPageAnimations();

            if (App.Services.GetRequiredService<IUserSettings>().Get<bool>(UserSettingsConstants.PerformanceMode))
            {
                SolidColorBrush mycolor = App.Current.Resources["FallbackBackgroundBrush"] as SolidColorBrush;
                if (mycolor != null)
                {
                    this.Background = mycolor;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.StopTimer();
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        private void TryStartPageAnimations()
        {
            var animation = ConnectedAnimationService
                .GetForCurrentView()
                .GetAnimation(AnimationConstants.CatalogueBack);

            if (animation != null)
            {
                animation.TryStart(CatalogueButton);
            }
        }

        private void GridScaleUp(object sender, PointerRoutedEventArgs e) 
            => SoundItemAnimations.ItemScaleUp(sender as UIElement, 1.1f, e.Pointer);

        private void GridScaleNormal(object sender, PointerRoutedEventArgs e) 
            => SoundItemAnimations.ItemScaleNormal(sender as UIElement);

        private void Flyout_Opened(object sender, object e)
        {
            // Ref: https://thinkrethink.net/2019/02/18/wpf-liferegionchanged-automationpeer-status-changes-announced/
            var peer = FrameworkElementAutomationPeer.FromElement(LimitWarningText);
            peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }

        private async void TryShowPinTeachingTip()
        {
            var tbmgr = TaskbarManager.GetDefault();
            var isPinned = await tbmgr.IsCurrentAppPinnedAsync();

            if (SystemInformation.Instance.IsFirstRun &&
                ApiInformation.IsTypePresent("Windows.UI.Shell.TaskbarManager") &&
                tbmgr.IsPinningAllowed &&
                !isPinned)
            {
                PinTeachingTip.IsOpen = true;
                App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.LaunchMessageShown);
            }
        }

        private async void PinTeachingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            sender.IsOpen = false;

            var tbmgr = TaskbarManager.GetDefault();
            var isPinned = await tbmgr.IsCurrentAppPinnedAsync();

            if (ApiInformation.IsTypePresent("Windows.UI.Shell.TaskbarManager") &&
                tbmgr.IsPinningAllowed &&
                !isPinned)
            {
                await tbmgr.RequestPinCurrentAppAsync();
                App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.PinnedToTaskbar);
            }
        }
    }
}
