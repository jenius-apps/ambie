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
        private static StoreContext? _context;

        /// <inheritdoc/>
        public async Task<bool> IsOwnedAsync(string iapId)
        {
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
                    return true;
                }
                else if (license.SkuStoreId.StartsWith(iapId) && license.IsActive)
                {
                    // Handle subscription scenario
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<string> GetPriceAsync(string iapId)
        {
            var addon = await GetAddOn(iapId);
            return addon?.Price?.FormattedPrice ?? "---";
        }

        /// <inheritdoc/>
        public Task<bool> BuyAsync(string iapId)
        {
            return PurchaseAddOn(iapId);
        }

        private static async Task<bool> PurchaseAddOn(string id)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                return false;
            }

            var addOnProduct = await GetAddOn(id);
            if (addOnProduct is null)
                return false;

            /// Attempt purchase
            var result = await addOnProduct.RequestPurchaseAsync();
            if (result is null)
                return false;

            bool purchased = false;
            switch (result.Status)
            {
                case StorePurchaseStatus.NotPurchased:
                case StorePurchaseStatus.ServerError:
                case StorePurchaseStatus.NetworkError:
                    break;
                case StorePurchaseStatus.Succeeded:
                case StorePurchaseStatus.AlreadyPurchased:
                    purchased = true;
                    break;
            }

            return purchased;
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
                else if (product.StoreId == id)
                {
                    // gets subscription
                    _productsCache.TryAdd(id, product);
                    return product;
                }
            }

            return null;
        }
    }
}
