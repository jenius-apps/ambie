using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;

namespace AmbientSounds.ViewModels;

public partial class ScreensaverViewModel : ObservableObject
{
    private const int ImageTimeLength = 30000; // milliseconds
    private readonly ITimerService _timerService;
    private readonly IMixMediaPlayerService _mediaPlayerService;
    private readonly ITelemetry _telemetry;
    private readonly ISoundService _soundDataProvider;
    private readonly IDispatcherQueue _dispatcherQueue;
    private IList<string> _images = new List<string>();

    [ObservableProperty]
    private string _imageSource1 = "https://localhost:8080";

    [ObservableProperty]
    private string _imageSource2 = "https://localhost:8080";

    [ObservableProperty]
    private bool _imageVisible1;

    [ObservableProperty]
    private bool _imageVisible2;
    private int _imageIndex1;
    private int _imageIndex2;

    [ObservableProperty]
    private bool _loading;

    public ScreensaverViewModel(
        ITimerService timerService,
        IMixMediaPlayerService mediaPlayerService,
        ISoundService soundDataProvider,
        ITelemetry telemetry,
        IDispatcherQueue dispatcherQueue)
    {
        Guard.IsNotNull(timerService);
        Guard.IsNotNull(mediaPlayerService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(soundDataProvider);
        Guard.IsNotNull(dispatcherQueue);

        _telemetry = telemetry;
        _mediaPlayerService = mediaPlayerService;
        _timerService = timerService;
        _soundDataProvider = soundDataProvider;
        _dispatcherQueue = dispatcherQueue;
        _timerService.Interval = ImageTimeLength;
    }

    public async void LoadAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.ScreensaverLoaded);

        if (_mediaPlayerService.Screensavers.Count > 0)
        {
            var images = new List<string>();
            foreach (var list in _mediaPlayerService.Screensavers.Values)
            {
                images.AddRange(list);
            }

            _images = images;
        }

        if (_images is null || _images.Count < 2)
        {
            var firstSound = (await _soundDataProvider.GetLocalSoundsAsync()).FirstOrDefault();
            _images = firstSound?.ScreensaverImagePaths ?? Array.Empty<string>();
        }

        if (_images is null || _images.Count < 2)
        {
            return;
        }

        Loading = true;
        _imageIndex1 = 0;
        _imageIndex2 = 1;
        ImageSource1 = _images[_imageIndex1];
        IncrementIndex(ref _imageIndex1);
        await Task.Delay(3000);
        Loading = false;
        ImageVisible1 = true;
        ImageVisible2 = false;

        // Preload next
        ImageSource2 = _images[_imageIndex2];

        _timerService.Start();
    }

    public void Unload()
    {
        _timerService.Stop();
    }

    private void TimerIntervalElapsed(object sender, TimeSpan e)
    {
        _dispatcherQueue.TryEnqueue(CycleImages);
    }

    private async void CycleImages()
    {
        if (_images is null || _images.Count < 2)
        {
            return;
        }

        if (ImageVisible1)
        {
            ImageVisible1 = false;
            ImageVisible2 = true;
            IncrementIndex(ref _imageIndex2);

            // Preload next
            await Task.Delay(3000);
            ImageSource1 = _images[_imageIndex1];
        }
        else
        {
            ImageVisible1 = true;
            ImageVisible2 = false;
            IncrementIndex(ref _imageIndex1);

            // Preload next
            await Task.Delay(3000);
            ImageSource2 = _images[_imageIndex2];
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

    public void Initialize()
    {
        _timerService.IntervalElapsed += TimerIntervalElapsed;
    }

    public void Dispose()
    {
        _timerService.IntervalElapsed -= TimerIntervalElapsed;
    }
}
