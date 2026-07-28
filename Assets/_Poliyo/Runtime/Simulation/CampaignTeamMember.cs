using System;

namespace Poliyo.Simulation
{
public sealed class CampaignTeamMember
{
    public CampaignTeamMember(string id, string roleId)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException("A team member requires an id and role.");
        }

        Id = id;
        RoleId = roleId;
    }

    public string Id { get; }
    public string RoleId { get; }
    public DelegatedTaskAssignment CurrentAssignment { get; private set; }

    public bool IsAvailable => CurrentAssignment == null;

    public void Assign(DelegatedTaskAssignment assignment)
    {
        if (assignment == null) throw new ArgumentNullException(nameof(assignment));
        if (!IsAvailable) throw new InvalidOperationException("A team member cannot execute two tasks at once.");
        if (assignment.MemberId != Id) throw new InvalidOperationException("An assignment belongs to another member.");
        CurrentAssignment = assignment;
    }

    public void ReleaseAssignment()
    {
        CurrentAssignment = null;
    }
}

}
