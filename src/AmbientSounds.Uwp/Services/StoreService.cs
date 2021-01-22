using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;
using Microsoft.Toolkit.Uwp.Connectivity;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for interacting with the Microsoft Store
    /// to determine IAP ownership and to purchase IAPs.
    /// </summary>
    public class StoreService : IIapService
    {
        private static Dictionary<string, StoreProduct> _productsCache = new Dictionary<string, StoreProduct>();
        private static StoreContext _context = null;

        /// <inheritdoc/>
        public async Task<bool> IsOwnedAsync(string iapId)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                return false;
            }

            if (_context == null)
                _context = StoreContext.GetDefault();

            StoreAppLicense appLicense = await _context.GetAppLicenseAsync();
            if (appLicense == null)
            {
                return false;
            }

            /// Check if user has an active license for given add-on id.
            foreach (var addOnLicense in appLicense.AddOnLicenses)
            {
                var license = addOnLicense.Value;
                if (license.InAppOfferToken == iapId && license.IsActive)
                {
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
            if (addOnProduct == null)
                return false;

            /// Attempt purchase
            var result = await addOnProduct.RequestPurchaseAsync();
            if (result == null)
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

        private static async Task<StoreProduct> GetAddOn(string id)
        {
            if (_productsCache.ContainsKey(id))
            {
                return _productsCache[id];
            }

            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                return null;
            }

            if (_context == null)
                _context = StoreContext.GetDefault();

            /// Get all add-ons for this app.
            var result = await _context.GetAssociatedStoreProductsAsync(new string[] { "Durable", "Consumable" });
            if (result.ExtendedError != null)
            {
                return null;
            }

            foreach (var item in result.Products)
            {
                var product = item.Value;

                if (product.InAppOfferToken == id)
                {
                    _productsCache.TryAdd(id, product);
                    return product;
                }
            }

            return null;
        }
    }
}
