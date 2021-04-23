using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Views
{
    /// <summary>
    /// The root frame used to power the backgrounds of the app.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ShellPageViewModel>();

            if (App.IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/turn-off-overscan
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
        }

        public ShellPageViewModel ViewModel => (ShellPageViewModel)this.DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var navigator = App.Services.GetRequiredService<INavigator>();
            navigator.Frame = MainFrame;

            UpdateBackground();
            ViewModel.PropertyChanged += OnVmPropChanged;
            MainFrame.Navigate(typeof(MainPage));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.PropertyChanged -= OnVmPropChanged;
        }

        private void OnVmPropChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShellPageViewModel.ShowBackgroundImage))
            {
                UpdateBackground();
            }
        }

        private void UpdateBackground()
        {
            if (!ViewModel.ShowBackgroundImage && ViewModel.TransparencyOn)
            {
                VisualStateManager.GoToState(this, nameof(TransparencyOnState), true);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(NormalState), true);
            }
        }
    }
}
