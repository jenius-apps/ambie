using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Allows programmatic page navigation.
/// </summary>
public interface INavigator
{
    /// <summary>
    /// Raised when the content page was changed.
    /// </summary>
    event EventHandler<ContentPageType>? ContentPageChanged; 

    /// <summary>
    /// The root frame of the app.
    /// </summary>
    object? RootFrame { get; set; }

    /// <summary>
    /// The inner frame that can navigate. This must be set before
    /// any method is called.
    /// </summary>
    object? Frame { get; set; }

    /// <summary>
    /// Returns the name of the current page, or returns
    /// empty string if no page is set.
    /// </summary>
    string GetContentPageName();

    /// <summary>
    /// Navigates to the screensaver.
    /// </summary>
    void ToScreensaver();

    /// <summary>
    /// Attempts to navigate back.
    /// </summary>
    /// <param name="sourcePage">Optional. If provided,
    /// then specific go back functionality may be applied.
    /// For example, if ScreensaverPage is provided, the 
    /// RootFrame is used for GoBack.</param>
    void GoBack(string? sourcePage = null);

    /// <summary>
    /// Attempts to trigger compact overlay mode
    /// and navigates to the requested page.
    /// </summary>
    Task ToCompactOverlayAsync(CompactViewMode requestedOverlayMode);

    /// <summary>
    /// Closes the compact overlay and navigates back to the
    /// page most relevant to the given <paramref name="closingOverlayMode"/>.
    /// </summary>
    /// <param name="closingOverlayMode">Used to determine which page to navigate back to.</param>
    Task CloseCompactOverlayAsync(CompactViewMode closingOverlayMode);
    
    /// <summary>
    /// Navigates to the page corresponding to the given enum.
    /// </summary>
    /// <param name="contentPage">The page to navigate to.</param>
    /// <param name="navArgs">Arguments to be passed to the new page during navigation.</param>
    void NavigateTo(ContentPageType contentPage, object? navArgs = null);
}

public enum ContentPageType
{
    Home,
    Catalogue,
    Focus,
    Meditate,
    Settings,
    Updates,
    Search,
}

public enum CompactViewMode
{
    /// <summary>
    /// Specifies the Home mode
    /// for compact view wheere users
    /// can play their sounds.
    /// </summary>
    Home,

    /// <summary>
    /// Specifies the Focus mode
    /// for compact view where users
    /// can start a focus session.
    /// </summary>
    Focus,

    /// <summary>
    /// Specifies the Interruption mode
    /// for compact view where users
    /// can log an interruption.
    /// </summary>
    Interruption,
}