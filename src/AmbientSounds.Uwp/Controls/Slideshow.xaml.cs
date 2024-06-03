using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class Slideshow : UserControl
{
    private const int SlideTimeLength = 30000; // 30 seconds
    private readonly ITimerService _timerService;
    private readonly IMixMediaPlayerService _mediaPlayerService;
    private readonly ITelemetry _telemetry;
    private readonly ISoundService _soundDataProvider;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly SemaphoreSlim _soundsChangedLock = new(1, 1);
    private CancellationTokenSource _loadingCts = new();
    private IList<string>? _images;
    private int _imageIndex1;
    private int _imageIndex2;

    public static readonly DependencyProperty Image1SourceProperty = DependencyProperty.Register(
        nameof(Image1Source),
        typeof(string),
        typeof(Slideshow),
        new PropertyMetadata("http://localhost"));

    public static readonly DependencyProperty Image2SourceProperty = DependencyProperty.Register(
        nameof(Image2Source),
        typeof(string),
        typeof(Slideshow),
        new PropertyMetadata("http://localhost"));

    public Slideshow()
    {
        this.InitializeComponent();
        this.SizeChanged += OnSizeChanged;
        this.Unloaded += OnUnloaded;

        _timerService = App.Services.GetRequiredService<ITimerService>();
        _mediaPlayerService = App.Services.GetRequiredService<IMixMediaPlayerService>();
        _telemetry = App.Services.GetRequiredService<ITelemetry>();
        _soundDataProvider = App.Services.GetRequiredService<ISoundService>();
        _dispatcherQueue = App.Services.GetRequiredService<IDispatcherQueue>();

        _timerService.Interval = SlideTimeLength;
        _timerService.IntervalElapsed += TimerIntervalElapsed;
        _mediaPlayerService.SoundsChanged += OnSoundsChanged;
    }

    public string Image1Source
    {
        get => (string)GetValue(Image1SourceProperty);
        set => SetValue(Image1SourceProperty, value);
    }

    public string Image2Source
    {
        get => (string)GetValue(Image2SourceProperty);
        set => SetValue(Image2SourceProperty, value);
    }

    public async Task LoadAsync(string? soundIdToUse = null)
    {
        _loadingCts.Cancel();
        _loadingCts = new();

        try
        {
            await InternalLoadAsync(soundIdToUse, _loadingCts.Token);
        }
        catch (OperationCanceledException) { }
    }

    private async Task InternalLoadAsync(string? soundIdToUse, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _images = null;

        if (_mediaPlayerService.Screensavers.Count > 0)
        {
            if (soundIdToUse is null)
            {
                var images = new List<string>();
                foreach (var list in _mediaPlayerService.Screensavers.Values)
                {
                    images.AddRange(list);
                }

                _images = images;
            }
            else if (_mediaPlayerService.Screensavers.TryGetValue(soundIdToUse, out var images))
            {
                _images = images;
            }
        }

        if (_images is null || _images.Count < 2)
        {
            var firstSound = (await _soundDataProvider.GetLocalSoundsAsync()).FirstOrDefault();
            _images = firstSound?.ScreensaverImagePaths ?? [];
        }

        if (_images is null || _images.Count < 2)
        {
            return;
        }

        _imageIndex1 = 0;
        _imageIndex2 = 1;
        Image1Source = _images[_imageIndex1];
        IncrementIndex(ref _imageIndex1);
        await Task.Delay(3000, ct);

        Image1.Visibility = Visibility.Visible;
        Image2.Visibility = Visibility.Collapsed;
        _ = Image1FadeInAndSlide.StartAsync(ct);
        
        Image2Source = _images[_imageIndex2]; // Preload next

        _timerService.Start();
    }

    private async Task CycleImagesAsync()
    {
        if (_images is null || _images.Count < 2)
        {
            return;
        }

        if (Image1.Visibility is Visibility.Visible)
        {
            Image2.Visibility = Visibility.Visible;
            _ = Image2FadeInAndSlide.StartAsync();

            await Image1FadeOut.StartAsync();
            Image1.Visibility = Visibility.Collapsed;

            IncrementIndex(ref _imageIndex2);
            Image1Source = _images[_imageIndex1]; // Preload next
        }
        else if (Image2.Visibility is Visibility.Visible)
        {
            Image1.Visibility = Visibility.Visible;
            _ = Image1FadeInAndSlide.StartAsync();

            await Image2FadeOut.StartAsync();
            Image2.Visibility = Visibility.Collapsed;

            IncrementIndex(ref _imageIndex1);
            Image2Source = _images[_imageIndex2]; // Preload next
        }
    }

    private void IncrementIndex(ref int index)
    {
        if (_images is null || _images.Count < 2)
        {
            return;
        }

        index += 2;
        if (index == _images.Count)
        {
            index = 0;
        }
        else if (index > _images.Count)
        {
            index = 1;
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width >= e.NewSize.Height)
        {
            Image1.Width = e.NewSize.Width * 1.3;
            Image2.Width = e.NewSize.Width * 1.3;
            Image1.Height = double.NaN;
            Image2.Height = double.NaN;
        }
        else
        {
            Image1.Height = e.NewSize.Height * 1.3;
            Image2.Height = e.NewSize.Height * 1.3;
            Image1.Width = double.NaN;
            Image2.Width = double.NaN;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _mediaPlayerService.SoundsChanged -= OnSoundsChanged;
        _timerService.IntervalElapsed -= TimerIntervalElapsed;
        _timerService.Stop();
    }

    private async void OnSoundsChanged(object sender, SoundChangedEventArgs e)
    {
        await HandleSoundChangeAsync();
        await LoadAsync(e.SoundsAdded is [Sound soundToUse, ..] ? soundToUse.Id : null);
    }

    private async Task HandleSoundChangeAsync()
    {
        await _soundsChangedLock.WaitAsync();
        _timerService.Stop();

        if (Image1.Visibility is Visibility.Visible)
        {
            await Image1FadeOut.StartAsync();
            Image1.Visibility = Visibility.Collapsed;
        }

        if (Image2.Visibility is Visibility.Visible)
        {
            await Image2FadeOut.StartAsync();
            Image2.Visibility = Visibility.Collapsed;
        }

        _soundsChangedLock.Release();
    }

    private void TimerIntervalElapsed(object sender, TimeSpan e)
    {
        _dispatcherQueue.TryEnqueue(async () => await CycleImagesAsync());
    }
}
