using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class SoundListViewModel : ObservableObject
{
    private readonly ISoundService _soundService;
    private readonly ITelemetry _telemetry;
    private readonly ISoundVmFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly IDownloadManager _downloadManager;
    private readonly IUserSettings _userSettings;
    private readonly INavigator _navigator;
    private bool _isDeleting;
    private bool _isAdding;
    private int _reorderedOldIndex;

    /// <summary>
    /// Default constructor. Must initialize with <see cref="LoadAsync"/>
    /// immediately after creation.
    /// </summary>
    public SoundListViewModel(
        ISoundService soundService,
        ITelemetry telemetry,
        ISoundVmFactory soundVmFactory,
        IDialogService dialogService,
        IDownloadManager downloadManager,
        IUserSettings userSettings,
        INavigator navigator)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(soundVmFactory);
        Guard.IsNotNull(dialogService);
        Guard.IsNotNull(downloadManager);
        Guard.IsNotNull(userSettings);
        Guard.IsNotNull(navigator);

        _soundService = soundService;
        _telemetry = telemetry;
        _factory = soundVmFactory;
        _dialogService = dialogService;
        _downloadManager = downloadManager;
        _userSettings = userSettings;
        _navigator = navigator;

        LoadCommand.PropertyChanged += OnLoadCommandPropertyChanged;
    }

    private void OnLoadCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoadCommand.IsRunning))
        {
            OnPropertyChanged(nameof(EmptyMessageVisible));
        }
    }

    public bool EmptyMessageVisible => !LoadCommand.IsRunning && Sounds.Count == 0;

    private void OnLocalSoundDeleted(object sender, string id)
    {
        var forDeletion = Sounds.FirstOrDefault(x => x.Id == id);
        if (forDeletion is null) return;
        _isDeleting = true;
        Sounds.Remove(forDeletion);
        _isDeleting = false;
        UpdateItemPositions();
        OnPropertyChanged(nameof(EmptyMessageVisible));
    }

    private void OnLocalSoundAdded(object sender, Models.Sound e)
    {
        var s = _factory.GetSoundVm(e);
        s.MixUnavailableCommand = MixUnavailableCommand;
        _isAdding = true;
        Sounds.Add(s);
        _isAdding = false;
        UpdateItemPositions();
        OnPropertyChanged(nameof(EmptyMessageVisible));
    }

    /// <summary>
    /// The list of sounds for this page.
    /// </summary>
    public ObservableCollection<SoundViewModel> Sounds { get; } = new();

    [RelayCommand]
    private async Task OnMixUnavailableAsync(IList<string>? unavailable)
    {
        if (unavailable is null)
        {
            return;
        }

        var download = await _dialogService.MissingSoundsDialogAsync();
        if (download)
        {
            await _downloadManager.QueueAndDownloadAsync(unavailable);
        }
    }

    [RelayCommand]
    private void OpenCatalogue()
    {
        _navigator.NavigateTo(ContentPageType.Catalogue);
        _telemetry.TrackEvent(TelemetryConstants.EmptyMessageButtonClicked);
    }

    /// <summary>
    /// Loads the list of sounds for this view model.
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (Sounds.Count > 0)
        {
            Sounds.Clear();
        }

        if (!_userSettings.Get<bool>(UserSettingsConstants.HasLoadedPackagedSoundsKey))
        {
            await _soundService.PrepopulateSoundsIfEmpty();
            _userSettings.Set(UserSettingsConstants.HasLoadedPackagedSoundsKey, true);
        }
        
        var soundList = await _soundService.GetLocalSoundsAsync();
        if (soundList is null || soundList.Count == 0)
        {
            return;
        }

        foreach (var sound in soundList.OrderBy(x => x.SortPosition))
        {
            var s = _factory.GetSoundVm(sound);
            s.MixUnavailableCommand = MixUnavailableCommand;

            try
            {
                _isAdding = true;
                Sounds.Add(s);
                _isAdding = false;
            }
            catch (Exception e)
            {
                _telemetry.TrackError(e);
            }
        }

        UpdateItemPositions();
        _soundService.LocalSoundAdded += OnLocalSoundAdded;
        _soundService.LocalSoundDeleted += OnLocalSoundDeleted;
        Sounds.CollectionChanged += OnSoundCollectionChanged;
        OnPropertyChanged(nameof(EmptyMessageVisible));
    }

    private void OnSoundCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove && !_isDeleting)
        {
            _reorderedOldIndex = e.OldStartingIndex;
        }
        else if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: [SoundViewModel svm, ..] } && !_isAdding)
        {
            _ = _soundService.UpdatePositionsAsync(
                svm.Id,
                _reorderedOldIndex,
                e.NewStartingIndex).ConfigureAwait(false);

            _telemetry.TrackEvent(TelemetryConstants.SoundReordered);
        }
    }

    public void Dispose()
    {
        _soundService.LocalSoundAdded -= OnLocalSoundAdded;
        _soundService.LocalSoundDeleted -= OnLocalSoundDeleted;
        Sounds.CollectionChanged -= OnSoundCollectionChanged;

        foreach (var s in Sounds)
        {
            s.Dispose();
        }

        Sounds.Clear();
    }

    private void UpdateItemPositions()
    {
        // required for a11y purposes.
        int index = 1;
        var size = Sounds.Count;
        foreach (var soundVm in Sounds)
        {
            soundVm.Position = index;
            soundVm.SetSize = size;
            index++;
        }
    }
}
