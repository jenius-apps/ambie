using System.Text.Json.Serialization;

namespace AmbientSounds.Models;

/// <summary>
/// Class representing a promo code.
/// </summary>
public class PromoCode
{
    /// <summary>
    /// The ID of the promo code. Also represents the
    /// promo code itself. E.g. DC2025.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the in-app purchase item in the Microsoft Store.
    /// </summary>
    public string MicrosoftStoreAddOnId { get; set; } = string.Empty;

    /// <summary>
    /// Date when this promo code expires expressed in .NET DateTime ticks.
    /// </summary>
    public long ExpiresOnTicks { get; set; }
}
