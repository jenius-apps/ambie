using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class ScreensaverViewModel : ObservableObject
    {
        private const int ImageTimeLength = 30000; // milliseconds
        private readonly ITimerService _timerService;
        private readonly IMediaPlayerService _mediaPlayerService;
        private readonly ITelemetry _telemetry;
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
            IMediaPlayerService mediaPlayerService,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(timerService, nameof(timerService));
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            _telemetry = telemetry;
            _mediaPlayerService = mediaPlayerService;
            _timerService = timerService;
            _timerService.Interval = ImageTimeLength;
            _timerService.IntervalElapsed += TimerIntervalElapsed;
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
            _telemetry.TrackEvent(TelemetryConstants.ScreensaverLoaded, new Dictionary<string, string>
            {
                { "sound", _mediaPlayerService.Current?.Name ?? "---" },
            });

            _images = _mediaPlayerService.Current?.ScreensaverImagePaths ?? new string[0];
            if (_images == null || _images.Count < 2)
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
            if (_images == null || _images.Count < 2)
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
            if (_images == null || _images.Count < 2)
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
    }
}
