using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class SoundGridControl : UserControl
    {
        public SoundGridControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundListViewModel>();
        }

        public SoundListViewModel ViewModel => (SoundListViewModel)this.DataContext;

        private void GridScaleUp(object sender, PointerRoutedEventArgs e)
        {
            // Source for the scaling: https://github.com/windows-toolkit/WindowsCommunityToolkit/blob/master/Microsoft.Toolkit.Uwp.SampleApp/SamplePages/Implicit%20Animations/ImplicitAnimationsPage.xaml.cs
            // Search for "Scale Element".
            var element = sender as UIElement;
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Scale = new Vector3(1.1f, 1.1f, 1);
        }

        private void GridScaleNormal(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as UIElement;
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Scale = new Vector3(1);
        }
    }
}
