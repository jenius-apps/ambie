using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class ConfirmCloseDialog : ContentDialog
{
    private readonly bool _originalConfirmCloseStatus;

    public static readonly DependencyProperty ConfirmCloseEnabledProperty = DependencyProperty.Register(
        nameof(ConfirmCloseEnabled),
        typeof(bool),
        typeof(ConfirmCloseDialog),
        new PropertyMetadata(false));

    public ConfirmCloseDialog()
    {
        this.InitializeComponent();
        ConfirmCloseEnabled = App.Services.GetRequiredService<IUserSettings>().Get<bool>(UserSettingsConstants.ConfirmCloseKey);
        _originalConfirmCloseStatus = ConfirmCloseEnabled;
    }

    public bool ConfirmCloseEnabled
    {
        get => (bool)GetValue(ConfirmCloseEnabledProperty);
        set => SetValue(ConfirmCloseEnabledProperty, value);
    }

    private void OnActionButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        App.Services.GetRequiredService<IUserSettings>().Set(UserSettingsConstants.ConfirmCloseKey, ConfirmCloseEnabled);

        if (_originalConfirmCloseStatus != ConfirmCloseEnabled)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(
                ConfirmCloseEnabled ? TelemetryConstants.ConfirmCloseEnabled : TelemetryConstants.ConfirmCloseDisabled,
                new Dictionary<string, string> { ["page"] = "confirmDialog" });
        }
    }
}
