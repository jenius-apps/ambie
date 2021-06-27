using System;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using MUXC = Microsoft.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SoundGridControl : UserControl
    {
        /// <summary>
        /// The duration for the items reorder animations, in milliseconds.
        /// </summary>
        private const int ReorderAnimationDuration = 250;

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(SoundGridControl),
            null);

        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
            nameof(Layout),
            typeof(MUXC.Layout),
            typeof(SoundGridControl),
            null);

        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register(
            nameof(InnerMargin),
            typeof(Thickness),
            typeof(SoundGridControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// The <see cref="ImplicitAnimationCollection"/> instance to animate items being reordered.
        /// </summary>
        private readonly ImplicitAnimationCollection _reorderAnimationCollection;

        public SoundGridControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundListViewModel>();
            this.Loaded += async (_, _) => { await ViewModel.LoadCommand.ExecuteAsync(null); };
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };

            _reorderAnimationCollection = CreateReorderAnimationCollection(SoundsGridView);
        }

        public SoundListViewModel ViewModel => (SoundListViewModel)this.DataContext;

        public MUXC.Layout? Layout
        {
            get => (MUXC.Layout?)GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        /// <summary>
        /// If true, the compact mode button is visible.
        /// Default is true.
        /// </summary>
        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public Thickness InnerMargin
        {
            get => (Thickness)GetValue(InnerMarginProperty);
            set => SetValue(InnerMarginProperty, value);
        }

        private void OnElementPrepared(
            MUXC.ItemsRepeater sender,
            MUXC.ItemsRepeaterElementPreparedEventArgs args)
        {
            if (sender.DataContext is SoundListViewModel listVm)
            {
                if (args.Element is UIElement element)
                {
                    element.GetVisual().ImplicitAnimations = _reorderAnimationCollection;
                }

                if (args.Element is SoundItemControl c)
                {
                    c.ViewModel = listVm.Sounds[args.Index];
                }
                else if (args.Element is SoundListItem l)
                {
                    l.ViewModel = listVm.Sounds[args.Index];
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="ImplicitAnimationCollection"/> instance to animate the visual of items within a <see cref="MUXC.ItemsRepeater"/> control.
        /// </summary>
        /// <param name="itemsRepeater">The input <see cref="MUXC.ItemsRepeater"/> to create the animation for.</param>
        /// <returns>A new <see cref="ImplicitAnimationCollection"/> instance to animate items within <paramref name="itemsRepeater"/>.</returns>
        private static ImplicitAnimationCollection CreateReorderAnimationCollection(MUXC.ItemsRepeater itemsRepeater)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(itemsRepeater).Compositor;
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();

            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(ReorderAnimationDuration);
            offsetAnimation.Target = nameof(Visual.Offset);

            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();

            animationGroup.Add(offsetAnimation);

            ImplicitAnimationCollection animationCollection = compositor.CreateImplicitAnimationCollection();

            animationCollection[nameof(Visual.Offset)] = animationGroup;

            return animationCollection;
        }
    }
}
