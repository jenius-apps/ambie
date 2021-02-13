using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class ShareResultsViewModel
    {
        private readonly IOnlineSoundDataProvider _onlineDataProvider;
        private readonly ISoundDataProvider _localDataProvider;
        private readonly ISoundVmFactory _soundVmFactory;

        public ShareResultsViewModel(
            IOnlineSoundDataProvider dataProvider,
            ISoundDataProvider localDataProvider,
            ISoundVmFactory soundVmFactory)
        {
            Guard.IsNotNull(dataProvider, nameof(dataProvider));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));
            Guard.IsNotNull(localDataProvider, nameof(localDataProvider));

            _localDataProvider = localDataProvider;
            _onlineDataProvider = dataProvider;
            _soundVmFactory = soundVmFactory;
        }

        /// <summary>
        /// The list of downloadable sounds for this page.
        /// </summary>
        public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

        /// <summary>
        /// Loads the given list of items into the sound collection.
        /// </summary>
        /// <param name="soundIds">The list of sounds to load into the collection.</param>
        public async Task LoadAsync(IList<string> soundIds)
        {
            if (soundIds == null || soundIds.Count == 0)
            {
                return;
            }

            List<Sound> onlineSounds;

            // fetch online sounds
            try
            {
                var result = await _onlineDataProvider.GetSoundsAsync(soundIds);
                onlineSounds = result.ToList();
            }
            catch
            {
                // todo log
                return;
            }

            // fetch local sounds
            if (onlineSounds.Count < soundIds.Count)
            {
                var onlineSoundIds = onlineSounds.Select(o => o.Id);
                var localSoundIds = soundIds.Where(localId => !onlineSoundIds.Contains(localId)).ToArray();
                var localSounds = await _localDataProvider.GetSoundsAsync(soundIds: localSoundIds);
                if (localSounds != null && localSounds.Count > 0)
                {
                    foreach (var l in localSounds)
                    {
                        onlineSounds.Add(l);
                    }
                }
            }

            foreach (var sound in onlineSounds)
            {
                var vm = _soundVmFactory.GetOnlineSoundVm(sound);
                if (vm != null)
                {
                    await vm.LoadCommand.ExecuteAsync(null);
                    Sounds.Add(vm);
                }
            }
        }
    }
}
