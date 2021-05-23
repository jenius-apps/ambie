using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class ScreensaverViewModel : ObservableObject
    {
        private const int ImageTimeLength = 30000; // milliseconds
        private readonly ITimerService _timerService;
        private readonly IMixMediaPlayerService _mediaPlayerService;
        private readonly ITelemetry _telemetry;
        private readonly ISoundDataProvider _soundDataProvider;
        private IList<string> _images = new List<string>();
        private string _imageSource1 = "https://localhost:8080";
        private string _imageSource2 = "https://localhost:8080";
        private bool _imageVisible1;
        private bool _imageVisible2;
        private int _imageIndex1;
        private int _imageIndex2;
        private bool _loading;

        public ScreensaverViewModel(
            ITimerService timerService,
            IMixMediaPlayerService mediaPlayerService,
            ISoundDataProvider soundDataProvider,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(timerService, nameof(timerService));
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));

            _telemetry = telemetry;
            _mediaPlayerService = mediaPlayerService;
            _timerService = timerService;
            _soundDataProvider = soundDataProvider;
            _timerService.Interval = ImageTimeLength;
        }

        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        public string ImageSource1
        {
            get => _imageSource1;
            set => SetProperty(ref _imageSource1, value);
        }

        public string ImageSource2
        {
            get => _imageSource2;
            set => SetProperty(ref _imageSource2, value);
        }

        public bool ImageVisible1
        {
            get => _imageVisible1;
            set => SetProperty(ref _imageVisible1, value);
        }

        public bool ImageVisible2
        {
            get => _imageVisible2;
            set => SetProperty(ref _imageVisible2, value);
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
                var firstSound = (await _soundDataProvider.GetSoundsAsync(refresh: false)).FirstOrDefault();
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

        private void TimerIntervalElapsed(object sender, int e)
        {
            CycleImages();
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
}
