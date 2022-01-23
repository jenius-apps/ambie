using System;
using AmbientSounds.Shaders;
using ComputeSharp.Uwp;

#nullable enable

namespace AmbientSounds.Converters;

/// <summary>
/// Creates an <see cref="IShaderRunner"/> from a given pixel shader type.
/// </summary>
public static class PixelShaderToShaderRunnerConverter
{
    /// <summary>
    /// Creates an <see cref="IShaderRunner"/> instance from a specified type name.
    /// </summary>
    /// <param name="type">The shader type to use.</param>
    /// <returns>An <see cref="IShaderRunner"/> instance using the specified shader type.</returns>
    public static IShaderRunner? Convert(string? type)
    {
        // We need explicit references to all type to help the .NET Native linker resolve all type dependencies
        return type switch
        {
            nameof(ColorfulInfinity) => new ShaderRunner<ColorfulInfinity>(time => new ColorfulInfinity((float)time.TotalSeconds / 16f)),
            nameof(Octagrams) => new ShaderRunner<Octagrams>(time => new Octagrams((float)time.TotalSeconds / 16f)),
            nameof(ProteanClouds) => new ShaderRunner<ProteanClouds>(time => new ProteanClouds((float)time.TotalSeconds / 16f)),
            _ => null
        };
    }

    /// <summary>
    /// Creates an <see cref="IShaderRunner"/> instance from a specified type.
    /// </summary>
    /// <param name="type">The shader type to use.</param>
    /// <returns>An <see cref="IShaderRunner"/> instance using the specified shader type.</returns>
    public static IShaderRunner? Convert(Type? type)
    {
        return Convert(type?.Name);
    }
}
