using AmbientSounds.Services;
using Autofac;
using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static IContainer Container { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            Container = ContainerService.Build();
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Do not repeat app initialization when the Window already has content
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (!e.PrelaunchActivated)
            {
                CoreApplication.EnablePrelaunch(true);

                // Navigate to the root page if one isn't loaded already
                if (rootFrame.Content is null)
                {
                    rootFrame.Navigate(typeof(Views.MainPage), e.Arguments);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }

            CustomizeTitleBar(rootFrame.ActualTheme == ElementTheme.Dark);
        }

        /// <summary>
        /// Invoked when navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load Page {e.SourcePageType.FullName}.");
        }

        /// <summary>
        /// Removes title bar and sets title bar button backgrounds to transparent.
        /// </summary>
        private void CustomizeTitleBar(bool darkTheme)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = darkTheme ? Colors.LightGray : Colors.Black;
        }
    }
}
