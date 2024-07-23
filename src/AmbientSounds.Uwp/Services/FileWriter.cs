using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

#nullable enable

namespace AmbientSounds.Services.Uwp;

/// <summary>
/// Writes ambie data to local directory.
/// </summary>
public class FileWriter : IFileWriter
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();
    private const string _imagesDirName = "images";

    /// <inheritdoc/>
    public async Task<string> ReadAsync(string relativeLocalPath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(relativeLocalPath))
        {
            return string.Empty;
        }

        IStorageItem targetLocation = await ApplicationData.Current.LocalFolder.TryGetItemAsync(relativeLocalPath);
        ct.ThrowIfCancellationRequested();

        if (targetLocation is StorageFile file)
        {
            return await FileIO.ReadTextAsync(file);
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFileAsync(string absolutePathInLocalStorage)
    {
        if (string.IsNullOrEmpty(absolutePathInLocalStorage))
        {
            return false;
        }

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(absolutePathInLocalStorage);
            await file.DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task WriteStringAsync(string content, string relativeLocalPath)
    {
        if (string.IsNullOrEmpty(relativeLocalPath))
        {
            return;
        }

        var semaphore = _semaphores.GetOrAdd(relativeLocalPath, new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            StorageFile targetLocation = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                relativeLocalPath,
                CreationCollisionOption.OpenIfExists);

            if (targetLocation != null)
            {
                await FileIO.WriteTextAsync(targetLocation, content);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public Task<string> WriteImageAsync(Stream stream, string nameWithExt)
    {
        return WriteFileAsync(stream, nameWithExt, _imagesDirName);
    }

    /// <inheritdoc/>
    public async Task<string> WriteFileAsync(Stream stream, string nameWithExt, string? localDirName = null)
    {
        StorageFolder dir;

        if (string.IsNullOrWhiteSpace(localDirName))
        {
            dir = ApplicationData.Current.LocalFolder;
        }
        else 
        {
            var dirSemaphore = _semaphores.GetOrAdd(localDirName!, new SemaphoreSlim(1, 1));
            await dirSemaphore.WaitAsync();

            try
            {
                dir = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                    localDirName,
                    CreationCollisionOption.OpenIfExists);
            }
            finally
            {
                dirSemaphore.Release();
            }
        }


        var fileSemaphore = _semaphores.GetOrAdd(nameWithExt, new SemaphoreSlim(1, 1));
        await fileSemaphore.WaitAsync();
        StorageFile storageFile;

        try
        {
            storageFile = await dir.CreateFileAsync(nameWithExt, CreationCollisionOption.ReplaceExisting);
            using IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
            await stream.CopyToAsync(fileStream.AsStreamForWrite());
            await fileStream.FlushAsync();
        }
        finally
        {
            fileSemaphore.Release();
        }

        return storageFile.Path;
    }
}
