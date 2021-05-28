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
        public SoundGridControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundListViewModel>();
            this.Loaded += async (_, _) => { await ViewModel.LoadCommand.ExecuteAsync(null); };
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public SoundListViewModel ViewModel => (SoundListViewModel)this.DataContext;

        /// <summary>
        /// If true, the compact mode button is visible.
        /// Default is true.
        /// </summary>
        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="ItemTemplate"/>.
        /// Default is true.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(bool),
            typeof(SoundGridControl),
            null);
    }
}
