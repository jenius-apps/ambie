using AmbientSounds.Constants;
using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class that orchestrates data synchronization.
    /// </summary>
    public class SyncEngine : ISyncEngine
    {
        private readonly ICloudFileWriter _cloudFileWriter;
        private readonly IDownloadManager _downloadManager;
        private readonly IAccountManager _accountManager;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
        private readonly ISoundMixService _soundMixService;
        private readonly ITelemetry _telemetry;
        private readonly string _cloudSyncFileUrl;
        private readonly Queue<string> _dataChangeQueue;
        private bool _syncing;
        private bool _processingDataChange;

        /// <inheritdoc/>
        public event EventHandler? SyncStarted;

        /// <inheritdoc/>
        public event EventHandler? SyncCompleted;

        public SyncEngine(
            ICloudFileWriter cloudFileWriter,
            IDownloadManager downloadManager,
            IAccountManager accountManager,
            ISoundDataProvider soundDataProvider,
            IOnlineSoundDataProvider onlineSoundDataProvider,
            IAppSettings appSettings,
            ISoundMixService soundMixService,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(cloudFileWriter, nameof(cloudFileWriter));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(onlineSoundDataProvider, nameof(onlineSoundDataProvider));
            Guard.IsNotNull(soundMixService, nameof(soundMixService));
            Guard.IsNotNull(appSettings, nameof(appSettings));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNullOrEmpty(appSettings.CloudSyncFileUrl, nameof(appSettings.CloudSyncFileUrl));

            _accountManager = accountManager;
            _cloudFileWriter = cloudFileWriter;
            _downloadManager = downloadManager;
            _soundDataProvider = soundDataProvider;
            _onlineSoundDataProvider = onlineSoundDataProvider;
            _soundMixService = soundMixService;
            _telemetry = telemetry;
            _cloudSyncFileUrl = appSettings.CloudSyncFileUrl;
            _dataChangeQueue = new Queue<string>();

            _downloadManager.DownloadsCompleted += OnDownloadsCompleted;
            _soundDataProvider.LocalSoundDeleted += OnLocalSoundDeleted;
            _soundDataProvider.LocalSoundAdded += OnLocalSoundAdded;
            _accountManager.SignInUpdated += OnSignInUpdated;
        }

        private async void OnSignInUpdated(object sender, bool isSignedIn)
        {
            if (isSignedIn)
            {
                await SyncDown();
            }
        }

        private void OnLocalSoundAdded(object sender, Sound e)
        {
            if (e.IsMix)
            {
                ProcessDataChangeAsync(e.Id);
            }
        }

        private void OnLocalSoundDeleted(object sender, string e)
        {
            ProcessDataChangeAsync(e);
        }

        private bool Syncing
        {
            get => _syncing;
            set
            {
                _syncing = value;
                if (value) SyncStarted?.Invoke(this, EventArgs.Empty);
                else SyncCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Helper method for syncing up any changes made
        /// to the local sound list. Changes we care about are
        /// any deleted sound or sound mixes, and newly created sound mixes.
        /// </summary>
        /// <param name="soundIdAddedOrDeleted">Id of the added or deleted sound.</param>
        private async void ProcessDataChangeAsync(string soundIdAddedOrDeleted)
        {
            if (_dataChangeQueue.Contains(soundIdAddedOrDeleted))
            {
                return;
            }

            _dataChangeQueue.Enqueue(soundIdAddedOrDeleted);

            if (_processingDataChange)
            {
                return;
            }

            _processingDataChange = true;

            while (_dataChangeQueue.Count > 0)
            {
                _dataChangeQueue.Clear();
                await SyncUp();
            }

            _processingDataChange = false;
        }

        private async void OnDownloadsCompleted(object sender, EventArgs e)
        {
            if (!Syncing)
            {
                await SyncUp();
            }
        }

        /// <inheritdoc/>
        public async Task SyncDown()
        {
            string? token = await _accountManager.GetGraphTokenAsync();
            if (Syncing || token is null || string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            Syncing = true;
            SyncData? data;

            try
            {
                string serialized = await _cloudFileWriter.ReadFileAsync(_cloudSyncFileUrl, token, default);
                data = JsonSerializer.Deserialize<SyncData>(serialized);
            }
            catch
            {
                data = null;
            }

            if (data?.InstalledSoundIds is null || data.InstalledSoundIds.Length == 0)
            {
                Syncing = false;
                await SyncUp();
                return;
            }

            var soundMixIds = data.SoundMixes?.Select(x => x.Id).ToList() ?? new List<string>();
            IList<Sound> installedSounds = await _soundDataProvider.GetLocalSoundsAsync();
            var installedIds = installedSounds.Select(x => x.Id);
            var soundIdsToDownload = new List<string>();

            foreach (var id in data.InstalledSoundIds)
            {
                if (!string.IsNullOrWhiteSpace(id) && 
                    !installedIds.Contains(id) &&
                    !soundMixIds.Contains(id)) // don't try to download a custom sound mix id.
                {
                    soundIdsToDownload.Add(id);
                }
            }

            if (soundIdsToDownload.Count > 0)
            {
                // download sounds
                IList<Sound> soundsToDownload = await _onlineSoundDataProvider.GetSoundsAsync(soundIdsToDownload);

                var tasks = new List<Task>();
                foreach (var s in soundsToDownload)
                {
                    var task = _downloadManager.QueueAndDownloadAsync(s, new Progress<double>());
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }

            if (data.SoundMixes is not null && data.SoundMixes.Length > 0)
            {
                await _soundMixService.ReconstructMixesAsync(data.SoundMixes);
            }

            _telemetry.TrackEvent(TelemetryConstants.SyncDown, new Dictionary<string, string>
            {
                { "downloadcount", soundIdsToDownload.Count.ToString() },
                { "mixcount", data.SoundMixes?.Length.ToString() ?? "0" }
            });

            Syncing = false;
        }

        /// <inheritdoc/>
        public async Task SyncUp()
        {
            string? token = await _accountManager.GetGraphTokenAsync();
            if (Syncing || token is null || string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            Syncing = true;

            var localSounds = await _soundDataProvider.GetLocalSoundsAsync();
            var syncData = new SyncData
            {
                InstalledSoundIds = localSounds.Select(static x => x.Id).ToArray(),
                SoundMixes = localSounds
                    .Where(static x => x.IsMix)
                    .Select(static mix => new Sound { Name = mix.Name, Id = mix.Id, SoundIds = mix.SoundIds })
                    .ToArray()
            };

            var serialized = JsonSerializer.Serialize(syncData);

            try
            {
                await _cloudFileWriter.WriteFileAsync(_cloudSyncFileUrl, serialized, token, default);
            }
            catch
            {
                // todo log
            }

            _telemetry.TrackEvent(TelemetryConstants.SyncUp);

            Syncing = false;
        }

        public void Dispose()
        {
            _downloadManager.DownloadsCompleted -= OnDownloadsCompleted;
            _soundDataProvider.LocalSoundDeleted -= OnLocalSoundDeleted;
            _soundDataProvider.LocalSoundAdded -= OnLocalSoundAdded;
            _accountManager.SignInUpdated -= OnSignInUpdated;
        }
    }
}
