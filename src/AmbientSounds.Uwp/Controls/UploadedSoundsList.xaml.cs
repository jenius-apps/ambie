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
    public sealed partial class UploadedSoundsList : UserControl
    {
        public UploadedSoundsList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<UploadedSoundsListViewModel>();
        }

        public UploadedSoundsListViewModel ViewModel => (UploadedSoundsListViewModel)this.DataContext;

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
