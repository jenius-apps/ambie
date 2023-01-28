using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.UI.ViewManagement;

#nullable enable

namespace AmbientSounds.Services.Uwp;

/// <summary>
/// Retrieves system information.
/// </summary>
public class SystemInfoProvider : ISystemInfoProvider
{
    /// <inheritdoc/>
    public bool IsFirstRun()
    {
        return SystemInformation.Instance.IsFirstRun;
    }

    /// <inheritdoc/>
    public string GetCulture()
    {
        return SystemInformation.Instance.Culture.Name;
    }

    /// <inheritdoc/>
    public bool IsDesktop()
    {
        return App.IsDesktop;
    }

    /// <inheritdoc/>
    public bool IsTenFoot()
    {
        return App.IsTenFoot;
    }

    /// <inheritdoc/>
    public bool IsCompact()
    {
        return ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay;
    }

    /// <inheritdoc/>
    public bool CanUseFluentSystemIcons()
    {
        var result = SystemInformation.Instance.OperatingSystemVersion;
        return result.Build >= 22000;
    }

    /// <inheritdoc/>
    public DateTime FirstUseDate()
    {
        return SystemInformation.Instance.FirstUseTime;
    }
}
