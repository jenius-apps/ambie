using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class CatalogueListViewModel
    {
        private readonly IOnlineSoundDataProvider _dataProvider;
        private readonly IDownloadManager _downloadManager;
        private readonly IMediaPlayerService _player;

        public CatalogueListViewModel(
            IOnlineSoundDataProvider dataProvider,
            IDownloadManager downloadManager,
            IMediaPlayerService mediaPlayerService)
        {
            Guard.IsNotNull(dataProvider, nameof(dataProvider));
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            _dataProvider = dataProvider;
            _downloadManager = downloadManager;
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
        public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

        private async Task LoadAsync()
        {
            if (Sounds.Count > 0)
            {
                return;
            }

            IList<Sound> sounds;

            try
            {
                sounds = await _dataProvider.GetSoundsAsync();
            }
            catch (Exception e)
            {
                // TODO log error
                Debug.WriteLine(e);
                return;
            }

            foreach (var sound in sounds)
            {
                Sounds.Add(new OnlineSoundViewModel(sound, _downloadManager));
            }
        }
    }
}
