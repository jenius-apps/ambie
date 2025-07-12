namespace AmbientSounds.Models;

/// <summary>
/// Class that holds price information.
/// </summary>
public class PriceInfo
{
    /// <summary>
    /// The localized and formatted price of the product.
    /// E.g. $1.49.
    /// </summary>
    public string FormattedPrice { get; init; } = string.Empty;

    /// <summary>
    /// Specifies if this product is a subscription.
    /// </summary>
    public bool IsSubscription { get; init; }

    /// <summary>
    /// If the product is a subscription,
    /// this specifies the length of recurrence.
    /// E.g. If it's a 1-month recurrence, then this property is 1.
    /// </summary>
    public int RecurrenceLength { get; init; }

    /// <summary>
    /// If the product is a subscription,
    /// this specifies the unit of recurrence.
    /// E.g. If it's a 1-month recurrence, then this property is <see cref="DurationUnit.Month"/>.
    /// </summary>
    public DurationUnit RecurrenceUnit { get; init; }

    /// <summary>
    /// If the product is a subscription,
    /// this specifies if there is a trial period.
    /// </summary>
    public bool HasSubTrial { get; init; }

    /// <summary>
    /// If the product is a subscription and has a trial period,
    /// this specifies the length of the trial.
    /// E.g. If it's a 1-week trial, then this property is 1.
    /// </summary>
    public int SubTrialLength { get; init; }


    /// <summary>
    /// If the product is a subscription and has a trial period,
    /// this specifies the unit of the trial period.
    /// E.g. If it's a 1-week trial, then this property is <see cref="DurationUnit.Week"/>.
    /// </summary>
    public DurationUnit SubTrialLengthUnit { get; init; }
}

public enum DurationUnit
{
    Minute,
    Hour,
    Day,
    Week,
    Month,
    Year,
}
