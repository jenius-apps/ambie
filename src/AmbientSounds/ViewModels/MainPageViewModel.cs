using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private readonly IMediaPlayerService _player;
        private readonly ISoundDataProvider _provider;

        /// <summary>
        /// Default constructor. Must initialize with <see cref="LoadAsync"/>
        /// immediately after creation.
        /// </summary>
        public MainPageViewModel(IMediaPlayerService mediaPlayerService, ISoundDataProvider soundDataProvider)
        {
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));

            _player = mediaPlayerService;
            _provider = soundDataProvider;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            PlaySoundCommand = new RelayCommand<SoundViewModel>(PlaySound);
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
            var soundList = await _provider.GetSoundsAsync();

            foreach (var sound in soundList)
            {
                Sounds.Add(new SoundViewModel(sound, _player));
            }
        }

        /// <summary>
        /// Loads the clicked sound into the player and plays it.
        /// </summary>
        private void PlaySound(SoundViewModel sound)
        {
            sound?.Play();
        }
    }
}
