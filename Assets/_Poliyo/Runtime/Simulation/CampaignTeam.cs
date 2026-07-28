using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Owns the team availability invariant and resolves delegated work exactly once.</summary>
public sealed class CampaignTeam
{
    private readonly Dictionary<string, CampaignTeamMember> _members;

    public CampaignTeam(IEnumerable<CampaignTeamMember> members)
    {
        if (members == null) throw new ArgumentNullException(nameof(members));

        _members = new Dictionary<string, CampaignTeamMember>();
        foreach (CampaignTeamMember member in members)
        {
            if (member == null) throw new ArgumentException("A team member is required.", nameof(members));
            _members.Add(member.Id, member);
        }
    }

    public IReadOnlyDictionary<string, CampaignTeamMember> Members => _members;

    public void Assign(int day, string memberId, DelegatedTaskType taskType, string targetId)
    {
        if (!_members.TryGetValue(memberId, out CampaignTeamMember member))
        {
            throw new KeyNotFoundException("The requested team member does not exist.");
        }

        member.Assign(new DelegatedTaskAssignment(day, memberId, taskType, targetId));
    }

    public IReadOnlyList<CauseRecord> ResolveAssignmentsForDay(CampaignState campaign)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));

        var causes = new List<CauseRecord>();
        foreach (CampaignTeamMember member in _members.Values)
        {
            DelegatedTaskAssignment assignment = member.CurrentAssignment;
            if (assignment == null || assignment.Day != campaign.Calendar.CurrentDay)
            {
                continue;
            }

            var cause = new CauseRecord(
                campaign.Calendar.CurrentDay,
                CauseCategory.DelegatedTask,
                member.Id,
                assignment.TargetId,
                assignment.TaskType.ToString(),
                1m);
            campaign.RecordCause(cause);
            causes.Add(cause);
            member.ReleaseAssignment();
        }

        return causes;
    }
}
}
