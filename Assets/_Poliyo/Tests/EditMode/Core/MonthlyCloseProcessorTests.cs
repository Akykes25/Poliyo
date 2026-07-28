using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class MonthlyCloseProcessorTests
{
    [Test]
    public void Process_AppliesIncomeBeforeExpenses()
    {
        var economy = new CampaignEconomy(10m);
        var commitments = new[]
        {
            new MonthlyCommitment("headquarters", MonthlyCommitmentType.Expense, 50m),
            new MonthlyCommitment("investor", MonthlyCommitmentType.Income, 100m),
        };

        var result = MonthlyCloseProcessor.Process(30, economy, commitments);

        Assert.That(economy.Funds, Is.EqualTo(60m));
        Assert.That(result.AppliedIncome, Is.EqualTo(1));
        Assert.That(result.PaidExpenses, Is.EqualTo(1));
        Assert.That(result.UnpaidExpenses, Is.EqualTo(0));
    }

    [Test]
    public void Process_WhenExpenseCannotBePaid_RecordsUnpaidExpense()
    {
        var economy = new CampaignEconomy(0m);
        var result = MonthlyCloseProcessor.Process(30, economy, new[]
        {
            new MonthlyCommitment("staff", MonthlyCommitmentType.Expense, 40m),
        });

        Assert.That(result.UnpaidExpenses, Is.EqualTo(1));
        Assert.That(economy.UnpaidObligations, Is.EqualTo(40m));
    }
}

}
