using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class UploadForm : UserControl
    {
        public UploadForm()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<UploadFormViewModel>();
        }

        public UploadFormViewModel ViewModel => (UploadFormViewModel)this.DataContext;
    }
}
