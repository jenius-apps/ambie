using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class RenameInputBox : UserControl
    {
        public RenameInputBox()
        {
            this.InitializeComponent();
        }

        public string Input { get; set; }

        public event EventHandler EnterClicked;

        private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is TextBox t)
            {
                if (e.Key == VirtualKey.Enter)
                {
                    e.Handled = true;
                    EnterClicked?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void InputBox_Loaded(object sender, RoutedEventArgs e)
        {
            InputBox.SelectAll();
        }
    }
}
