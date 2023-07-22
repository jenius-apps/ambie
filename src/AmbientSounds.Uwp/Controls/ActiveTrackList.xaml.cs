using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class ActiveTrackList : UserControl
    {
        public static readonly DependencyProperty ShowListProperty = DependencyProperty.Register(
            nameof(ShowList),
            typeof(bool),
            typeof(ActiveTrackList),
            new PropertyMetadata(true));

        public event EventHandler? TrackListChanged;

        public ActiveTrackList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
            ViewModel.TrackListChanged += OnTrackListChanged;
            this.Unloaded += OnUnloaded;
        }

        public bool ShowList
        {
            get => (bool)GetValue(ShowListProperty);
            set => SetValue(ShowListProperty, value);
        }

        public ActiveTrackListViewModel ViewModel => (ActiveTrackListViewModel)this.DataContext;

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.TrackListChanged -= OnTrackListChanged;
            ViewModel.Dispose();
        }

        private void OnTrackListChanged(object sender, EventArgs e)
        {
            TrackListChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void OnListLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadPreviousStateAsync();
        }

        public static string FormatDeleteMessage(string soundName)
        {
            return string.Format(Strings.Resources.RemoveActiveButton, soundName);
        }

        private void OnPlaylistClicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement b)
            {
                PlaylistFlyout.ShowAt(b);
            }
        }
    }
}
