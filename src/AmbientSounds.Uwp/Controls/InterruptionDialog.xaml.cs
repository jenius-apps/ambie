using Windows.UI.Xaml.Controls;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class InterruptionDialog : ContentDialog
    {
        public InterruptionDialog()
        {
            this.InitializeComponent();
        }

        public double MinutesLogged { get; private set; }

        public string InterruptionNotes { get; private set; }


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MinutesLogged = MinutesBox.Value;
            InterruptionNotes = NotesBox.Text;
            this.Hide();
        }

        private void NumberBox_ValueChanged(WinUI.NumberBox sender, WinUI.NumberBoxValueChangedEventArgs args)
        {
            IsPrimaryButtonEnabled = args.NewValue > 0d;
        }
    }
}
