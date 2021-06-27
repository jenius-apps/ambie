using AmbientSounds.Animations;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SoundGridControl : UserControl
    {
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

            _reorderAnimationCollection = SoundItemAnimations.CreateReorderAnimationCollection(SoundsGridView);
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
                    Visual visual = ElementCompositionPreview.GetElementVisual(element);

                    visual.ImplicitAnimations = _reorderAnimationCollection;
                    visual.Opacity = 1;
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

        private void SoundsGridView_ElementClearing(
            MUXC.ItemsRepeater sender,
            MUXC.ItemsRepeaterElementClearingEventArgs args)
        {
            if (args.Element is UIElement element)
            {
                Visual visual = ElementCompositionPreview.GetElementVisual(element);

                visual.ImplicitAnimations = null;
                visual.Opacity = 0;
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

            CompositionAnimationGroup offsetAnimationGroup = compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();

            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(ReorderAnimationDuration);
            offsetAnimation.Target = nameof(Visual.Offset);

            offsetAnimationGroup.Add(offsetAnimation);

            CompositionAnimationGroup opacityAnimationGroup = compositor.CreateAnimationGroup();
            ScalarKeyFrameAnimation opacityAnimation = compositor.CreateScalarKeyFrameAnimation();

            // Note: this is a temporary workaround to hide the initial incorrect offset animation caused by the
            // ItemsRepeater control seemingly move items around from an undefined starting point when they're
            // added back to the visual tree. This animation snaps the opacity to the final value (set from 0 to
            // 1 when items are loaded) right before the final keyframe, which results in items remaining invisible
            // for the entire duration of the animation. Since the length is the same as that of the visual animation,
            // and then two values are set at roughly the same time, this effectively hides away the other animation.
            opacityAnimation.InsertExpressionKeyFrame(0.999f, "this.StartingValue");
            opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(ReorderAnimationDuration);
            opacityAnimation.Target = nameof(Visual.Opacity);

            opacityAnimationGroup.Add(opacityAnimation);

            ImplicitAnimationCollection animationCollection = compositor.CreateImplicitAnimationCollection();

            animationCollection[nameof(Visual.Offset)] = offsetAnimationGroup;
            animationCollection[nameof(Visual.Opacity)] = opacityAnimationGroup;

            return animationCollection;
        }
    }
}
