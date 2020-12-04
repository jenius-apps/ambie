using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class ViewCatalogueButton : UserControl
    {
        public ViewCatalogueButton()
        {
            this.InitializeComponent();
        }

        private void NavigateToCatalogue()
        {
            App.AppFrame.Navigate(
                typeof(Views.CataloguePage), 
                null, 
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
