using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class FocusTaskModuleViewModel : ObservableObject
    {
        public FocusTaskModuleViewModel()
        {
            Tasks.Add("alskfdj alskdfj lkjalsdkfjalskdfjalskdjf laksdjf laksdjflaksdjflaksdjf lkasjdfl aksdjf ");
            Tasks.Add("alskfdj alskdfj lkjalsdkfjalskdfjalskdjf laksdjf laksdjflaksdjflaksdjf lkasjdfl aksdjf ");
            Tasks.Add("alskfdj alskdfj lkjalsdkfjalskdfjalskdjf laksdjf laksdjflaksdjflaksdjf lkasjdfl aksdjf ");
            Tasks.Add("alskfdj alskdfj lkjalsdkfjalskdfjalskdjf laksdjf laksdjflaksdjflaksdjf lkasjdfl aksdjf ");
            Tasks.Add("alskfdj alskdfj lkjalsdkfjalskdfjalskdjf laksdjf laksdjflaksdjflaksdjf lkasjdfl aksdjf ");
        }

        public ObservableCollection<string> Tasks { get; } = new();
    }
}
