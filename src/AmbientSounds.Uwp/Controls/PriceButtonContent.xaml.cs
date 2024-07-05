using AmbientSounds.Models;
using Humanizer;
using Humanizer.Localisation;
using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            PrimaryText = _localizer.GetString("StartFreeTrialText");
            CaptionText = string.Format(
                _localizer.GetString("SubWithTrialCaptionTemplate"),
                HumanizeTime(PriceInfo.SubTrialLength, PriceInfo.SubTrialLengthUnit),
                $"{PriceInfo.FormattedPrice}/{HumanizeRecurrence(PriceInfo.RecurrenceUnit)}");
        }
        else
        {
            PrimaryText = PriceInfo.FormattedPrice;
            CaptionText = string.Empty;
        }

        PrimaryTextFontWeight = CaptionText.Length > 0 ? FontWeights.SemiBold : FontWeights.Normal;
    }

    private static string HumanizeRecurrence(DurationUnit unit)
    {
        var result = unit switch
        {
            DurationUnit.Minute => TimeUnit.Minute.Humanize(),
            DurationUnit.Hour => TimeUnit.Hour.Humanize(),
            DurationUnit.Day => TimeUnit.Day.Humanize(),
            DurationUnit.Week => TimeUnit.Week.Humanize(),
            DurationUnit.Month => TimeUnit.Month.Humanize(),
            DurationUnit.Year => TimeUnit.Year.Humanize(),
            _ => string.Empty
        };

        return result.ToLower();
    }

    private static string HumanizeTime(int length, DurationUnit unit)
    {
        if (length == 0)
        {
            return string.Empty;
        }

        return unit switch
        {
            DurationUnit.Minute => TimeSpan.FromMinutes(length).Humanize(),
            DurationUnit.Hour => TimeSpan.FromHours(length).Humanize(),
            DurationUnit.Day => TimeSpan.FromDays(length).Humanize(maxUnit: TimeUnit.Day),
            DurationUnit.Week => TimeSpan.FromDays(length * 7).Humanize(minUnit: TimeUnit.Week),
            DurationUnit.Month => TimeSpan.FromDays(length * 30).Humanize(minUnit: TimeUnit.Month),
            DurationUnit.Year => TimeSpan.FromDays(length * 365).Humanize(minUnit: TimeUnit.Year),
            _ => string.Empty
        };
    }
}
