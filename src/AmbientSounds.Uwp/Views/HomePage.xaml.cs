﻿using AmbientSounds.Constants;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        App.Services.GetRequiredService<ITelemetry>().TrackEvent(
            TelemetryConstants.PageNavTo,
            new Dictionary<string, string>
            {
                { "name", "home" }
            });
    }
}
