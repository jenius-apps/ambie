using AmbientSounds.ViewModels;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusHistoryDetails : UserControl
    {
        public FocusHistoryDetails()
        {
            this.InitializeComponent();
        }

        public FocusHistoryDetails(FocusHistoryViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
        }

        public FocusHistoryViewModel ViewModel { get; }

        private string FormatStartEnd(string start, string end)
        {
            return $"{start} ‒ {end}";
        }
    }
}
