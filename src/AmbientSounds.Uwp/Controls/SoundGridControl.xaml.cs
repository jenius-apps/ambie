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
        //private readonly ImplicitAnimationCollection _reorderAnimationCollection;

        public SoundGridControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundListViewModel>();
            this.Loaded += async (_, _) => { await ViewModel.LoadCommand.ExecuteAsync(null); };
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };

            //_reorderAnimationCollection = SoundItemAnimations.CreateReorderAnimationCollection(SoundsGridView);
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
                // TODO: disabling until the "fly in from top left" bug is fixed in IR
                //if (args.Element is UIElement element)
                //{
                //    element.GetVisual().ImplicitAnimations = _reorderAnimationCollection;
                //}

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
    }
}
