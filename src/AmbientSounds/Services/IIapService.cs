using System;
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
        /// A product was successfully purchased. Payload is
        /// the ID of the IAP add-on or subscription.
        /// </summary>
        event EventHandler<string>? ProductPurchased;

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

        /// <summary>
        /// Retrieves the price of the item.
        /// </summary>
        /// <param name="iapId">The <see cref="Models.Sound.IapId"/>.</param>
        /// <returns>The price of the item formatted like $1.99.</returns>
        Task<string> GetPriceAsync(string iapId);
    }
}
