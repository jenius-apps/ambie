using AmbientSounds.Animations;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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

        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
            nameof(Layout),
            typeof(Microsoft.UI.Xaml.Controls.Layout),
            typeof(SoundGridControl),
            null);

        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register(
            nameof(InnerMargin),
            typeof(Thickness),
            typeof(SoundGridControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        public SoundGridControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundListViewModel>();
            this.Loaded += async (_, _) => { await ViewModel.LoadCommand.ExecuteAsync(null); };
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public SoundListViewModel ViewModel => (SoundListViewModel)this.DataContext;

        public Microsoft.UI.Xaml.Controls.Layout? Layout
        {
            get => (Microsoft.UI.Xaml.Controls.Layout?)GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        /// <summary>
        /// If true, the compact mode button is visible.
        /// Default is true.
        /// </summary>
        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public Thickness InnerMargin
        {
            get => (Thickness)GetValue(InnerMarginProperty);
            set => SetValue(InnerMarginProperty, value);
        }
    }
}
