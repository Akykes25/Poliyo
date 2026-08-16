using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignEconomyTests
{
    [Test]
    public void TryPayExpense_WhenFundsAreInsufficient_PreservesFundsAndRecordsObligation()
    {
        var economy = new CampaignEconomy(100m);

        var paid = economy.TryPayExpense(1, "act", 120m);

        Assert.That(paid, Is.False);
        Assert.That(economy.Funds, Is.EqualTo(100m));
        Assert.That(economy.UnpaidObligations, Is.EqualTo(120m));
    }

    [Test]
    public void AddIncome_ThenPayExpense_UpdatesFundsAndLedger()
    {
        var economy = new CampaignEconomy(100m);
        economy.AddIncome(1, "investor", 80m);

        var paid = economy.TryPayExpense(1, "act", 150m);

        Assert.That(paid, Is.True);
        Assert.That(economy.Funds, Is.EqualTo(30m));
        Assert.That(economy.Transactions, Has.Count.EqualTo(2));
    }
}

}
