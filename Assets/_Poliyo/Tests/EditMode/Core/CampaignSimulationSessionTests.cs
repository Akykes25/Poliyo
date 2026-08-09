using System.Collections.Generic;
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
