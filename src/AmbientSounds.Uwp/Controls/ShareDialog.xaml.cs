using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class ShareDialog : ContentDialog
{
    public ShareDialog()
    {
        this.InitializeComponent();
    }

    public Task InitializeAsync(IReadOnlyList<string> soundIds)
    {
        return ShareControl.ViewModel.InitializeAsync(soundIds);
    }

    public void Uninitialize()
    {
        ShareControl.ViewModel.Uninitialize();
    }
}
