using AmbientSounds.Models;
using AmbientSounds.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public class StreakHistoryRepository : IStreakHistoryRepository
{
    private const string StreakHistoryFile = "streakHistory.json";
    private readonly IFileWriter _fileWriter;

    public StreakHistoryRepository(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    public async Task<StreakHistory?> GetStreakHistoryAsync()
    {
        var content = await _fileWriter.ReadAsync(StreakHistoryFile);
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        try
        {
            var result = JsonSerializer.Deserialize(content, AmbieJsonSerializerContext.Default.StreakHistory);
            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateStreakHistoryAsync(StreakHistory history)
    {
        if (history is null)
        {
            return;
        }

        await _fileWriter.WriteStringAsync(
            JsonSerializer.Serialize(history, AmbieJsonSerializerContext.Default.StreakHistory),
            StreakHistoryFile);
    }
}
