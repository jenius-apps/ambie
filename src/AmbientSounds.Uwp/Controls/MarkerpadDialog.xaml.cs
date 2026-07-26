using System;
using System.Text.Json;
using Windows.Services.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class MarkerpadDialog : ContentDialog
{
    public MarkerpadDialog()
    {
        this.InitializeComponent();
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private async void OnStoreLinkClicked(object sender, RoutedEventArgs e)
    {
        var storecontext = StoreContext.GetDefault();
        if (storecontext is null)
        {
            return;
        }

        StoreInstallParameters parameters = new()
        {
            StoreAction = 1,
            ProductId = "9nh0wpdrk28t",
            AutoInstall = true,
            AutoOpenOnInstallComplete = true
        };

        await StoreRequestHelper.SendRequestAsync(storecontext, 32, JsonSerializer.Serialize(parameters, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public class StoreInstallParameters
{
    public int StoreAction { get; set; }

    public string ProductId { get; set; } = string.Empty;

    public bool AutoInstall { get; set; }

    public bool AutoOpenOnInstallComplete { get; set; }
}
