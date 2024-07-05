using AmbientSounds.Models;
using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class PriceButtonContent : UserControl
{
    private readonly ILocalizer _localizer;

    public static readonly DependencyProperty PrimaryTextProperty = DependencyProperty.Register(
        nameof(PrimaryText),
        typeof(string),
        typeof(PriceButtonContent),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CaptionTextProperty = DependencyProperty.Register(
        nameof(CaptionText),
        typeof(string),
        typeof(PriceButtonContent),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PriceInfoProperty = DependencyProperty.Register(
        nameof(PriceInfo),
        typeof(PriceInfo),
        typeof(PriceButtonContent),
        new PropertyMetadata(null, OnPriceInfoChanged));

    public static readonly DependencyProperty PrimaryTextFontWeightProperty = DependencyProperty.Register(
        nameof(PrimaryTextFontWeight),
        typeof(FontWeight),
        typeof(PriceButtonContent),
        new PropertyMetadata(FontWeights.Normal));

    private static void OnPriceInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PriceButtonContent)d).UpdateText();
    }

    public PriceButtonContent()
    {
        this.InitializeComponent();
        _localizer = App.Services.GetRequiredService<ILocalizer>();
    }

    public string PrimaryText
    {
        get => (string)GetValue(PrimaryTextProperty);
        set => SetValue(PrimaryTextProperty, value);
    }

    public string CaptionText
    {
        get => (string)GetValue(CaptionTextProperty);
        set => SetValue(CaptionTextProperty, value);
    }

    public PriceInfo? PriceInfo
    {
        get => (PriceInfo)GetValue(PriceInfoProperty);
        set => SetValue(PriceInfoProperty, value);
    }

    public FontWeight PrimaryTextFontWeight
    {
        get => (FontWeight)GetValue(PrimaryTextFontWeightProperty);
        set => SetValue(PrimaryTextFontWeightProperty, value);
    }

    private void UpdateText()
    {
        if (PriceInfo is null)
        {
            return;
        }

        if (PriceInfo is { IsSubscription: true, HasSubTrial: true })
        {
            PrimaryText = "Start free trial";
            CaptionText = $"{PriceInfo.SubTrialLength} {PriceInfo.SubTrialLengthUnit} free, then {PriceInfo.FormattedPrice}/{PriceInfo.RecurrenceUnit}";
        }
        else
        {
            PrimaryText = PriceInfo.FormattedPrice;
            CaptionText = string.Empty;
        }

        PrimaryTextFontWeight = CaptionText.Length > 0 ? FontWeights.SemiBold : FontWeights.Normal;
    }
}
