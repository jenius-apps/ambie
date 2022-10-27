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
    public sealed partial class FreeBadge : UserControl
    {
        public FreeBadge()
        {
            this.InitializeComponent();
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Grid g)
            {
                g.ContextFlyout.ShowAt(g);
                e.Handled = true;
            }
        }
    }
}
