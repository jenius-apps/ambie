using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.ViewModels;

public sealed partial class CompactPageViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private CompactViewMode _currentView;

    public CompactPageViewModel(INavigator navigator)
    {
        Guard.IsNotNull(navigator);
        _navigator = navigator;
    }

    public async Task InitializeAsync(CompactViewMode requestedViewMode)
    {
        await Task.Delay(1);
        _currentView = requestedViewMode;
    }

    [RelayCommand]
    private async Task CloseCompactAsync()
    {
        await _navigator.CloseCompactOverlayAsync(_currentView);
    }
}
