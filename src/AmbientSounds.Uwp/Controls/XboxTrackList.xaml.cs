using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class XboxTrackList : UserControl
{
    private readonly ImageBrush[] _imageBrushes;

    public XboxTrackList()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
        _imageBrushes = [Image1Brush, Image2Brush, Image3Brush];
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
            index++;
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
            _ = ExpandImage2.StartAsync();
            _ = ExpandImage3.StartAsync();
        }
    }

    private void OnImage1LostFocus(object sender, RoutedEventArgs e)
    {
        _ = CollapseImage2.StartAsync();
        _ = CollapseImage3.StartAsync();
    }
}
