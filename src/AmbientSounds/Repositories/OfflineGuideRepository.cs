using AmbientSounds.Models;
using AmbientSounds.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public class OfflineGuideRepository : IOfflineGuideRepository
{
    private const string LocalGuideDataFile = "localGuides.json";
    private readonly IFileWriter _fileWriter;

    public OfflineGuideRepository(
        IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guide>> GetAsync()
    {
        string content = await _fileWriter.ReadAsync(LocalGuideDataFile);
        if (string.IsNullOrEmpty(content))
        {
            return Array.Empty<Guide>();
        }

        try
        {
            var result = JsonSerializer.Deserialize(content, AmbieJsonSerializerContext.Default.GuideArray);
            return result ?? Array.Empty<Guide>();
        }
        catch
        {
            return Array.Empty<Guide>();
        }
    }

    public Task SaveAsync(IReadOnlyList<Guide> guides)
    {
        return _fileWriter.WriteStringAsync(
            JsonSerializer.Serialize(guides, AmbieJsonSerializerContext.Default.IReadOnlyListGuide),
            LocalGuideDataFile);
    }
}
