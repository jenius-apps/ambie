using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class CatalogueListViewModel
    {
        private readonly IOnlineSoundDataProvider _dataProvider;
        private readonly IMediaPlayerService _player;

        public CatalogueListViewModel(
            IOnlineSoundDataProvider dataProvider,
            IMediaPlayerService mediaPlayerService)
        {
            Guard.IsNotNull(dataProvider, nameof(dataProvider));
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            _dataProvider = dataProvider;
            _player = mediaPlayerService;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
        }

        /// <summary>
        /// The <see cref="IAsyncRelayCommand"/> responsible for loading the viewmodel data.
        /// </summary>
        public IAsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// The list of sounds for this page.
        /// </summary>
        public ObservableCollection<SoundViewModel> Sounds { get; } = new();

        private async Task LoadAsync()
        {
            IList<Sound> sounds;

            try
            {
                sounds = await _dataProvider.GetSoundsAsync();
            }
            catch
            {
                // TODO log error
                return;
            }

            foreach (var sound in sounds)
            {
                // TODO create different viewmodel
                Sounds.Add(new SoundViewModel(sound, _player));
            }
        }
    }
}
