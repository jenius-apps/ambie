using AmbientSounds.Constants;
using AmbientSounds.Models;
using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Services.Store;

#nullable enable

namespace AmbientSounds.Services.Uwp;

/// <summary>
/// Class for interacting with the Microsoft Store
/// to determine IAP ownership and to purchase IAPs.
/// </summary>
public class StoreService : IIapService
{
    private readonly ConcurrentDictionary<string, (int Version, StoreProduct Product)> _versionedProductsCache = new();
    private readonly ConcurrentDictionary<string, StoreProduct> _productsCache = new();
    private readonly ConcurrentDictionary<string, bool> _ownershipCache = new();
    private StoreContext? _context;
    private readonly SemaphoreSlim _versionedProductsLock = new(1, 1);

    /// <inheritdoc/>
    public event EventHandler<string>? ProductPurchased;

    /// <inheritdoc/>
    public async Task<bool> IsOwnedAsync(string iapId)
    {
#if DEBUG
        //return true;
#endif

        if (_ownershipCache.TryGetValue(iapId, out bool isOwned))
        {
            return isOwned;
        }

        _context ??= StoreContext.GetDefault();

        StoreAppLicense appLicense = await _context.GetAppLicenseAsync();
        if (appLicense is null)
        {
            return false;
        }

        foreach (var addOnLicense in appLicense.AddOnLicenses)
        {
            StoreLicense license = addOnLicense.Value;
            if (!license.IsActive)
            {
                continue;
            }

            if (license.InAppOfferToken == iapId ||
                (iapId.ContainsAmbiePlus() && license.InAppOfferToken.ContainsAmbiePlus()))
            {
                // 2nd condition: if requested iap is ambieplus but license is for ambieplus_2, we still count it as ownership.
                _ownershipCache.TryAdd(iapId, true);
                return true;
            }
        }

        _ownershipCache.TryAdd(iapId, false);
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> IsSubscriptionOwnedAsync()
    {
        return await IsAnyOwnedAsync([IapConstants.MsStoreAmbiePlusId, IapConstants.MsStoreAmbiePlusAnnualId]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> CanShowPremiumButtonsAsync()
    {
        return !await IsAnyOwnedAsync([IapConstants.MsStoreAmbiePlusId, IapConstants.MsStoreAmbiePlusLifetimeId, IapConstants.MsStoreAmbiePlusAnnualId]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsAnyOwnedAsync(IReadOnlyList<string> iapIds)
    {
        foreach (var id in iapIds)
        {
            var owned = await IsOwnedAsync(id).ConfigureAwait(false);
            if (owned)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<PriceInfo> GetLatestPriceAsync(string iapId)
    {
        (string idOnly, _) = iapId.SplitIdAndVersion();
        var addon = await GetLatestAddonAsync(idOnly).ConfigureAwait(false);

        if (addon?.Price is null)
        {
            return new PriceInfo { FormattedPrice = "-" };
        }

        var sku = addon.Skus?.FirstOrDefault();
        bool isSub = sku?.IsSubscription ?? false;

        return new PriceInfo
        {
            FormattedPrice = isSub ? addon.Price.FormattedRecurrencePrice : addon.Price.FormattedPrice,
            IsSubscription = isSub,
            RecurrenceLength = (int)(sku?.SubscriptionInfo?.BillingPeriod ?? 0),
            RecurrenceUnit = ToDurationUnit(sku?.SubscriptionInfo?.BillingPeriodUnit),
            HasSubTrial = sku?.SubscriptionInfo?.HasTrialPeriod ?? false,
            SubTrialLength = (int)(sku?.SubscriptionInfo?.TrialPeriod ?? 0),
            SubTrialLengthUnit = ToDurationUnit(sku?.SubscriptionInfo?.TrialPeriodUnit),
        };
    }

    private DurationUnit ToDurationUnit(StoreDurationUnit? storeDurationUnit)
    {
        return storeDurationUnit switch
        {
            StoreDurationUnit.Minute => DurationUnit.Minute,
            StoreDurationUnit.Hour => DurationUnit.Hour,
            StoreDurationUnit.Day => DurationUnit.Day,
            StoreDurationUnit.Week => DurationUnit.Week,
            StoreDurationUnit.Month => DurationUnit.Month,
            StoreDurationUnit.Year => DurationUnit.Year,
            _ => DurationUnit.Minute
        };
    }

    private async Task<StoreProduct?> GetLatestAddonAsync(string idOnly)
    {
        if (_versionedProductsCache.TryGetValue(idOnly, out var cachedResult))
        {
            return cachedResult.Product;
        }

        if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        {
            return null;
        }

        // At this point, the product cache is likely not populated,
        // so obtain the lock and then run the populate method.
        await _versionedProductsLock.WaitAsync();

        // Check cache again in case it changed while waiting.
        if (_versionedProductsCache.TryGetValue(idOnly, out cachedResult))
        {
            return cachedResult.Product;
        }

        // Populate the cache.
        await PopulateAddonCacheAsync();

        _versionedProductsLock.Release();

        // Try to return the desired add on.
        return _versionedProductsCache.TryGetValue(idOnly, out cachedResult)
            ? cachedResult.Product
            : null;
    }

    private async Task PopulateAddonCacheAsync()
    {
        _context ??= StoreContext.GetDefault();

        // Get all add-ons for this app.
        var result = await _context.GetAssociatedStoreProductsAsync(["Durable", "Consumable"]);
        if (result.ExtendedError is not null)
        {
            return;
        }

        // Find all addons and cache the latest version
        foreach (var item in result.Products)
        {
            StoreProduct product = item.Value;

            (string id, int newVersion) = product.InAppOfferToken.SplitIdAndVersion();
            if (_versionedProductsCache.TryGetValue(id, out var cachedResult) && newVersion > cachedResult.Version)
            {
                _versionedProductsCache[id] = (newVersion, product);
            }
            else
            {
                _versionedProductsCache.TryAdd(id, (newVersion, product));
            }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> BuyAsync(string iapId, bool latest = false, string? iapIdCacheOverride = null)
    {
        StorePurchaseStatus result = await PurchaseAddOn(iapId, latest);

        if (result == StorePurchaseStatus.Succeeded || result == StorePurchaseStatus.AlreadyPurchased)
        {
            _ownershipCache[iapIdCacheOverride ?? iapId] = true;
            ProductPurchased?.Invoke(this, iapIdCacheOverride ?? iapId);
        }

        return result switch
        {
            StorePurchaseStatus.Succeeded => true,
            StorePurchaseStatus.AlreadyPurchased => true,
            _ => false
        };
    }

    private async Task<StorePurchaseStatus> PurchaseAddOn(string id, bool latest = false)
    {
        if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        {
            return StorePurchaseStatus.NetworkError;
        }

        (string idOnly, _) = id.SplitIdAndVersion();

        var addOnProduct = latest
            ? await GetLatestAddonAsync(idOnly)
            : await GetAddOn(id);

        if (addOnProduct is null)
            return StorePurchaseStatus.ServerError;

        /// Attempt purchase
        var result = await addOnProduct.RequestPurchaseAsync();
        if (result is null)
            return StorePurchaseStatus.ServerError;

        return result.Status;
    }

    private async Task<StoreProduct?> GetAddOn(string id)
    {
        if (_productsCache.ContainsKey(id))
        {
            return _productsCache[id];
        }

        if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        {
            return null;
        }

        if (_context is null)
            _context = StoreContext.GetDefault();

        /// Get all add-ons for this app.
        var result = await _context.GetAssociatedStoreProductsAsync(new string[] { "Durable", "Consumable" });
        if (result.ExtendedError is not null)
        {
            return null;
        }

        foreach (var item in result.Products)
        {
            StoreProduct product = item.Value;

            if (product.InAppOfferToken == id)
            {
                // gets add-on
                _productsCache.TryAdd(id, product);
                return product;
            }
        }

        return null;
    }
}
