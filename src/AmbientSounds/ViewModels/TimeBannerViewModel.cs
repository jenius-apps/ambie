using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Diagnostics;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using AmbientSounds.Extensions;

namespace AmbientSounds.ViewModels
{
    public partial class TimeBannerViewModel : ObservableObject
    {
        private readonly IFocusService _focusService;
        private readonly ILocalizer _localizer;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private string _timeText = string.Empty;

        public TimeBannerViewModel(
            IFocusService focusService,
            ILocalizer localizer)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            Guard.IsNotNull(localizer, nameof(localizer));
            _focusService = focusService;
            _localizer = localizer;

            _focusService.TimeUpdated += OnTimeUpdated;
        }

        private void OnTimeUpdated(object sender, FocusSession e)
        {
            TimeText = e.Remaining.ToCountdownFormat();
            StatusText = e.SessionType.ToDisplayString(_localizer);
        }
    }
}
