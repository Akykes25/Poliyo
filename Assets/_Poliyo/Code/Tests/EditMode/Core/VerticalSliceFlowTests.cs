using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Poliyo.Application;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class VerticalSliceFlowTests
{
    [Test]
    public void WeeklyMeeting_ChangesPhaseAndProducesRivalFeedback()
    {
        CampaignSimulationSession session = CreateSession(requireWeeklyMeetings: true);

        CampaignDecisionPlan plan = CreatePlan(
            "weekly-meeting",
            CampaignActivity.WeeklyMeeting,
            ElectoralMetric.Trust,
            1.5m,
            rivalImpactMagnitude: 0.5m);

        CampaignDecisionResolution result = session.ResolveDecision(plan, session.Electorate);

        Assert.That(result.WasResolved, Is.True);
        Assert.That(session.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.Planning));
        Assert.That(session.DecisionRecords, Has.Count.EqualTo(1));
        Assert.That(session.DecisionRecords[0].Activity, Is.EqualTo(CampaignActivity.WeeklyMeeting));
        Assert.That(session.Runtime.State.CauseRecords.Any(cause => cause.Category == CauseCategory.RivalAction), Is.True);
        Assert.That(session.News.Items.Any(item => item.TopicId == "rival-response"), Is.True);
    }

    [Test]
    public void DelegatedTask_ChangesElectoralStateAndReleasesMember()
    {
        var member = new CampaignTeamMember("operator", CampaignTeamRoleIds.OperationsChief);
        var team = new CampaignTeam(new[] { member });
        CampaignSimulationSession session = CreateSession(team, requireWeeklyMeetings: false);
        decimal participationBefore = session.Electorate[0].Participation;

        session.AssignTeamTask(member.Id, DelegatedTaskType.AffiliateRecruitment, "nacional");
        CampaignDayAdvanceResult result = session.AdvanceDay();

        Assert.That(member.CurrentAssignment, Is.Null);
        Assert.That(session.Electorate[0].Participation, Is.GreaterThan(participationBefore));
        Assert.That(result.CompletedTaskCauses, Is.Not.Empty);
        Assert.That(result.CompletedTaskCauses.Any(cause => cause.Category == CauseCategory.DelegatedTask), Is.True);
        Assert.That(session.News.Items.Any(item => item.TopicId == "delegated-task"), Is.True);
    }

    [Test]
    public void DeferredCrisis_SurvivesSaveAndResolvesOnDueDay()
    {
        CampaignSimulationSession source = CreateSession(requireWeeklyMeetings: true);
        source.ResolveDecision(CreatePlan("weekly-meeting", CampaignActivity.WeeklyMeeting, ElectoralMetric.Trust, 1m), source.Electorate);

        var deferred = new CampaignDeferredConsequenceDefinition(
            "crisis-follow-up",
            "crisis-puente",
            "nacional",
            "crisis-confirmed",
            CampaignCandidateIds.Player,
            ElectoralMetric.Rejection,
            2m,
            2,
            CauseCategory.Event);
        source.ResolveDecision(
            CreatePlan("crisis-puente", CampaignActivity.Crisis, ElectoralMetric.Trust, 1m, deferredConsequences: new[] { deferred }),
            source.Electorate);

        CampaignSaveData saveData = source.CreateSaveData();
        CampaignRuntime restoredRuntime = CampaignRuntime.Restore(saveData, new List<MonthlyCommitment>());
        CampaignSimulationSession restored = new CampaignSimulationSession(
            restoredRuntime,
            CampaignSaveMapper.RestoreElectorate(saveData),
            new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory(),
            saveData.ElectionRulesVersion,
            saveData.TeamSelectionCompleted);
        restored.RestoreActivityLimits(saveData);
        restored.RestoreNews(saveData);
        restored.RestorePoliticalMemory(saveData);
        restored.RestoreDeferredConsequences(saveData);

        Assert.That(restored.DeferredConsequences, Has.Count.EqualTo(1));
        decimal rejectionBefore = restored.Electorate[0].GetCandidate(CampaignCandidateIds.Player).Rejection;
        restored.AdvanceDay();
        CampaignDayAdvanceResult dueBeforeSecondDay = restored.AdvanceDay();

        Assert.That(restored.DeferredConsequences, Is.Empty);
        Assert.That(restored.Electorate[0].GetCandidate(CampaignCandidateIds.Player).Rejection, Is.GreaterThan(rejectionBefore));
        Assert.That(dueBeforeSecondDay.DeferredConsequenceCauses, Is.Not.Empty);
        Assert.That(restored.News.Items.Any(item => item.TopicId == "deferred-consequence"), Is.True);
    }

    [Test]
    public void FourteenDaySlice_ClosesWithElectionReadoutAndCausalFactors()
    {
        var member = new CampaignTeamMember("operator", CampaignTeamRoleIds.OperationsChief);
        CampaignSimulationSession session = CreateSession(
            new CampaignTeam(new[] { member }),
            requireWeeklyMeetings: true);
        session.ResolveDecision(CreatePlan("weekly-meeting-day-1", CampaignActivity.WeeklyMeeting, ElectoralMetric.Trust, 1m), session.Electorate);
        session.AssignTeamTask(member.Id, DelegatedTaskType.AffiliateRecruitment, "nacional");

        session.ResolveDecision(
            CreatePlan("rally-locality-a", CampaignActivity.Rally, ElectoralMetric.VotingIntention, 2m, contextId: "locality-a"),
            new[] { session.Electorate[0] });
        CampaignDayAdvanceResult dayOne = session.AdvanceDay();
        Assert.That(dayOne.CompletedTaskCauses, Is.Not.Empty);
        Assert.That(dayOne.CompletedTaskCauses.Any(cause => cause.Category == CauseCategory.DelegatedTask), Is.True);
        Assert.That(session.News.Items.Any(item => item.TopicId == "delegated-task"), Is.True);
        Assert.That(member.CurrentAssignment, Is.Null);
        session.ResolveDecision(CreatePlan("interview-day-2", CampaignActivity.Interview, ElectoralMetric.Trust, 1m), session.Electorate);

        while (session.Runtime.State.Calendar.CurrentDay < 5)
        {
            session.AdvanceDay();
        }

        session.ResolveDecision(CreatePlan("crisis-puente", CampaignActivity.Crisis, ElectoralMetric.Rejection, -1m), session.Electorate);
        while (session.Runtime.State.Calendar.CurrentDay < 8)
        {
            session.AdvanceDay();
        }

        Assert.That(session.CanResolveWeeklyMeeting, Is.True);
        session.ResolveDecision(CreatePlan("weekly-meeting-day-8", CampaignActivity.WeeklyMeeting, ElectoralMetric.Trust, 1m), session.Electorate);
        while (session.Runtime.State.Calendar.CurrentDay < CampaignSliceClosureResolver.RequiredDecisionDay)
        {
            session.AdvanceDay();
        }

        Assert.That(session.CanResolveSliceClosure, Is.True);
        CampaignSliceClosureResult closure = session.ResolveSliceClosure();

        Assert.That(closure.Day, Is.EqualTo(CampaignSliceClosureResolver.RequiredDecisionDay));
        Assert.That(closure.DecisionCount, Is.GreaterThanOrEqualTo(4));
        Assert.That(closure.Tally.ValidVotes, Is.GreaterThan(0m));
        Assert.That(closure.DecisiveFactors, Is.Not.Empty);
        Assert.That(session.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.SliceClosure));
        Assert.That(session.News.Items.Any(item => item.TopicId == "slice-closure"), Is.True);

        CampaignSaveData saveData = session.CreateSaveData();
        CampaignRuntime restoredRuntime = CampaignRuntime.Restore(saveData, new List<MonthlyCommitment>());
        CampaignSimulationSession restored = new CampaignSimulationSession(
            restoredRuntime,
            CampaignSaveMapper.RestoreElectorate(saveData),
            new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory(),
            saveData.ElectionRulesVersion,
            saveData.TeamSelectionCompleted);
        restored.RestoreActivityLimits(saveData);
        restored.RestoreNews(saveData);
        restored.RestorePoliticalMemory(saveData);
        restored.RestoreDeferredConsequences(saveData);
        restored.RestoreSliceClosure(saveData);

        Assert.That(restored.SliceClosureResult, Is.Not.Null);
        Assert.That(restored.SliceClosureResult.DecisionCount, Is.EqualTo(closure.DecisionCount));
        Assert.That(restored.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.SliceClosure));
        Assert.That(restored.News.Items.Any(item => item.TopicId == "slice-closure"), Is.True);
    }

    private static CampaignSimulationSession CreateSession(
        CampaignTeam team = null,
        bool requireWeeklyMeetings = false)
    {
        var runtime = new CampaignRuntime(
            new CampaignSeed(73UL),
            1000m,
            new List<MonthlyCommitment>(),
            requireWeeklyMeetings);
        runtime.StartCampaign();
        return new CampaignSimulationSession(
            runtime,
            CreateElectorate(),
            team ?? new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory(),
            teamSelectionCompleted: true);
    }

    private static IReadOnlyList<MicroElector> CreateElectorate()
    {
        return new List<MicroElector>
        {
            CreateElector("elector-a", "locality-a", 100m, 66m),
            CreateElector("elector-b", "locality-b", 180m, 58m),
        };
    }

    private static MicroElector CreateElector(string id, string localityId, decimal weight, decimal participation)
    {
        return new MicroElector(id, localityId, weight, participation, new[]
        {
            new CandidateElectoralState(CampaignCandidateIds.Player, 30m, 30m, 4m),
            new CandidateElectoralState(CampaignCandidateIds.Liberales, 24m, 24m, 8m),
            new CandidateElectoralState(CampaignCandidateIds.Contr, 20m, 20m, 12m),
            new CandidateElectoralState(CampaignCandidateIds.Zurditos, 16m, 16m, 14m),
            new CandidateElectoralState(CampaignCandidateIds.Federales, 10m, 10m, 16m),
        });
    }

    private static CampaignDecisionPlan CreatePlan(
        string id,
        CampaignActivity activity,
        ElectoralMetric metric,
        decimal magnitude,
        decimal rivalImpactMagnitude = 0m,
        IEnumerable<CampaignDeferredConsequenceDefinition> deferredConsequences = null,
        string contextId = "nacional")
    {
        return new CampaignDecisionPlan(
            id,
            activity,
            activity == CampaignActivity.WeeklyMeeting ? "mesa-campana" : id,
            0m,
            new[]
            {
                new ElectoralImpact(id, CampaignCandidateIds.Player, metric, magnitude, 1m, 1m, 1m, 1m, 1m, 1m),
            },
            new[] { "decision" },
            deferredConsequences: deferredConsequences,
            rivalImpactMagnitude: rivalImpactMagnitude,
            rivalResponseId: rivalImpactMagnitude == 0m ? null : id + "-rival",
            contextId: contextId);
    }
}
}
