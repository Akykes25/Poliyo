using System;
using System.Collections.Generic;
using Poliyo.Simulation;

namespace Poliyo.Application
{
/// <summary>Pure application-level composition for the playable campaign loop. Unity only supplies content and presentation.</summary>
public sealed class CampaignSimulationSession
{
    private readonly CampaignElectionService _electionService;
    private readonly int _electionRulesVersion;
    private CampaignElectionResult _electionResult;
    private int _lastActionDay;
    private int _activityWeek;
    private int _publicActivitiesThisWeek;
    private bool _teamSelectionCompleted;
    private readonly Dictionary<string, PoliticalRelationship> _relationships = new Dictionary<string, PoliticalRelationship>();
    private readonly List<PoliticalPromise> _promises = new List<PoliticalPromise>();
    private readonly List<CampaignDecisionRecord> _decisionRecords = new List<CampaignDecisionRecord>();

    public CampaignSimulationSession(
        CampaignRuntime runtime,
        IReadOnlyList<MicroElector> electorate,
        CampaignTeam team,
        NewsMemory news,
        int electionRulesVersion = CampaignSaveData.CurrentElectionRulesVersion,
        bool teamSelectionCompleted = true)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        Electorate = electorate ?? throw new ArgumentNullException(nameof(electorate));
        Team = team ?? throw new ArgumentNullException(nameof(team));
        News = news ?? throw new ArgumentNullException(nameof(news));
        if (electionRulesVersion < CampaignSaveData.LegacyElectionRulesVersion ||
            electionRulesVersion > CampaignSaveData.CurrentElectionRulesVersion)
        {
            throw new NotSupportedException($"Election rules version {electionRulesVersion} is not supported.");
        }

        _electionRulesVersion = electionRulesVersion;
        _teamSelectionCompleted = teamSelectionCompleted;
        _electionService = new CampaignElectionService(CampaignCandidateIds.All);
    }

    public CampaignRuntime Runtime { get; }
    public IReadOnlyList<MicroElector> Electorate { get; }
    public CampaignTeam Team { get; }
    public NewsMemory News { get; }
    public CampaignElectionResult ElectionResult => _electionResult;
    public bool TeamSelectionCompleted => _teamSelectionCompleted;
    public IReadOnlyDictionary<string, PoliticalRelationship> Relationships => _relationships;
    public IReadOnlyList<PoliticalPromise> Promises => _promises;
    public IReadOnlyList<CampaignDecisionRecord> DecisionRecords => _decisionRecords;
    public bool CanAssignTeamTask => _teamSelectionCompleted && !AreCampaignActionsLockedByElection;
    public bool CanResolvePublicAction
    {
        get
        {
            int currentDay = Runtime.State.Calendar.CurrentDay;
            int currentWeek = Runtime.State.Calendar.CurrentWeek;
            int activitiesThisWeek = _activityWeek == currentWeek ? _publicActivitiesThisWeek : 0;
            return _teamSelectionCompleted && !AreCampaignActionsLockedByElection && _lastActionDay != currentDay && activitiesThisWeek < 3;
        }
    }

