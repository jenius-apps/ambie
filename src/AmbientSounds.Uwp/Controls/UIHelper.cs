using AmbientSounds.Animations;
using AmbientSounds.ViewModels;
using Windows.UI.Input;
using Windows.UI.Xaml;
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

        public static void GridScaleUp(object sender, PointerRoutedEventArgs e)
            => SoundItemAnimations.ItemScaleUp((UIElement)sender, 1.1f, e.Pointer);

        public static void GridScaleNormal(object sender, PointerRoutedEventArgs e)
            => SoundItemAnimations.ItemScaleNormal((UIElement)sender);
    }
}
