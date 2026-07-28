using System;
using System.Collections.Generic;
using Poliyo.Core;

namespace Poliyo.Simulation
{
/// <summary>
/// Creates deterministic placeholder electorates for the vertical slice.
/// The numeric ranges are test scaffolding, not approved balance values.
/// </summary>
public static class VerticalSliceElectorateFactory
{
    public static IReadOnlyList<MicroElector> Create(CampaignSeed seed, IEnumerable<LocalityElectorateSeed> localities)
    {
        if (localities == null) throw new ArgumentNullException(nameof(localities));

        var electorate = new List<MicroElector>();
        foreach (LocalityElectorateSeed locality in localities)
        {
            var random = new DeterministicRandom(seed.Derive("electorate:" + locality.LocalityId).Value);
            electorate.Add(CreateMicroElector(locality, random));
        }

        return electorate;
    }

    private static MicroElector CreateMicroElector(LocalityElectorateSeed locality, DeterministicRandom random)
    {
        const int CandidateDistribution = 82;
        const int BlankVoteIntention = 6;
        const int UndecidedIntention = 12;
        var rawWeights = new int[CampaignCandidateIds.All.Count];
        var rawTotal = 0;
        for (var index = 0; index < rawWeights.Length; index++)
        {
            rawWeights[index] = random.NextInt(25, 101);
            rawTotal += rawWeights[index];
        }

        var candidates = new List<CandidateElectoralState>(rawWeights.Length);
        var assignedIntention = 0;
        for (var index = 0; index < rawWeights.Length; index++)
        {
            int intention = index == rawWeights.Length - 1
                ? CandidateDistribution - assignedIntention
                : rawWeights[index] * CandidateDistribution / rawTotal;
            assignedIntention += intention;
            candidates.Add(new CandidateElectoralState(
                CampaignCandidateIds.All[index],
                random.NextInt(40, 66),
                intention,
                random.NextInt(8, 36)));
        }

        return new MicroElector(
            "electorate:" + locality.LocalityId,
            locality.LocalityId,
            locality.ElectoralWeight,
            random.NextInt(62, 91),
            candidates,
            BlankVoteIntention,
            UndecidedIntention);
    }
}
}
