using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels;

public class UpdateSoundViewModel : ObservableObject
{
    private readonly IAssetLocalizer _assetLocalizer;

    public UpdateSoundViewModel(
        Sound s,
        IAssetLocalizer assetLocalizer)
    {
        Guard.IsNotNull(s);
        Guard.IsNotNull(assetLocalizer);

        Sound = s;
        _assetLocalizer = assetLocalizer;
    }

    public Sound Sound { get; }

    public string Name => _assetLocalizer.GetLocalName(Sound);

    public string MetaDataVersion => Sound.MetaDataVersion.ToString();

    public string FileVersion => Sound.MetaDataVersion.ToString();
}
