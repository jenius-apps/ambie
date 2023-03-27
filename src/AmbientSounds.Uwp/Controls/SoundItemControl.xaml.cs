using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace AmbientSounds.Controls
{
    public sealed partial class SoundItemControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SoundViewModel),
                typeof(SoundItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(SoundItemControl),
                new PropertyMetadata(false));

        public SoundItemControl()
        {
            this.InitializeComponent();
        }

        public SoundViewModel ViewModel
        {
            get => (SoundViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        private void BitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is BitmapImage img)
            {
                img.UriSource = new Uri("http://localhost");
            }
        }

        private async void OnEditHomePageClicked(object sender, RoutedEventArgs e)
        {
            await App.Services.GetRequiredService<IDialogService>().OpenTutorialAsync();
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.ReorderClicked);
        }
    }
}
