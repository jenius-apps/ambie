using AmbientSounds.Animations;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinUI = Microsoft.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class CatalogueListControl : UserControl
    {
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register(
            nameof(InnerMargin),
            typeof(Thickness),
            typeof(CatalogueListControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// The <see cref="ImplicitAnimationCollection"/> instance to animate items being reordered.
        /// </summary>
        //private readonly ImplicitAnimationCollection _reorderAnimationCollection;

        public CatalogueListControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<CatalogueListViewModel>();
            this.Loaded += async (_, _) => 
            {
                await ViewModel.InitializeAsync();
            };
            this.Unloaded += (_, _) =>
            {
                ViewModel.Dispose();
            };

            //_reorderAnimationCollection = SoundItemAnimations.CreateReorderAnimationCollection(SoundItemsRepeater);
        }

        public Thickness InnerMargin
        {
            get => (Thickness)GetValue(InnerMarginProperty);
            set => SetValue(InnerMarginProperty, value);
        }

        public CatalogueListViewModel ViewModel => (CatalogueListViewModel)this.DataContext;

        private void ItemsRepeater_ElementPrepared(
            WinUI.ItemsRepeater sender,
            WinUI.ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is Grid g && g.DataContext is CatalogueListViewModel vm)
            {
                // TODO: disabling until the "fly in from top left" bug is fixed in IR
                // Setup reorder animation
                //g.GetVisual().ImplicitAnimations = _reorderAnimationCollection;

                var dataContext = vm.Sounds[args.Index];
                var imageGrid = g.FindControl<Grid>("ImageGrid");
                if (imageGrid.Background is ImageBrush brush)
                {
                    brush.ImageSource = new BitmapImage
                    {
                        DecodePixelHeight = 208, // Same as ImageCardHeight in App.xaml
                        UriSource = new System.Uri(dataContext.ImagePath)
                    };
                }
            }
        }

        private void ItemsRepeater_ElementClearing(
            WinUI.ItemsRepeater sender,
            WinUI.ItemsRepeaterElementClearingEventArgs args)
        {
            if (args.Element is Grid g)
            {
                var imageGrid = g.FindControl<Grid>("ImageGrid");

                if (imageGrid.Background is ImageBrush brush)
                {
                    brush.ImageSource = null;
                }
            }
        }
    }
}
