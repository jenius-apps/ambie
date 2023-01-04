using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels;

public partial class InterruptionPageViewModel : ObservableObject
{
    private const int MaxMinutes = 99;
    private const int MinMinutes = 0;
    private readonly IFocusHistoryService _focusHistoryService;
    private readonly ICompactNavigator _compactNavigator;
    private readonly ISystemInfoProvider _systemInfoProvider;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfirmEnabled))]
    private int _minutesLogged; 
    
    [ObservableProperty]
    private string _notes = string.Empty;

    public InterruptionPageViewModel(
        IFocusHistoryService focusHistoryService,
        ICompactNavigator compactNavigator,
        ISystemInfoProvider systemInfoProvider)
    {
        Guard.IsNotNull(focusHistoryService);
        Guard.IsNotNull(compactNavigator);
        Guard.IsNotNull(systemInfoProvider);

        _focusHistoryService = focusHistoryService;
        _compactNavigator = compactNavigator;
        _systemInfoProvider = systemInfoProvider;
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

        MinutesLogged = 0;
        Notes = string.Empty;

        if (_systemInfoProvider.IsCompact())
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
