using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class ActiveTrackList : UserControl
    {
        private readonly IUserSettings _userSettings;

        public static readonly DependencyProperty ShowListProperty = DependencyProperty.Register(
            nameof(ShowList),
            typeof(bool),
            typeof(ActiveTrackList),
            new PropertyMetadata(true, OnShowListChanged));

        public ActiveTrackList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
            _userSettings = App.Services.GetRequiredService<IUserSettings>();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        public bool ShowList
        {
            get => (bool)GetValue(ShowListProperty);
            set => SetValue(ShowListProperty, value);
        }

        public ActiveTrackListViewModel ViewModel => (ActiveTrackListViewModel)this.DataContext;

        private static void OnShowListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ActiveTrackList atl)
            {
                atl.UpdateStates();
            }
        }

        private void UpdateStates()
        {
            if (ShowList)
            {
                VisualStateManager.GoToState(this, nameof(ShowListState), false);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(HideListState), false);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _userSettings.SettingSet += OnSettingSet;
            UpdateBackgroundState();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _userSettings.SettingSet -= OnSettingSet;
            ViewModel.Dispose();
        }

        private void NameInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.SaveCommand.ExecuteAsync(NameInput.Text);
                e.Handled = true;
                SaveFlyout.Hide();
            }
        }

        private void SaveFlyout_Closed(object sender, object e)
        {
            NameInput.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFlyout.Hide();
        }

        private async void OnListLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadPreviousStateAsync();
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

        public static string FormatDeleteMessage(string soundName)
        {
            return string.Format(Strings.Resources.RemoveActiveButton, soundName);
        }
    }
}
