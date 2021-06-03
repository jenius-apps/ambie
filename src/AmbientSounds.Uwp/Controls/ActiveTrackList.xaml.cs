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
        public static readonly DependencyProperty ShowListProperty = DependencyProperty.Register(
            nameof(ShowList),
            typeof(bool),
            typeof(ActiveTrackList),
            new PropertyMetadata(true, OnShowListChanged));

        public ActiveTrackList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
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

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
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

        public static string FormatDeleteMessage(string soundName)
        {
            return string.Format(Strings.Resources.RemoveActiveButton, soundName);
        }
    }
}
