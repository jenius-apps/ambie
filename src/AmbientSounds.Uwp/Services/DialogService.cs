using AmbientSounds.Constants;
using AmbientSounds.Controls;
using AmbientSounds.Converters;
using AmbientSounds.ViewModels;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Services.Uwp;

/// <summary>
/// Class for opening dialogs.
/// </summary>
public class DialogService : IDialogService
{
    private readonly IUserSettings _userSettings;
    private readonly ISystemInfoProvider _systemInfoProvider;

    public DialogService(
        IUserSettings userSettings,
        ISystemInfoProvider systemInfoProvider)
    {
        Guard.IsNotNull(userSettings);
        Guard.IsNotNull(systemInfoProvider);
        _userSettings = userSettings;
        _systemInfoProvider = systemInfoProvider;
    }

    /// <summary>
    /// UWP apps crash if more than one content dialog
    /// is opened at the same time. This flag
    /// will be used to help ensure only one
    /// dialog is open at a time.
    /// </summary>
    public static bool IsDialogOpen;

    /// <inheritdoc/>
    public async Task OpenTutorialAsync()
    {
        if (IsDialogOpen)
        {
            return;
        }

        IsDialogOpen = true;
        var dialog = new TutorialDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme()
        };

        await dialog.ShowAsync();
        IsDialogOpen = false;
    }

    /// <inheritdoc/>
    public async Task<bool> MissingSoundsDialogAsync()
    {
        if (IsDialogOpen)
        {
            return false;
        }

        IsDialogOpen = true;
        var dialog = new ContentDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
            Title = Strings.Resources.MissingSoundsTitle,
            PrimaryButtonText = Strings.Resources.DownloadText,
            CloseButtonText = Strings.Resources.CancelText,
            Content = Strings.Resources.MissingSoundsMessage
        };

        var result = await dialog.ShowAsync();
        IsDialogOpen = false;
        return result == ContentDialogResult.Primary;
    }

    /// <inheritdoc/>
    public async Task<string> RenameAsync(string currentName)
    {
        if (IsDialogOpen)
            return currentName;

        IsDialogOpen = true;
        var inputBoxControl = new RenameInputBox() { Input = currentName };
        bool enterClicked = false;
        var dialog = new ContentDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Title = Strings.Resources.RenameText,
            CloseButtonText = Strings.Resources.CancelText,
            PrimaryButtonText = Strings.Resources.RenameText,
            Content = inputBoxControl
        };
        inputBoxControl.EnterClicked += (s, e) => { dialog.Hide(); enterClicked = true; };

        var result = await dialog.ShowAsync();
        IsDialogOpen = false;

        return result == ContentDialogResult.Primary || enterClicked ? inputBoxControl.Input : currentName;
    }

    /// <inheritdoc/>
    public async Task OpenPremiumAsync()
    {
        if (IsDialogOpen || _systemInfoProvider.IsCompact())
        {
            return;
        }

        IsDialogOpen = true;
        var content = new PremiumControl();
        var dialog = new NoPaddingDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
            Content = content
        };

        content.CloseRequested += (s, e) => dialog.Hide();
        await dialog.ShowAsync();
        IsDialogOpen = false;
    }

    /// <inheritdoc/>
    public async Task OpenVideosMenuAsync()
    {
        if (IsDialogOpen)
        {
            return;
        }

        IsDialogOpen = true;
        var dialog = new ContentDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Title = Strings.Resources.ScreensaverCatalogue,
            CloseButtonText = Strings.Resources.CloseText,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
            Content = new VideosMenu()
        };

        await dialog.ShowAsync();
        IsDialogOpen = false;
    }

    /// <inheritdoc/>
    public async Task<(double, string)> OpenInterruptionAsync()
    {
        if (IsDialogOpen)
        {
            return (0, string.Empty);
        }

        IsDialogOpen = true;
        var dialog = new InterruptionDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
        };

        var result = await dialog.ShowAsync();
        IsDialogOpen = false;

        return result == ContentDialogResult.Primary 
            ? (dialog.MinutesLogged, dialog.InterruptionNotes) 
            : (0, string.Empty);
    }

    /// <inheritdoc/>
    public async Task OpenHistoryDetailsAsync(FocusHistoryViewModel historyViewModel)
    {
        if (historyViewModel is null || IsDialogOpen)
        {
            return;
        }

        IsDialogOpen = true;
        var dialog = new ContentDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Title = Strings.Resources.History,
            CloseButtonText = Strings.Resources.CloseText,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
            Content = new FocusHistoryDetails(historyViewModel)
        };

        await dialog.ShowAsync();
        IsDialogOpen = false;
    }

    /// <inheritdoc/>
    public async Task<string?> EditTextAsync(
        string prepopulatedText,
        int? maxSize = null)
    {
        if (IsDialogOpen)
        {
            return null;
        }

        IsDialogOpen = true;
        var dialog = new EditTextDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Text = prepopulatedText,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
        };

        if (maxSize.HasValue)
        {
            dialog.MaxLength = maxSize.Value;
        }

        var result = await dialog.ShowAsync();
        IsDialogOpen = false;

        return result == ContentDialogResult.Primary && prepopulatedText != dialog.Text
            ? dialog.Text.Trim()
            : null;
    }

    /// <inheritdoc/>
    public async Task OpenShareAsync(IReadOnlyList<string> soundIds)
    {
        if (IsDialogOpen)
        {
            return;
        }

        IsDialogOpen = true;

        var dialog = new ShareDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
        };

        _ = dialog.InitializeAsync(soundIds);
        await dialog.ShowAsync();
        dialog.Uninitialize();
        IsDialogOpen = false;
    }

    /// <inheritdoc/>
    public async Task MissingShareSoundsDialogAsync()
    {
        if (IsDialogOpen)
        {
            return;
        }

        IsDialogOpen = true;
        var content = new DownloadMissingList();
        _ = content.InitializeAsync();
        var dialog = new ContentDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Title = Strings.Resources.MissingSoundsTitle,
            CloseButtonText = Strings.Resources.CloseText,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
            Content = content
        };

        await dialog.ShowAsync();
        content.Uninitialize();
        IsDialogOpen = false;
    }

    public async Task RecentInterruptionsAsync()
    {
        if (IsDialogOpen)
        {
            return;
        }

        IsDialogOpen = true;
        var content = new InterruptionInsights();
        _ = content.InitializeAsync();

        var dialog = new ContentDialog()
        {
            FlowDirection = App.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Title = Strings.Resources.RecentInterruptionsText,
            CloseButtonText = Strings.Resources.CloseText,
            RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
            Content = content
        };

        await dialog.ShowAsync();
        content.Uninitialize();

        IsDialogOpen = false;
    }
}
