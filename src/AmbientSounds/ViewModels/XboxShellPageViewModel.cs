using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class XboxShellPageViewModel : ObservableObject
{
    private readonly IIapService _iapService;
    private readonly ITelemetry _telemetry;
    private readonly IDialogService _dialogService;

    public XboxShellPageViewModel(
        IMixMediaPlayerService mixMediaPlayerService,
        IIapService iapService,
        ITelemetry telemetry,
        IDialogService dialogService)
    {
        // For xbox, there's no such thing as a custom global volume.
        // We let the user adjust their TV volume for that.
        // So ensure that we always set this to 1 on Xbox.
        mixMediaPlayerService.GlobalVolume = 1;

        _iapService = iapService;
        _telemetry = telemetry;
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private bool _premiumButtonVisible;

    public async Task InitializeAsync()
    {
        await UpdatePremiumButtonAsync();
    }

    private async Task UpdatePremiumButtonAsync()
    {
        PremiumButtonVisible = await _iapService.CanShowPremiumButtonsAsync();
    }

    [RelayCommand]
    private async Task OpenPremiumAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.ShellPagePremiumClicked);
        await _dialogService.OpenPremiumAsync();
    }
}
