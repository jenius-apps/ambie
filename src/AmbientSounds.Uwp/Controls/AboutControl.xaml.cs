using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class AboutControl : UserControl
    {
        public AboutControl()
        {
            this.InitializeComponent();
        }

        private string Version => SystemInformation.Instance.ApplicationVersion.ToFormattedString();
    }
}
