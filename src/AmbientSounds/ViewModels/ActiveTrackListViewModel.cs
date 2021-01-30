using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class ActiveTrackListViewModel
    {
        // edit sound vm to track volume and change it.
        

        public ObservableCollection<ActiveTrackViewModel> ActiveTracks = new();
    }
}
