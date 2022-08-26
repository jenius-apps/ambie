using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusTimerModuleViewModel : ObservableObject
    {
        public async Task InitializeAsync()
        {
            await Task.Delay(1);
        }

        public void Uninitialize()
        {

        }
    }
}
