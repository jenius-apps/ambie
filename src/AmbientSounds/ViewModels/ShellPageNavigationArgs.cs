using AmbientSounds.Services;

namespace AmbientSounds.ViewModels;

public sealed class ShellPageNavigationArgs
{
    public ContentPageType? FirstPageOverride { get; init; }

    public string LaunchArguments { get; init; } = string.Empty;

    /// <summary>
    /// Used to delay the inner frame navigation. 
    /// </summary>
    /// <remarks>
    /// This was exposed because sometimes, when the inner frame navigation
    /// occurs too quickly with the root frame navigation, the content
    /// of the inner frame doesn't become visible.
    /// </remarks>
    public int MillisecondsDelay { get; init; } = 0;
}
