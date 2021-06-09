using AmbientSounds.Animations;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Shell;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IUserSettings _userSettings;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<MainPageViewModel>();
            _userSettings = App.Services.GetRequiredService<IUserSettings>();
        }

        public bool IsNotTenFoot => !App.IsTenFoot;

        public MainPageViewModel ViewModel => (MainPageViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateBackgroundState();
            _userSettings.SettingSet += OnSettingSet;
            ViewModel.Initialize();
            ViewModel.StartTimer();

            if (e.NavigationMode == NavigationMode.New)
            {
                await TryShowPinTeachingTip();
            }

            TryStartPageAnimations();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _userSettings.SettingSet -= OnSettingSet;
            ViewModel.StopTimer();
            ViewModel.Dispose();
        }

        private void OnSettingSet(object sender, string settingKey)
        {
            if (settingKey == UserSettingsConstants.BackgroundImage)
            {
                UpdateBackgroundState();
            }
        }

        private void UpdateBackgroundState()
        {
            bool backgroundImageActive = !string.IsNullOrEmpty(_userSettings.Get<string>(UserSettingsConstants.BackgroundImage));
            VisualStateManager.GoToState(
                this,
                backgroundImageActive ? nameof(ImageBackgroundState) : nameof(RegularBackgroundState),
                false);
        }

        private void TryStartPageAnimations()
        {
            var animation = ConnectedAnimationService
                .GetForCurrentView()
                .GetAnimation(AnimationConstants.CatalogueBack);

            if (animation is not null)
            {
                animation.TryStart(HomeBackplate);
            }
        }

        private async Task TryShowPinTeachingTip()
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

        private void GoToCatalogue(object sender, RoutedEventArgs e)
        {
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.MoreSoundsClicked);

            var animation = ConnectedAnimationService
                .GetForCurrentView()
                .PrepareToAnimate(AnimationConstants.CatalogueForward, HomeBackplate);
            animation.Configuration = new DirectConnectedAnimationConfiguration();

            ViewModel.ToCatalogue();
        }
    }
}
