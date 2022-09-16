﻿using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Services.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class MoreButton : UserControl
    {
        private const string Translations = "https://github.com/jenius-apps/ambie#translation";
        private const string Contact = "mailto:jenius_apps@outlook.com";
        private const string StorePage = "https://www.microsoft.com/store/productId/9P07XNM5CHP0";

        public MoreButton()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private bool CanShowOnPc => !App.IsTenFoot;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var dtm = DataTransferManager.GetForCurrentView();
            dtm.DataRequested += OnDataRequested;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var dtm = DataTransferManager.GetForCurrentView();
            dtm.DataRequested -= OnDataRequested;
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.SetWebLink(new Uri(StorePage));
            request.Data.Properties.Title = StorePage;
            request.Data.Properties.Description = "Ambie";
        }

        private void ShareClicked(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private async void RateUsClicked(object sender, RoutedEventArgs e)
        {
            var storeContext = StoreContext.GetDefault();
            await storeContext.RequestRateAndReviewAppAsync();
        }

        private async void ContactClicked(object sender, RoutedEventArgs e)
        {
            await LaunchAsync(Contact);
        }

        private async void TranslationClicked(object sender, RoutedEventArgs e)
        {
            await LaunchAsync(Translations);
        }

        private async Task LaunchAsync(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) 
                return;

            try
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
            catch { }
        }

        private async void SettingsClicked(object sender, RoutedEventArgs e)
        {
            var dialogService = App.Services.GetRequiredService<IDialogService>();
            await dialogService.OpenSettingsAsync();
        }

        private async void ThemeSettingsClicked(object sender, RoutedEventArgs e)
        {
            var dialogService = App.Services.GetRequiredService<IDialogService>();
            await dialogService.OpenThemeSettingsAsync();
        }
    }
}
