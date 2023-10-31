using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;

namespace AmbientSounds.ViewModels;

public partial class InterruptionPageViewModel : ObservableObject
{
    private const int MaxMinutes = 99;
    private const int MinMinutes = 0;
    private readonly IFocusHistoryService _focusHistoryService;
    private readonly ICompactNavigator _compactNavigator;
    private readonly ISystemInfoProvider _systemInfoProvider;
    private readonly ITelemetry _telemetry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfirmEnabled))]
    private int _minutesLogged; 
    
    [ObservableProperty]
    private string _notes = string.Empty;

    public InterruptionPageViewModel(
        IFocusHistoryService focusHistoryService,
        ICompactNavigator compactNavigator,
        ISystemInfoProvider systemInfoProvider,
        ITelemetry telemetry)
    {
        Guard.IsNotNull(focusHistoryService);
        Guard.IsNotNull(compactNavigator);
        Guard.IsNotNull(systemInfoProvider);
        Guard.IsNotNull(telemetry);

        _focusHistoryService = focusHistoryService;
        _compactNavigator = compactNavigator;
        _systemInfoProvider = systemInfoProvider;
        _telemetry = telemetry;
    }

    public int MaximumMinutes => MaxMinutes;

    public int MinimumMinutes => MinMinutes;

    public bool IsConfirmEnabled => MinutesLogged > MinMinutes && MinutesLogged <= MaxMinutes;

    [RelayCommand]
    private void LogInterruption()
    {
        if (MinutesLogged <= MinMinutes || MinutesLogged > MaxMinutes)
        {
            return;
        }

        _focusHistoryService.LogInterruption(MinutesLogged, Notes);
        bool isCompact = _systemInfoProvider.IsCompact();
        _telemetry.TrackEvent(
            TelemetryConstants.FocusInterruptionLogged,
            _focusHistoryService.GatherInterruptionTelemetry(MinutesLogged, Notes, isCompact));

        MinutesLogged = 0;
        Notes = string.Empty;

        if (isCompact)
        {
            _compactNavigator.GoBackSafely();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        MinutesLogged = 0;
        Notes = string.Empty;

        if (_systemInfoProvider.IsCompact())
        {
            _compactNavigator.GoBackSafely();
        }
    }
}
