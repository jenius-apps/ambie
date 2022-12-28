using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AmbientSounds.Controls;

public sealed partial class TutorialControl : UserControl
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(TutorialControl),
            new PropertyMetadata(string.Empty)); 
    
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(TutorialControl),
            new PropertyMetadata(null));

    public TutorialControl()
    {
        this.InitializeComponent();
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ImageSource Source
    {
        get => (ImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private void OnGifOpened(object sender, RoutedEventArgs e)
    {
        GifLoadingRing.IsActive = false;
    }
}
