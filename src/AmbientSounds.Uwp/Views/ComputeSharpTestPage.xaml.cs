using AmbientSounds.Shaders;
using ComputeSharp.Uwp;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ComputeSharpTestPage : Page
    {
        public ComputeSharpTestPage()
        {
            this.InitializeComponent();

            ShaderPanel.ShaderRunner = new ShaderRunner<ColorfulInfinity>(static time => new ColorfulInfinity((float)time.TotalSeconds / 4f));
        }
    }
}
