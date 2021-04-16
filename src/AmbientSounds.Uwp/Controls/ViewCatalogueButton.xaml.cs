using AmbientSounds.Animations;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class ViewCatalogueButton : UserControl
    {
        public ViewCatalogueButton()
        {
            this.InitializeComponent();
        }

        public bool IconOnly
        {
            get => (bool)GetValue(IconOnlyProperty);
            set => SetValue(IconOnlyProperty, value);
        }

        public static DependencyProperty IconOnlyProperty = DependencyProperty.Register(
            nameof(IconOnly),
            typeof(bool),
            typeof(ViewCatalogueButton),
            new PropertyMetadata(false));

        private void IconNavigateToCatalogue()
        {
            ConnectedAnimationService
                .GetForCurrentView()
                .PrepareToAnimate(AnimationConstants.CatalogueForward, IconVersion);

            NavigateToCatalogue();
        }

        private void NavigateToCatalogue()
        {
            ITelemetry telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.MoreSoundsClicked);
            INavigator navigator = App.Services.GetRequiredService<INavigator>();
            navigator.ToCatalogue();
        }

        private bool Not(bool value) => !value;
    }
}
