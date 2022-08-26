using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusTimerModule : UserControl, ICanInitialize
    {
        public FocusTimerModule()
        {
            this.InitializeComponent();
            DataContext = App.Services.GetRequiredService<FocusTimerModuleViewModel>();
        }

        public FocusTimerModuleViewModel ViewModel => (FocusTimerModuleViewModel)this.DataContext;

        public async Task InitializeAsync()
        {
            await ViewModel.InitializeAsync();
        }

        public void Uninitialize() => ViewModel.Uninitialize();
    }
}
