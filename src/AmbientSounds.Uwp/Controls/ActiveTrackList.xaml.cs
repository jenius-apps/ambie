using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class ActiveTrackList : UserControl
    {
        public ActiveTrackList()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ActiveTrackListViewModel>();
        }

        public ActiveTrackListViewModel ViewModel => (ActiveTrackListViewModel)this.DataContext;

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(ActiveTrackList),
            new PropertyMetadata(Orientation.Horizontal));

        private async void UserControl_Loaded()
        {
            await ViewModel.LoadPreviousStateAsync();
        }

        private void NameInput_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.SaveCommand.ExecuteAsync(NameInput.Text);
                e.Handled = true;
                SaveFlyout.Hide();
            }
        }

        private void SaveFlyout_Closed(object sender, object e)
        {
            NameInput.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFlyout.Hide();
        }
    }
}
