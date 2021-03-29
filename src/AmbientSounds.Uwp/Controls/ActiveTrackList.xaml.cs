using AmbientSounds.Animations;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class ActiveTrackList : UserControl
    {
        public ActiveTrackList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
        }

        public ActiveTrackListViewModel ViewModel => (ActiveTrackListViewModel)this.DataContext;

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(ActiveTrackList),
            new PropertyMetadata(Orientation.Horizontal));

        private async void UserControl_Loaded()
        {
            await ViewModel.LoadPreviousStateAsync();
            ViewModel.ActiveTracks.CollectionChanged += OnCollectedChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ActiveTracks.CollectionChanged -= OnCollectedChanged;
        }

        private async void OnCollectedChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && !ViewModel.IsMix)
            {
                var item = e.NewItems[0];
                TrackList.ScrollIntoView(item);
                var animation = ConnectedAnimationService
                    .GetForCurrentView()
                    .GetAnimation(AnimationConstants.TrackListItemLoad);

                if (animation != null)
                {
                    await TrackList.TryStartConnectedAnimationAsync(
                        animation, item, "ImagePanel");
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add && ViewModel.IsMix)
            {
                var item = e.NewItems[0];
                string animationString;

                if (e.NewStartingIndex == 0) animationString = AnimationConstants.TrackListItemLoad;
                else if (e.NewStartingIndex == 1) animationString = AnimationConstants.TrackListItem2Load;
                else animationString = AnimationConstants.TrackListItem3Load;

                var animation = ConnectedAnimationService
                    .GetForCurrentView()
                    .GetAnimation(animationString);

                if (animation != null)
                {
                    await TrackList.TryStartConnectedAnimationAsync(
                        animation, item, "ImagePanel");
                }
            }
        }

        private void NameInput_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
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

        private void RemoveOnMiddleClick(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Image fe && fe.DataContext is ActiveTrackViewModel vm)
            {
                var pointer = e.GetCurrentPoint(fe);
                if (pointer.Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed)
                {
                    vm.RemoveCommand.Execute(vm.Sound);
                    e.Handled = true;
                }
            }
        }
    }
}
