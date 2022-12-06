namespace AmbientSounds.Services;

public interface ICompactNavigator
{
    /// <summary>
    /// The inner frame that can navigate. This must be set before
    /// any method is called.
    /// </summary>
    object? ContentFrame { get; set; }

    /// <summary>
    /// Navigates to the page corresponding to the given mode.
    /// </summary>
    /// <param name="mode">The mode to navigate to.</param>
    void NavigateTo(CompactViewMode mode);
}
