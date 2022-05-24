using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusTimer : UserControl
    {
        private const double RingSizeRatio = 0.83;

        public static readonly DependencyProperty SecondsRingVisibleProperty = DependencyProperty.Register(
            nameof(SecondsRingVisible),
            typeof(Visibility),
            typeof(FocusTimer),
            new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty SecondsRemainingProperty = DependencyProperty.Register(
            nameof(SecondsRemaining),
            typeof(int),
            typeof(FocusTimer),
            new PropertyMetadata(0));

        public static readonly DependencyProperty FocusLengthRemainingProperty = DependencyProperty.Register(
            nameof(FocusLengthRemaining),
            typeof(int),
            typeof(FocusTimer),
            new PropertyMetadata(0));

        public static readonly DependencyProperty RestLengthRemainingProperty = DependencyProperty.Register(
            nameof(RestLengthRemaining),
            typeof(int),
            typeof(FocusTimer),
            new PropertyMetadata(0));

        public static readonly DependencyProperty RepetitionsRemainingProperty = DependencyProperty.Register(
            nameof(RepetitionsRemaining),
            typeof(int),
            typeof(FocusTimer),
            new PropertyMetadata(0));

        public FocusTimer()
        {
            this.InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        public Visibility SecondsRingVisible
        {
            get => (Visibility)GetValue(SecondsRingVisibleProperty);
            set => SetValue(SecondsRingVisibleProperty, value);
        }

        public int SecondsRemaining
        {
            get => (int)GetValue(SecondsRemainingProperty);
            set => SetValue(SecondsRemainingProperty, value);
        }

        public int FocusLengthRemaining
        {
            get => (int)GetValue(FocusLengthRemainingProperty);
            set => SetValue(FocusLengthRemainingProperty, value);
        }

        public int RestLengthRemaining
        {
            get => (int)GetValue(RestLengthRemainingProperty);
            set => SetValue(RestLengthRemainingProperty, value);
        }

        public int RepetitionsRemaining
        {
            get => (int)GetValue(RepetitionsRemainingProperty);
            set => SetValue(RepetitionsRemainingProperty, value);
        }

        private double Ring1Height => this.ActualHeight;
        private double Ring1Width => this.ActualWidth;

        private double Ring2Height => Ring1Height * RingSizeRatio;
        private double Ring2Width => Ring1Width * RingSizeRatio;

        private double Ring3Height => Ring2Height * RingSizeRatio;
        private double Ring3Width => Ring2Width * RingSizeRatio;

        private double Ring4Height => Ring3Height * RingSizeRatio;
        private double Ring4Width => Ring3Width * RingSizeRatio;

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Bindings.Update();
        }
    }
}
