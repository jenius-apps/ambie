using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.System;
using Windows.Web;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for downloading items in the background
    /// using WinRT APIs.
    /// </summary>
    public class BackgroundDownloadService : IDisposable
    {
        public static BackgroundDownloadService Instance = new BackgroundDownloadService();

        private readonly DispatcherQueue _dispatcherQueue;
        private readonly Dictionary<string, IProgress<double>> _activeProgress = new Dictionary<string, IProgress<double>>();
        private CancellationTokenSource? _cts;
        private Dictionary<string, DownloadOperation>? _activeDownloads;

        public BackgroundDownloadService()
        {
            _cts = new CancellationTokenSource();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public bool IsDownloadActive(string destinationPath) => _activeDownloads?.ContainsKey(destinationPath) ?? false;

        public IProgress<double>? GetProgress(string destinationPath)
        {
            return _activeProgress.ContainsKey(destinationPath)
                ? _activeProgress[destinationPath]
                : null;
        }

        /// <summary>
        /// Cancel all active downloads
        /// </summary>
        /// <param name="download"></param>
        public void CancelAllActiveDownloads()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            // Re-create the CancellationTokenSource and activeDownloads for future downloads.
            _cts = new CancellationTokenSource();
            _activeDownloads?.Clear();
            _activeDownloads = new Dictionary<string, DownloadOperation>();
        }

        /// <summary>
        /// Enumerate the downloads that were going on in the background while the app was closed.
        /// </summary>
        /// <returns></returns>
        public async Task DiscoverActiveDownloadsAsync()
        {
            _activeDownloads = new Dictionary<string, DownloadOperation>();

            IReadOnlyList<DownloadOperation>? downloads;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                if (IsExceptionHandled(ex))
                {
                    return;
                }

                throw;
            }

            if (downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    // Attach progress and completion handlers.
                    tasks.Add(HandleDownloadAsync(download, false));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Start downloading a file with the fileName and serverAddress provided
        /// </summary>
        /// <param name="destinationPath">The destination path of the download.</param>
        /// <param name="url">The URL of the item to download.</param>
        public async void StartDownload(
            StorageFile destinationFile,
            string url,
            IProgress<double>? progress = null,
            BackgroundTransferPriority priority = BackgroundTransferPriority.Default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri source))
            {
                return;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);
            download.Priority = priority;

            if (progress != null)
            {
                _activeProgress.TryAdd(destinationFile.Path, progress);
            }

            await HandleDownloadAsync(download, true);
        }

        private void DownloadProgress(DownloadOperation download)
        {
            string resultFilePath = download.ResultFile.Path;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                double percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
                _dispatcherQueue.TryEnqueue(() => 
                {
                    if (_activeProgress.ContainsKey(resultFilePath))
                    {
                        _activeProgress[resultFilePath].Report(percent);
                    }
                });
            }
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            if (_cts is null)
            {
                _cts = new CancellationTokenSource();
            }

            if (_activeDownloads is null)
            {
                _activeDownloads = new Dictionary<string, DownloadOperation>();
            }

            try
            {
                // Store the download so we can pause/resume.
                _activeDownloads.TryAdd(download.ResultFile.Path, download);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(_cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(_cts.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled(ex))
                {
                    throw;
                }
            }
            finally
            {
                _activeDownloads.Remove(download.ResultFile.Path);
                _activeProgress.Remove(download.ResultFile.Path);
            }
        }

        private bool IsExceptionHandled(Exception ex)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            return error != WebErrorStatus.Unknown;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_cts is not null)
            {
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}