    public CampaignSaveData CreateSaveData()
    {
        CampaignSaveData saveData = CampaignSaveMapper.Create(Runtime, Electorate);
        saveData.ElectionRulesVersion = _electionRulesVersion;
        saveData.LastActionDay = _lastActionDay;
        saveData.ActivityWeek = _activityWeek;
        saveData.PublicActivitiesThisWeek = _publicActivitiesThisWeek;
        saveData.TeamSelectionCompleted = _teamSelectionCompleted;
        saveData.TeamMembers = CreateTeamSaveData();
        saveData.NewsItems = CampaignSaveMapper.CreateNewsData(News.Items);
        saveData.Relationships = CampaignSaveMapper.CreateRelationshipData(_relationships.Values);
        saveData.Promises = CampaignSaveMapper.CreatePromiseData(_promises);
        saveData.DecisionRecords = CampaignSaveMapper.CreateDecisionRecordData(_decisionRecords);
        saveData.CauseRecords = CampaignSaveMapper.CreateCauseData(Runtime.State.CauseRecords);
        saveData.ElectionResult = CampaignSaveMapper.CreateElectionResultData(_electionResult);
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
        _teamSelectionCompleted = saveData.TeamSelectionCompleted;
        foreach (TeamMemberSaveData memberData in saveData.TeamMembers)
        {
            if (memberData == null)
            {
                continue;
            }

            string profileId = string.IsNullOrWhiteSpace(memberData.ProfileId) ? memberData.Id : memberData.ProfileId;
            CampaignTeamMember member;
            if (!Team.Members.TryGetValue(memberData.Id, out member))
            {
                member = new CampaignTeamMember(memberData.Id, memberData.RoleId, profileId);
                Team.RestoreMember(member);
            }
            else if (member.RoleId != memberData.RoleId)
            {
                throw new InvalidOperationException("The save references an incompatible campaign team member.");
            }

            if (memberData.Assignment == null)
            {
                continue;
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

    public void RestorePoliticalMemory(CampaignSaveData saveData)
    {
        CampaignSaveMapper.Validate(saveData);
        _relationships.Clear();
        foreach (PoliticalRelationship relationship in CampaignSaveMapper.RestoreRelationships(saveData))
        {
            _relationships.Add(relationship.ActorId, relationship);
        }

        _promises.Clear();
        _promises.AddRange(CampaignSaveMapper.RestorePromises(saveData));
        _decisionRecords.Clear();
        _decisionRecords.AddRange(CampaignSaveMapper.RestoreDecisionRecords(saveData));
        Runtime.State.RestoreCauseRecords(CampaignSaveMapper.RestoreCauses(saveData));
    }

    public void SelectTeamMember(string roleId, string profileId)
    {
        if (AreCampaignActionsLockedByElection)
        {
            throw new InvalidOperationException("The initial team cannot be changed after the election starts.");
        }

        if (_teamSelectionCompleted && Team.IsSelectionComplete)
        {
            throw new InvalidOperationException("The initial campaign team is already confirmed for this campaign.");
        }

        Team.SelectMember(roleId, profileId);
        _teamSelectionCompleted = false;
    }

    public void FinalizeTeamSelection()
    {
        if (!Team.IsSelectionComplete)
        {
            throw new InvalidOperationException("The initial team must contain one selected profile for each required role.");
        }

        _teamSelectionCompleted = true;
    }

    public void RestoreElectionResult(CampaignSaveData saveData)
    {
        CampaignElectionResult restoredResult = CampaignSaveMapper.RestoreElectionResult(saveData);
        if (saveData.ElectionRulesVersion != _electionRulesVersion)
        {
            throw new InvalidOperationException("The session and save use different election rules versions.");
        }

        if (_electionResult != null)
        {
            return;
        }

        if (restoredResult != null)
        {
            _electionResult = restoredResult;
            CompleteElectionPhase(_electionResult);
            return;
        }

        if (Runtime.State.Calendar.IsElectionDay && Electorate.Count > 0)
        {
            _electionResult = ResolveConfiguredFirstRound();
            CompleteElectionPhase(_electionResult);
        }
    }

    public void AssignTeamTask(string memberId, DelegatedTaskType taskType, string targetId)
    {
        if (!CanAssignTeamTask)
        {
            throw new InvalidOperationException("Campaign team tasks cannot be assigned on or after election day.");
        }

        Team.Assign(Runtime.State.Calendar.CurrentDay, memberId, taskType, targetId);
    }

    public CampaignActionResolution ResolveAction(CampaignActionDefinition action, IEnumerable<MicroElector> targets)
    {
        int currentDay = Runtime.State.Calendar.CurrentDay;
        int currentWeek = Runtime.State.Calendar.CurrentWeek;
        if (AreCampaignActionsLockedByElection)
        {
            throw new InvalidOperationException("Public campaign activities cannot be resolved on or after election day.");
        }

        if (!_teamSelectionCompleted)
        {
            throw new InvalidOperationException("Choose and confirm the initial campaign team before resolving public activities.");
        }

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

        if (!Runtime.Economy.CanAfford(action.Cost))
        {
            return new CampaignActionResolution(false, Array.Empty<CauseRecord>());
        }

        CampaignActionResolution result = CampaignActionResolver.Resolve(Runtime.State, Runtime.Economy, action, targets);
        if (!result.WasPaid)
        {
            return result;
        }
        News.Publish(CampaignNewsFactory.CreateForActivity(currentDay, action, result));
        _lastActionDay = currentDay;
        _publicActivitiesThisWeek++;
        return result;
    }

    public CampaignDecisionResolution ResolveDecision(CampaignDecisionPlan plan, IEnumerable<MicroElector> targets)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (targets == null) throw new ArgumentNullException(nameof(targets));
        int currentDay = Runtime.State.Calendar.CurrentDay;
        int currentWeek = Runtime.State.Calendar.CurrentWeek;
        if (!CanResolvePublicAction)
        {
            throw new InvalidOperationException("This campaign cannot resolve another public decision today.");
        }

        if (!Runtime.Economy.CanAfford(plan.Cost))
        {
            var unaffordable = new CampaignActionResolution(false, Array.Empty<CauseRecord>());
            return new CampaignDecisionResolution(unaffordable, null, Array.Empty<PoliticalRelationshipChange>(), Array.Empty<PoliticalPromise>());
        }

        CampaignActionResolution actionResolution = CampaignActionResolver.Resolve(Runtime.State, Runtime.Economy, plan.CreateActionDefinition(), targets);
        if (!actionResolution.WasPaid)
        {
            return new CampaignDecisionResolution(actionResolution, null, Array.Empty<PoliticalRelationshipChange>(), Array.Empty<PoliticalPromise>());
        }

        var appliedChanges = new List<PoliticalRelationshipChange>();
        foreach (PoliticalRelationshipChange change in plan.RelationshipChanges)
        {
            if (!_relationships.TryGetValue(change.ActorId, out PoliticalRelationship relationship))
            {
                relationship = new PoliticalRelationship(change.ActorId);
                _relationships.Add(change.ActorId, relationship);
            }

            relationship.Apply(change.TrustDelta, change.AffinityDelta, change.ObligationDelta, change.GrievanceDelta);
            appliedChanges.Add(change);
            Runtime.State.RecordCause(new CauseRecord(currentDay, CauseCategory.PoliticalRelationship, plan.Id, change.ActorId, "relationship-change", change.TrustDelta + change.AffinityDelta + change.ObligationDelta - change.GrievanceDelta));
        }

        var createdPromises = new List<PoliticalPromise>();
        foreach (CampaignPromiseDefinition definition in plan.Promises)
        {
            var promise = new PoliticalPromise(definition.Id, definition.CounterpartId, definition.Description, currentDay);
            _promises.Add(promise);
            createdPromises.Add(promise);
            Runtime.State.RecordCause(new CauseRecord(currentDay, CauseCategory.PoliticalPromise, plan.Id, promise.CounterpartId, promise.Id, 1m));
        }

        var record = new CampaignDecisionRecord(
            plan.Id + "-day-" + currentDay,
            plan.Id,
            currentDay,
            plan.Activity,
            plan.ActorId,
            plan.Cost,
            plan.SelectedOptionIds);
        _decisionRecords.Add(record);
        _lastActionDay = currentDay;
        if (_activityWeek != currentWeek)
        {
            _activityWeek = currentWeek;
            _publicActivitiesThisWeek = 0;
        }

        _publicActivitiesThisWeek++;
        News.Publish(CampaignNewsFactory.CreateForActivity(currentDay, plan.CreateActionDefinition(), actionResolution));
        return new CampaignDecisionResolution(actionResolution, record, appliedChanges, createdPromises);
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

        _electionResult = ResolveConfiguredFirstRound();
        CompleteElectionPhase(_electionResult);
        return new CampaignDayAdvanceResult(monthlyClose, taskCauses, _electionResult, false);
    }

    private void CompleteElectionPhase(CampaignElectionResult electionResult)
    {
        CampaignPhase currentPhase = Runtime.PhaseMachine.Current;
        if (currentPhase == CampaignPhase.ElectionDay)
        {
            Runtime.PhaseMachine.MoveTo(CampaignPhase.Scrutiny);
            currentPhase = CampaignPhase.Scrutiny;
        }

        CampaignPhase expectedPhase = electionResult.Outcome.RequiresRunoff
            ? CampaignPhase.Runoff
            : CampaignPhase.Finished;
        if (currentPhase == CampaignPhase.Scrutiny)
        {
            Runtime.PhaseMachine.MoveTo(expectedPhase);
            return;
        }

        if (currentPhase != expectedPhase)
        {
            throw new InvalidOperationException("The campaign phase is incompatible with the restored election outcome.");
        }
    }

    private CampaignElectionResult ResolveConfiguredFirstRound()
    {
        return _electionRulesVersion == CampaignSaveData.LegacyElectionRulesVersion
            ? _electionService.ResolveFirstRoundLegacy(Runtime.State, Electorate)
            : _electionService.ResolveFirstRound(Runtime.State, Electorate);
    }

    private bool AreCampaignActionsLockedByElection
    {
        get
        {
            CampaignPhase phase = Runtime.PhaseMachine.Current;
            return _electionResult != null ||
                   Runtime.State.Calendar.IsElectionDay ||
                   phase == CampaignPhase.ElectionDay ||
                   phase == CampaignPhase.Scrutiny ||
                   phase == CampaignPhase.Runoff ||
                   phase == CampaignPhase.Finished;
        }
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
                ProfileId = member.ProfileId,
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
