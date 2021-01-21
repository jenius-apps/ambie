using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for retrieving IAP ownwership info
    /// or purchasing IAP.
    /// </summary>
    public interface IIapService
    {
        /// <summary>
        /// Check if the sound is already owned.
        /// </summary>
        /// <param name="iapId">The <see cref="Models.Sound.IapId"/>.</param>
        /// <returns>True if the sound is owned.</returns>
        Task<bool> IsOwnedAsync(string iapId);

        /// <summary>
        /// Attempts to buy the sound.
        /// </summary>
        /// <param name="iapId">The <see cref="Models.Sound.IapId"/>.</param>
        /// <returns>True if the sound is purchased successfully.</returns>
        Task<bool> BuyAsync(string iapId);
    }
}
