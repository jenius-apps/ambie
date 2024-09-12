using AmbientSounds.Constants;
using JeniusApps.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class InterruptionDialog : ContentDialog
{
    public InterruptionDialog()
    {
        this.InitializeComponent();
        this.Opened += OnDialogOpened;
    }

    public double MinutesLogged { get; private set; }

    public string InterruptionNotes { get; private set; }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        MinutesLogged = MinutesBox.Value;
        InterruptionNotes = NotesBox.Text;
    }

    private void NumberBox_ValueChanged(WinUI.NumberBox sender, WinUI.NumberBoxValueChangedEventArgs args)
    {
        IsPrimaryButtonEnabled = args.NewValue > 0d;
    }

    private void OnDialogOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        var settings = App.Services.GetRequiredService<IUserSettings>();
        HelpMessageBar.IsOpen = !settings.Get<bool>(UserSettingsConstants.HasClosedInterruptionMessageKey);
    }

    private void HelpMessageBar_CloseButtonClick(WinUI.InfoBar sender, object args)
    {
        var settings = App.Services.GetRequiredService<IUserSettings>();
        settings.Set(UserSettingsConstants.HasClosedInterruptionMessageKey, true);
        HelpMessageBar.IsOpen = false;
    }
}
