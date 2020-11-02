using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

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
            var element = sender as UIElement;
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Scale = new Vector3(1.3f, 1.3f, 1);
        }

        private void ScaleNormal(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as UIElement;
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Scale = new Vector3(1, 1, 1);
        }
    }
}
