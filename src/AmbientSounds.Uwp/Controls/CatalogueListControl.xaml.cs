using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinUI = Microsoft.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class CatalogueListControl : UserControl
    {
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
        }

        public CatalogueListViewModel ViewModel => (CatalogueListViewModel)this.DataContext;

        private void ItemsRepeater_ElementPrepared(WinUI.ItemsRepeater sender, WinUI.ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is Grid g && g.DataContext is CatalogueListViewModel vm)
            {
                var dataContext = vm.Sounds[args.Index];
                var imageGrid = g.FindControl<Grid>("ImageGrid");
                if (imageGrid.Background is ImageBrush brush)
                {
                    brush.ImageSource = new BitmapImage
                    {
                        DecodePixelHeight = 240,
                        UriSource = new System.Uri(dataContext.ImagePath)
                    };
                }
            }
        }

        private void ItemsRepeater_ElementClearing(WinUI.ItemsRepeater sender, WinUI.ItemsRepeaterElementClearingEventArgs args)
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
