// Required to support xbind
// in resource dictionaries.
// Ref: https://docs.microsoft.com/en-us/windows/uwp/data-binding/data-binding-in-depth#resource-dictionaries-with-xbind

using AmbientSounds.ViewModels;
using Windows.UI.Xaml;

namespace AmbientSounds.ResourceDictionaries;

public partial class SharedTemplates
{
    public SharedTemplates()
    {
        InitializeComponent();
    }

    private void OnGridTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement g && g.DataContext is OnlineSoundViewModel vm)
        {
            _ = vm.PrimaryActionCommand.ExecuteAsync(null);
        }
    }
}
