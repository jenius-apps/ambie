using JeniusApps.Common.Telemetry;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Downloads and saves sounds.
/// </summary>
public class FileDownloader : IFileDownloader
{
    private readonly HttpClient _client;
    private readonly IFileWriter _fileWriter;
    private readonly ITelemetry _telemetry;

    public FileDownloader(
        HttpClient httpClient,
        IFileWriter fileWriter,
        ITelemetry telemetry)
    {
        _fileWriter = fileWriter;
        _client = httpClient;
        _telemetry = telemetry;
    }

    /// <inheritdoc/>
    public async Task<string> ImageDownloadAndSaveAsync(string? url, string name)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return "";
        }

        HttpResponseMessage response = await _client.GetAsync(url);

        var contentType = response.Content.Headers.ContentType.MediaType;
        string nameWithExt;
        try
        {
            nameWithExt = name + MimeTypeMap.GetExtension(contentType);
        }
        catch (Exception e)
        {
            // GetExtension can crash if the contentType has no natural mapping.
            // This can happen if the image or URL is corrupted.
            // So we fall back to no extension, which fine.
            nameWithExt = name;
            _telemetry.TrackError(e, new Dictionary<string, string>
            {
                { "contentType", contentType },
                { "imageUrl", url ?? string.Empty }
            });
        }
        using var stream = await response.Content.ReadAsStreamAsync();
        return await _fileWriter.WriteImageAsync(stream, nameWithExt);
    }
}
