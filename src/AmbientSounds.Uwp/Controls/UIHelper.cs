using AmbientSounds.Animations;
using AmbientSounds.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

#nullable enable

namespace AmbientSounds.Controls;

public static class UIHelper
{
    public static double PreviewColumnSpan(bool isPreviewVisible)
    {
        return isPreviewVisible ? 4d : 0d;
    }

    public static GridLength PreviewButtonColumnWidth(bool isPreviewVisible)
    {
        return isPreviewVisible
            ? new GridLength(1, GridUnitType.Star)
            : new GridLength(0, GridUnitType.Pixel);
    }

    public static Visibility InvertVisibility(bool isVisible)
    {
        return isVisible ? Visibility.Collapsed : Visibility.Visible;
    }

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

    public static Color ToLighterColour(this string colourHex, double percentLighter = 0.2)
    {
        var colour = colourHex.ToColour();

        return Color.FromArgb(
          colour.A,
          (byte)Math.Min(255, colour.R + 255 * percentLighter),
          (byte)Math.Min(255, colour.G + 255 * percentLighter),
          (byte)Math.Min(255, colour.B + 255 * percentLighter));
    }

    public static Color ToDarkerColour(this string colourHex, double percentDarker = 0.2)
    {
        var colour = colourHex.ToColour();

        return Color.FromArgb(
          colour.A,
          (byte)Math.Max(0, colour.R - 255 * percentDarker),
          (byte)Math.Max(0, colour.G - 255 * percentDarker),
          (byte)Math.Max(0, colour.B - 255 * percentDarker));
    }

    public static Color ToColour(this string colourHex)
    {
        if (string.IsNullOrEmpty(colourHex))
        {
            colourHex = "#1F1F1F";
        }
        
        return colourHex.ToColor();
    }

    public static SolidColorBrush ToBrush(string colourHex)
    {
        return new SolidColorBrush(colourHex.ToColour());
    }

    public static SolidColorBrush ToLighterBrush(string colourHex, double percentLighter)
    {
        return new SolidColorBrush(colourHex.ToLighterColour(percentLighter));
    }

    public static SolidColorBrush ToDarkerBrush(string colourHex, double percentDarker)
    {
        return new SolidColorBrush(colourHex.ToDarkerColour(percentDarker));
    }

    public static Color ToTransparent(string colourHex)
    {
        if (string.IsNullOrEmpty(colourHex))
        {
            colourHex = "#000000";
        }

        return colourHex.Replace("#", "#00").ToColor();
    }

    public static void GridScaleUp(object? sender, PointerRoutedEventArgs e)
    {
        if (sender is not null)
        {
            SoundItemAnimations.ItemScaleUp((UIElement)sender, 1.1f, e.Pointer);
        }
    }

    public static void GridScaleNormal(object? sender, PointerRoutedEventArgs e)
    {
        if (sender is not null)
        {
            SoundItemAnimations.ItemScaleNormal((UIElement)sender);
        }
    }

    public static void ScaleUpChildImage(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement parent)
        {
            var element = parent.FindControl<Grid>("ImageGrid");
            GridScaleUp(element, e);
        }
    }

    public static void ScaleNormalChildImage(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement parent)
        {
            var element = parent.FindControl<Grid>("ImageGrid");
            GridScaleNormal(element, e);
        }
    }

    public static T? FindControl<T>(this UIElement parent, string ControlName) where T : FrameworkElement
    {
        // Source: https://stackoverflow.com/a/58091583/10953422

        if (parent == null)
            return null;

        if (parent.GetType() == typeof(T) && ((T)parent).Name == ControlName)
        {
            return (T)parent;
        }
        T? result = null;
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

            if (FindControl<T>(child, ControlName) != null)
            {
                result = FindControl<T>(child, ControlName);
                break;
            }
        }
        return result;
    }
}
