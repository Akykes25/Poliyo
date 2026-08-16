using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Owns the team availability invariant and resolves delegated work exactly once.</summary>
public sealed class CampaignTeam
{
    private readonly Dictionary<string, CampaignTeamMember> _members;
    private readonly Dictionary<string, CampaignTeamMember> _membersByRole;

    public CampaignTeam(IEnumerable<CampaignTeamMember> members)
    {
        if (members == null) throw new ArgumentNullException(nameof(members));

        _members = new Dictionary<string, CampaignTeamMember>();
        _membersByRole = new Dictionary<string, CampaignTeamMember>();
        foreach (CampaignTeamMember member in members)
        {
            if (member == null) throw new ArgumentException("A team member is required.", nameof(members));
            _members.Add(member.Id, member);
            _membersByRole.Add(member.RoleId, member);
        }
    }

    public IReadOnlyDictionary<string, CampaignTeamMember> Members => _members;
    public bool IsSelectionComplete
    {
        get
        {
            foreach (string roleId in CampaignTeamRoleIds.All)
            {
                if (!_membersByRole.ContainsKey(roleId))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public CampaignTeamMember SelectMember(string roleId, string profileId)
    {
        if (string.IsNullOrWhiteSpace(roleId)) throw new ArgumentException("A team role is required.", nameof(roleId));
        if (string.IsNullOrWhiteSpace(profileId)) throw new ArgumentException("A team profile is required.", nameof(profileId));
        if (!ContainsRequiredRole(roleId)) throw new ArgumentException("The requested team role is not part of the campaign team.", nameof(roleId));

        var member = new CampaignTeamMember(roleId, roleId, profileId);
        AddOrReplaceMember(member);
        return member;
    }

    public void RestoreMember(CampaignTeamMember member)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        AddOrReplaceMember(member);
    }

    public bool TryGetMemberByRole(string roleId, out CampaignTeamMember member)
    {
        if (string.IsNullOrWhiteSpace(roleId))
        {
            member = null;
            return false;
        }

        return _membersByRole.TryGetValue(roleId, out member);
    }

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

    private void AddOrReplaceMember(CampaignTeamMember member)
    {
        if (_membersByRole.TryGetValue(member.RoleId, out CampaignTeamMember previousMember))
        {
            if (!previousMember.IsAvailable)
            {
                throw new InvalidOperationException("A team member with pending work cannot be replaced.");
            }

            _members.Remove(previousMember.Id);
        }

        if (_members.TryGetValue(member.Id, out CampaignTeamMember memberWithSameId) && memberWithSameId.RoleId != member.RoleId)
        {
            throw new InvalidOperationException("A team member id cannot represent two roles.");
        }

        _members[member.Id] = member;
        _membersByRole[member.RoleId] = member;
    }

    private static bool ContainsRequiredRole(string roleId)
    {
        foreach (string requiredRoleId in CampaignTeamRoleIds.All)
        {
            if (requiredRoleId == roleId)
            {
                return true;
            }
        }

        return false;
    }
}
}
