using AmbientSounds.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class SoundItemControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(SoundViewModel),
            typeof(SoundItemControl),
            new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SoundItemControl c)
            {
                c.Update();
            }
        }

        public SoundItemControl()
        {
            this.InitializeComponent();
        }

        public SoundViewModel ViewModel
        {
            get => (SoundViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        
        private void Update()
        {
            this.Bindings.Update();
        }

        private void OnGettingFocus(UIElement sender, Windows.UI.Xaml.Input.GettingFocusEventArgs args)
        {
            if (args.FocusState == FocusState.Keyboard || args.FocusState == FocusState.Programmatic)
            {
                VisualStateManager.GoToState(this, nameof(Focused), false);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(PointerFocused), false);
            }
        }

        private void OnLosingFocus(UIElement sender, Windows.UI.Xaml.Input.LosingFocusEventArgs args)
        {
            VisualStateManager.GoToState(this, nameof(Unfocused), false);
        }
    }
}
