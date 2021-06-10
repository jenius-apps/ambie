using AmbientSounds.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class SoundListItem : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(SoundViewModel),
            typeof(SoundItemControl),
            new PropertyMetadata(null));

        public SoundListItem()
        {
            this.InitializeComponent();
        }

        public SoundViewModel ViewModel
        {
            get => (SoundViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}
