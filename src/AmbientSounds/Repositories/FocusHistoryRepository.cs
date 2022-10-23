using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public class FocusHistoryRepository : IFocusHistoryRepository
    {
        private const string SummaryFileName = "focusHistorySummary.json";
        private const string HistoryDirectory = "focusHistories";
        private readonly IFileWriter _fileWriter;

        public FocusHistoryRepository(IFileWriter fileWriter)
        {
            Guard.IsNotNull(fileWriter, nameof(fileWriter));
            _fileWriter = fileWriter;
        }

        private string HistoryPath(long startTimeTicks) => Path.Combine(HistoryDirectory, startTimeTicks.ToString());

        public async Task<FocusHistory?> GetHistoryAsync(long startTimeTicks)
        {
            var path = HistoryPath(startTimeTicks);
            string content = await _fileWriter.ReadAsync(path);
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            try
            {
                var result = JsonSerializer.Deserialize(content, AmbieJsonSerializerContext.Default.FocusHistory);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<FocusHistorySummary> GetSummaryAsync()
        {
            string content = await _fileWriter.ReadAsync(SummaryFileName);
            if (string.IsNullOrEmpty(content))
            {
                return new FocusHistorySummary();
            }

            try
            {
                var result = JsonSerializer.Deserialize(content, AmbieJsonSerializerContext.Default.FocusHistorySummary);
                return result ?? new FocusHistorySummary();
            }
            catch
            {
                return new FocusHistorySummary();
            }
        }

        public Task SaveHistoryAsync(FocusHistory history)
        {
            return _fileWriter.WriteStringAsync(JsonSerializer.Serialize(history, AmbieJsonSerializerContext.Default.FocusHistory), HistoryPath(history.StartUtcTicks));
        }

        public Task SaveSummaryAsync(FocusHistorySummary summary)
        {
            return _fileWriter.WriteStringAsync(JsonSerializer.Serialize(summary, AmbieJsonSerializerContext.Default.FocusHistorySummary), SummaryFileName);
        }
    }
}
