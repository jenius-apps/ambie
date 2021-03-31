using AmbientSounds.Animations;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class SoundGridControl : UserControl
    {
        public SoundGridControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SoundListViewModel>();
        }

        public SoundListViewModel ViewModel => (SoundListViewModel)this.DataContext;

        /// <summary>
        /// If true, the compact mode button is visible.
        /// Default is true.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
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

        private void GridScaleUp(object sender, PointerRoutedEventArgs e)
        {
            // Source for the scaling: https://github.com/windows-toolkit/WindowsCommunityToolkit/blob/master/Microsoft.Toolkit.Uwp.SampleApp/SamplePages/Implicit%20Animations/ImplicitAnimationsPage.xaml.cs
            // Search for "Scale Element".
            var element = sender as UIElement;
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Scale = new Vector3(1.1f, 1.1f, 1);
        }

        private void GridScaleNormal(object sender, PointerRoutedEventArgs e)
        {
            var element = sender as UIElement;
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Scale = new Vector3(1);
        }

        private Visibility Not(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (sender is ListViewBase l &&
                e.ClickedItem is SoundViewModel vm &&
                ! vm.IsCurrentlyPlaying &&
                App.AppFrame.CurrentSourcePageType == typeof(Views.MainPage))
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
