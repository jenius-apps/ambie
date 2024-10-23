using AmbientSounds.Controls;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

#nullable enable

namespace AmbientSounds.Animations;

internal static class ChannelItemAnimations
{
    private static readonly ConcurrentDictionary<int, CancellationTokenSource> _cts = new();

    public static async void ScaleUpChannelIcon(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement parent &&
            parent.FindControl<Border>("PrimaryIconBorder") is Border border)
        {
            if (_cts.TryGetValue(border.GetHashCode(), out var oldCts))
            {
                oldCts.Cancel();
            }

            var newCts = new CancellationTokenSource();
            _cts[border.GetHashCode()] = newCts;

            var visual = ElementCompositionPreview.GetElementVisual(border);
            border.Visibility = Visibility.Visible;
            visual.Opacity = 0;
            var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, 1f);
            animation.Duration = TimeSpan.FromSeconds(0.6);
            visual.StartAnimation("Opacity", animation);

            try
            {
                await Task.Delay(600, newCts.Token);
            }
            catch (OperationCanceledException)
            {
                visual.StopAnimation("Opacity");
            }
        }
    }

    public static async void ScaleDownChannelIcon(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement parent &&
            parent.FindControl<Border>("PrimaryIconBorder") is Border border)
        {
            if (_cts.TryGetValue(border.GetHashCode(), out var oldCts))
            {
                oldCts.Cancel();
            }

            var newCts = new CancellationTokenSource();
            _cts[border.GetHashCode()] = newCts;

            var visual = ElementCompositionPreview.GetElementVisual(border);
            border.Visibility = Visibility.Visible;
            visual.Opacity = 1;
            var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, 0f);
            animation.Duration = TimeSpan.FromSeconds(0.3);
            visual.StartAnimation("Opacity", animation);
            try
            {
                await Task.Delay(300, newCts.Token);
            }
            catch (OperationCanceledException)
            {
                visual.StopAnimation("Opacity");
            }
            border.Visibility = Visibility.Collapsed;
        }
    }

    public static void ScaleUpImageRectangle(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement parent &&
            parent.FindControl<Rectangle>("ImageRect") is Rectangle element)
        {
            SoundItemAnimations.ItemScaleUp(element, 1.1f, e.Pointer);
        }
    }

    public static void ScaleUpDownImageRectangle(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement parent &&
            parent.FindControl<Rectangle>("ImageRect") is Rectangle element)
        {
            SoundItemAnimations.ItemScaleNormal(element);
        }
    }

    public static void OnHoverChannelImage(object sender, PointerRoutedEventArgs e)
    {
        ScaleUpImageRectangle(sender, e);
        ScaleUpChannelIcon(sender, e);
    }

    public static void OnRestChannelImage(object sender, PointerRoutedEventArgs e)
    {
        ScaleUpDownImageRectangle(sender, e);
        ScaleDownChannelIcon(sender, e);
    }
}
