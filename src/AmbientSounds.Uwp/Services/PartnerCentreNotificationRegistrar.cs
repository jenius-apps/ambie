using Microsoft.Services.Store.Engagement;
using System;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for registering notifications
    /// from partner centre.
    /// </summary>
    public class PartnerCentreNotificationRegistrar : IStoreNotificationRegistrar
    {
        /// <inheritdoc/>
        public async Task Register()
        {
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.RegisterNotificationChannelAsync();
        }

        /// <inheritdoc/>
        public string TrackLaunch(string launchArgs)
        {
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            string originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(launchArgs);
            return originalArgs;
        }

        /// <inheritdoc/>
        public async Task Unregiser()
        {
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.UnregisterNotificationChannelAsync();
        }
    }
}
