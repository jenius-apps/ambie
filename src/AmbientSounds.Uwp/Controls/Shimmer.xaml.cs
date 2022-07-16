using System;
using Windows.UI.Xaml.Controls;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;

namespace AmbientSounds.Controls
{
    public sealed partial class Shimmer : UserControl
    {
        private Compositor _compositor;
        private PointLight _pointLight;

        public Shimmer()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _pointLight?.StopAnimation("Offset.X");
            _pointLight = null;
            _compositor = null;
        }

        private void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (this.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                return;
            }

            // Ref: https://github.com/microsoft/WindowsCompositionSamples/tree/master/SampleGallery/Samples/SDK%2014393/TextShimmer

            _compositor = ElementCompositionPreview.GetElementVisual(ShimmerControl).Compositor;
            var shimmerVisual = ElementCompositionPreview.GetElementVisual(ShimmerControl);
            _pointLight = _compositor.CreatePointLight();
            _pointLight.Color = Colors.White;
            _pointLight.CoordinateSpace = shimmerVisual;
            _pointLight.Targets.Add(shimmerVisual);
            _pointLight.Offset = new Vector3(-(float)ShimmerControl.ActualWidth, (float)ShimmerControl.ActualHeight / 2, (float)ShimmerControl.ActualHeight);
            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1, 2 * (float)ShimmerControl.ActualWidth);
            animation.Duration = TimeSpan.FromSeconds(1.2f);
            animation.IterationBehavior = AnimationIterationBehavior.Forever;
            _pointLight.StartAnimation("Offset.X", animation);
        }
    }
}
