using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ActiveTrackListViewModel : ObservableObject
{
    private readonly IMixMediaPlayerService _player;
    private readonly ISoundVmFactory _soundVmFactory;
    private readonly IUserSettings _userSettings;
    private readonly ISoundService _soundDataProvider;
    private readonly ITelemetry _telemetry;
    private readonly IPresenceService _presenceService;
    private readonly IShareService _shareService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly bool _loadPreviousState;
    private bool _loaded;

    public event EventHandler? TrackListChanged;

    public ActiveTrackListViewModel(
        IMixMediaPlayerService player,
        ISoundVmFactory soundVmFactory,
        IUserSettings userSettings,
        ITelemetry telemetry,
        ISoundService soundDataProvider,
        IAppSettings appSettings,
        IPresenceService presenceService,
        IShareService shareService,
        IDispatcherQueue dispatcherQueue)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(soundVmFactory);
        Guard.IsNotNull(userSettings);
        Guard.IsNotNull(soundDataProvider);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(appSettings);
        Guard.IsNotNull(presenceService);
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(dispatcherQueue);

        _loadPreviousState = appSettings.LoadPreviousState;
        _telemetry = telemetry;
        _soundDataProvider = soundDataProvider;
        _userSettings = userSettings;
        _soundVmFactory = soundVmFactory;
        _player = player;
        _presenceService = presenceService;
        _shareService = shareService;
        _dispatcherQueue = dispatcherQueue;
    }

    /// <summary>
    /// List of active sounds being played.
    /// </summary>
    public ObservableCollection<ActiveTrackViewModel> ActiveTracks { get; } = new();

    /// <summary>
    /// Determines if the clear button is visible.
    /// </summary>
    public bool IsClearVisible => ActiveTracks.Count > 0 && _loaded;

    /// <summary>
    /// Determines if the placeholder is visible.
    /// </summary>
    public bool IsPlaceholderVisible => ActiveTracks.Count == 0 && _loaded;

    /// <summary>
    /// Loads prevoius state of the active track list.
    /// </summary>
    public async Task LoadPreviousStateAsync()
    {
        _shareService.ShareRequested += OnShareRequested;
        _player.SoundAdded += OnSoundAdded;
        _player.SoundRemoved += OnSoundRemoved;
        ActiveTracks.CollectionChanged += ActiveTracks_CollectionChanged;

        //// This track list is what we use
        //// to determine if a user should report presence
        //// for a sound. Thus, we initialize the presence service
        //// the same time this viewmodel is initialized.
        //var task = _presenceService.EnsureInitializedAsync();

        if (ActiveTracks.Count > 0 || !_loadPreviousState)
        {
            return;
        }

        IEnumerable<string> soundIds = _player.GetSoundIds(oldestToNewest: true);
        if (soundIds.Any())
        {
            // This case is when the track list is returning to view because of a page navigation.

            var sounds = await _soundDataProvider.GetLocalSoundsAsync(soundIds: soundIds.ToArray());
            if (sounds is { Count: > 0 })
            {
                foreach (var s in sounds)
                {
                    AddSoundTrack(s);
                }
            }
        }

        _loaded = true;
        OnPropertyChanged(nameof(IsClearVisible));
        OnPropertyChanged(nameof(IsPlaceholderVisible));
        //await task;
    }

    [RelayCommand]
    private void ClearAll()
    {
        var count = ActiveTracks.Count;

        if (count > 0)
        {
            ActiveTracks.Clear();
            _player.RemoveAll();
            UpdateStoredState();
        }
    }

    private void UpdateStoredState()
    {
        var ids = ActiveTracks.Select(static x => x.Sound.Id).ToArray();
        _userSettings.SetAndSerialize(UserSettingsConstants.ActiveTracks, ids, AmbieJsonSerializerContext.Default.StringArray);
        _userSettings.Set(UserSettingsConstants.ActiveMixId, _player.CurrentMixId);
    }

    private void ActiveTracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        TrackListChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(IsClearVisible));
        OnPropertyChanged(nameof(IsPlaceholderVisible));
    }

    private void OnSoundRemoved(object sender, SoundPausedArgs args)
    {
        var sound = ActiveTracks.FirstOrDefault(x => x.Sound?.Id == args.SoundId);
        if (sound is not null)
        {
            ActiveTracks.Remove(sound);
            UpdateStoredState();

            //if (!sound.Sound.IsMix)
            //{
            //    await _presenceService.DecrementAsync(args.SoundId);
            //}
        }
    }

    private void OnSoundAdded(object sender, SoundPlayedArgs args)
    {
        if (args?.Sound is not null)
        {
            AddSoundTrack(args.Sound);

            //if (!args.Sound.IsMix)
            //{
            //    await _presenceService.IncrementAsync(args.Sound.Id);
            //}
        }
    }

    private void AddSoundTrack(Sound sound)
    {
        if (!ActiveTracks.Any(x => x.Sound?.Id == sound.Id))
        {
            ActiveTracks.Add(_soundVmFactory.GetActiveTrackVm(sound, RemoveSoundCommand));
            UpdateStoredState();
        }
    }

    [RelayCommand]
    private void RemoveSound(Sound? s)
    {
        if (s is not null)
        {
            _player.RemoveSound(s.Id);
        }
    }

    public void Dispose()
    {
        ActiveTracks.CollectionChanged -= ActiveTracks_CollectionChanged;
        _player.SoundAdded -= OnSoundAdded;
        _player.SoundRemoved -= OnSoundRemoved;
        _shareService.ShareRequested -= OnShareRequested;
    }

    private async void OnShareRequested(object sender, IReadOnlyList<string> soundIds)
    {
        var sounds = await _soundDataProvider.GetLocalSoundsAsync(soundIds);
        _ = Task.Run(() =>
        {
            if (sounds.Count != soundIds.Count)
            {
                _shareService.LogShareFailed(soundIds);
            }
        });

        if (sounds is { Count: > 0 })
        {
            ClearAll();
            foreach (var s in sounds)
            {
                await _player.ToggleSoundAsync(s);
                await Task.Delay(300); // delay needed because for some reason sounds don't play without it.
            }

            _telemetry.TrackEvent(TelemetryConstants.SharePlayed, new Dictionary<string, string>
            {
                { "missingSounds", (sounds.Count < soundIds.Count).ToString() },
                { "id count", soundIds.Count.ToString() }
            });
        }
    }
}
