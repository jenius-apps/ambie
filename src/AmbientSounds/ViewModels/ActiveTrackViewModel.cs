using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class ActiveTrackViewModel
    {
        public ActiveTrackViewModel(SoundViewModel soundViewModel)
        {
            Guard.IsNotNull(soundViewModel, nameof(soundViewModel));
            SoundVm = soundViewModel;
        }

        public SoundViewModel SoundVm { get; }

        public string Name => SoundVm.Name ?? "";
    }
}
