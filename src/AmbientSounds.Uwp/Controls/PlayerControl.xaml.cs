using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class PlayerControl : UserControl
    {
        public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(PlayerControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty PlayVisibleProperty = DependencyProperty.Register(
            nameof(PlayButtonVisible),
            typeof(bool),
            typeof(PlayerControl),
            new PropertyMetadata(Visibility.Visible));

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

        public Visibility PlayButtonVisible
        {
            get => (Visibility)GetValue(PlayVisibleProperty);
            set => SetValue(PlayVisibleProperty, value);
        }
    }
}
