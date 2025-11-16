using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.Globalization.NumberFormatting;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class DigitalClock : UserControl
{
    public DigitalClock()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<DigitalClockViewModel>();
        SetNumberBoxNumberFormatter();
    }

    public DigitalClockViewModel ViewModel { get; }

    public void Initialize() => ViewModel.Initialize();

    public void Uninitialize() => ViewModel.Uninitialize();

    private void SetNumberBoxNumberFormatter()
    {
        IncrementNumberRounder rounder = new()
        {
            Increment = 1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfAwayFromZero
        };

        DecimalFormatter formatter = new()
        {
            IsDecimalPointAlwaysDisplayed = false,
            IntegerDigits = 2,
            FractionDigits = 0,
            NumberRounder = rounder
        };

        HourBox.NumberFormatter = formatter;
        MinuteBox.NumberFormatter = formatter;
        SecondBox.NumberFormatter = formatter;
    }

    private string GetPlayButtonAutomationName(bool canStart)
    {
        return canStart ? Strings.Resources.PlayerPlayText : Strings.Resources.PlayerPauseText;
    }
}
