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

        /// <summary>
        /// 
        /// </summary>
        public double ItemGridMaxWidth
        {
            get => (double)GetValue(ItemGridMaxWidthProperty);
            set => SetValue(ItemGridMaxWidthProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="ItemGridMaxWidth"/>.
        /// Default is true.
        /// </summary>
        public static readonly DependencyProperty ItemGridMaxWidthProperty = DependencyProperty.Register(
            nameof(ItemGridMaxWidth),
            typeof(double),
            typeof(SoundGridControl),
            new PropertyMetadata(double.MaxValue));

        /// <summary>
        /// If true, the catalogue button will be shown.
        /// </summary>
        public bool ShowCatalogueButton
        {
            get => (bool)GetValue(ShowCatalogueButtonProperty);
            set => SetValue(ShowCatalogueButtonProperty, value);
        }

        /// <summary>
        /// Dependency property for showing the catalogue button. Default false.
        /// </summary>
        public static readonly DependencyProperty ShowCatalogueButtonProperty = DependencyProperty.Register(
            nameof(ShowCatalogueButton),
            typeof(bool),
            typeof(SoundGridControl),
            new PropertyMetadata(false));

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var navigator = App.Services.GetRequiredService<INavigator>();

            // The frame check in the if statement
            // below is to prevent a crash in Compact
            // mode when the user clicks a sound.

            if (sender is ListViewBase l &&
                e.ClickedItem is SoundViewModel vm &&
                !vm.IsCurrentlyPlaying &&
                navigator.Frame is Frame f &&
                f.CurrentSourcePageType == typeof(Views.MainPage))
            {
                if (!vm.IsMix)
                {
                    l.PrepareConnectedAnimation(
                        AnimationConstants.TrackListItemLoad,
                        e.ClickedItem,
                        "RootGrid");
                }
                else
                {
                    l.PrepareConnectedAnimation(
                        AnimationConstants.TrackListItemLoad,
                        e.ClickedItem,
                        "RootGrid");

                    if (vm.HasSecondImage)
                    {
                        l.PrepareConnectedAnimation(
                        AnimationConstants.TrackListItem2Load,
                        e.ClickedItem,
                        "Image2");
                    }
                    else if (vm.HasThirdImage)
                    {
                        l.PrepareConnectedAnimation(
                            AnimationConstants.TrackListItem2Load,
                            e.ClickedItem,
                            "SecondImage");
                        l.PrepareConnectedAnimation(
                            AnimationConstants.TrackListItem3Load,
                            e.ClickedItem,
                            "ThirdImage");
                    }
                }
            }
        }
    }
}
