namespace Core.Enums
{
    /// <summary>
    /// In-memory reward channel (UI, bundles, analytics). Save data stays numeric fields, not this enum.
    /// </summary>
    public enum RewardType
    {
        SoftCurrency,
        HardCurrency,
        Stars
    }
}
