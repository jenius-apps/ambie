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

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class GuideDetailsControl : UserControl
{
    public event EventHandler? CloseRequested;

    public GuideDetailsControl(OnlineGuideViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
    }

    public OnlineGuideViewModel ViewModel { get; }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
