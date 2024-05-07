using AmbientSounds.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class SoundDownloadDialog : NoPaddingDialog
{
    public SoundDownloadDialog(OnlineSoundViewModel sound)
    {
        this.InitializeComponent();
        this.Closed += OnClosed;
        Sound = sound;
        Sound.DownloadCompleted += OnDownloadCompleted;
    }

    public OnlineSoundViewModel Sound { get; }

    public bool Result { get; private set; }

    private string PrimaryText => Sound switch
    {
        _ when Sound.CanPlay => Strings.Resources.PlayerPlayText,
        _ when Sound.CanBuy => Strings.Resources.UnlockText,
        _ when Sound.DownloadButtonVisible => Strings.Resources.DownloadText,
        _ => string.Empty
    };

    private string PrimaryGlyph => Sound switch
    {
        _ when Sound.CanPlay => "\uF5B0",
        _ when Sound.CanBuy => "\uE785",
        _ when Sound.DownloadButtonVisible => "\uEBD3",
        _ => string.Empty
    };

    private Thickness PrimaryIconMargin => Sound switch
    {
        _ when Sound.DownloadButtonVisible => new Thickness(0,2,0,0),
        _ => new Thickness()
    };

    private void OnPrimaryClicked(object sender, RoutedEventArgs e)
    {
        if (Sound.CanPlay)
        {
            _ = Sound.PlayCommand.ExecuteAsync(null);
            return;
        }
        else if (Sound.IsOwned && Sound.DownloadButtonVisible)
        {
            _ = Sound.DownloadCommand.ExecuteAsync(null);
            return;
        }

        Result = true;
        Hide();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        Result = false;
        Hide();
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Sound.DownloadCompleted -= OnDownloadCompleted;
    }

    private void OnDownloadCompleted(object sender, EventArgs e)
    {
        this.Bindings.Update();
    }
}
