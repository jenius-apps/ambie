using AmbientSounds.Factories;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class SoundListViewModel : ObservableObject
    {
        private readonly ISoundDataProvider _provider;
        private readonly ITelemetry _telemetry;
        private readonly ISoundVmFactory _factory;

        /// <summary>
        /// Default constructor. Must initialize with <see cref="LoadAsync"/>
        /// immediately after creation.
        /// </summary>
        public SoundListViewModel(
            ISoundDataProvider soundDataProvider,
            ITelemetry telemetry,
            ISoundVmFactory soundVmFactory)
        {
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));

            _provider = soundDataProvider;
            _telemetry = telemetry;
            _factory = soundVmFactory;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
        }

        private void OnLocalSoundDeleted(object sender, string id)
        {
            var forDeletion = Sounds.FirstOrDefault(x => x.Id == id);
            if (forDeletion is null) return;
            Sounds.Remove(forDeletion);
            UpdateItemPositions();
        }

        private void OnLocalSoundAdded(object sender, Models.Sound e)
        {
            var s = _factory.GetSoundVm(e);
            Sounds.Add(s);
            UpdateItemPositions();
        }

        /// <summary>
        /// The <see cref="IAsyncRelayCommand"/> responsible for loading the viewmodel data.
        /// </summary>
        public IAsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// The list of sounds for this page.
        /// </summary>
        public ObservableCollection<SoundViewModel> Sounds { get; } = new();

        /// <summary>
        /// Loads the list of sounds for this view model.
        /// </summary>
        private async Task LoadAsync()
        {
            _provider.LocalSoundAdded += OnLocalSoundAdded;
            _provider.LocalSoundDeleted += OnLocalSoundDeleted;

            var soundList = await _provider.GetSoundsAsync();
            if (soundList is null || soundList.Count == 0)
            {
                Sounds.Clear();
                return;
            }

            var localIds = soundList.Select(s => s.Id);
            var forDeletion = new List<SoundViewModel>();

            if (Sounds.Count > 0)
            {
                foreach (var soundVm in Sounds)
                {
                    if (!localIds.Contains(soundVm.Id))
                    {
                        forDeletion.Add(soundVm);
                    }
                    else
                    {
                        // Ensure viewmodels are initialized.
                        soundVm.Initialize();
                    }
                }
            }

            foreach (var soundVm in forDeletion)
            {
                Sounds.Remove(soundVm);
            }

            var existingIds = Sounds.Select(s => s.Id);
            foreach (var sound in soundList.Where(x => !existingIds.Contains(x.Id)))
            {
                var s = _factory.GetSoundVm(sound);
                Sounds.Add(s);
            }

            UpdateItemPositions();
        }

        public void Dispose()
        {
            _provider.LocalSoundAdded -= OnLocalSoundAdded;
            _provider.LocalSoundDeleted -= OnLocalSoundDeleted;

            foreach (var s in Sounds)
            {
                s.Dispose();
            }
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
}
