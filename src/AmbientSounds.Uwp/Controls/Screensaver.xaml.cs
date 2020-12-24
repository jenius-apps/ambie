using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Controls
{
    // Ref: https://github.com/ms-iot/samples/blob/995f850e0ed6c1cd2bc87c8af39b7a4cf41fe425/IoTCoreDefaultApp/IoTCoreDefaultApp/App.xaml.cs
    public sealed partial class Screensaver : UserControl
    {
        private static DispatcherTimer timeoutTimer;
        private static Popup screensaverContainer;

        /// <summary>
        /// Initializes the screensaver
        /// </summary>
        public static void InitializeScreensaver()
        {
            screensaverContainer = new Popup()
            {
                Child = new Screensaver(),
                Margin = new Thickness(0),
                IsOpen = false
            };
            //Set screen saver to activate after 1 minute
            timeoutTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
            timeoutTimer.Tick += TimeoutTimer_Tick;
            Window.Current.Content.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(App_KeyDown), true);
            Window.Current.Content.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(App_PointerEvent), true);
            Window.Current.Content.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(App_PointerEvent), true);
            Window.Current.Content.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(App_PointerEvent), true);
            Window.Current.Content.AddHandler(UIElement.PointerEnteredEvent, new PointerEventHandler(App_PointerEvent), true);
            if (IsScreensaverEnabled)
            {
                timeoutTimer.Start();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the screen saver should listen for inactivity and 
        /// show after a little while. The default is <c>false</c>.
        /// </summary>
        public static bool IsScreensaverEnabled
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("EnableScreenSaver"))
                {
                    return (bool)ApplicationData.Current.LocalSettings.Values["EnableScreenSaver"];
                }
                return true;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["EnableScreenSaver"] = value;
                if (timeoutTimer != null)
                {
                    if (value)
                    {
                        timeoutTimer.Start();
                    }
                    else
                    {
                        timeoutTimer.Stop();
                    }
                }
            }
        }

        // Triggered when there hasn't been any key or pointer events in a while
        private static void TimeoutTimer_Tick(object sender, object e)
        {
            ShowScreensaver();
        }

        private static void ShowScreensaver()
        {
            timeoutTimer.Stop();
            var bounds = CoreWindow.GetForCurrentThread().Bounds;
            var view = (Screensaver)screensaverContainer.Child;
            view.Width = bounds.Width;
            view.Height = bounds.Height;
            screensaverContainer.IsOpen = true;
        }

        private static void App_KeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (IsScreensaverEnabled)
            {
                ResetScreensaverTimeout();
            }
        }

        private static void App_PointerEvent(object sender, PointerRoutedEventArgs e)
        {
            if (IsScreensaverEnabled)
            {
                ResetScreensaverTimeout();
            }
        }

        // Resets the timer and starts over.
        private static void ResetScreensaverTimeout()
        {
            if (timeoutTimer != null)
            {
                timeoutTimer.Stop();
                timeoutTimer.Start();
            }
            if (screensaverContainer != null)
            {
                screensaverContainer.IsOpen = false;
            }
        }

        private DispatcherTimer moveTimer;
        private Random randomizer = new Random();

        private Screensaver()
        {
            this.InitializeComponent();
            moveTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
            moveTimer.Tick += MoveTimer_Tick;

            this.Loaded += async (sender, e) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    ScreenSaver_Loaded(sender, e);
                });
            };
            this.Unloaded += ScreenSaver_Unloaded;
            image.Source = new BitmapImage(new Uri("https://images.unsplash.com/photo-1577899831505-233c0a599869?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=2252&q=80", UriKind.Absolute));
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            if (IsScreensaverEnabled)
            {
                ResetScreensaverTimeout();
            }
        }

        private void MoveTimer_Tick(object sender, object e)
        {
            //var left = randomizer.NextDouble() * (this.ActualWidth - image.ActualWidth);
            //var top = randomizer.NextDouble() * (this.ActualHeight - image.ActualHeight);
            //image.Margin = new Thickness(left, top, 0, 0);
        }

        private void ScreenSaver_Unloaded(object sender, RoutedEventArgs e)
        {
            moveTimer.Stop();
        }

        private void ScreenSaver_Loaded(object sender, RoutedEventArgs e)
        {
            moveTimer.Start();
            MoveTimer_Tick(moveTimer, null);
        }
    }
}
