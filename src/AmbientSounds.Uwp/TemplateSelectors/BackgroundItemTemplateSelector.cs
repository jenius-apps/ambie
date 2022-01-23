using System;
using ComputeSharp;
using Microsoft.Toolkit.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.TemplateSelectors;

/// <summary>
/// A template selector for backgrounds.
/// </summary>
public sealed class BackgroundItemTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets or sets the <see cref="DataTemplate"/> for images.
    public DataTemplate? ImageTemplate { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DataTemplate"/> for animated backgrounds.
    /// </summary>
    public DataTemplate? AnimatedBackgroundTemplate { get; set; }

    /// <inheritdoc/>
    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        Guard.IsNotNull(item, nameof(item));

        DataTemplate? template = item switch
        {
            string _ => ImageTemplate,
            Type type when typeof(IPixelShader<float4>).IsAssignableFrom(type) => AnimatedBackgroundTemplate,
            _ => ThrowHelper.ThrowArgumentException<DataTemplate>("Invalid input item type")
        };

        if (template is null)
        {
            ThrowHelper.ThrowInvalidOperationException("The requested template is null");
        }

        return template;
    }
}