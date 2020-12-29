using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class ScreensaverViewModel : ObservableObject
    {
        private const int ImageTimeLength = 10000; // milliseconds
        private readonly IScreensaverService _screensaverService;
        private readonly ITimerService _timerService;
        private IList<string> _images = new List<string>();
        private string _imageSource1 = "https://www.bing.com";
        private string _imageSource2 = "https://www.bing.com";
        private bool _imageVisible1;
        private bool _imageVisible2;
        private int _imageIndex1;
        private int _imageIndex2;

        public ScreensaverViewModel(
            IScreensaverService screensaverService,
            ITimerService timerService)
        {
            Guard.IsNotNull(screensaverService, nameof(screensaverService));
            Guard.IsNotNull(timerService, nameof(timerService));
            _screensaverService = screensaverService;
            _timerService = timerService;
            _timerService.Interval = ImageTimeLength;
            _timerService.IntervalElapsed += TimerIntervalElapsed;
        }

        private void TimerIntervalElapsed(object sender, int e)
        {
            CycleImages();
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
            await Task.Delay(1);
            //_images = await _screensaverService.GetImagePathsAsync();
            _images = new List<string>
            {
                "https://images.unsplash.com/photo-1585495898471-0fa227b7f193?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=2255&q=80",
                "https://images.unsplash.com/photo-1577899831505-233c0a599869?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=2252&q=80",
                "https://images.unsplash.com/photo-1605936995786-fa5749a143ea?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=2250&q=80"
             };

            if (_images == null || _images.Count < 2)
            {
                return;
            }

            _imageIndex1 = 0;
            _imageIndex2 = 1;
            ImageSource1 = _images[_imageIndex1];
            IncrementIndex(ref _imageIndex1);

            ImageVisible1 = true;
            ImageVisible2 = false;
            // start roll timer

            _timerService.Start();
        }

        public void Unload()
        {
            _timerService.Stop();
        }

        private void CycleImages()
        {
            if (_images == null || _images.Count < 2)
            {
                return;
            }

            if (ImageVisible1)
            {
                ImageSource2 = _images[_imageIndex2];
                ImageVisible1 = false;
                ImageVisible2 = true;
                IncrementIndex(ref _imageIndex2);
            }
            else
            {
                ImageSource1 = _images[_imageIndex1];
                ImageVisible1 = true;
                ImageVisible2 = false;
                IncrementIndex(ref _imageIndex1);
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
