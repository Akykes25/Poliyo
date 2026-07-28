using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
public sealed class MonthlyCloseResult
{
    public MonthlyCloseResult(int day, int appliedIncome, int paidExpenses, int unpaidExpenses)
    {
        Day = day; AppliedIncome = appliedIncome; PaidExpenses = paidExpenses; UnpaidExpenses = unpaidExpenses;
    }

    public int Day { get; }
    public int AppliedIncome { get; }
    public int PaidExpenses { get; }
    public int UnpaidExpenses { get; }
}

/// <summary>Applies monthly commitments in the GDD order: income first, then fixed costs and unpaid consequences.</summary>
public static class MonthlyCloseProcessor
{
    public static MonthlyCloseResult Process(int day, CampaignEconomy economy, IEnumerable<MonthlyCommitment> commitments)
    {
        if (economy == null) throw new ArgumentNullException(nameof(economy));
        if (commitments == null) throw new ArgumentNullException(nameof(commitments));

        var income = new List<MonthlyCommitment>();
        var expenses = new List<MonthlyCommitment>();
        foreach (var commitment in commitments)
        {
            (commitment.Type == MonthlyCommitmentType.Income ? income : expenses).Add(commitment);
        }

        foreach (var commitment in income) economy.AddIncome(day, commitment.Id, commitment.Amount);

        var paidExpenses = 0;
        var unpaidExpenses = 0;
        foreach (var commitment in expenses)
        {
            if (economy.TryPayExpense(day, commitment.Id, commitment.Amount)) paidExpenses++;
            else unpaidExpenses++;
        }

        return new MonthlyCloseResult(day, income.Count, paidExpenses, unpaidExpenses);
    }
}

}
