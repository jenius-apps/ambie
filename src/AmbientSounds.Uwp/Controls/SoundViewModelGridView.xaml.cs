using AmbientSounds.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

/// <summary>
/// Base level control that displays sound view models on screen in a grid.
/// </summary>
public sealed partial class SoundViewModelGridView : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(object),
        typeof(SoundViewModelGridView),
        new PropertyMetadata(null));

    public SoundViewModelGridView()
    {
        this.InitializeComponent();
    }

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private async void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SoundViewModel vm)
        {
            await vm.PlayCommand.ExecuteAsync(null);
        }
    }
}
