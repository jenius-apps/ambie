using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class SoundListViewModel
    {
        private readonly IMediaPlayerService _player;
        private readonly ISoundDataProvider _provider;
        private readonly ITelemetry _telemetry;

        /// <summary>
        /// Default constructor. Must initialize with <see cref="LoadAsync"/>
        /// immediately after creation.
        /// </summary>
        public SoundListViewModel(
            IMediaPlayerService mediaPlayerService,
            ISoundDataProvider soundDataProvider,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _player = mediaPlayerService;
            _provider = soundDataProvider;
            _telemetry = telemetry;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            PlaySoundCommand = new RelayCommand<SoundViewModel>(PlaySound);

            _provider.LocalSoundAdded += OnLocalSoundAdded;
        }

        private async void OnLocalSoundAdded(object sender, Models.Sound e)
        {
            Sounds.Add(new SoundViewModel(e, _player, Sounds.Count));
            await _player.AddToPlaylistAsync(e);
        }

        /// <summary>
        /// The <see cref="IAsyncRelayCommand"/> responsible for loading the viewmodel data.
        /// </summary>
        public IAsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// The <see cref="IRelayCommand{T}"/> responsible for playing a selected sound.
        /// </summary>
        public IRelayCommand<SoundViewModel> PlaySoundCommand { get; }

        /// <summary>
        /// The list of sounds for this page.
        /// </summary>
        public ObservableCollection<SoundViewModel> Sounds { get; } = new();

        /// <summary>
        /// Loads the list of sounds for this view model.
        /// </summary>
        private async Task LoadAsync()
        {
            if (Sounds.Count > 0) return; // already initialized

            var soundList = await _provider.GetSoundsAsync();

            int index = 0;
            foreach (var sound in soundList)
            {
                Sounds.Add(new SoundViewModel(sound, _player, index));
                index++;
            }

           await  _player.Initialize(soundList);
        }

        /// <summary>
        /// Loads the clicked sound into the player and plays it.
        /// </summary>
        private void PlaySound(SoundViewModel sound)
        {
            if (sound is null)
            {
                return;
            }

            sound.Play();
            _telemetry.TrackEvent(TelemetryConstants.SoundClicked, new Dictionary<string, string>
            {
                { "id", sound.Id }
            });
        }
    }
}
