using AmbientSounds.Factories;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
        }

        private void OnLocalSoundAdded(object sender, Models.Sound e)
        {
            var s = _factory.GetSoundVm(e);
            Sounds.Add(s);
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

            if (Sounds.Count > 0)
            {
                foreach (var sound in Sounds)
                {
                    // ensure viewmodels are initialized.
                    sound.Initialize();
                }
                return;
            }

            var soundList = await _provider.GetSoundsAsync();

            foreach (var sound in soundList)
            {
                var s = _factory.GetSoundVm(sound);
                Sounds.Add(s);
            }
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
    }
}
