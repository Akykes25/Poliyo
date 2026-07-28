using System;

namespace Poliyo.AI
{
/// <summary>Only information legitimately available to a rival at planning time.</summary>
public sealed class RivalKnownSituation
{
    public RivalKnownSituation(decimal funds, decimal territorialPressure, decimal mediaOpportunity, decimal knownThreat)
    {
        Funds = ValidateNonNegative(funds, nameof(funds));
        TerritorialPressure = ValidateUnit(territorialPressure, nameof(territorialPressure));
        MediaOpportunity = ValidateUnit(mediaOpportunity, nameof(mediaOpportunity));
        KnownThreat = ValidateUnit(knownThreat, nameof(knownThreat));
    }

    public decimal Funds { get; }
    public decimal TerritorialPressure { get; }
    public decimal MediaOpportunity { get; }
    public decimal KnownThreat { get; }

    private static decimal ValidateNonNegative(decimal value, string name) => value < 0m ? throw new ArgumentOutOfRangeException(name) : value;
    private static decimal ValidateUnit(decimal value, string name) => value < 0m || value > 1m ? throw new ArgumentOutOfRangeException(name) : value;
}

}
