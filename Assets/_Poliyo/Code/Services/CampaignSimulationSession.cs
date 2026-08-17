using System;
using System.Collections.Generic;
using Poliyo.AI;
using Poliyo.Simulation;

namespace Poliyo.Application
{
/// <summary>Pure application-level composition for the playable campaign loop. Unity only supplies content and presentation.</summary>
public sealed class CampaignSimulationSession
{
    private readonly CampaignElectionService _electionService;
    private readonly int _electionRulesVersion;
    private CampaignElectionResult _electionResult;
    private CampaignElectionResult _runoffResult;
    private IReadOnlyList<string> _runoffCandidateIds = Array.Empty<string>();
    private int _lastActionDay;
    private int _activityWeek;
    private int _publicActivitiesThisWeek;
    private int _lastWeeklyMeetingDay;
    private bool _teamSelectionCompleted;
    private bool _crisisResolved;
    private CampaignSliceClosureResult _sliceClosureResult;
    private readonly Dictionary<string, PoliticalRelationship> _relationships = new Dictionary<string, PoliticalRelationship>();
    private readonly List<PoliticalPromise> _promises = new List<PoliticalPromise>();
    private readonly List<CampaignDecisionRecord> _decisionRecords = new List<CampaignDecisionRecord>();
    private readonly List<CampaignDeferredConsequence> _deferredConsequences = new List<CampaignDeferredConsequence>();
    private readonly RivalPlanner _rivalPlanner = new RivalPlanner();

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
    public CampaignElectionResult RunoffResult => _runoffResult;
    public IReadOnlyList<string> ActiveElectionCandidateIds => _runoffCandidateIds.Count == 0 ? CampaignCandidateIds.All : _runoffCandidateIds;
    public bool HasPendingRunoff => _electionResult != null && _electionResult.Outcome.RequiresRunoff && _runoffResult == null;
    public bool PlayerReachedRunoff => _electionResult != null &&
                                       _electionResult.Outcome.RequiresRunoff &&
                                       IsRunoffCandidate(CampaignCandidateIds.Player);
    public bool CanResolveRunoff => HasPendingRunoff &&
                                    PlayerReachedRunoff &&
                                    Runtime.PhaseMachine.Current == CampaignPhase.Runoff;
    public bool IsPlayerCampaignFinished => _runoffResult != null ||
                                            (_electionResult != null && (!_electionResult.Outcome.RequiresRunoff || !PlayerReachedRunoff));
    public bool TeamSelectionCompleted => _teamSelectionCompleted;
    public IReadOnlyDictionary<string, PoliticalRelationship> Relationships => _relationships;
    public IReadOnlyList<PoliticalPromise> Promises => _promises;
    public IReadOnlyList<CampaignDecisionRecord> DecisionRecords => _decisionRecords;
    public IReadOnlyList<CampaignDeferredConsequence> DeferredConsequences => _deferredConsequences;
    public CampaignSliceClosureResult SliceClosureResult => _sliceClosureResult;
    public bool CrisisResolved => _crisisResolved;
    public bool CanAssignTeamTask => _teamSelectionCompleted && !AreCampaignActionsLockedByElection;
    public bool CanResolveWeeklyMeeting =>
        _teamSelectionCompleted &&
        Runtime.PhaseMachine.Current == CampaignPhase.WeeklyMeeting &&
        _lastWeeklyMeetingDay != Runtime.State.Calendar.CurrentDay;
    public bool CanResolveCrisis =>
        _teamSelectionCompleted &&
        !_crisisResolved &&
        Runtime.PhaseMachine.Current == CampaignPhase.Planning &&
        CanResolvePublicAction;
    public bool CanResolveSliceClosure =>
        _sliceClosureResult == null &&
        _crisisResolved &&
        Runtime.State.Calendar.CurrentDay >= CampaignSliceClosureResolver.RequiredDecisionDay &&
        (Runtime.PhaseMachine.Current == CampaignPhase.Planning || Runtime.PhaseMachine.Current == CampaignPhase.ElectoralFog) &&
        HasDecision(CampaignActivity.WeeklyMeeting) &&
        HasDecision(CampaignActivity.Rally) &&
        HasDecision(CampaignActivity.Interview);
    public bool CanResolvePublicAction
    {
        get
        {
            int currentDay = Runtime.State.Calendar.CurrentDay;
            int currentWeek = Runtime.State.Calendar.CurrentWeek;
            int activitiesThisWeek = _activityWeek == currentWeek ? _publicActivitiesThisWeek : 0;
            CampaignPhase phase = Runtime.PhaseMachine.Current;
            bool isActionPhase = phase == CampaignPhase.Planning || phase == CampaignPhase.DailyResolution || phase == CampaignPhase.ElectoralFog;
            return _teamSelectionCompleted && isActionPhase && !AreCampaignActionsLockedByElection && _lastActionDay != currentDay && activitiesThisWeek < 3;
        }
    }

