using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class MenuItemControl : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(MenuItemControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
        nameof(ToolTipText),
        typeof(string),
        typeof(MenuItemControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ToolTipSubtitleProperty = DependencyProperty.Register(
        nameof(ToolTipSubtitle),
        typeof(string),
        typeof(MenuItemControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph),
        typeof(string),
        typeof(MenuItemControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected),
        typeof(bool),
        typeof(MenuItemControl),
        new PropertyMetadata(false));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command),
        typeof(ICommand),
        typeof(MenuItemControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter),
        typeof(object),
        typeof(MenuItemControl),
        new PropertyMetadata(null));

    public MenuItemControl()
    {
        this.InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string ToolTipText
    {
        get
        {
            var result = (string)GetValue(ToolTipTextProperty);
            return string.IsNullOrEmpty(result) ? Text : result;
        }
        set => SetValue(ToolTipTextProperty, value);
    }

    public string ToolTipSubtitle
    {
        get => (string)GetValue(ToolTipSubtitleProperty);
        set => SetValue(ToolTipSubtitleProperty, value);
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}
