using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class XboxTrackList : UserControl
{
    private readonly ImageBrush[] _imageBrushes;
    private readonly ContentControl[] _contentControls;
    private bool _doNotRunExpandAnimation;
    private bool _volumeFlyoutOpen;
    private ActiveTrackViewModel? _currentVolumeSliderDataSource;

    public XboxTrackList()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
        _imageBrushes = [Image1Brush, Image2Brush, Image3Brush];
        _contentControls = [ContentControl1, ContentControl2, ContentControl3];
    }

    public ActiveTrackListViewModel ViewModel => (ActiveTrackListViewModel)this.DataContext;

    public async Task InitializeAsync()
    {
        await ViewModel.LoadPreviousStateAsync();
        LoadTracks();
        ViewModel.ActiveTracks.CollectionChanged += OnCollectionChanged;
    }

    private void LoadTracks()
    {
        int index = 0;
        foreach (var track in ViewModel.ActiveTracks.Reverse())
        {
            if (index >= _imageBrushes.Length)
            {
                break;
            }

            _imageBrushes[index].ImageSource = new BitmapImage(new Uri(track.ImagePath));
            _contentControls[index].Visibility = Visibility.Visible;
            index++;
        }

        // Ensure unused content controls aren't visible.
        for (int i = index; i < _contentControls.Length; i++)
        {
            _contentControls[i].Visibility = Visibility.Collapsed;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        LoadTracks();
    }

    public void Uninitialize()
    {
        ViewModel.ActiveTracks.CollectionChanged -= OnCollectionChanged;
        ViewModel.Dispose();
    }

    private void OnImage1GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is ContentControl { FocusState: Windows.UI.Xaml.FocusState.Keyboard })
        {
            if (_doNotRunExpandAnimation)
            {
                _doNotRunExpandAnimation = false; // reset
                return;
            }

            ContentControl2.IsTabStop = true;
            ContentControl3.IsTabStop = true;
            _ = ExpandImage2.StartAsync();
            _ = ExpandImage3.StartAsync();
            UpdateInstructionVisibility();
        }
    }

    private void OnImage1LostFocus(object sender, RoutedEventArgs e)
    {
        if (!_volumeFlyoutOpen &&
            ContentControl2.FocusState is FocusState.Unfocused &&
            ContentControl3.FocusState is FocusState.Unfocused)
        {
            _ = CollapseImage2.StartAsync();
            _ = CollapseImage3.StartAsync();
            ContentControl2.IsTabStop = false;
            ContentControl3.IsTabStop = false;
            UpdateInstructionVisibility();
        }
    }

    private void OnNonImage1LostFocus(object sender, RoutedEventArgs e)
    {
        if (!_volumeFlyoutOpen && 
            ContentControl1.FocusState is FocusState.Unfocused &&
            ContentControl2.FocusState is FocusState.Unfocused &&
            ContentControl3.FocusState is FocusState.Unfocused)
        {
            _ = CollapseImage2.StartAsync();
            _ = CollapseImage3.StartAsync();
            ContentControl2.IsTabStop = false;
            ContentControl3.IsTabStop = false;
            UpdateInstructionVisibility();
        }
    }

    private void OnImage1GettingFocus(UIElement sender, GettingFocusEventArgs args)
    {
        if (ContentControl2.FocusState is FocusState.Keyboard)
        {
            // When the user is navigating UP
            // from image 2 to image 1, then
            // ensure we don't rerun the expand animation
            // since the images are already expanded
            _doNotRunExpandAnimation = true;
        }
    }

    private async void UpdateInstructionVisibility()
    {
        var newVisibility =
            ContentControl1.FocusState is FocusState.Unfocused &&
            ContentControl2.FocusState is FocusState.Unfocused &&
            ContentControl3.FocusState is FocusState.Unfocused
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (newVisibility is Visibility.Collapsed)
        {
            await InstructionFadeOut.StartAsync();
        }

        VolumeInstruction.Visibility = newVisibility;
    }

    private void OnImageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.OriginalKey is VirtualKey.GamepadA || e.Key is VirtualKey.GamepadA)
        {
            e.Handled = true;
            int itemIndex;
            if (sender == ContentControl1)
            {
                itemIndex = ViewModel.ActiveTracks.Count - 1;
            }
            else if (sender == ContentControl2)
            {
                itemIndex = ViewModel.ActiveTracks.Count - 2;
            }
            else
            {
                itemIndex = 0;
            }

            if (ViewModel.ActiveTracks.Count <= itemIndex)
            {
                // invalid index, so do nothing.
                return;
            }

            _doNotRunExpandAnimation = true;
            _volumeFlyoutOpen = true;
            _currentVolumeSliderDataSource = ViewModel.ActiveTracks[itemIndex];
            VolumeSlider.Value = _currentVolumeSliderDataSource.Volume;
            VolumeSlider.ValueChanged += OnSliderValueChanged;
            VolumeFlyout.ShowAt(sender as FrameworkElement);
        }
    }

    private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_currentVolumeSliderDataSource is null)
        {
            return;
        }

        _currentVolumeSliderDataSource.Volume = e.NewValue;
    }

    private void OnVolumeFlyoutClosed(object sender, object e)
    {
        VolumeSlider.ValueChanged -= OnSliderValueChanged;
        _currentVolumeSliderDataSource = null;
        _volumeFlyoutOpen = false;
    }
}