    public CampaignSaveData CreateSaveData()
    {
        CampaignSaveData saveData = CampaignSaveMapper.Create(Runtime, Electorate);
        saveData.ElectionRulesVersion = _electionRulesVersion;
        saveData.LastActionDay = _lastActionDay;
        saveData.ActivityWeek = _activityWeek;
        saveData.PublicActivitiesThisWeek = _publicActivitiesThisWeek;
        saveData.LastWeeklyMeetingDay = _lastWeeklyMeetingDay;
        saveData.CrisisResolved = _crisisResolved;
        saveData.TeamSelectionCompleted = _teamSelectionCompleted;
        saveData.TeamMembers = CreateTeamSaveData();
        saveData.NewsItems = CampaignSaveMapper.CreateNewsData(News.Items);
        saveData.Relationships = CampaignSaveMapper.CreateRelationshipData(_relationships.Values);
        saveData.Promises = CampaignSaveMapper.CreatePromiseData(_promises);
        saveData.DecisionRecords = CampaignSaveMapper.CreateDecisionRecordData(_decisionRecords);
        saveData.DeferredConsequences = CampaignSaveMapper.CreateDeferredConsequenceData(_deferredConsequences);
        saveData.SliceClosureResult = CampaignSaveMapper.CreateSliceClosureData(_sliceClosureResult);
        saveData.CauseRecords = CampaignSaveMapper.CreateCauseData(Runtime.State.CauseRecords);
        saveData.ElectionResult = CampaignSaveMapper.CreateElectionResultData(_electionResult);
        saveData.RunoffResult = CampaignSaveMapper.CreateRunoffResultData(_runoffResult);
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

        if (saveData.LastWeeklyMeetingDay < 0 || saveData.LastWeeklyMeetingDay > Runtime.State.Calendar.CurrentDay)
        {
            throw new InvalidOperationException("The save contains an invalid last weekly meeting day.");
        }

        _lastActionDay = saveData.LastActionDay;
        _activityWeek = saveData.ActivityWeek;
        _publicActivitiesThisWeek = saveData.PublicActivitiesThisWeek;
        _lastWeeklyMeetingDay = saveData.LastWeeklyMeetingDay;
        _crisisResolved = saveData.CrisisResolved;
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

    public void RestoreDeferredConsequences(CampaignSaveData saveData)
    {
        _deferredConsequences.Clear();
        _deferredConsequences.AddRange(CampaignSaveMapper.RestoreDeferredConsequences(saveData));
    }

    public void RestoreSliceClosure(CampaignSaveData saveData)
    {
        CampaignSliceClosureResult restoredClosure = CampaignSaveMapper.RestoreSliceClosure(saveData);
        _sliceClosureResult = restoredClosure;
        if (restoredClosure != null && Runtime.PhaseMachine.Current != CampaignPhase.SliceClosure)
        {
            Runtime.CompleteSliceClosure();
        }
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
        CampaignElectionResult restoredRunoff = CampaignSaveMapper.RestoreRunoffResult(saveData);
        if (saveData.ElectionRulesVersion != _electionRulesVersion)
        {
            throw new InvalidOperationException("The session and save use different election rules versions.");
        }

        if (_electionResult == null && restoredResult != null)
        {
            _electionResult = restoredResult;
            ConfigureRunoffCandidates(_electionResult);
            if (restoredRunoff == null)
            {
                CompleteElectionPhase(_electionResult);
            }
        }

        if (_runoffResult == null && restoredRunoff != null)
        {
            if (_electionResult == null || !_electionResult.Outcome.RequiresRunoff)
            {
                throw new InvalidOperationException("The save contains a runoff without a pending first-round runoff.");
            }

            ConfigureRunoffCandidates(_electionResult);
            _runoffResult = restoredRunoff;
            CompleteRunoffPhase();
            return;
        }

        if (_electionResult != null)
        {
            return;
        }

        if (Runtime.State.Calendar.IsElectionDay && Electorate.Count > 0)
        {
            _electionResult = ResolveConfiguredFirstRound();
            ConfigureRunoffCandidates(_electionResult);
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
        if (!CanResolvePublicAction)
        {
            throw new InvalidOperationException("Public campaign activities cannot be resolved during the current campaign phase or after election day.");
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

        if (plan.Activity == CampaignActivity.WeeklyMeeting)
        {
            return ResolveWeeklyMeeting(plan, targets);
        }

        if (plan.Activity == CampaignActivity.Crisis && !CanResolveCrisis)
        {
            throw new InvalidOperationException("The crisis is not available in the current campaign phase.");
        }

        return ResolveDecisionInternal(plan, targets, false);
    }

    public CampaignSliceClosureResult ResolveSliceClosure()
    {
        if (!CanResolveSliceClosure)
        {
            throw new InvalidOperationException("The fourteen-day slice cannot close until its meeting, rally, interview and crisis decisions are resolved.");
        }

        _sliceClosureResult = CampaignSliceClosureResolver.Resolve(Runtime.State, Electorate, _decisionRecords);
        Runtime.CompleteSliceClosure();
        News.Publish(CampaignNewsFactory.CreateForSliceClosure(_sliceClosureResult));
        return _sliceClosureResult;
    }

    private CampaignDecisionResolution ResolveWeeklyMeeting(CampaignDecisionPlan plan, IEnumerable<MicroElector> targets)
    {
        if (!CanResolveWeeklyMeeting)
        {
            throw new InvalidOperationException("The weekly campaign meeting is not waiting for a resolution.");
        }

        CampaignDecisionResolution resolution = ResolveDecisionInternal(plan, targets, true);
        if (resolution.WasResolved)
        {
            _lastWeeklyMeetingDay = Runtime.State.Calendar.CurrentDay;
            Runtime.CompleteWeeklyMeeting();
            ResolveRivalWeeklyPlan();
        }

        return resolution;
    }

    private CampaignDecisionResolution ResolveDecisionInternal(
        CampaignDecisionPlan plan,
        IEnumerable<MicroElector> targets,
        bool isWeeklyMeeting)
    {
        int currentDay = Runtime.State.Calendar.CurrentDay;
        int currentWeek = Runtime.State.Calendar.CurrentWeek;
        if (isWeeklyMeeting)
        {
            if (!CanResolveWeeklyMeeting)
            {
                throw new InvalidOperationException("The weekly campaign meeting is not waiting for a resolution.");
            }
        }
        else if (!CanResolvePublicAction)
        {
            throw new InvalidOperationException("This campaign cannot resolve another public decision today.");
        }

        var targetList = new List<MicroElector>();
        foreach (MicroElector target in targets)
        {
            if (target == null) throw new ArgumentException("A decision target is required.", nameof(targets));
            targetList.Add(target);
        }

        if (!Runtime.Economy.CanAfford(plan.Cost))
        {
            var unaffordable = new CampaignActionResolution(false, Array.Empty<CauseRecord>());
            return new CampaignDecisionResolution(unaffordable, null, Array.Empty<PoliticalRelationshipChange>(), Array.Empty<PoliticalPromise>());
        }

        CampaignActionResolution actionResolution = CampaignActionResolver.Resolve(Runtime.State, Runtime.Economy, plan.CreateActionDefinition(), targetList);
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

        foreach (CampaignDeferredConsequenceDefinition definition in plan.DeferredConsequences)
        {
            int dueDay = Math.Min(CampaignCalendar.TotalCampaignDays, currentDay + definition.DaysAfterDecision);
            _deferredConsequences.Add(new CampaignDeferredConsequence(
                definition.Id + "-day-" + currentDay,
                dueDay,
                definition.SourceId,
                definition.TargetId,
                definition.EffectId,
                definition.CandidateId,
                definition.Metric,
                definition.Magnitude,
                definition.Category));
        }

        var record = new CampaignDecisionRecord(
            plan.Id + "-day-" + currentDay,
            plan.Id,
            currentDay,
            plan.Activity,
            plan.ActorId,
            plan.Cost,
            plan.SelectedOptionIds,
            plan.ContextId);
        _decisionRecords.Add(record);
        if (!isWeeklyMeeting)
        {
            _lastActionDay = currentDay;
            if (_activityWeek != currentWeek)
            {
                _activityWeek = currentWeek;
                _publicActivitiesThisWeek = 0;
            }

            _publicActivitiesThisWeek++;
        }

        if (plan.Activity == CampaignActivity.Crisis)
        {
            _crisisResolved = true;
        }

        News.Publish(CampaignNewsFactory.CreateForActivity(currentDay, plan.CreateActionDefinition(), actionResolution));
        if (plan.RivalImpactMagnitude != 0m)
        {
            ResolveAuthoredRivalResponse(plan, targetList);
        }

        return new CampaignDecisionResolution(actionResolution, record, appliedChanges, createdPromises);
    }

    public CampaignDayAdvanceResult AdvanceDay()
    {
        if (HasPendingRunoff)
        {
            if (!PlayerReachedRunoff)
            {
                throw new InvalidOperationException("Tu candidatura no llegó al balotaje; la campaña terminó para vos.");
            }

            return ResolveRunoffAtCurrentDay();
        }

        if (Runtime.State.Calendar.IsElectionDay && _electionResult == null)
        {
            return ResolveElectionAtCurrentDay(
                null,
                Array.Empty<CauseRecord>(),
                Array.Empty<CauseRecord>());
        }

        if (Runtime.PhaseMachine.Current == CampaignPhase.WeeklyMeeting)
        {
            throw new InvalidOperationException("Resolve the weekly campaign meeting before advancing the day.");
        }

        IReadOnlyList<CauseRecord> taskCauses = CampaignDelegatedTaskResolver.Resolve(Team, Runtime.State, Runtime.Economy, Electorate);
        MonthlyCloseResult monthlyClose = Runtime.AdvanceDay();
        News.AdvanceDay();
        IReadOnlyList<CauseRecord> deferredConsequenceCauses = ResolveDeferredConsequences();

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
            return new CampaignDayAdvanceResult(monthlyClose, taskCauses, deferredConsequenceCauses, _electionResult, false);
        }

        return ResolveElectionAtCurrentDay(monthlyClose, taskCauses, deferredConsequenceCauses);
    }

    private CampaignDayAdvanceResult ResolveElectionAtCurrentDay(
        MonthlyCloseResult monthlyClose,
        IReadOnlyList<CauseRecord> taskCauses,
        IReadOnlyList<CauseRecord> deferredConsequenceCauses)
    {
        if (Electorate.Count == 0)
        {
            return new CampaignDayAdvanceResult(monthlyClose, taskCauses, deferredConsequenceCauses, null, true);
        }

        if (Runtime.PhaseMachine.Current == CampaignPhase.ElectoralBan)
        {
            Runtime.PhaseMachine.MoveTo(CampaignPhase.ElectionDay);
        }

        _electionResult = ResolveConfiguredFirstRound();
        ConfigureRunoffCandidates(_electionResult);
        CompleteElectionPhase(_electionResult);
        return new CampaignDayAdvanceResult(monthlyClose, taskCauses, deferredConsequenceCauses, _electionResult, false);
    }

    private CampaignDayAdvanceResult ResolveRunoffAtCurrentDay()
    {
        if (!CanResolveRunoff)
        {
            throw new InvalidOperationException("El balotaje no está disponible para esta campaña.");
        }

        _runoffResult = _electionService.ResolveRunoff(Runtime.State, Electorate, _runoffCandidateIds);
        CompleteRunoffPhase();
        return new CampaignDayAdvanceResult(
            null,
            Array.Empty<CauseRecord>(),
            Array.Empty<CauseRecord>(),
            _runoffResult,
            false);
    }

    private IReadOnlyList<CauseRecord> ResolveDeferredConsequences()
    {
        int currentDay = Runtime.State.Calendar.CurrentDay;
        var dueConsequences = new List<CampaignDeferredConsequence>();
        foreach (CampaignDeferredConsequence consequence in _deferredConsequences)
        {
            if (consequence.DueDay <= currentDay)
            {
                dueConsequences.Add(consequence);
            }
        }

        var causes = new List<CauseRecord>();
        foreach (CampaignDeferredConsequence consequence in dueConsequences)
        {
            IReadOnlyList<MicroElector> targets = SelectConsequenceTargets(consequence.TargetId);
            if (targets.Count == 0)
            {
                throw new InvalidOperationException("A deferred consequence references a target with no electorate data.");
            }

            var impact = new ElectoralImpact(
                consequence.SourceId,
                consequence.CandidateId,
                consequence.Metric,
                consequence.Magnitude,
                1m,
                1m,
                1m,
                1m,
                1m,
                1m);
            IReadOnlyList<CauseRecord> effectCauses = ElectoralImpactApplier.Apply(Runtime.State, impact, targets, consequence.Category);
            foreach (CauseRecord cause in effectCauses)
            {
                causes.Add(cause);
            }

            News.Publish(CampaignNewsFactory.CreateForDeferredConsequence(currentDay, consequence));
            _deferredConsequences.Remove(consequence);
        }

        return causes.AsReadOnly();
    }

    private IReadOnlyList<MicroElector> SelectConsequenceTargets(string targetId)
    {
        if (string.Equals(targetId, "nacional", StringComparison.OrdinalIgnoreCase))
        {
            return Electorate;
        }

        var targets = new List<MicroElector>();
        foreach (MicroElector elector in Electorate)
        {
            if (elector.Id == targetId || elector.LocalityId == targetId)
            {
                targets.Add(elector);
            }
        }

        return targets;
    }

    private void ResolveAuthoredRivalResponse(CampaignDecisionPlan plan, IReadOnlyList<MicroElector> targets)
    {
        var impact = new ElectoralImpact(
            string.IsNullOrWhiteSpace(plan.RivalResponseId) ? plan.Id + "-rival-response" : plan.RivalResponseId,
            CampaignCandidateIds.Player,
            ElectoralMetric.Rejection,
            plan.RivalImpactMagnitude,
            1m,
            1m,
            1m,
            1m,
            1m,
            1m);
        IReadOnlyList<CauseRecord> causes = ElectoralImpactApplier.Apply(Runtime.State, impact, targets, CauseCategory.RivalAction);
        News.Publish(CampaignNewsFactory.CreateForRivalAction(Runtime.State.Calendar.CurrentDay, plan.Activity, new CampaignActionResolution(true, causes)));
    }

    private void ResolveRivalWeeklyPlan()
    {
        RivalKnownSituation situation = BuildRivalKnownSituation();
        RivalPlan plan = _rivalPlanner.CreateWeeklyPlan(
            RivalStyle.Opportunistic,
            situation,
            Runtime.State.Seed.CreateRandom("rival-week/" + Runtime.State.Calendar.CurrentWeek));

        ElectoralMetric metric;
        decimal magnitude;
        switch (plan.DelegatedTask)
        {
            case DelegatedTaskType.MediaStatement:
                metric = ElectoralMetric.VotingIntention;
                magnitude = -0.8m;
                break;
            case DelegatedTaskType.TerritorialCampaign:
                metric = ElectoralMetric.VotingIntention;
                magnitude = -0.7m;
                break;
            case DelegatedTaskType.CrisisAnalysis:
                metric = ElectoralMetric.Rejection;
                magnitude = 1.0m;
                break;
            case DelegatedTaskType.Investigation:
                metric = ElectoralMetric.Trust;
                magnitude = -0.7m;
                break;
            case DelegatedTaskType.AffiliateRecruitment:
                metric = ElectoralMetric.Participation;
                magnitude = -0.6m;
                break;
            default:
                metric = ElectoralMetric.Rejection;
                magnitude = 0.5m;
                break;
        }

        var impact = new ElectoralImpact(
            "rival-week-" + Runtime.State.Calendar.CurrentWeek,
            CampaignCandidateIds.Player,
            metric,
            magnitude,
            1m,
            1m,
            1m,
            1m,
            1m,
            1m);
        IReadOnlyList<CauseRecord> causes = ElectoralImpactApplier.Apply(Runtime.State, impact, Electorate, CauseCategory.RivalAction);
        News.Publish(CampaignNewsFactory.CreateForRivalAction(Runtime.State.Calendar.CurrentDay, plan.PrimaryActivity, new CampaignActionResolution(true, causes)));
    }

    private RivalKnownSituation BuildRivalKnownSituation()
    {
        decimal rejection = 0m;
        int count = 0;
        foreach (MicroElector elector in Electorate)
        {
            rejection += elector.GetCandidate(CampaignCandidateIds.Player).Rejection / 100m;
            count++;
        }

        decimal territorialPressure = count == 0 ? 0m : Math.Min(1m, rejection / count);
        decimal mediaOpportunity = Math.Min(1m, 0.25m + News.Items.Count * 0.03m);
        decimal knownThreat = Math.Min(1m, territorialPressure * 0.7m + (Runtime.Economy.Funds < 500m ? 0.25m : 0m));
        return new RivalKnownSituation(Runtime.Economy.Funds, territorialPressure, mediaOpportunity, knownThreat);
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

    private void CompleteRunoffPhase()
    {
        CampaignPhase currentPhase = Runtime.PhaseMachine.Current;
        if (currentPhase == CampaignPhase.Runoff)
        {
            Runtime.PhaseMachine.MoveTo(CampaignPhase.Finished);
            return;
        }

        if (currentPhase != CampaignPhase.Finished)
        {
            throw new InvalidOperationException("The campaign phase is incompatible with the restored runoff outcome.");
        }
    }

    private void ConfigureRunoffCandidates(CampaignElectionResult electionResult)
    {
        if (electionResult == null || !electionResult.Outcome.RequiresRunoff)
        {
            _runoffCandidateIds = Array.Empty<string>();
            return;
        }

        _runoffCandidateIds = new List<string>
        {
            electionResult.Outcome.RunoffFirstId,
            electionResult.Outcome.RunoffSecondId,
        }.AsReadOnly();
    }

    private bool IsRunoffCandidate(string candidateId)
    {
        foreach (string finalistId in _runoffCandidateIds)
        {
            if (finalistId == candidateId) return true;
        }

        return false;
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
                   _sliceClosureResult != null ||
                   Runtime.State.Calendar.IsElectionDay ||
                   phase == CampaignPhase.ElectionDay ||
                   phase == CampaignPhase.Scrutiny ||
                   phase == CampaignPhase.Runoff ||
                   phase == CampaignPhase.SliceClosure ||
                   phase == CampaignPhase.Finished;
        }
    }

    private bool HasDecision(CampaignActivity activity)
    {
        foreach (CampaignDecisionRecord record in _decisionRecords)
        {
            if (record.Activity == activity)
            {
                return true;
            }
        }

        return false;
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
