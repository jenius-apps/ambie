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

namespace AmbientSounds.Controls
{
    public sealed partial class FocusTaskModule : UserControl, ICanUninitialize
    {
        public FocusTaskModule()
        {
            this.InitializeComponent();
            DataContext = App.Services.GetRequiredService<FocusTaskModuleViewModel>();
        }

        public FocusTaskModuleViewModel ViewModel => (FocusTaskModuleViewModel)this.DataContext;

        public void Uninitialize()
        {
            ViewModel.Uninitialize();
        }

        private void OnTaskKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.AddTask();
            }
        }
    }
}
