using AmbientSounds.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AmbientSounds.Xamarin
{
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = DependencyService.Resolve<ShellPageViewModel>();
        }

        public ShellPageViewModel ViewModel => (ShellPageViewModel)BindingContext;
    }
}
