using System;

namespace Poliyo.Simulation
{
public enum MonthlyCommitmentType { Income, Expense }

public sealed class MonthlyCommitment
{
    public MonthlyCommitment(string id, MonthlyCommitmentType type, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A commitment requires an id.", nameof(id));
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
        Id = id; Type = type; Amount = amount;
    }

    public string Id { get; }
    public MonthlyCommitmentType Type { get; }
    public decimal Amount { get; }
}

}
