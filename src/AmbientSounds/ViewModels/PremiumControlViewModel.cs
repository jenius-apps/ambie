using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Store;
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
    private readonly IPromoCodeService _promoCodeService;
    private readonly IUserSettings _userSettings;

    public PremiumControlViewModel(
        IIapService iapService,
        ITelemetry telemetry,
        ILocalizer localizer,
        ISystemInfoProvider infoProvider,
        IPromoCodeService promoCodeService,
        IUserSettings userSettings,
        IExperimentationService experimentationService)
    {
        _iapService = iapService;
        _telemetry = telemetry;
        _localizer = localizer;
        _infoProvider = infoProvider;
        _promoCodeService = promoCodeService;
        _userSettings = userSettings;
        AnnualSubExperimentEnabled = false; // Disabling until further notice.

        PromoCodeHyperlinkVisible = false; // Disabling until promo codes are confirmed ready.
    }

    [ObservableProperty]
    private bool _promoCodeHyperlinkVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PurchasePageVisible))]
    private bool _promoCodePageVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LifetimeButtonVisible))]
    [NotifyPropertyChangedFor(nameof(AnnualButtonVisible))]
    private bool _annualSubExperimentEnabled;

    public string MonthlyPriceButtonAutomationName => MonthlyPriceInfo?.FormattedPrice ?? string.Empty;

    public string LifetimePrice => LifetimePriceInfo?.FormattedPrice is { Length: > 0 } price
        ? _localizer.GetString("PriceForLifetime", price)
        : string.Empty;

    [ObservableProperty]
    private bool _buttonLoading;

    [ObservableProperty]
    private bool _lifetimeButtonLoading;

    [ObservableProperty]
    private bool _annualButtonLoading;

    [ObservableProperty]
    private bool _promoCodeBackButtonVisible = true;

    public bool PurchasePageVisible => !ThanksTextVisible && !PromoCodePageVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LifetimeButtonVisible))]
    [NotifyPropertyChangedFor(nameof(AnnualButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PurchasePageVisible))]
    private bool _thanksTextVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthlyPriceButtonAutomationName))]
    private PriceInfo? _monthlyPriceInfo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LifetimePrice))]
    [NotifyPropertyChangedFor(nameof(LifetimePriceCaption))]
    [NotifyPropertyChangedFor(nameof(LifetimePriceCaptionVisible))]
    private PriceInfo? _lifetimePriceInfo;

    public string LifetimePriceCaption => LifetimePriceInfo?.IsOnSale is true
        ? _localizer.FormatString("SaleCaption", LifetimePriceInfo.FormattedBasePrice, LifetimePriceInfo.SaleEndDateUtc.ToLocalTime().Humanize())
        : string.Empty;

    public bool LifetimePriceCaptionVisible => !string.IsNullOrEmpty(LifetimePriceCaption);

    [ObservableProperty]
    private string _promoCodeInput = string.Empty;

    public bool LifetimeButtonVisible => !ThanksTextVisible && !AnnualSubExperimentEnabled;

    public bool AnnualButtonVisible => !ThanksTextVisible && AnnualSubExperimentEnabled;

    public async Task InitializeAsync(
        bool launchPromoCodesDirectly,
        string? prefilledCode = null)
    {
        PromoCodeInput = prefilledCode ?? string.Empty;
        PromoCodeBackButtonVisible = !launchPromoCodesDirectly;
        PromoCodePageVisible = launchPromoCodesDirectly;

        await Task.WhenAll(InitializeMonthlyAsync(), InitializeLifetimeAsync());
    }

    private async Task InitializeMonthlyAsync()
    {
        if (MonthlyPriceInfo is { } || ButtonLoading)
        {
            return;
        }

        ButtonLoading = true;
        MonthlyPriceInfo = await _iapService.GetPriceAsync(IapConstants.MsStoreAmbiePlusId, isSubscriptionIdFormat: true);
#if DEBUG
        MonthlyPriceInfo = new PriceInfo
        {
            FormattedPrice = "$1.50",
            HasSubTrial = true,
            IsSubscription = true,
            SubTrialLength = 1,
            SubTrialLengthUnit = DurationUnit.Week,
            RecurrenceLength = 1,
            RecurrenceUnit = DurationUnit.Month
        };
#endif
        ButtonLoading = false;
    }

    private async Task InitializeLifetimeAsync()
    {
        if (LifetimePrice is { Length: > 0 } || LifetimeButtonLoading || AnnualSubExperimentEnabled)
        {
            return;
        }

        LifetimeButtonLoading = true;
        LifetimePriceInfo = await _iapService.GetPriceAsync(IapConstants.MsStoreAmbiePlusLifetimeId);
#if DEBUG
        LifetimePriceInfo = new PriceInfo
        {
            SaleEndDateUtc = DateTime.UtcNow.AddDays(7),
            IsOnSale = true,
            FormattedPrice = "$10.00",
            FormattedBasePrice = "$15.00"
        };
#endif
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
        bool purchaseSuccessful = await _iapService.BuyAsync(IapConstants.MsStoreAmbiePlusId, isSubscriptionIdFormat: true);
        ThanksTextVisible = purchaseSuccessful;

        if (purchaseSuccessful)
        {
            LogSuccessfulPurchaseTelemetry(TelemetryConstants.Purchased);
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
            LogSuccessfulPurchaseTelemetry(TelemetryConstants.LifetimePurchased);
        }
        else
        {
            _telemetry.TrackEvent(TelemetryConstants.LifetimeCanceled);
        }
        LifetimeButtonLoading = false;
    }

    [RelayCommand]
    private void TogglePromoCodePage()
    {
        PromoCodePageVisible = !PromoCodePageVisible;

        if (PromoCodePageVisible)
        {
            _telemetry.TrackEvent(TelemetryConstants.PromoCodePageOpened);
        }
        else
        {
            _telemetry.TrackEvent(TelemetryConstants.PromoCodeBackClicked);
        }
    }

    [RelayCommand]
    private async Task SubmitCodeAsync()
    {
        if (PromoCodeInput is not string { Length: > 0 } input)
        {
            return;
        }

        _telemetry.TrackEvent(TelemetryConstants.PromoCodeAttempted, new Dictionary<string, string>
        {
            { "code", input },
        });

        string? iapId = await _promoCodeService.TryGetAddOnAsync(input);
        if (iapId is not string { Length: > 0 })
        {
            return;
        }

        _telemetry.TrackEvent(TelemetryConstants.PromoCodeAddonShown, new Dictionary<string, string>
        {
            { "code", input },
            { "iapid", iapId },
        });

        bool success = await _iapService.BuyAsync(iapId, isSubscriptionIdFormat: true, iapIdCacheOverride: IapConstants.MsStoreAmbiePlusId);

        if (success)
        {
            PromoCodeInput = string.Empty;
            ThanksTextVisible = true;
            PromoCodePageVisible = false;

            LogSuccessfulPurchaseTelemetry(TelemetryConstants.PromoCodePurchased, code: input, iapId: iapId);
        }
        else
        {
            _telemetry.TrackEvent(TelemetryConstants.PromoCodeCancelled, new Dictionary<string, string>
            {
                { "code", input },
                { "iapid", iapId },
            });
        }
    }

    private void LogSuccessfulPurchaseTelemetry(string purchaseEventName, string? code = null, string? iapId = null)
    {
        DateTime? lastPurchaseDate = _userSettings.Get<long>(UserSettingsConstants.PremiumPurchaseUtcDateTicks) is long ticks && ticks > 0L
            ? new DateTime(ticks, DateTimeKind.Utc)
            : null;

        Dictionary<string, string> payload = new()
        {
            { "DaysSinceFirstUse", (DateTime.Now - _infoProvider.FirstUseDate()).Days.ToString() },
            { "LastPremiumPurhcaseUtcDate", lastPurchaseDate?.ToShortDateString() ?? "--" }
        };

        if (code is { Length: > 0 } && iapId is { Length: > 0 })
        {
            payload.Add("code", code);
            payload.Add("iapid", iapId);
        }

        _telemetry.TrackEvent(purchaseEventName, payload);

        _userSettings.Set(UserSettingsConstants.PremiumPurchaseUtcDateTicks, DateTime.UtcNow.Ticks);
    }
}
