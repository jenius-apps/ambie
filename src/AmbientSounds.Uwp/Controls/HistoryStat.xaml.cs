using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class HistoryStat : UserControl
    {
        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
            nameof(Glyph),
            typeof(string),
            typeof(HistoryStat),
            null);

        public static readonly DependencyProperty StatTextProperty = DependencyProperty.Register(
            nameof(StatText),
            typeof(string),
            typeof(HistoryStat),
            new PropertyMetadata(""));

        public static readonly DependencyProperty DescriptionTextProperty = DependencyProperty.Register(
            nameof(DescriptionText),
            typeof(string),
            typeof(HistoryStat),
            new PropertyMetadata(""));

        public HistoryStat()
        {
            this.InitializeComponent();
        }

        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
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
