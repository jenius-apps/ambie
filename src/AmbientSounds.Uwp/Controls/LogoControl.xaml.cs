using System.Numerics;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class LogoControl : UserControl
    {
        public LogoControl()
        {
            this.InitializeComponent();
        }

        private void ImageTapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void ScaleUp(object sender, PointerRoutedEventArgs e)
        {
            // Source for the scaling: https://github.com/windows-toolkit/WindowsCommunityToolkit/blob/master/Microsoft.Toolkit.Uwp.SampleApp/SamplePages/Implicit%20Animations/ImplicitAnimationsPage.xaml.cs
            // Search for "Scale Element".
            if (sender is UIElement element)
            {
                element.GetVisual().Scale = new Vector3(1.3f, 1.3f, 1);
            }
        }

        private void ScaleNormal(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.GetVisual().Scale = Vector3.One;
            }
        }
    }
}
