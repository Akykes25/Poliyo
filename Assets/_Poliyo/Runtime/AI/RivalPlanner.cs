using System;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.AI
{
public sealed class RivalPlanner
{
    public RivalPlan CreateWeeklyPlan(RivalStyle style, RivalKnownSituation situation, DeterministicRandom random)
    {
        if (situation == null) throw new ArgumentNullException(nameof(situation));
        if (random == null) throw new ArgumentNullException(nameof(random));

        if (situation.KnownThreat >= 0.70m)
        {
            return new RivalPlan(CampaignActivity.Interview, DelegatedTaskType.CrisisAnalysis);
        }

        if (style == RivalStyle.Territorial || situation.TerritorialPressure >= 0.65m)
        {
            return new RivalPlan(CampaignActivity.Rally, DelegatedTaskType.TerritorialCampaign);
        }

        if (style == RivalStyle.Calculating)
        {
            return new RivalPlan(CampaignActivity.Negotiation, DelegatedTaskType.Investigation);
        }

        if (style == RivalStyle.Confrontational || situation.MediaOpportunity >= 0.65m)
        {
            return new RivalPlan(CampaignActivity.Interview, DelegatedTaskType.MediaStatement);
        }

        return random.NextInt(0, 2) == 0
            ? new RivalPlan(CampaignActivity.Negotiation, DelegatedTaskType.PoliticalContact)
            : new RivalPlan(CampaignActivity.Rally, DelegatedTaskType.AffiliateRecruitment);
    }
}

}
