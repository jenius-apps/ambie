using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public enum DisplayMode
{
    Default,
    Compact,
    Xbox
}

public sealed partial class PlayerControl : UserControl
{
    public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
        nameof(DisplayMode),
        typeof(DisplayMode),
        typeof(PlayerControl),
        new PropertyMetadata(DisplayMode.Default));

    public static readonly DependencyProperty PlayVisibleProperty = DependencyProperty.Register(
        nameof(PlayButtonVisible),
        typeof(bool),
        typeof(PlayerControl),
        new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty VolumeVisibleProperty = DependencyProperty.Register(
        nameof(VolumeVisible),
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

    public DisplayMode DisplayMode
    {
        get => (DisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public Visibility PlayButtonVisible
    {
        get => (Visibility)GetValue(PlayVisibleProperty);
        set => SetValue(PlayVisibleProperty, value);
    }

    public Visibility VolumeVisible
    {
        get => (Visibility)GetValue(VolumeVisibleProperty);
        set => SetValue(VolumeVisibleProperty, value);
    }

    private bool IsCompact(DisplayMode mode) => mode is DisplayMode.Compact;

    private bool IsXbox(DisplayMode mode) => mode is DisplayMode.Xbox;

    private bool IsDefault(DisplayMode mode) => mode is DisplayMode.Default;
}
