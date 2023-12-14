using AmbientSounds.ViewModels;
using AmbientSounds.Views;
using System;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

#nullable enable

namespace AmbientSounds.Services.Uwp;

/// <summary>
/// Navigates programmatically in a UWP app.
/// </summary>
public class Navigator : INavigator
{
    /// <inheritdoc/>
    public event EventHandler<ContentPageType>? ContentPageChanged;

    public object? RootFrame { get; set; }

    /// <inheritdoc/>
    public object? Frame { get; set; }

    /// <inheritdoc/>
    public void GoBack(string? sourcePage = null)
    {
        switch (sourcePage)
        {
            case nameof(CompactPage):
                GoBackSafely(RootFrame, new SuppressNavigationTransitionInfo());
                break;
            case nameof(ScreensaverPage):
                GoBackSafely(RootFrame, new DrillInNavigationTransitionInfo());
                NavigateTo(ContentPageType.Home);
                break;
            default:
                GoBackSafely(Frame);
                break;
        }
    }

    private void GoBackSafely(object? frame, NavigationTransitionInfo? transition = null)
    {
        if (frame is Frame f && f.CanGoBack)
        {
            f.GoBack(transition);
        }
    }

    /// <inheritdoc/>
    public void ToScreensaver()
    {
        if (RootFrame is Frame f && f.CurrentSourcePageType != typeof(ScreensaverPage))
        {
            f.Navigate(typeof(ScreensaverPage), null, new DrillInNavigationTransitionInfo());
        }
    }

    /// <inheritdoc/>
    public void NavigateTo(ContentPageType contentPage, object? navArgs = null)
    {
        Type pageType = contentPage switch
        {
            ContentPageType.Focus => typeof(FocusPage),
            ContentPageType.Catalogue => typeof(CataloguePage),
            ContentPageType.Settings => typeof(SettingsPage),
            ContentPageType.Updates => typeof(UpdatesPage),
            ContentPageType.Meditate => typeof(MeditatePage),
            ContentPageType.Search => typeof(SearchPage),
            _ => typeof(HomePage)
        };

        if (Frame is Frame f && f.CurrentSourcePageType != pageType)
        {
            f.Navigate(pageType, navArgs, new SuppressNavigationTransitionInfo());
            ContentPageChanged?.Invoke(this, contentPage);
        }
    }

    /// <inheritdoc/>
    public string GetContentPageName()
    {
        if (Frame is Frame f)
        {
            return f.CurrentSourcePageType?.Name ?? string.Empty;
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task ToCompactOverlayAsync(CompactViewMode mode)
    {
        if (!App.IsDesktop)
        {
            return;
        }

        if (RootFrame is Frame f)
        {
            if (f.CurrentSourcePageType == typeof(CompactPage))
            {
                return;
            }

            // Ref: https://programmer.group/uwp-use-compact-overlay-mode-to-always-display-on-the-front-end.html
            var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            preferences.CustomSize = new Windows.Foundation.Size(320, 100);
            bool success = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(
                ApplicationViewMode.CompactOverlay,
                preferences);

            if (success)
            {
                BlankOutPage(Frame as Frame);
                f.Navigate(typeof(CompactPage), mode, new SuppressNavigationTransitionInfo());
            }
        }
    }

    /// <inheritdoc/>
    public async Task CloseCompactOverlayAsync(CompactViewMode closingOverlayMode)
    {
        if (!App.IsDesktop)
        {
            return;
        }

        if (RootFrame is not Frame f)
        {
            return;
        }

        var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.Default);
        await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default, preferences);

        ShellPageNavigationArgs args = new()
        {
            MillisecondsDelay = 300, // Required to avoid bug with the inner frame navigation happening too quickly and leads to content not being visible.
            FirstPageOverride = closingOverlayMode switch
            {
                CompactViewMode.Focus => ContentPageType.Focus,
                _ => ContentPageType.Home
            }
        };

        f.Navigate(typeof(ShellPage), args, new SuppressNavigationTransitionInfo());
    }

    private void BlankOutPage(Frame? f)
    {
        // Why blank out a frame?
        // This is a workaround to ensure the uninitialize code
        // of the frame page is executed. 
        // This is required to avoid bugs related to viewmodels being
        // initialized twice. 

        f?.Navigate(typeof(BlankPage));
    }
}
