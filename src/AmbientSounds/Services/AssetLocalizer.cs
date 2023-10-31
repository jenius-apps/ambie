using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Tools;
using System.Globalization;

#nullable enable

namespace AmbientSounds.Services;

public class AssetLocalizer : IAssetLocalizer
{
    private readonly ISystemInfoProvider _systemInfoProvider;

    public AssetLocalizer(ISystemInfoProvider systemInfoProvider)
    {
        Guard.IsNotNull(systemInfoProvider);

        _systemInfoProvider = systemInfoProvider;
    }

    /// <inheritdoc/>
    public string GetLocalName(IAsset asset)
    {
        return GetLocalInfo(asset)?.Name ?? asset.Name;
    }

    /// <inheritdoc/>
    public bool LocalNameContains(IAsset asset, string nameQuery)
    {
        return CultureInfo.CurrentCulture.CompareInfo.IndexOf(GetLocalName(asset), nameQuery, CompareOptions.IgnoreCase) >= 0;
    }

    /// <inheritdoc/>
    public string GetLocalDescription(IAsset asset)
    {
        return GetLocalInfo(asset)?.Description ?? asset.Description;
    }

    private DisplayInformation? GetLocalInfo(IAsset asset)
    {
        string languageCode = _systemInfoProvider.GetCulture();

        if (asset.Localizations.TryGetValue(languageCode, out DisplayInformation info))
        {
            return info;
        }

        if (languageCode.Contains("-"))
        {
            var split = languageCode.Split('-');
            if (asset.Localizations.TryGetValue(split[0], out info))
            {
                return info;
            }
        }

        return null;
    }
}
