using AmbientSounds.ViewModels;
using Windows.UI.Xaml;
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

        private Visibility CanShowRestStat(double restMinutes)
        {
            if (double.IsNaN(restMinutes) || restMinutes <= 0)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        private string GetResultText(double focusMinutes, double totalFocusMinutes)
        {
            if (focusMinutes / totalFocusMinutes == 1)
            {
                return Strings.Resources.ResultMessageSuccess1;
            }
            else
            {
                return Strings.Resources.ResultMessageFail1;
            }
        }
    }
}
