using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Views
{
    public sealed partial class StudyHomePage : Page
    {
        public StudyHomePage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<StudyPageViewModel>();
        }

        public StudyPageViewModel ViewModel => (StudyPageViewModel)this.DataContext;
    }
}
