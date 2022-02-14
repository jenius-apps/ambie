﻿using AmbientSounds.ViewModels;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace AmbientSounds.Controls
{
    public sealed partial class SoundItemControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(SoundViewModel),
            typeof(SoundItemControl),
            new PropertyMetadata(null));

        public SoundItemControl()
        {
            this.InitializeComponent();
        }

        public SoundViewModel ViewModel
        {
            get => (SoundViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void OnGettingFocus(UIElement sender, Windows.UI.Xaml.Input.GettingFocusEventArgs args)
        {
            if (args.FocusState == FocusState.Keyboard || args.FocusState == FocusState.Programmatic)
            {
                VisualStateManager.GoToState(this, nameof(Focused), false);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(PointerFocused), false);
            }
        }

        private void OnLosingFocus(UIElement sender, Windows.UI.Xaml.Input.LosingFocusEventArgs args)
        {
            VisualStateManager.GoToState(this, nameof(Unfocused), false);
        }

        private async void OnControlKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key is VirtualKey.Enter or VirtualKey.Space or VirtualKey.GamepadA)
            {
                e.Handled = true;
                await ViewModel.PlayCommand.ExecuteAsync(null);
            }
        }

        private void BitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is BitmapImage img)
            {
                img.UriSource = new Uri("http://localhost");
            }
        }
    }
}
