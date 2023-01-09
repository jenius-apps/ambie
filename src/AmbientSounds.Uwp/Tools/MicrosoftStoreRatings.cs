using System;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace AmbientSounds.Tools.Uwp;

public class MicrosoftStoreRatings : IAppStoreRatings
{
    /// <inheritdoc/>
    public async Task<bool> RequestInAppRatingsAsync()
    {
        var storeContext = StoreContext.GetDefault();
        var result = await storeContext.RequestRateAndReviewAppAsync();
        return result.WasUpdated;
    }
}
