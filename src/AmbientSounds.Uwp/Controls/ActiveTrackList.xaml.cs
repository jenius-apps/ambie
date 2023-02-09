using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
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
    }
}
