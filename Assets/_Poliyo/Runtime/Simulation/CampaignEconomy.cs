using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
public enum CampaignTransactionType { Income, Expense }

public sealed class CampaignTransaction
{
    public CampaignTransaction(int day, CampaignTransactionType type, string sourceId, decimal amount)
    {
        if (day < 1 || day > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(day));
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("A transaction requires a source.", nameof(sourceId));
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
        Day = day;
        Type = type;
        SourceId = sourceId;
        Amount = amount;
    }

    public int Day { get; }
    public CampaignTransactionType Type { get; }
    public string SourceId { get; }
    public decimal Amount { get; }
}

/// <summary>Campaign funds never drop below zero; unpaid costs remain visible for later consequences.</summary>
public sealed class CampaignEconomy
{
    private readonly List<CampaignTransaction> _transactions = new List<CampaignTransaction>();

    public CampaignEconomy(decimal initialFunds, decimal initialUnpaidObligations = 0m)
    {
        if (initialFunds < 0m) throw new ArgumentOutOfRangeException(nameof(initialFunds));
        if (initialUnpaidObligations < 0m) throw new ArgumentOutOfRangeException(nameof(initialUnpaidObligations));
        Funds = initialFunds;
        UnpaidObligations = initialUnpaidObligations;
    }

    public decimal Funds { get; private set; }
    public decimal UnpaidObligations { get; private set; }
    public IReadOnlyList<CampaignTransaction> Transactions => _transactions;

    public void AddIncome(int day, string sourceId, decimal amount)
    {
        var transaction = new CampaignTransaction(day, CampaignTransactionType.Income, sourceId, amount);
        Funds += amount;
        _transactions.Add(transaction);
    }

    public bool TryPayExpense(int day, string sourceId, decimal amount)
    {
        var transaction = new CampaignTransaction(day, CampaignTransactionType.Expense, sourceId, amount);
        if (Funds < amount)
        {
            UnpaidObligations += amount;
            return false;
        }

        Funds -= amount;
        _transactions.Add(transaction);
        return true;
    }
}
}