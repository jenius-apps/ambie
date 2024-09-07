using AmbientSounds.Constants;
using AmbientSounds.Models;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class ResumeOnLaunchService : IResumeOnLaunchService
{
    private readonly IUserSettings _userSettings;
    private readonly ISoundService _soundDataProvider;
    private readonly IMixMediaPlayerService _player;
    private readonly ITelemetry _telemetry;

    public ResumeOnLaunchService(
        IMixMediaPlayerService player,
        IUserSettings userSettings,
        ITelemetry telemetry,
        ISoundService soundDataProvider)
    {
        _telemetry = telemetry;
        _soundDataProvider = soundDataProvider;
        _userSettings = userSettings;
        _player = player;
    }

    public async Task LoadSoundsFromPreviousSessionAsync()
    {
        var mixId = _userSettings.Get<string>(UserSettingsConstants.ActiveMixId);
        var previousActiveTrackIds = _userSettings.GetAndDeserialize(UserSettingsConstants.ActiveTracks, AmbieJsonSerializerContext.Default.StringArray);
        var sounds = await _soundDataProvider.GetLocalSoundsAsync(soundIds: previousActiveTrackIds);
        if (sounds is not null && sounds.Count > 0)
        {
            foreach (var s in sounds)
            {
                await _player.ToggleSoundAsync(s, keepPaused: true, parentMixId: mixId);
            }
        }
    }

    public void TryResumePlayback(bool force = false)
    {
        // Since this is when the app is launching, try to resume automatically
        // after populating the track list.
        if ((force || _userSettings.Get<bool>(UserSettingsConstants.ResumeOnLaunchKey)) &&
            _player.GetSoundIds().Length > 0)
        {
            _player.Play();
            _telemetry.TrackEvent(TelemetryConstants.PlaybackAutoResume);
        }
    }
}
