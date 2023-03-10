﻿using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    public sealed partial class CataloguePage : Page
    {
        private CancellationTokenSource? _cts;

        public CataloguePage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<CataloguePageViewModel>();
        }

        public CataloguePageViewModel ViewModel => (CataloguePageViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _cts = new CancellationTokenSource();
            var initializeTask = ViewModel.InitializeAsync(_cts.Token);

            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "catalogue" }
            });

            try
            {
                await initializeTask;
            }
            catch (OperationCanceledException)
            {
                _cts.Dispose();
                _cts = null;
                return;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _cts?.Cancel();
            ViewModel.Uninitialize();
        }
    }
}
