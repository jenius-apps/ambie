namespace AmbientSounds.Models;

public class PriceInfo
{
    public required string FormattedPrice { get; init; }

    public bool IsSubscription { get; init; }

    public int RecurrenceLength { get; init; }

    public DurationUnit RecurrenceUnit { get; init; }

    public bool HasSubTrial { get; init; }

    public int SubTrialLength { get; init; }

    public DurationUnit SubTrialLengthUnit { get; init; }
}

public enum DurationUnit
{
    Minute,
    Hour,
    Day,
    Week,
    Month,
    Year
}
