using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class CatalogueListControl : UserControl
    {
        public CatalogueListControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<CatalogueListViewModel>();
        }

        public CatalogueListViewModel ViewModel => (CatalogueListViewModel)this.DataContext;
    }
}
