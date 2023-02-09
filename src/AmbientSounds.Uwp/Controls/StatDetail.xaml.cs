using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class StatDetail : UserControl
    {
        public static readonly DependencyProperty HalfLengthLineProperty = DependencyProperty.Register(
            nameof(HalfLengthLine),
            typeof(bool),
            typeof(StatDetail),
            new PropertyMetadata(false));

        public static readonly DependencyProperty StatTextProperty = DependencyProperty.Register(
            nameof(StatText),
            typeof(string),
            typeof(StatDetail),
            new PropertyMetadata(""));

        public static readonly DependencyProperty DescriptionTextProperty = DependencyProperty.Register(
            nameof(DescriptionText),
            typeof(string),
            typeof(StatDetail),
            new PropertyMetadata(""));

        public StatDetail()
        {
            this.InitializeComponent();
        }

        public bool HalfLengthLine
        {
            get => (bool)GetValue(HalfLengthLineProperty);
            set => SetValue(HalfLengthLineProperty, value);
        }

        public string StatText
        {
            get => (string)GetValue(StatTextProperty);
            set => SetValue(StatTextProperty, value);
        }

        public string DescriptionText
        {
            get => (string)GetValue(DescriptionTextProperty);
            set => SetValue(DescriptionTextProperty, value);
        }
    }
}
