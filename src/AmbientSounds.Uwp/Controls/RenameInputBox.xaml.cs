using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class RenameInputBox : UserControl
    {
        public RenameInputBox()
        {
            this.InitializeComponent();
        }

        public string? Input { get; set; }

        public event EventHandler? EnterClicked;

        private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is TextBox t && e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                EnterClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        private void InputBox_Loaded(object sender, RoutedEventArgs e)
        {
            InputBox.SelectAll();
        }
    }
}
