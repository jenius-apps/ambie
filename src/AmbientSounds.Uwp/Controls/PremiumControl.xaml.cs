using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class PremiumControl : UserControl
    {
        private readonly IIapService _iapService;
        private readonly ITelemetry _telemetry;

        public event EventHandler CloseRequested;

        public PremiumControl()
        {
            this.InitializeComponent();
            _iapService = App.Services.GetRequiredService<IIapService>();
            _telemetry = App.Services.GetRequiredService<ITelemetry>();
            SubscriptionTexts = new string[]
            {
                Strings.Resources.SubscriptionText1,
                Strings.Resources.SubscriptionText2,
                Strings.Resources.SubscriptionText3,
            };
        }

        private string[] SubscriptionTexts { get; }

        private string Price { get; set; } = string.Empty;

        private bool ButtonLoading { get; set; }

        private bool ThanksTextVisible { get; set; } = false;

        private async void OnPurchaseClicked(object sender, RoutedEventArgs e)
        {
            ButtonLoading = true;
            this.Bindings.Update();

            _telemetry.TrackEvent(TelemetryConstants.SubscribeClicked);
            bool purchaseSuccessful = await _iapService.BuyAsync(IapConstants.MsStoreAmbiePlusId, latest: true);
            ThanksTextVisible = purchaseSuccessful;

            if (purchaseSuccessful)
            {
                _telemetry.TrackEvent(TelemetryConstants.Purchased, new Dictionary<string, string>
                {
                    { "DaysSinceFirstUse", (DateTime.Now - SystemInformation.Instance.FirstUseTime).Days.ToString() },
                });
            }
            else
            {
                _telemetry.TrackEvent(TelemetryConstants.PurchaseCancelled);
            }

            ButtonLoading = false;
            this.Bindings.Update();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private async void OnPurchaseButtonLoaded(object sender, RoutedEventArgs e)
        {
            ButtonLoading = true;
            this.Bindings.Update();

            string price = await _iapService.GetLatestPriceAsync(IapConstants.MsStoreAmbiePlusId);
            Price = string.Format(Strings.Resources.PricePerMonth, price);
            
            ButtonLoading = false;
            this.Bindings.Update();
        }
    }
}
