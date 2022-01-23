using System;
using System.Reflection;
using AmbientSounds.Shaders;
using ComputeSharp;
using ComputeSharp.Uwp;
using Microsoft.Toolkit.Diagnostics;

#nullable enable

namespace AmbientSounds.Converters;

/// <summary>
/// Creates an <see cref="IShaderRunner"/> from a given pixel shader type.
/// </summary>
public static class PixelShaderToShaderRunnerConverter
{
    /// <summary>
    /// Creates an <see cref="IShaderRunner"/> instance from a specified type.
    /// </summary>
    /// <param name="type">The shader type to use.</param>
    /// <returns>An <see cref="IShaderRunner"/> instance using the specified shader type.</returns>
    public static IShaderRunner Convert(Type type)
    {
        Guard.IsNotNull(type, nameof(type));
        Guard.IsTrue(typeof(IPixelShader<float4>).IsAssignableFrom(type), nameof(type));

        static IShaderRunner Create<T>()
            where T : struct, IPixelShader<float4>
        {
            return new ShaderRunner<T>(time => (T)Activator.CreateInstance(typeof(T), (float)(time.TotalSeconds / 8.0)));
        }

        MethodInfo createMethod = new Func<IShaderRunner>(Create<ColorfulInfinity>).Method;
        MethodInfo createMethodOfType = createMethod.GetGenericMethodDefinition().MakeGenericMethod(type);

        return (IShaderRunner)createMethodOfType.Invoke(null, Array.Empty<object>());
    }
}
