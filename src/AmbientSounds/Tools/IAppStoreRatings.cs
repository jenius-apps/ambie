using System.Threading.Tasks;

namespace AmbientSounds.Tools;

/// <summary>
/// Interface for handling app store ratings logic.
/// </summary>
public interface IAppStoreRatings
{
    /// <summary>
    /// Requests in-app ratings using the APIs
    /// provided by the native app store.
    /// </summary>
    /// <returns>True if the rating was submitted or updated.</returns>
    Task<bool> RequestInAppRatingsAsync();
}
