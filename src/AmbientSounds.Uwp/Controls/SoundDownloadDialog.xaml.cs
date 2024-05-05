using AmbientSounds.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class SoundDownloadDialog : NoPaddingDialog
{
    public SoundDownloadDialog(OnlineSoundViewModel sound)
    {
        this.InitializeComponent();
        Sound = sound;
    }

    public OnlineSoundViewModel Sound { get; }

    public bool Result { get; private set; }

    private string PrimaryText => Sound switch
    {
        _ when Sound.CanBuy => Strings.Resources.UnlockText,
        _ when Sound.DownloadButtonVisible => Strings.Resources.DownloadText,
        _ => string.Empty
    };

    private string PrimaryGlyph => Sound switch
    {
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
        Result = true;
        Hide();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        Result = false;
        Hide();
    }
}
