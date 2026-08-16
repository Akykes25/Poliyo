using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Poliyo.Application;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignSimulationSessionTests
{
    [Test]
    public void ResolveAction_WhenActivityWasAlreadyUsedToday_RejectsSecondAction()
    {
        var runtime = new CampaignRuntime(new CampaignSeed(3UL), 100m, new List<MonthlyCommitment>());
        runtime.StartCampaign();
        var electorate = new List<MicroElector>
        {
            new MicroElector("elector", "locality", 100m, 100m, new[]
            {
                new CandidateElectoralState(CampaignCandidateIds.Player, 50m, 50m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Liberales, 50m, 50m, 0m),
            }),
        };
        var session = new CampaignSimulationSession(runtime, electorate, new CampaignTeam(new CampaignTeamMember[0]), new NewsMemory());
        var action = new CampaignActionDefinition(
            "rally",
            CampaignActivity.Rally,
            0m,
            new ElectoralImpact("rally", CampaignCandidateIds.Player, ElectoralMetric.Trust, 1m, 1m, 1m, 1m, 1m, 1m, 1m));

        session.ResolveAction(action, electorate);

        Assert.That(() => session.ResolveAction(action, electorate), Throws.TypeOf<System.InvalidOperationException>());
    }

    [Test]
    public void ResolveDecision_AfterTeamSelectionPersistsRelationshipPromiseAndRecord()
    {
        CampaignSimulationSession session = CreateDecisionSession(250m);
        SelectCompleteTeam(session);

        var plan = new CampaignDecisionPlan(
            "negotiation-rival-day-1",
            CampaignActivity.Negotiation,
            "rival-contr",
            40m,
            new[]
            {
                new ElectoralImpact("negotiation-rival-day-1", CampaignCandidateIds.Player, ElectoralMetric.Trust, 2m, 1m, 1m, 1m, 1m, 1m, 1m),
            },
            new[] { "offer-information" },
            new[] { new PoliticalRelationshipChange("rival-contr", 5m, 3m, 2m, -1m) },
            new[] { new CampaignPromiseDefinition("promise-territory", "rival-contr", "Compartir una señal territorial verificable.") });

        CampaignDecisionResolution result = session.ResolveDecision(plan, session.Electorate);

        Assert.That(result.WasResolved, Is.True);
        Assert.That(session.Runtime.Economy.Funds, Is.EqualTo(210m));
        Assert.That(session.Relationships["rival-contr"].Trust, Is.EqualTo(5m));
        Assert.That(session.Promises, Has.Count.EqualTo(1));
        Assert.That(session.DecisionRecords, Has.Count.EqualTo(1));
        Assert.That(session.Runtime.State.CauseRecords.Any(cause => cause.Category == CauseCategory.PoliticalPromise), Is.True);
    }

    [Test]
    public void ResolveDecision_WhenUnaffordableIsAtomicAndDoesNotCreatePoliticalMemory()
    {
        CampaignSimulationSession session = CreateDecisionSession(10m);
        SelectCompleteTeam(session);
        var plan = new CampaignDecisionPlan(
            "negotiation-rival-day-1",
            CampaignActivity.Negotiation,
            "rival-contr",
            40m,
            new[]
            {
                new ElectoralImpact("negotiation-rival-day-1", CampaignCandidateIds.Player, ElectoralMetric.Trust, 2m, 1m, 1m, 1m, 1m, 1m, 1m),
            },
            new[] { "offer-information" },
            new[] { new PoliticalRelationshipChange("rival-contr", 5m, 3m, 2m, -1m) },
            new[] { new CampaignPromiseDefinition("promise-territory", "rival-contr", "Compartir una señal territorial verificable.") });

        CampaignDecisionResolution result = session.ResolveDecision(plan, session.Electorate);

        Assert.That(result.WasResolved, Is.False);
        Assert.That(session.Runtime.Economy.Funds, Is.EqualTo(10m));
        Assert.That(session.Relationships, Is.Empty);
        Assert.That(session.Promises, Is.Empty);
        Assert.That(session.DecisionRecords, Is.Empty);
        Assert.That(session.News.Items, Is.Empty);
        Assert.That(session.CanResolvePublicAction, Is.True);
    }

    [Test]
    public void SaveRoundTrip_RestoresTeamSelectionAndDecisionMemory()
    {
        CampaignSimulationSession source = CreateDecisionSession(250m);
        SelectCompleteTeam(source);
        var plan = new CampaignDecisionPlan(
            "negotiation-rival-day-1",
            CampaignActivity.Negotiation,
            "rival-contr",
            40m,
            new[]
            {
                new ElectoralImpact("negotiation-rival-day-1", CampaignCandidateIds.Player, ElectoralMetric.Trust, 2m, 1m, 1m, 1m, 1m, 1m, 1m),
            },
            new[] { "offer-information" },
            new[] { new PoliticalRelationshipChange("rival-contr", 5m, 3m, 2m, -1m) },
            new[] { new CampaignPromiseDefinition("promise-territory", "rival-contr", "Compartir una señal territorial verificable.") });
        source.ResolveDecision(plan, source.Electorate);

        CampaignSaveData saveData = source.CreateSaveData();
        CampaignRuntime restoredRuntime = CampaignRuntime.Restore(saveData, new List<MonthlyCommitment>());
        var restored = new CampaignSimulationSession(
            restoredRuntime,
            CampaignSaveMapper.RestoreElectorate(saveData),
            new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory(),
            saveData.ElectionRulesVersion,
            saveData.TeamSelectionCompleted);
        restored.RestoreActivityLimits(saveData);
        restored.RestoreTeam(saveData);
        restored.RestoreNews(saveData);
        restored.RestorePoliticalMemory(saveData);

        Assert.That(saveData.SchemaVersion, Is.EqualTo(CampaignSaveData.CurrentSchemaVersion));
        Assert.That(restored.TeamSelectionCompleted, Is.True);
        Assert.That(restored.Team.IsSelectionComplete, Is.True);
        Assert.That(restored.Relationships["rival-contr"].Affinity, Is.EqualTo(3m));
        Assert.That(restored.Promises, Has.Count.EqualTo(1));
        Assert.That(restored.DecisionRecords, Has.Count.EqualTo(1));
        Assert.That(restored.Runtime.State.CauseRecords, Has.Count.EqualTo(source.Runtime.State.CauseRecords.Count));
    }

    [Test]
    public void SelectTeamMember_AfterFinalizationRejectsRosterChanges()
    {
        CampaignSimulationSession session = CreateDecisionSession(250m);
        SelectCompleteTeam(session);

        Assert.That(
            () => session.SelectTeamMember(CampaignTeamRoleIds.VicePresident, "replacement-profile"),
            Throws.TypeOf<System.InvalidOperationException>());
        Assert.That(session.Team.TryGetMemberByRole(CampaignTeamRoleIds.VicePresident, out CampaignTeamMember member), Is.True);
        Assert.That(member.ProfileId, Is.EqualTo("profile-0"));
    }

    [Test]
    public void AdvanceDay_OnElectionDayResolvesElectionExactlyOnce()
    {
        CampaignSimulationSession session = CreateSessionOnDayBeforeElection();

        CampaignDayAdvanceResult result = session.AdvanceDay();

        Assert.That(result.ElectionResult, Is.Not.Null);
        Assert.That(session.ElectionResult, Is.SameAs(result.ElectionResult));
        Assert.That(session.Runtime.State.CauseRecords, Has.Count.EqualTo(CampaignCandidateIds.All.Count));
        Assert.That(session.CanResolvePublicAction, Is.False);
        Assert.That(session.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.Finished));
    }

    [Test]
    public void AdvanceDay_RepeatedFromCampaignStart_ReachesFogAndResolvesElectionAtDaySixty()
    {
        CampaignSimulationSession session = CreateSessionAtCampaignStart();

        while (session.Runtime.State.Calendar.CurrentDay < CampaignCalendar.FogStartDay)
        {
            session.AdvanceDay();
        }

        Assert.That(session.Runtime.State.Calendar.CurrentDay, Is.EqualTo(CampaignCalendar.FogStartDay));
        Assert.That(session.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.ElectoralFog));
        Assert.That(session.ElectionResult, Is.Null);

        while (session.Runtime.State.Calendar.CurrentDay < CampaignCalendar.TotalCampaignDays - 1)
        {
            session.AdvanceDay();
        }

        Assert.That(session.Runtime.State.Calendar.CurrentDay, Is.EqualTo(CampaignCalendar.TotalCampaignDays - 1));
        Assert.That(session.ElectionResult, Is.Null);

        CampaignDayAdvanceResult electionDay = session.AdvanceDay();

        Assert.That(session.Runtime.State.Calendar.CurrentDay, Is.EqualTo(CampaignCalendar.TotalCampaignDays));
        Assert.That(electionDay.ElectionResult, Is.Not.Null);
        Assert.That(session.ElectionResult, Is.SameAs(electionDay.ElectionResult));
        Assert.That(session.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.Finished));
    }

    [Test]
    public void AdvanceDay_WhenFirstRoundHasNoWinner_MovesCampaignToRunoff()
    {
        CampaignSimulationSession session = CreateSessionOnDayBeforeElection(requiresRunoff: true);

        CampaignDayAdvanceResult result = session.AdvanceDay();

        Assert.That(result.ElectionResult.Outcome.RequiresRunoff, Is.True);
        Assert.That(session.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.Runoff));
    }

    [Test]
    public void ResolveAction_OnElectionDayWithoutResult_RejectsAtomically()
    {
        CampaignSimulationSession session = CreateSessionOnDayBeforeElection();
        session.Runtime.AdvanceDay();
        CandidateElectoralState player = session.Electorate[0].GetCandidate(CampaignCandidateIds.Player);
        var action = new CampaignActionDefinition(
            "rally",
            CampaignActivity.Rally,
            10m,
            new ElectoralImpact("rally", CampaignCandidateIds.Player, ElectoralMetric.Trust, 5m, 1m, 1m, 1m, 1m, 1m, 1m));
        CampaignSaveData before = session.CreateSaveData();
        decimal fundsBefore = session.Runtime.Economy.Funds;
        decimal trustBefore = player.Trust;
        int causesBefore = session.Runtime.State.CauseRecords.Count;
        int newsBefore = session.News.Items.Count;

        Assert.That(session.CanResolvePublicAction, Is.False);
        Assert.That(
            () => session.ResolveAction(action, session.Electorate),
            Throws.TypeOf<System.InvalidOperationException>());

        CampaignSaveData after = session.CreateSaveData();
        Assert.That(session.Runtime.Economy.Funds, Is.EqualTo(fundsBefore));
        Assert.That(player.Trust, Is.EqualTo(trustBefore));
        Assert.That(session.Runtime.State.CauseRecords, Has.Count.EqualTo(causesBefore));
        Assert.That(session.News.Items, Has.Count.EqualTo(newsBefore));
        Assert.That(after.LastActionDay, Is.EqualTo(before.LastActionDay));
        Assert.That(after.ActivityWeek, Is.EqualTo(before.ActivityWeek));
        Assert.That(after.PublicActivitiesThisWeek, Is.EqualTo(before.PublicActivitiesThisWeek));
        Assert.That(after.ElectionResult, Is.Null);
    }

    [Test]
    public void AssignTeamTask_AfterElectionResolution_RejectsAtomically()
    {
        var member = new CampaignTeamMember("press", "jefatura-prensa");
        var team = new CampaignTeam(new[] { member });
        CampaignSimulationSession session = CreateSessionOnDayBeforeElection(team: team);
        Assert.That(session.CanAssignTeamTask, Is.True);
        session.AdvanceDay();

        Assert.That(session.CanAssignTeamTask, Is.False);
        Assert.That(
            () => session.AssignTeamTask("press", DelegatedTaskType.MediaStatement, "nacional"),
            Throws.TypeOf<System.InvalidOperationException>());
        Assert.That(member.CurrentAssignment, Is.Null);
    }

    [Test]
    public void RestoreElectionResult_WithSchemaFiveResult_RestoresCompleteResultWithoutRecalculation()
    {
        CampaignSimulationSession source = CreateSessionOnDayBeforeElection();
        CampaignElectionResult expected = source.AdvanceDay().ElectionResult;
        CampaignSaveData saveData = source.CreateSaveData();
        var commitments = new List<MonthlyCommitment>();
        CampaignRuntime restoredRuntime = CampaignRuntime.Restore(saveData, commitments);
        IReadOnlyList<MicroElector> restoredElectorate = CampaignSaveMapper.RestoreElectorate(saveData);
        var restored = new CampaignSimulationSession(
            restoredRuntime,
            restoredElectorate,
            new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory(),
            saveData.ElectionRulesVersion);

        restored.RestoreElectionResult(saveData);

        Assert.That(saveData.SchemaVersion, Is.EqualTo(CampaignSaveData.CurrentSchemaVersion));
        Assert.That(saveData.ElectionResult, Is.Not.Null);
        Assert.That(restored.Runtime.State.CauseRecords, Is.Empty);
        Assert.That(restored.Runtime.PhaseMachine.Current, Is.EqualTo(source.Runtime.PhaseMachine.Current));
        AssertElectionResultsAreEqual(expected, restored.ElectionResult);
    }

    [Test]
    public void RestoreElectionResult_WithSchemaFourElectionDaySave_RebuildsResultDeterministically()
    {
        CampaignSimulationSession source = CreateSessionOnDayBeforeElection(requiresRunoff: true, includeUndecided: true);
        ElectionTally legacyTally = ElectionTallyCalculator.Calculate(source.Electorate, CampaignCandidateIds.All);
        var expected = new CampaignElectionResult(
            legacyTally,
            ElectionOutcomeCalculator.CalculateFirstRound(legacyTally, CampaignCandidateIds.All));
        source.AdvanceDay();
        CampaignSaveData legacySave = source.CreateSaveData();
        legacySave.SchemaVersion = 4;
        legacySave.ElectionResult = null;
        legacySave.Phase = CampaignPhase.ElectionDay.ToString();
        var commitments = new List<MonthlyCommitment>();
        CampaignRuntime restoredRuntime = CampaignRuntime.Restore(legacySave, commitments);
        IReadOnlyList<MicroElector> restoredElectorate = CampaignSaveMapper.RestoreElectorate(legacySave);
        var restored = new CampaignSimulationSession(
            restoredRuntime,
            restoredElectorate,
            new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory(),
            legacySave.ElectionRulesVersion);

        restored.RestoreElectionResult(legacySave);

        Assert.That(legacySave.SchemaVersion, Is.EqualTo(CampaignSaveData.CurrentSchemaVersion));
        Assert.That(legacySave.ElectionRulesVersion, Is.EqualTo(CampaignSaveData.LegacyElectionRulesVersion));
        Assert.That(restored.Runtime.State.CauseRecords, Has.Count.EqualTo(CampaignCandidateIds.All.Count));
        Assert.That(restored.Runtime.PhaseMachine.Current, Is.EqualTo(CampaignPhase.Runoff));
        AssertElectionResultsAreEqual(expected, restored.ElectionResult);
    }

    private static CampaignSimulationSession CreateSessionOnDayBeforeElection(
        bool requiresRunoff = false,
        CampaignTeam team = null,
        bool includeUndecided = false)
    {
        var runtime = new CampaignRuntime(new CampaignSeed(3UL), 100m, new List<MonthlyCommitment>());
        runtime.StartCampaign();
        while (runtime.State.Calendar.CurrentDay < CampaignCalendar.TotalCampaignDays - 1)
        {
            runtime.AdvanceDay();
        }

        var electorate = new List<MicroElector>
        {
            includeUndecided
                ? new MicroElector("elector", "locality", 100m, 100m, new[]
                {
                    new CandidateElectoralState(CampaignCandidateIds.Player, 50m, 30m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Liberales, 50m, 25m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Contr, 50m, 10m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Zurditos, 50m, 10m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Federales, 50m, 5m, 0m),
                }, blankVoteIntention: 5m, undecidedIntention: 15m)
                : new MicroElector("elector", "locality", 100m, 100m, new[]
                {
                    new CandidateElectoralState(CampaignCandidateIds.Player, 50m, requiresRunoff ? 39m : 50m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Liberales, 50m, requiresRunoff ? 31m : 20m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Contr, 50m, 10m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Zurditos, 50m, 10m, 0m),
                    new CandidateElectoralState(CampaignCandidateIds.Federales, 50m, 10m, 0m),
                }),
        };

        return new CampaignSimulationSession(
            runtime,
            electorate,
            team ?? new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory());
    }

    private static CampaignSimulationSession CreateDecisionSession(decimal funds)
    {
        var runtime = new CampaignRuntime(new CampaignSeed(19UL), funds, new List<MonthlyCommitment>());
        runtime.StartCampaign();
        var electorate = new List<MicroElector>
        {
            new MicroElector("elector", "locality", 100m, 100m, new[]
            {
                new CandidateElectoralState(CampaignCandidateIds.Player, 50m, 50m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Liberales, 50m, 20m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Contr, 50m, 10m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Zurditos, 50m, 10m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Federales, 50m, 10m, 0m),
            }),
        };

        return new CampaignSimulationSession(
            runtime,
            electorate,
            new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory(),
            teamSelectionCompleted: false);
    }

    private static void SelectCompleteTeam(CampaignSimulationSession session)
    {
        var index = 0;
        foreach (string roleId in CampaignTeamRoleIds.All)
        {
            session.SelectTeamMember(roleId, "profile-" + index++);
        }

        session.FinalizeTeamSelection();
    }

    private static CampaignSimulationSession CreateSessionAtCampaignStart()
    {
        var runtime = new CampaignRuntime(new CampaignSeed(3UL), 100m, new List<MonthlyCommitment>());
        runtime.StartCampaign();
        var electorate = new List<MicroElector>
        {
            new MicroElector("elector", "locality", 100m, 100m, new[]
            {
                new CandidateElectoralState(CampaignCandidateIds.Player, 50m, 50m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Liberales, 50m, 20m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Contr, 50m, 10m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Zurditos, 50m, 10m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Federales, 50m, 10m, 0m),
            }),
        };

        return new CampaignSimulationSession(
            runtime,
            electorate,
            new CampaignTeam(new CampaignTeamMember[0]),
            new NewsMemory());
    }

    private static void AssertElectionResultsAreEqual(CampaignElectionResult expected, CampaignElectionResult actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Tally.ParticipatingWeight, Is.EqualTo(expected.Tally.ParticipatingWeight));
        Assert.That(actual.Tally.ValidVotes, Is.EqualTo(expected.Tally.ValidVotes));
        Assert.That(actual.Tally.BlankVotes, Is.EqualTo(expected.Tally.BlankVotes));
        Assert.That(actual.Tally.UndecidedVotes, Is.EqualTo(expected.Tally.UndecidedVotes));
        foreach (string candidateId in CampaignCandidateIds.All)
        {
            Assert.That(actual.Tally.GetCandidateVotes(candidateId), Is.EqualTo(expected.Tally.GetCandidateVotes(candidateId)));
        }

        Assert.That(actual.Outcome.WinnerId, Is.EqualTo(expected.Outcome.WinnerId));
        Assert.That(actual.Outcome.RunoffFirstId, Is.EqualTo(expected.Outcome.RunoffFirstId));
        Assert.That(actual.Outcome.RunoffSecondId, Is.EqualTo(expected.Outcome.RunoffSecondId));
    }
}
}
