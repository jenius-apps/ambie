using System;
using ComputeSharp.D2D1;
using ComputeSharp.D2D1.Uwp;

#nullable enable

namespace AmbientSounds.Effects;

/// <summary>
/// An base effect for an animated wallpaper.
/// </summary>
public abstract class AnimatedWallpaperEffect : CanvasEffect
{
    /// <summary>
    /// The current screen width in raw pixels.
    /// </summary>
    private int _screenWidth;

    /// <summary>
    /// The current screen height in raw pixels.
    /// </summary>
    private int _screenHeight;

    /// <summary>
    /// The current elapsed time.
    /// </summary>
    private TimeSpan _elapsedTime;

    /// <summary>
    /// Gets or sets the screen width in raw pixels.
    /// </summary>
    public int ScreenWidth
    {
        get => _screenWidth;
        set => SetAndInvalidateEffectGraph(ref _screenWidth, value);
    }

    /// <summary>
    /// Gets or sets the screen height in raw pixels.
    /// </summary>
    public int ScreenHeight
    {
        get => _screenHeight;
        set => SetAndInvalidateEffectGraph(ref _screenHeight, value);
    }

    /// <summary>
    /// Gets or sets the total elapsed time.
    /// </summary>
    public TimeSpan ElapsedTime
    {
        get => _elapsedTime;
        set => SetAndInvalidateEffectGraph(ref _elapsedTime, value);
    }

    /// <summary>
    /// An effect for an animated wallpaper.
    /// </summary>
    /// <typeparam name="T">The type of wallpepr to render.</typeparam>
    public sealed class For<T> : AnimatedWallpaperEffect
        where T : unmanaged, ID2D1PixelShader
    {
        /// <summary>
        /// The <see cref="PixelShaderEffect{T}"/> node instance in use.
        /// </summary>
        private static readonly EffectNode<PixelShaderEffect<T>> EffectNode = new();

        /// <summary>
        /// The <typeparamref name="T"/> factory to use.
        /// </summary>
        private readonly Factory _factory;

        /// <summary>
        /// Creates a new <see cref="For{T}"/> instance with the specified parameters.
        /// </summary>
        /// <param name="factory">The input <typeparamref name="T"/> factory.</param>
        public For(Factory factory)
        {
            _factory = factory;
        }

        /// <inheritdoc/>
        protected override void BuildEffectGraph(EffectGraph effectGraph)
        {
            effectGraph.RegisterOutputNode(EffectNode, new PixelShaderEffect<T>());
        }

        /// <inheritdoc/>
        protected override void ConfigureEffectGraph(EffectGraph effectGraph)
        {
            effectGraph.GetNode(EffectNode).ConstantBuffer = _factory(ScreenWidth, ScreenHeight, ElapsedTime);
        }

        /// <summary>
        /// A factory of a given shader instance.
        /// </summary>
        /// <param name="screenWidth">The screen width in raw pixels.</param>
        /// <param name="screenHeight">The screen height in raw pixels.</param>
        /// <param name="elapsedTime">The total elapsed time.</param>
        /// <returns>A shader instance to render.</returns>
        public delegate T Factory(int screenWidth, int screenHeight, TimeSpan elapsedTime);
    }
}
