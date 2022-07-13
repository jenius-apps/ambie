using AmbientSounds.ViewModels;
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
    public sealed partial class FocusHistoryDetails : UserControl
    {
        public FocusHistoryDetails()
        {
            this.InitializeComponent();
        }

        public FocusHistoryDetails(FocusHistoryViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
        }

        public FocusHistoryViewModel ViewModel { get; }
    }
}
