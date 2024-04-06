using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Views;

public sealed partial class XboxShellPage : Page
{
    public XboxShellPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<XboxShellPageViewModel>();
    }

    public XboxShellPageViewModel ViewModel => (XboxShellPageViewModel)this.DataContext;
}
