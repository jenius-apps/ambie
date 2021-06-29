using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
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

            bool purchaseSuccessful = await _iapService.BuyAsync(IapConstants.MsStoreAmbiePlusId);
            ThanksTextVisible = purchaseSuccessful;
            _telemetry.TrackEvent(TelemetryConstants.SubscribeClicked, new Dictionary<string, string>() 
            {
                { "purchased", purchaseSuccessful.ToString() }
            });

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

            string price = await _iapService.GetPriceAsync(IapConstants.MsStoreAmbiePlusId);
            Price = string.Format(Strings.Resources.PricePerMonth, price);
            
            ButtonLoading = false;
            this.Bindings.Update();
        }
    }
}
