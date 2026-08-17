using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Resolves delegated work into explicit electoral or economic effects before the day advances.</summary>
public static class CampaignDelegatedTaskResolver
{
    public static IReadOnlyList<CauseRecord> Resolve(
        CampaignTeam team,
        CampaignState campaign,
        CampaignEconomy economy,
        IReadOnlyList<MicroElector> electorate)
    {
        if (team == null) throw new ArgumentNullException(nameof(team));
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (economy == null) throw new ArgumentNullException(nameof(economy));
        if (electorate == null) throw new ArgumentNullException(nameof(electorate));

        var causes = new List<CauseRecord>();
        foreach (CampaignTeamMember member in team.Members.Values)
        {
            DelegatedTaskAssignment assignment = member.CurrentAssignment;
            if (assignment == null || assignment.Day != campaign.Calendar.CurrentDay)
            {
                continue;
            }

            var completionCause = new CauseRecord(
                campaign.Calendar.CurrentDay,
                CauseCategory.DelegatedTask,
                member.Id,
                assignment.TargetId,
                assignment.TaskType.ToString(),
                1m);
            campaign.RecordCause(completionCause);
            causes.Add(completionCause);

            if (assignment.TaskType == DelegatedTaskType.Fundraising)
            {
                economy.AddIncome(campaign.Calendar.CurrentDay, member.Id + "-fundraising", 35m);
                var incomeCause = new CauseRecord(
                    campaign.Calendar.CurrentDay,
                    CauseCategory.Economy,
                    member.Id,
                    "campaign",
                    "fundraising-income",
                    35m);
                campaign.RecordCause(incomeCause);
                causes.Add(incomeCause);
            }
            else
            {
                ElectoralImpact impact = CreateImpact(member, assignment);
                IReadOnlyList<MicroElector> targets = SelectTargets(electorate, assignment.TargetId);
                if (impact != null && targets.Count > 0)
                {
                    IReadOnlyList<CauseRecord> effectCauses = ElectoralImpactApplier.Apply(
                        campaign,
                        impact,
                        targets,
                        CauseCategory.DelegatedTask);
                    foreach (CauseRecord effectCause in effectCauses)
                    {
                        causes.Add(effectCause);
                    }
                }
            }

            member.ReleaseAssignment();
        }

        return causes.AsReadOnly();
    }

    private static ElectoralImpact CreateImpact(CampaignTeamMember member, DelegatedTaskAssignment assignment)
    {
        ElectoralMetric metric;
        decimal magnitude;
        switch (assignment.TaskType)
        {
            case DelegatedTaskType.PoliticalContact:
                metric = ElectoralMetric.Trust;
                magnitude = 0.8m;
                break;
            case DelegatedTaskType.Investigation:
                metric = ElectoralMetric.Trust;
                magnitude = 1.1m;
                break;
            case DelegatedTaskType.MediaStatement:
                metric = ElectoralMetric.VotingIntention;
                magnitude = 0.9m;
                break;
            case DelegatedTaskType.CrisisAnalysis:
                metric = ElectoralMetric.Rejection;
                magnitude = -1.2m;
                break;
            case DelegatedTaskType.TerritorialCampaign:
                metric = ElectoralMetric.VotingIntention;
                magnitude = 1.4m;
                break;
            case DelegatedTaskType.AffiliateRecruitment:
                metric = ElectoralMetric.Participation;
                magnitude = 1.0m;
                break;
            default:
                return null;
        }

        return new ElectoralImpact(
            "team-" + member.Id + "-" + assignment.TaskType,
            CampaignCandidateIds.Player,
            metric,
            magnitude,
            1m,
            1m,
            1m,
            1m,
            1m,
            1m);
    }

    private static IReadOnlyList<MicroElector> SelectTargets(IReadOnlyList<MicroElector> electorate, string targetId)
    {
        if (string.Equals(targetId, "nacional", StringComparison.OrdinalIgnoreCase))
        {
            return electorate;
        }

        var targets = new List<MicroElector>();
        foreach (MicroElector elector in electorate)
        {
            if (elector.LocalityId == targetId)
            {
                targets.Add(elector);
            }
        }

        return targets;
    }
}
}
