using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Transforms traceable campaign outcomes into concise, player-facing news memory.</summary>
public static class CampaignNewsFactory
{
    public static NewsItem CreateForActivity(int day, CampaignActionDefinition action, CampaignActionResolution resolution)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (resolution == null) throw new ArgumentNullException(nameof(resolution));

        return new NewsItem(
            $"activity-{action.Id}-{day}",
            day,
            $"activity-{action.Activity}",
            action.Id,
            CampaignCandidateIds.Player,
            resolution.WasPaid ? EvidenceQuality.Indication : EvidenceQuality.Rumor,
            resolution.WasPaid ? 0.65m : 0.35m,
            resolution.WasPaid ? 0.25m : -0.45m);
    }

    public static NewsItem CreateForDelegatedTasks(int day, IReadOnlyList<CauseRecord> causes)
    {
        if (causes == null) throw new ArgumentNullException(nameof(causes));
        if (causes.Count == 0) throw new ArgumentException("At least one delegated task cause is required.", nameof(causes));

        CauseRecord firstCause = causes[0];
        return new NewsItem(
            $"team-{firstCause.SourceId}-{day}",
            day,
            "delegated-task",
            firstCause.SourceId,
            CampaignCandidateIds.Player,
            EvidenceQuality.Indication,
            Math.Min(0.75m, 0.25m + causes.Count * 0.1m),
            0.10m);
    }

    public static NewsItem CreateForMonthlyClose(MonthlyCloseResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        bool hasUnpaidExpenses = result.UnpaidExpenses > 0;
        return new NewsItem(
            $"monthly-close-{result.Day}",
            result.Day,
            "monthly-close",
            "campaign-finance",
            CampaignCandidateIds.Player,
            hasUnpaidExpenses ? EvidenceQuality.Proof : EvidenceQuality.Indication,
            hasUnpaidExpenses ? 0.70m : 0.40m,
            hasUnpaidExpenses ? -0.50m : 0.05m);
    }

    public static NewsItem CreateForRivalAction(int day, CampaignActivity activity, CampaignActionResolution resolution)
    {
        if (resolution == null) throw new ArgumentNullException(nameof(resolution));

        return new NewsItem(
            $"rival-{activity}-{day}",
            day,
            "rival-response",
            "rival-" + activity,
            CampaignCandidateIds.Player,
            EvidenceQuality.Indication,
            resolution.WasPaid ? 0.70m : 0.35m,
            -0.35m);
    }

    public static NewsItem CreateForDeferredConsequence(int day, CampaignDeferredConsequence consequence)
    {
        if (consequence == null) throw new ArgumentNullException(nameof(consequence));

        return new NewsItem(
            $"deferred-{consequence.Id}-{day}",
            day,
            "deferred-consequence",
            consequence.SourceId,
            consequence.CandidateId,
            EvidenceQuality.Proof,
            0.75m,
            consequence.Magnitude >= 0m ? 0.20m : -0.45m);
    }

    public static NewsItem CreateForSliceClosure(CampaignSliceClosureResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        return new NewsItem(
            $"slice-closure-{result.Day}",
            result.Day,
            "slice-closure",
            "simulated-election",
            result.Outcome.WinnerId ?? result.Outcome.RunoffFirstId,
            EvidenceQuality.Proof,
            1m,
            result.Outcome.WinnerId == CampaignCandidateIds.Player ? 0.60m : -0.20m);
    }
}
}
