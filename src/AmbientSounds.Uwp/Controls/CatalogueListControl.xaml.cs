using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class CatalogueListControl : UserControl
    {
        public CatalogueListControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<CatalogueListViewModel>();
            this.Unloaded += (_, _) =>
            {
                ViewModel.Dispose();
            };
        }

        public CatalogueListViewModel ViewModel => (CatalogueListViewModel)this.DataContext;
    }
}
