using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
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
        private readonly string _cloudSyncFileUrl;
        private bool _syncing;

        /// <inheritdoc/>
        public event EventHandler? SyncStarted;

        /// <inheritdoc/>
        public event EventHandler? SyncCompleted;

        public SyncEngine(
            ICloudFileWriter cloudFileWriter,
            IDownloadManager downloadManager,
            IAccountManager accountManager,
            ISoundDataProvider soundDataProvider,
            IAppSettings appSettings)
        {
            Guard.IsNotNull(cloudFileWriter, nameof(cloudFileWriter));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(appSettings, nameof(appSettings));
            Guard.IsNotNullOrEmpty(appSettings.CloudSyncFileUrl, nameof(appSettings.CloudSyncFileUrl));

            _accountManager = accountManager;
            _cloudFileWriter = cloudFileWriter;
            _downloadManager = downloadManager;
            _soundDataProvider = soundDataProvider;
            _cloudSyncFileUrl = appSettings.CloudSyncFileUrl;

            _downloadManager.DownloadsCompleted += OnDownloadsCompleted;
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

        private async void OnDownloadsCompleted(object sender, EventArgs e)
        {
            if (!Syncing)
            {
                await SyncUp();
            }
            else
            {
                // TODO create mix
            }
        }

        /// <inheritdoc/>
        public async Task SyncUp()
        {
            if (Syncing || !(await _accountManager.IsSignedInAsync()))
            {
                return;
            }

            Syncing = true;
            string? token = await _accountManager.GetTokenAsync();
            if (token == null || string.IsNullOrWhiteSpace(token))
            {
                Syncing = false;
                return;
            }

            var localSounds = await _soundDataProvider.GetLocalSoundsAsync();
            var syncData = new SyncData
            {
                InstalledSoundIds = localSounds.Select(x => x.Id).ToArray(),
                SoundMixes = localSounds
                    .Where(x => x.IsMix == true)
                    .Select(mix => new Sound { Name = mix.Name, Id = mix.Id, SoundIds = mix.SoundIds })
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

            Syncing = false;
        }
    }
}
