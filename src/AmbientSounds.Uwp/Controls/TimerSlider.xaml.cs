using AmbientSounds.Converters;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace AmbientSounds.Controls
{
    public sealed partial class TimerSlider : ObservableUserControl
    {
        public string AutomationName
        {
            get => (string)GetValue(AutomationNameProperty);
            set => SetValueDp(AutomationNameProperty, value);
        }

        public static readonly DependencyProperty AutomationNameProperty = DependencyProperty.Register(
            nameof(AutomationName),
            typeof(string),
            typeof(TimerSlider),
            new PropertyMetadata(string.Empty));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValueDp(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(TimerSlider),
            new PropertyMetadata(string.Empty));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValueDp(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(TimerSlider),
            new PropertyMetadata(0d, OnValueChanged));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(TimerSlider),
            new PropertyMetadata(1d));

        public double StepFrequency
        {
            get => (double)GetValue(StepFrequencyProperty);
            set => SetValue(StepFrequencyProperty, value);
        }

        public static readonly DependencyProperty StepFrequencyProperty = DependencyProperty.Register(
            nameof(StepFrequency),
            typeof(double),
            typeof(TimerSlider),
            new PropertyMetadata(1d));

        public double TickFrequency
        {
            get => (double)GetValue(TickFrequencyProperty);
            set => SetValue(TickFrequencyProperty, value);
        }

        public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
            nameof(TickFrequency),
            typeof(double),
            typeof(TimerSlider),
            new PropertyMetadata(0d));

        private static async void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimerSlider s && e.OldValue is double oldVal && e.NewValue is double newVal)
            {
                await s.AnimateTextAsync(oldVal, newVal);
            }
        }

        public TimerSlider()
        {
            this.InitializeComponent();
        }

        private async Task AnimateTextAsync(double oldValue, double newValue)
        {
            if (newValue > oldValue)
            {
                await IncreaseAnimation.StartAsync();
            }
            else
            {
                await DecreaseAnimation.StartAsync();
            }
        }
    }
}
