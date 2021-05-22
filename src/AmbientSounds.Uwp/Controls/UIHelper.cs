using AmbientSounds.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace AmbientSounds.Controls
{
    public static class UIHelper
    {
        public static void RemoveOnMiddleClick(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Image fe && fe.DataContext is ActiveTrackViewModel vm)
            {
                var pointer = e.GetCurrentPoint(fe);
                if (pointer.Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed)
                {
                    vm.RemoveCommand.Execute(vm.Sound);
                    e.Handled = true;
                }
            }
        }
    }
}
