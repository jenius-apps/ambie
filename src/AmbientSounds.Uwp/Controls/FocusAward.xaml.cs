using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusAward : UserControl
    {
        public static readonly DependencyProperty AwardProperty = DependencyProperty.Register(
            nameof(Award),
            typeof(HistoryAward),
            typeof(FocusAward),
            new PropertyMetadata(HistoryAward.None, OnAwardChanged));

        private static void OnAwardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FocusAward f)
            {
                f.Update();
            }
        }

        public FocusAward()
        {
            this.InitializeComponent();
        }

        public HistoryAward Award
        {
            get => (HistoryAward)GetValue(AwardProperty);
            set => SetValue(AwardProperty, value);
        }

        private string GlpyhCode { get; set; }

        private void Update()
        {
            GlpyhCode = Award switch
            {
                HistoryAward.Bronze => "\uEAEC",
                HistoryAward.Silver => "\uEB3B",
                HistoryAward.Gold => "\uEC04",
                _ => string.Empty
            };

            this.Bindings.Update();
        }
    }
}
