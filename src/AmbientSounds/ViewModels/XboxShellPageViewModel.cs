using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels;

public partial class XboxShellPageViewModel : ObservableObject
{
    public XboxShellPageViewModel(IMixMediaPlayerService mixMediaPlayerService)
    {
        // For xbox, there's no such thing as a custom global volume.
        // We let the user adjust their TV volume for that.
        // So ensure that we always set this to 1 on Xbox.
        mixMediaPlayerService.GlobalVolume = 1;
    }
}
