using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private IList<string> _images;
        private int _img1;
        private int _img2;

        public void StartScreensaverTimer(IList<string> images)
        {
            _images = images;
            _img1 = 0;
            _img2 = 1;
        }

        private DispatcherTimer moveTimer;

        public Screensaver()
        {
            this.InitializeComponent();
            StartScreensaverTimer(new List<string>
            {
                "https://images.unsplash.com/photo-1585495898471-0fa227b7f193?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=2255&q=80",
                "https://images.unsplash.com/photo-1577899831505-233c0a599869?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=2252&q=80",
                "https://images.unsplash.com/photo-1605936995786-fa5749a143ea?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=2250&q=80"
             });
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
        }

        private void MoveTimer_Tick(object sender, object e)
        {
            if (image.Visibility == Visibility.Visible)
            {
                Debug.WriteLine("image two: " + _img2);
                image2.Source = new BitmapImage(new Uri(_images[_img2], UriKind.Absolute));
                image.Visibility = Visibility.Collapsed;
                image2.Visibility = Visibility.Visible;
                CycleImageIndex(ref _img1);
            }
            else 
            {
                Debug.WriteLine("image one: " + _img2);
                image.Source = new BitmapImage(new Uri(_images[_img1], UriKind.Absolute));
                image2.Visibility = Visibility.Collapsed;
                image.Visibility = Visibility.Visible;
                CycleImageIndex(ref _img2);
            }           
        }

        private void CycleImageIndex(ref int index)
        {
            index += 2;
            if (index == _images.Count)
            {
                index = 0;
            }
            else if (index > _images.Count)
            {
                index = 1;
            }
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
