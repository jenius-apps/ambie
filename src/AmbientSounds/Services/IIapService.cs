﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

/// <summary>
/// Interface for retrieving IAP ownwership info
/// or purchasing IAP.
/// </summary>
public interface IIapService
{
    /// <summary>
    /// A product was successfully purchased. Payload is
    /// the ID of the IAP add-on.
    /// </summary>
    event EventHandler<string>? ProductPurchased;

    /// <summary>
    /// Check if the sound is already owned.
    /// </summary>
    /// <param name="iapId">An ID from <see cref="Models.Sound.IapIds"/>.</param>
    /// <returns>True if the sound is owned.</returns>
    Task<bool> IsOwnedAsync(string iapId);

    /// <summary>
    /// Returns true if any of the given in-app purchase IDs
    /// are owned.
    /// </summary>
    /// <param name="iapIds">IDs from <see cref="Models.Sound.IapIds"/>.</param>
    Task<bool> IsAnyOwnedAsync(IReadOnlyList<string> iapIds);

    /// <summary>
    /// Attempts to buy the sound.
    /// </summary>
    /// <param name="iapId">The IAP ID of the add-on we want to purchase.</param>
    /// <returns>True if the sound is purchased successfully.</returns>
    Task<bool> BuyAsync(string iapId, bool latest = false);

    /// <summary>
    /// Retrieves the latest price of the item.
    /// </summary>
    /// <param name="iapId">An IAP ID whose price we want to check.</param>
    /// <returns>The price of the item formatted like $1.99.</returns>
    Task<string> GetLatestPriceAsync(string iapId);
}
