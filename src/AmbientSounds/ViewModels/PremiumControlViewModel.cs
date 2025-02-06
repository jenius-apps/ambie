using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class PremiumControlViewModel : ObservableObject
{
    private readonly IIapService _iapService;
    private readonly ITelemetry _telemetry;
    private readonly ILocalizer _localizer;
    private readonly ISystemInfoProvider _infoProvider;

    public PremiumControlViewModel(
        IIapService iapService,
        ITelemetry telemetry,
        ILocalizer localizer,
        ISystemInfoProvider infoProvider)
    {
        _iapService = iapService;
        _telemetry = telemetry;
        _localizer = localizer;
        _infoProvider = infoProvider;
        AnnualSubExperimentEnabled = false; // Disabling until further notice.
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LifetimeButtonVisible))]
    [NotifyPropertyChangedFor(nameof(AnnualButtonVisible))]
    private bool _annualSubExperimentEnabled;

    public string MonthlyPriceButtonAutomationName => MonthlyPriceInfo?.FormattedPrice ?? string.Empty;

    [ObservableProperty]
    private string _lifetimePrice = string.Empty;

    [ObservableProperty]
    private bool _buttonLoading;

    [ObservableProperty]
    private bool _lifetimeButtonLoading;

    [ObservableProperty]
    private bool _annualButtonLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LifetimeButtonVisible))]
    [NotifyPropertyChangedFor(nameof(AnnualButtonVisible))]
    private bool _thanksTextVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthlyPriceButtonAutomationName))]
    private PriceInfo? _monthlyPriceInfo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AnnualPriceButtonAutomationName))]
    private PriceInfo? _annualPriceInfo;

    public string AnnualPriceButtonAutomationName => AnnualPriceInfo?.FormattedPrice ?? string.Empty;

    public bool LifetimeButtonVisible => !ThanksTextVisible && !AnnualSubExperimentEnabled;

    public bool AnnualButtonVisible => !ThanksTextVisible && AnnualSubExperimentEnabled;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(InitializeMonthlyAsync(), InitializeLifetimeAsync(), InitializeAnnualAsync());
    }

    private async Task InitializeMonthlyAsync()
    {
        if (MonthlyPriceInfo is { } || ButtonLoading)
        {
            return;
        }

        ButtonLoading = true;
        MonthlyPriceInfo = await _iapService.GetLatestPriceAsync(IapConstants.MsStoreAmbiePlusId);
        ButtonLoading = false;
    }

    private async Task InitializeLifetimeAsync()
    {
        if (LifetimePrice is { Length: > 0} || LifetimeButtonLoading || AnnualSubExperimentEnabled)
        {
            return;
        }

        LifetimeButtonLoading = true;
        var priceInfo = await _iapService.GetLatestPriceAsync(IapConstants.MsStoreAmbiePlusLifetimeId);
        LifetimePrice = _localizer.GetString("PriceForLifetime", priceInfo.FormattedPrice);
        LifetimeButtonLoading = false;
    }

    private async Task InitializeAnnualAsync()
    {
        if (AnnualPriceInfo is { } || AnnualButtonLoading || !AnnualSubExperimentEnabled)
        {
            return;
        }

        AnnualButtonLoading = true;
        AnnualPriceInfo = await _iapService.GetLatestPriceAsync(IapConstants.MsStoreAmbiePlusAnnualId);
        AnnualButtonLoading = false;
    }

    [RelayCommand]
    private async Task PurchaseMonthlyAsync()
    {
        if (ButtonLoading || LifetimeButtonLoading)
        {
            return;
        }

        ButtonLoading = true;
        _telemetry.TrackEvent(TelemetryConstants.SubscribeClicked);
        bool purchaseSuccessful = await _iapService.BuyAsync(IapConstants.MsStoreAmbiePlusId, latest: true);
        ThanksTextVisible = purchaseSuccessful;

        if (purchaseSuccessful)
        {
            _telemetry.TrackEvent(TelemetryConstants.Purchased, new Dictionary<string, string>
            {
                { "DaysSinceFirstUse", (DateTime.Now - _infoProvider.FirstUseDate()).Days.ToString() },
            });
        }
        else
        {
            _telemetry.TrackEvent(TelemetryConstants.PurchaseCancelled);
        }
        ButtonLoading = false;
    }

    [RelayCommand]
    private async Task PurchaseAnnualAsync()
    {
        if (ButtonLoading || LifetimeButtonLoading || AnnualButtonLoading || !AnnualSubExperimentEnabled)
        {
            return;
        }

        AnnualButtonLoading = true;

        _telemetry.TrackEvent(TelemetryConstants.PremiumAnnualClicked);
        bool successful = await _iapService.BuyAsync(IapConstants.MsStoreAmbiePlusAnnualId, latest: true);
        ThanksTextVisible = successful;

        if (successful)
        {
            _telemetry.TrackEvent(TelemetryConstants.PremiumAnnualPurchased, new Dictionary<string, string>
            {
                { "DaysSinceFirstUse", (DateTime.Now - _infoProvider.FirstUseDate()).Days.ToString() },
            });
        }
        else
        {
            _telemetry.TrackEvent(TelemetryConstants.PremiumAnnualCanceled);
        }

        AnnualButtonLoading = false;
    }

    [RelayCommand]
    private async Task PurchaseLifetimeAsync()
    {
        if (ButtonLoading || LifetimeButtonLoading)
        {
            return;
        }

        LifetimeButtonLoading = true;
        _telemetry.TrackEvent(TelemetryConstants.LifetimeClicked);
        bool purchaseSuccessful = await _iapService.BuyAsync(IapConstants.MsStoreAmbiePlusLifetimeId);
        ThanksTextVisible = purchaseSuccessful;

        if (purchaseSuccessful)
        {
            _telemetry.TrackEvent(TelemetryConstants.LifetimePurchased, new Dictionary<string, string>
            {
                { "DaysSinceFirstUse", (DateTime.Now - _infoProvider.FirstUseDate()).Days.ToString() },
            });
        }
        else
        {
            _telemetry.TrackEvent(TelemetryConstants.LifetimeCanceled);
        }
        LifetimeButtonLoading = false;
    }
}
