using AmbientSounds.Models;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IFocusHistoryRepository
    {
        /// <summary>
        /// Retrieve history summary stored locally.
        /// Returns new summary object if no data in storage.
        /// Can return null if the file was not found.
        /// </summary>
        Task<FocusHistorySummary> GetSummaryAsync();

        /// <summary>
        /// Overwrites the history summary in local storage
        /// with the given summary data.
        /// </summary>
        Task SaveSummaryAsync(FocusHistorySummary summary);

        /// <summary>
        /// Retrieves the history item based on the given start time ticks.
        /// </summary>
        Task<FocusHistory?> GetHistoryAsync(long startTimeTicks);
        Task SaveHistoryAsync(FocusHistory history);
    }
}