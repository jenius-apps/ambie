using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class ShareControl : UserControl
{
    public ShareControl()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ShareViewModel>();
    }

    public ShareViewModel ViewModel => (ShareViewModel)this.DataContext;

    private string GetCopyText(bool isCopySuccessful)
    {
        return isCopySuccessful
            ? Strings.Resources.CopiedText
            : Strings.Resources.CopyText;
    }
}
