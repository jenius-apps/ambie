using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using Windows.UI;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusHistoryModule : UserControl
    {
        public FocusHistoryModule()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<FocusHistoryModuleViewModel>();
        }

        public FocusHistoryModuleViewModel ViewModel => (FocusHistoryModuleViewModel)this.DataContext;

        private async void Test(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

                Frame frame = new Frame();
                frame.Navigate(typeof(Views.AllHistoryPage), null);
                Window.Current.Content = frame;
                // You have to activate the window in order to show it later.
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;

                var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
                viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
                viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                viewTitleBar.ButtonForegroundColor = frame.ActualTheme == ElementTheme.Dark ? Colors.LightGray : Colors.Black;
            });

            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }
    }
}
