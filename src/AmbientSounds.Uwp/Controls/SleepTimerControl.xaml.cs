using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SleepTimerControl : UserControl
    {
        private readonly IUserSettings _userSettings;

        public SleepTimerControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SleepTimerViewModel>();
            _userSettings = App.Services.GetRequiredService<IUserSettings>();
            this.Loaded += (_, _) =>
            {
                _userSettings.SettingSet += OnSettingSet;
                LoadAutomationNames();
                ViewModel.Initialize();
                UpdateBackgroundState();
            };
            this.Unloaded += (_, _) =>
            {
                _userSettings.SettingSet -= OnSettingSet;
                ViewModel.Dispose();
            };
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

        public SleepTimerViewModel ViewModel => (SleepTimerViewModel)this.DataContext;

        private string PlayTimerAutomationName { get; set; } = string.Empty;

        private string StopTimerAutomationName { get; set; } = string.Empty;

        private void LoadAutomationNames()
        {
            var loader = ResourceLoader.GetForCurrentView();
            PlayTimerAutomationName = loader.GetString("PlayTimerButton/AutomationProperties/Name");
            StopTimerAutomationName = loader.GetString("StopTimerButton/AutomationProperties/Name");
            this.Bindings.Update();
        }
    }
}
