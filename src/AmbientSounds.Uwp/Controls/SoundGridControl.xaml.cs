using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SoundGridControl : UserControl
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(SoundGridControl),
            null);

        public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(SoundGridControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsXboxProperty = DependencyProperty.Register(
            nameof(IsXbox),
            typeof(bool),
            typeof(SoundGridControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty CanScrollOutOfBoundsProperty = DependencyProperty.Register(
            nameof(CanScrollOutOfBounds),
            typeof(bool),
            typeof(SoundGridControl),
            new PropertyMetadata(false));

        public SoundGridControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundListViewModel>();
            this.Loaded += async (_, _) => { await ViewModel.LoadCommand.ExecuteAsync(null); };
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public SoundListViewModel ViewModel => (SoundListViewModel)this.DataContext;

        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        public bool IsXbox
        {
            get => (bool)GetValue(IsXboxProperty);
            set => SetValue(IsXboxProperty, value);
        }

        public bool CanScrollOutOfBounds
        {
            get => (bool)GetValue(CanScrollOutOfBoundsProperty);
            set => SetValue(CanScrollOutOfBoundsProperty, value);
        }

        private async void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is SoundViewModel vm)
            {
                await vm.PlayCommand.ExecuteAsync(null);
            }
        }

        private void OnGridViewLoaded(object sender, RoutedEventArgs e)
        {
            if (CanScrollOutOfBounds && sender is GridView gridView)
            {
                ScrollViewer? s = gridView.FindDescendant<ScrollViewer>();
                if (s is not null)
                {
                    s.CanContentRenderOutsideBounds = true;
                }
            }
        }
    }
}
