using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Diagnostics;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using AmbientSounds.Extensions;
using AmbientSounds.Tools;

namespace AmbientSounds.ViewModels
{
    public partial class TimeBannerViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private readonly ILocalizer _localizer;
        private readonly IDispatcherQueue _dispatcherQueue;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private string _timeText = string.Empty;

        public TimeBannerViewModel(
            IFocusService focusService,
            ILocalizer localizer,
            IDispatcherQueue dispatcherQueue)
        {
            Guard.IsNotNull(focusService);
            Guard.IsNotNull(localizer);
            Guard.IsNotNull(dispatcherQueue);

            _focusService = focusService;
            _localizer = localizer;
            _dispatcherQueue = dispatcherQueue;

            _focusService.TimeUpdated += OnTimeUpdated;
        }

        private void OnTimeUpdated(object sender, FocusSession e)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                TimeText = e.Remaining.ToCountdownFormat();
                StatusText = e.SessionType.ToDisplayString(_localizer);
            });
        }
    }
}
