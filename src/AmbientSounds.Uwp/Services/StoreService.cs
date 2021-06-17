using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;
using Microsoft.Toolkit.Uwp.Connectivity;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for interacting with the Microsoft Store
    /// to determine IAP ownership and to purchase IAPs.
    /// </summary>
    public class StoreService : IIapService
    {
        private static readonly Dictionary<string, StoreProduct> _productsCache = new();
        private static readonly Dictionary<string, bool> _ownershipCache = new();
        private static StoreContext? _context;

        /// <inheritdoc/>
        public event EventHandler<string>? ProductPurchased;

        /// <inheritdoc/>
        public async Task<bool> IsOwnedAsync(string iapId)
        {
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
                if (license.InAppOfferToken == iapId && license.IsActive)
                {
                    // Handle add-on scenario
                    _ownershipCache.TryAdd(iapId, true);
                    return true;
                }
            }

            _ownershipCache.TryAdd(iapId, false);
            return false;
        }

        /// <inheritdoc/>
        public async Task<string> GetPriceAsync(string iapId)
        {
            var addon = await GetAddOn(iapId);
            return addon?.Price?.FormattedPrice ?? "---";
        }

        /// <inheritdoc/>
        public async Task<bool> BuyAsync(string iapId)
        {
            StorePurchaseStatus result = await PurchaseAddOn(iapId);

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

        private static async Task<StorePurchaseStatus> PurchaseAddOn(string id)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                return StorePurchaseStatus.NetworkError;
            }

            var addOnProduct = await GetAddOn(id);
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
