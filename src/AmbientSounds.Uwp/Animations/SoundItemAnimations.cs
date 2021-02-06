using System.Numerics;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace AmbientSounds.Animations
{
    /// <summary>
    /// Class with reusable animations for sound items.
    /// </summary>
    public static class SoundItemAnimations
    {
        /// <summary>
        /// Scales item up equally in x and y axes according to given scale.
        /// </summary>
        /// <param name="sender">A <see cref="UIElement"/>.</param>
        /// <param name="scale">A float of how much to scale the item up.</param>
        public static void ItemScaleUp(UIElement sender, float scale, Pointer p)
        {
            // Source for the scaling: https://github.com/windows-toolkit/WindowsCommunityToolkit/blob/master/Microsoft.Toolkit.Uwp.SampleApp/SamplePages/Implicit%20Animations/ImplicitAnimationsPage.xaml.cs
            // Search for "Scale Element".
            if (p.PointerDeviceType == PointerDeviceType.Mouse ||
                (p.PointerDeviceType == PointerDeviceType.Pen && p.IsInRange))
            {
                // Only allow animation for pen and mouse.
                // Disabled for touch because hover state doesn't
                // make sense for touch. Also looks bad.
                var visual = ElementCompositionPreview.GetElementVisual(sender);
                visual.Scale = new Vector3(scale, scale, 1);
            }
        }

        /// <summary>
        /// Scales the item back to normal with animations.
        /// </summary>
        /// <param name="sender">A <see cref="UIElement"/>.</param>
        public static void ItemScaleNormal(UIElement sender)
        {
            var visual = ElementCompositionPreview.GetElementVisual(sender);
            visual.Scale = new Vector3(1);
        }
    }
}
