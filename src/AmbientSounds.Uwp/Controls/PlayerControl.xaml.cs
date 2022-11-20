﻿using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class PlayerControl : UserControl
    {
        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(PlayerControl),
                new PropertyMetadata(false));

        public PlayerControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<PlayerViewModel>();
            this.Loaded += (_, _) => { ViewModel.Initialize(); };
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public PlayerViewModel ViewModel => (PlayerViewModel)this.DataContext;

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        private string GetDynamicIcon(double volume)
        {
            if (volume > 70)
            {
                return "\uEB7D";
            }
            else if (volume > 30)
            {
                return "\uEB7C";
            }
            else if (volume >= 1)
            {
                return "\uEB7B";
            }

            return "\uEB80";
        }
    }
}
