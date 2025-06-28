using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.ViewModels;

public sealed partial class CompactPageViewModel : ObservableObject
{
    private readonly IUserSettings _userSettings;
    private readonly INavigator _navigator;
    private readonly ICompactNavigator _compactNavigator;
    private readonly ITelemetry _telemetry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeSelected))]
    [NotifyPropertyChangedFor(nameof(IsFocusSelected))]
    private CompactViewMode _currentView;

    public CompactPageViewModel(
        INavigator navigator,
        IUserSettings userSettings,
        ICompactNavigator compactNavigator,
        ITelemetry telemetry)
    {                   
        Guard.IsNotNull(navigator);
        Guard.IsNotNull(userSettings);
        Guard.IsNotNull(compactNavigator);
        Guard.IsNotNull(telemetry);
        _navigator = navigator;
        _userSettings = userSettings;
        _compactNavigator = compactNavigator;
        _telemetry = telemetry;
    }

    public bool IsHomeSelected => CurrentView == CompactViewMode.Home;

    public bool IsFocusSelected => CurrentView == CompactViewMode.Focus;

    public bool UseCompactMode
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.CompactOnFocusKey);
        set => _userSettings.Set(UserSettingsConstants.CompactOnFocusKey, value);
    }

    public bool PlayAfterFocusEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.PlayAfterFocusKey);
        set => _userSettings.Set(UserSettingsConstants.PlayAfterFocusKey, value);
    }

    public async Task InitializeAsync(CompactViewMode requestedViewMode)
    {
        await Task.Delay(1);
        NavigateTo(requestedViewMode);
    }

    public void NavigateTo(CompactViewMode requestedViewMode)
    {
        _telemetry.TrackEvent(TelemetryConstants.MiniNavigate, new Dictionary<string, string>
        {
            { "to", requestedViewMode.ToString() }
        });

        CurrentView = requestedViewMode;
        _compactNavigator.NavigateTo(requestedViewMode);
    }

    [RelayCommand]
    private async Task CloseCompactAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.MiniBack);
        await _navigator.CloseCompactOverlayAsync(CurrentView);
    }
}
