﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;
using Microsoft.Toolkit.Uwp.Connectivity;
using System.Collections.Concurrent;
using AmbientSounds.Constants;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for interacting with the Microsoft Store
    /// to determine IAP ownership and to purchase IAPs.
    /// </summary>
    public class StoreService : IIapService
    {
        private static readonly ConcurrentDictionary<string, (int Version, StoreProduct Product)> _versionedProductsCache = new();
        private static readonly ConcurrentDictionary<string, StoreProduct> _productsCache = new();
        private static readonly ConcurrentDictionary<string, bool> _ownershipCache = new();
        private static StoreContext? _context;

        /// <inheritdoc/>
        public event EventHandler<string>? ProductPurchased;

        /// <inheritdoc/>
        public async Task<bool> IsOwnedAsync(string iapId)
        {
            if (iapId == IapConstants.MsStoreFreeRotationId)
            {
                return true;
            }

            if (_ownershipCache.TryGetValue(iapId, out bool isOwned))
            {
                return isOwned;
            }

            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                return false;
            }

            if (_context is null)
                _context = StoreContext.GetDefault();

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
        public async Task<bool> IsAnyOwnedAsync(IReadOnlyList<string> iapIds)
        {
            foreach (var id in iapIds)
            {
                var owned = await IsOwnedAsync(id);
                if (owned)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<string> GetLatestPriceAsync(string iapId)
        {
            (string idOnly, _) = iapId.SplitIdAndVersion();
            var addon = await GetLatestAddonAsync(idOnly);
            return addon?.Price?.FormattedPrice ?? "—";
        }

        private static async Task<StoreProduct?> GetLatestAddonAsync(string idOnly)
        {
            if (_versionedProductsCache.ContainsKey(idOnly))
            {
                return _versionedProductsCache[idOnly].Product;
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

                if (product.InAppOfferToken.StartsWith(idOnly))
                {
                    (string id, int version) = product.InAppOfferToken.SplitIdAndVersion();
                    if (_versionedProductsCache.ContainsKey(idOnly) && version > _versionedProductsCache[idOnly].Version)
                    {
                        _versionedProductsCache[idOnly] = (version, product);
                    }
                    else
                    {
                        _versionedProductsCache.TryAdd(id, (version, product));
                    }
                }
            }

            return _versionedProductsCache.ContainsKey(idOnly)
                ? _versionedProductsCache[idOnly].Product
                : null;
        }

        /// <inheritdoc/>
        public async Task<bool> BuyAsync(string iapId, bool latest = false)
        {
            StorePurchaseStatus result = await PurchaseAddOn(iapId, latest);

            if (result == StorePurchaseStatus.Succeeded || result == StorePurchaseStatus.AlreadyPurchased)
            {
                _ownershipCache[iapId] = true;
            }

            if (result == StorePurchaseStatus.Succeeded)
            {
                ProductPurchased?.Invoke(this, iapId);
            }

            return result switch
            {
                StorePurchaseStatus.Succeeded => true,
                StorePurchaseStatus.AlreadyPurchased => true,
                _ => false
            };
        }

        private static async Task<StorePurchaseStatus> PurchaseAddOn(string id, bool latest = false)
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

        private static async Task<StoreProduct?> GetAddOn(string id)
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
}
