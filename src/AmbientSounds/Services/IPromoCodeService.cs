using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Interface for handling promo codes.
/// </summary>
public interface IPromoCodeService
{
    /// <summary>
    /// Fetches the IAP addon ID for the given promo code.
    /// </summary>
    /// <returns>
    /// Returns the IAP ID if found. Returns null otherwise.
    /// </returns>
    Task<string?> TryGetAddOnAsync(string promoCode);
}