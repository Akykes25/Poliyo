using System;
using System.Collections.Generic;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Application
{
public static class CampaignSaveMapper
{
    public static CampaignSaveData Create(CampaignRuntime runtime, IEnumerable<MicroElector> electorate = null)
    {
        if (runtime == null) throw new ArgumentNullException(nameof(runtime));

        return new CampaignSaveData
        {
            Seed = runtime.State.Seed.Value,
            CurrentDay = runtime.State.Calendar.CurrentDay,
            Funds = runtime.Economy.Funds,
            UnpaidObligations = runtime.Economy.UnpaidObligations,
            Phase = runtime.PhaseMachine.Current.ToString(),
            Electorate = CreateElectorateData(electorate),
        };
    }

    public static CampaignSeed GetSeed(CampaignSaveData saveData)
    {
        Validate(saveData);
        return new CampaignSeed(saveData.Seed);
    }

    public static CampaignPhase GetPhase(CampaignSaveData saveData)
    {
        Validate(saveData);
        if (!Enum.TryParse(saveData.Phase, out CampaignPhase phase) || !Enum.IsDefined(typeof(CampaignPhase), phase))
        {
            throw new InvalidOperationException("The save contains an invalid campaign phase.");
        }

        return phase;
    }

    public static IReadOnlyList<MicroElector> RestoreElectorate(CampaignSaveData saveData)
    {
        Validate(saveData);
        var electorate = new List<MicroElector>(saveData.Electorate.Length);
        foreach (ElectorSaveData electorData in saveData.Electorate)
        {
            if (electorData == null) throw new InvalidOperationException("The save contains an invalid elector.");
            var candidates = new List<CandidateElectoralState>(electorData.Candidates.Length);
            foreach (CandidateElectoralSaveData candidateData in electorData.Candidates)
            {
                if (candidateData == null) throw new InvalidOperationException("The save contains an invalid candidate state.");
                candidates.Add(new CandidateElectoralState(candidateData.CandidateId, candidateData.Trust, candidateData.VotingIntention, candidateData.Rejection));
            }

            electorate.Add(new MicroElector(
                electorData.Id,
                electorData.LocalityId,
                electorData.ElectoralWeight,
                electorData.Participation,
                candidates,
                electorData.BlankVoteIntention,
                electorData.UndecidedIntention));
        }

        return electorate;
    }

    public static IReadOnlyList<NewsItem> RestoreNews(CampaignSaveData saveData)
    {
        Validate(saveData);
        var newsItems = new List<NewsItem>(saveData.NewsItems.Length);
        foreach (NewsItemSaveData itemData in saveData.NewsItems)
        {
            if (itemData == null || !Enum.TryParse(itemData.Evidence, out EvidenceQuality evidence) || !Enum.IsDefined(typeof(EvidenceQuality), evidence))
            {
                throw new InvalidOperationException("The save contains an invalid news item.");
            }

            newsItems.Add(new NewsItem(
                itemData.Id,
                itemData.Day,
                itemData.TopicId,
                itemData.SourceId,
                itemData.AffectedCandidateId,
                evidence,
                itemData.Reach,
                itemData.Framing,
                itemData.CurrentIntensity));
        }

        return newsItems;
    }

    public static NewsItemSaveData[] CreateNewsData(IEnumerable<NewsItem> newsItems)
    {
        if (newsItems == null) return Array.Empty<NewsItemSaveData>();

        var data = new List<NewsItemSaveData>();
        foreach (NewsItem item in newsItems)
        {
            if (item == null) throw new ArgumentException("A news item is required.", nameof(newsItems));
            data.Add(new NewsItemSaveData
            {
                Id = item.Id,
                Day = item.Day,
                TopicId = item.TopicId,
                SourceId = item.SourceId,
                AffectedCandidateId = item.AffectedCandidateId,
                Evidence = item.Evidence.ToString(),
                Reach = item.Reach,
                Framing = item.Framing,
                CurrentIntensity = item.CurrentIntensity,
            });
        }

        return data.ToArray();
    }

    public static void Validate(CampaignSaveData saveData)
    {
        if (saveData == null) throw new ArgumentNullException(nameof(saveData));
        MigrateInPlace(saveData);
        if (saveData.SchemaVersion != CampaignSaveData.CurrentSchemaVersion)
        {
            throw new NotSupportedException("The campaign save schema is not supported.");
        }

        if (saveData.CurrentDay < 1 || saveData.CurrentDay > CampaignCalendar.TotalCampaignDays)
        {
            throw new InvalidOperationException("The save contains an invalid campaign day.");
        }

        if (saveData.Funds < 0m || saveData.UnpaidObligations < 0m)
        {
            throw new InvalidOperationException("The save contains invalid economy values.");
        }

        if (string.IsNullOrWhiteSpace(saveData.Phase))
        {
            throw new InvalidOperationException("The save has no campaign phase.");
        }

        if (saveData.Electorate == null || saveData.TeamMembers == null || saveData.NewsItems == null)
        {
            throw new InvalidOperationException("The save contains a missing collection.");
        }
    }

    private static void MigrateInPlace(CampaignSaveData saveData)
    {
        if (saveData.SchemaVersion == 1)
        {
            saveData.SchemaVersion = 2;
            saveData.Electorate = Array.Empty<ElectorSaveData>();
        }

        if (saveData.SchemaVersion == 2)
        {
            saveData.SchemaVersion = 3;
            saveData.TeamMembers = Array.Empty<TeamMemberSaveData>();
        }

        if (saveData.SchemaVersion == 3)
        {
            saveData.SchemaVersion = 4;
            saveData.NewsItems = Array.Empty<NewsItemSaveData>();
        }
    }

    private static ElectorSaveData[] CreateElectorateData(IEnumerable<MicroElector> electorate)
    {
        if (electorate == null) return Array.Empty<ElectorSaveData>();

        var data = new List<ElectorSaveData>();
        foreach (MicroElector elector in electorate)
        {
            if (elector == null) throw new ArgumentException("An elector is required.", nameof(electorate));
            var candidateData = new List<CandidateElectoralSaveData>(elector.Candidates.Count);
            foreach (CandidateElectoralState candidate in elector.Candidates.Values)
            {
                candidateData.Add(new CandidateElectoralSaveData
                {
                    CandidateId = candidate.CandidateId,
                    Trust = candidate.Trust,
                    VotingIntention = candidate.VotingIntention,
                    Rejection = candidate.Rejection,
                });
            }

            data.Add(new ElectorSaveData
            {
                Id = elector.Id,
                LocalityId = elector.LocalityId,
                ElectoralWeight = elector.ElectoralWeight,
                Participation = elector.Participation,
                BlankVoteIntention = elector.BlankVoteIntention,
                UndecidedIntention = elector.UndecidedIntention,
                Candidates = candidateData.ToArray(),
            });
        }

        return data.ToArray();
    }
}
}