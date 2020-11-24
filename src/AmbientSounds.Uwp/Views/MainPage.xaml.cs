using AmbientSounds.Animations;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Grid control;

        public MainPage()
        {
            this.InitializeComponent();

            if (App.IsTenFoot)
            {
                this.GotFocus += MainPage_GotFocus;
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/turn-off-overscan
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
        }

        private void MainPage_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement focus = FocusManager.GetFocusedElement() as FrameworkElement;
            if (focus != null)
            {
                if (focus.GetType() == typeof(GridViewItem))
                {

                    control = ((focus.FindDescendantByName("TemplateRoot")) as Grid);
                    if (control != null)
                    {
                        try
                        {
                            SoundItemAnimations.ItemScaleUp(control, 1.1f);
                        }
                        catch
                        {

                        }
                    }
                }
                focus.LostFocus += (f, g) =>
                {
                    if (focus.GetType() == typeof(GridViewItem))
                    {
                        control = ((focus.FindDescendantByName("TemplateRoot")) as Grid);
                        if (control != null)
                        {

                            try
                            {
                                SoundItemAnimations.ItemScaleNormal(control);
                            }
                            catch
                            {

                            }
                        }
                    }
                };
            }
        }

        private void GridScaleUp(object sender, PointerRoutedEventArgs e) => SoundItemAnimations.ItemScaleUp(sender as UIElement, 1.1f);

        private void GridScaleNormal(object sender, PointerRoutedEventArgs e) => SoundItemAnimations.ItemScaleNormal(sender as UIElement);
    }
}
