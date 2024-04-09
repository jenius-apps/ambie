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

public sealed partial class Slideshow : UserControl
{
    public Slideshow()
    {
        this.InitializeComponent();
        this.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width >= e.NewSize.Height)
        {
            Image1.Width = e.NewSize.Width * 1.3;
            Image2.Width = e.NewSize.Width * 1.3;
            Image1.Height = double.NaN;
            Image2.Height = double.NaN;
        }
        else
        {
            Image1.Height = e.NewSize.Height * 1.3;
            Image2.Height = e.NewSize.Height * 1.3;
            Image1.Width = double.NaN;
            Image2.Width = double.NaN;
        }
    }
}
