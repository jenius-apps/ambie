using AmbientSounds.Constants;
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
        private readonly IDialogService _dialogService;
        private readonly IDownloadManager _downloadManager;

        /// <summary>
        /// Default constructor. Must initialize with <see cref="LoadAsync"/>
        /// immediately after creation.
        /// </summary>
        public SoundListViewModel(
            ISoundDataProvider soundDataProvider,
            ITelemetry telemetry,
            ISoundVmFactory soundVmFactory,
            IDialogService dialogService,
            IDownloadManager downloadManager)

        {
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));

            _provider = soundDataProvider;
            _telemetry = telemetry;
            _factory = soundVmFactory;
            _dialogService = dialogService;
            _downloadManager = downloadManager;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            MixUnavailableCommand = new AsyncRelayCommand<IList<string>>(OnMixUnavailableAsync);
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
            s.MixUnavailableCommand = MixUnavailableCommand;
            Sounds.Add(s);
            UpdateItemPositions();
        }

        /// <summary>
        /// The <see cref="IAsyncRelayCommand"/> responsible for loading the viewmodel data.
        /// </summary>
        public IAsyncRelayCommand LoadCommand { get; }

        public IAsyncRelayCommand<IList<string>> MixUnavailableCommand { get; }

        /// <summary>
        /// The list of sounds for this page.
        /// </summary>
        public ObservableCollection<SoundViewModel> Sounds { get; } = new();

        private async Task OnMixUnavailableAsync(IList<string>? unavailable)
        {
            if (unavailable is null)
            {
                return;
            }

            var download = await _dialogService.MissingSoundsDialogAsync();
            if (download)
            {
                _telemetry.TrackEvent(TelemetryConstants.MissingSoundsDownloaded);
                await _downloadManager.QueueAndDownloadAsync(unavailable);
            }
            else
            {
                _telemetry.TrackEvent(TelemetryConstants.MissingSoundsCanceled);
            }
        }

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
                s.MixUnavailableCommand = MixUnavailableCommand;
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
