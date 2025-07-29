using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class SaveMixButton : UserControl
{
    public static readonly DependencyProperty ShowInRelaxPageProperty = DependencyProperty.Register(
        nameof(ShowInRelaxPage),
        typeof(bool),
        typeof(SaveMixButton),
        new PropertyMetadata(true));

    /// <summary>
    /// Raised when a mix was saved.
    /// </summary>
    public event EventHandler? MixSaved;

    public SaveMixButton()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Controls checkbox for whether the mix should be shown in relax page.
    /// </summary>
    public bool ShowInRelaxPage
    {
        get => (bool)GetValue(ShowInRelaxPageProperty);
        set => SetValue(ShowInRelaxPageProperty, value);
    }

    private async void NameInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            string input = NameInput.Text;
            e.Handled = true;
            SaveFlyout.Hide();

            await SaveMixAsync(input, "keyboard");
        }
    }

    private void SaveFlyout_Closed(object sender, object e)
    {
        NameInput.Text = string.Empty;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        string input = NameInput.Text;
        SaveFlyout.Hide();
        await SaveMixAsync(input, "button");
    }

    private async Task SaveMixAsync(string input, string telemtrySource)
    {
        string[] tags = ShowInRelaxPage ? [AssetTagConstants.MeditatePageTag] : [];

        var id = await App.Services.GetRequiredService<ISoundMixService>().SaveCurrentMixAsync(input, tags: tags);

        if (!string.IsNullOrEmpty(id))
        {
            MixSaved?.Invoke(this, EventArgs.Empty);
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.MixSaved, new Dictionary<string, string>
            {
                { "invokedBy", telemtrySource }
            });
        }
    }
}
