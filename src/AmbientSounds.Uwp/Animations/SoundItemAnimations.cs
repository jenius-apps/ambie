using System;
using System.Numerics;
using Windows.Devices.Input;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using MUXC = Microsoft.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Animations
{
    /// <summary>
    /// Class with reusable animations for sound items.
    /// </summary>
    public static class SoundItemAnimations
    {
        /// <summary>
        /// The duration for the items reorder animations, in milliseconds.
        /// </summary>
        private const int ReorderAnimationDuration = 250;

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

        /// <summary>
        /// Creates a new <see cref="ImplicitAnimationCollection"/> instance to animate the visual of items within a <see cref="MUXC.ItemsRepeater"/> control.
        /// </summary>
        /// <param name="itemsRepeater">The input <see cref="MUXC.ItemsRepeater"/> to create the animation for.</param>
        /// <returns>A new <see cref="ImplicitAnimationCollection"/> instance to animate items within <paramref name="itemsRepeater"/>.</returns>
        public static ImplicitAnimationCollection CreateReorderAnimationCollection(MUXC.ItemsRepeater itemsRepeater)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(itemsRepeater).Compositor;
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();

            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(ReorderAnimationDuration);
            offsetAnimation.Target = nameof(Visual.Offset);

            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();

            animationGroup.Add(offsetAnimation);

            ImplicitAnimationCollection animationCollection = compositor.CreateImplicitAnimationCollection();

            animationCollection[nameof(Visual.Offset)] = animationGroup;

            return animationCollection;
        }
    }
}
