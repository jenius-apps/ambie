using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
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
        Guard.IsNotNull(iapService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(localizer);
        Guard.IsNotNull(infoProvider);

        _iapService = iapService;
        _telemetry = telemetry;
        _localizer = localizer;
        _infoProvider = infoProvider;

        SubscriptionTexts =
        [
            _localizer.GetString("SubscriptionText1"),
            _localizer.GetString("SubscriptionText2"),
            _localizer.GetString("SubscriptionText3"),
        ];
    }

    public IReadOnlyList<string> SubscriptionTexts { get; }

    [ObservableProperty]
    private string _price = string.Empty;

    [ObservableProperty]
    private string _lifetimePrice = string.Empty;

    [ObservableProperty]
    private bool _buttonLoading;

    [ObservableProperty]
    private bool _lifetimeButtonLoading;

    [ObservableProperty]
    private bool _thanksTextVisible;

    [ObservableProperty]
    private PriceInfo? _priceInfo;

    public async Task InitializeAsync()
    {
        if (PriceInfo is { } && 
            LifetimePrice is { Length: > 0 })
        {
            return;
        }

        ButtonLoading = true;
        LifetimeButtonLoading = true;

        var priceTask = _iapService.GetLatestPriceAsync(IapConstants.MsStoreAmbiePlusId);
        var lifetimePriceTask = _iapService.GetLatestPriceAsync(IapConstants.MsStoreAmbiePlusLifetimeId);

        PriceInfo = await priceTask;
        LifetimePrice = _localizer.GetString("PriceForLifetime", (await lifetimePriceTask).FormattedPrice);
        Price = PriceInfo.FormattedPrice;

        ButtonLoading = false;
        LifetimeButtonLoading = false;
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
