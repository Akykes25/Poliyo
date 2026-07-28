using System;
using System.Collections.Generic;
using Poliyo.Simulation;

namespace Poliyo.Application
{
/// <summary>Pure application-level composition for the playable campaign loop. Unity only supplies content and presentation.</summary>
public sealed class CampaignSimulationSession
{
    private readonly CampaignElectionService _electionService;
    private CampaignElectionResult _electionResult;
    private int _lastActionDay;
    private int _activityWeek;
    private int _publicActivitiesThisWeek;

    public CampaignSimulationSession(CampaignRuntime runtime, IReadOnlyList<MicroElector> electorate, CampaignTeam team, NewsMemory news)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        Electorate = electorate ?? throw new ArgumentNullException(nameof(electorate));
        Team = team ?? throw new ArgumentNullException(nameof(team));
        News = news ?? throw new ArgumentNullException(nameof(news));
        _electionService = new CampaignElectionService(CampaignCandidateIds.All);
    }

    public CampaignRuntime Runtime { get; }
    public IReadOnlyList<MicroElector> Electorate { get; }
    public CampaignTeam Team { get; }
    public NewsMemory News { get; }
    public CampaignElectionResult ElectionResult => _electionResult;

    public CampaignSaveData CreateSaveData()
    {
        CampaignSaveData saveData = CampaignSaveMapper.Create(Runtime, Electorate);
        saveData.LastActionDay = _lastActionDay;
        saveData.ActivityWeek = _activityWeek;
        saveData.PublicActivitiesThisWeek = _publicActivitiesThisWeek;
        saveData.TeamMembers = CreateTeamSaveData();
        saveData.NewsItems = CampaignSaveMapper.CreateNewsData(News.Items);
        return saveData;
    }

    public void RestoreActivityLimits(CampaignSaveData saveData)
    {
        CampaignSaveMapper.Validate(saveData);
        if (saveData.LastActionDay < 0 || saveData.LastActionDay > Runtime.State.Calendar.CurrentDay)
        {
            throw new InvalidOperationException("The save contains an invalid last action day.");
        }

        if (saveData.ActivityWeek < 0 || saveData.ActivityWeek > Runtime.State.Calendar.CurrentWeek || saveData.PublicActivitiesThisWeek < 0 || saveData.PublicActivitiesThisWeek > 3)
        {
            throw new InvalidOperationException("The save contains invalid weekly activity limits.");
        }

        _lastActionDay = saveData.LastActionDay;
        _activityWeek = saveData.ActivityWeek;
        _publicActivitiesThisWeek = saveData.PublicActivitiesThisWeek;
    }

    public void RestoreTeam(CampaignSaveData saveData)
    {
        CampaignSaveMapper.Validate(saveData);
        foreach (TeamMemberSaveData memberData in saveData.TeamMembers)
        {
            if (memberData == null || memberData.Assignment == null)
            {
                continue;
            }

            if (!Team.Members.TryGetValue(memberData.Id, out CampaignTeamMember member) || member.RoleId != memberData.RoleId)
            {
                throw new InvalidOperationException("The save references an incompatible campaign team member.");
            }

            if (!Enum.TryParse(memberData.Assignment.TaskType, out DelegatedTaskType taskType) || !Enum.IsDefined(typeof(DelegatedTaskType), taskType))
            {
                throw new InvalidOperationException("The save contains an invalid delegated task.");
            }

            Team.Assign(memberData.Assignment.Day, memberData.Id, taskType, memberData.Assignment.TargetId);
        }
    }

    public void RestoreNews(CampaignSaveData saveData)
    {
        News.Restore(CampaignSaveMapper.RestoreNews(saveData));
    }

    public void AssignTeamTask(string memberId, DelegatedTaskType taskType, string targetId)
    {
        Team.Assign(Runtime.State.Calendar.CurrentDay, memberId, taskType, targetId);
    }

    public CampaignActionResolution ResolveAction(CampaignActionDefinition action, IEnumerable<MicroElector> targets)
    {
        int currentDay = Runtime.State.Calendar.CurrentDay;
        int currentWeek = Runtime.State.Calendar.CurrentWeek;
        if (_lastActionDay == currentDay)
        {
            throw new InvalidOperationException("Only one public campaign activity can be resolved per day.");
        }

        if (_activityWeek != currentWeek)
        {
            _activityWeek = currentWeek;
            _publicActivitiesThisWeek = 0;
        }

        if (_publicActivitiesThisWeek >= 3)
        {
            throw new InvalidOperationException("The weekly public activity limit has been reached.");
        }

        CampaignActionResolution result = CampaignActionResolver.Resolve(Runtime.State, Runtime.Economy, action, targets);
        News.Publish(CampaignNewsFactory.CreateForActivity(currentDay, action, result));
        _lastActionDay = currentDay;
        _publicActivitiesThisWeek++;
        return result;
    }

    public CampaignDayAdvanceResult AdvanceDay()
    {
        IReadOnlyList<CauseRecord> taskCauses = Team.ResolveAssignmentsForDay(Runtime.State);
        MonthlyCloseResult monthlyClose = Runtime.AdvanceDay();
        News.AdvanceDay();

        if (taskCauses.Count > 0)
        {
            News.Publish(CampaignNewsFactory.CreateForDelegatedTasks(taskCauses[0].Day, taskCauses));
        }

        if (monthlyClose != null)
        {
            News.Publish(CampaignNewsFactory.CreateForMonthlyClose(monthlyClose));
        }

        if (!Runtime.State.Calendar.IsElectionDay || _electionResult != null)
        {
            return new CampaignDayAdvanceResult(monthlyClose, taskCauses, _electionResult, false);
        }

        if (Electorate.Count == 0)
        {
            return new CampaignDayAdvanceResult(monthlyClose, taskCauses, null, true);
        }

        _electionResult = _electionService.ResolveFirstRound(Runtime.State, Electorate);
        return new CampaignDayAdvanceResult(monthlyClose, taskCauses, _electionResult, false);
    }

    private TeamMemberSaveData[] CreateTeamSaveData()
    {
        var savedMembers = new List<TeamMemberSaveData>(Team.Members.Count);
        foreach (CampaignTeamMember member in Team.Members.Values)
        {
            DelegatedTaskAssignment assignment = member.CurrentAssignment;
            savedMembers.Add(new TeamMemberSaveData
            {
                Id = member.Id,
                RoleId = member.RoleId,
                Assignment = assignment == null ? null : new DelegatedTaskSaveData
                {
                    Day = assignment.Day,
                    TaskType = assignment.TaskType.ToString(),
                    TargetId = assignment.TargetId,
                },
            });
        }

        return savedMembers.ToArray();
    }
}
}