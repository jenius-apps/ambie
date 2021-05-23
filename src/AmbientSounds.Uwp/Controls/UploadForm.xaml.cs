using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class UploadForm : UserControl
    {
        public UploadForm()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<UploadFormViewModel>();
            this.Unloaded += (_, _) => { ViewModel.Dispose(); };
        }

        public UploadFormViewModel ViewModel => (UploadFormViewModel)this.DataContext;
    }
}
