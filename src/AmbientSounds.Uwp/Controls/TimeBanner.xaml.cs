using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class TimeBanner : UserControl
    {
        public event EventHandler<RoutedEventArgs>? Click;

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(TimeBanner),
                new PropertyMetadata(null));

        public TimeBanner()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<TimeBannerViewModel>();
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public TimeBannerViewModel ViewModel => (TimeBannerViewModel)this.DataContext;

        private void OnClicked(object sender, RoutedEventArgs e) => Click?.Invoke(sender, e);
    }
}
