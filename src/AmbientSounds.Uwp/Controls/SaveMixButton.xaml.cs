using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using JeniusApps.Common.Telemetry;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SaveMixButton : UserControl
    {
        public event EventHandler? MixSaved;

        public SaveMixButton()
        {
            this.InitializeComponent();
        }

        private async void NameInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string input = NameInput.Text;
                e.Handled = true;
                SaveFlyout.Hide();
                var id = await App.Services.GetRequiredService<ISoundMixService>().SaveCurrentMixAsync(input);

                if (!string.IsNullOrEmpty(id))
                {
                    MixSaved?.Invoke(this, EventArgs.Empty);
                    App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.MixSaved, new Dictionary<string, string>
                    {
                        { "invokedBy", "keyboard" }
                    });
                }
            }
        }

        private void SaveFlyout_Closed(object sender, object e)
        {
            NameInput.Text = "";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string input = NameInput.Text;
            SaveFlyout.Hide();
            var id = await App.Services.GetRequiredService<ISoundMixService>().SaveCurrentMixAsync(input);

            if (!string.IsNullOrEmpty(id))
            {
                MixSaved?.Invoke(this, EventArgs.Empty);
                App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.MixSaved, new Dictionary<string, string>
                {
                    { "invokedBy", "button" }
                });
            }
        }
    }
}
