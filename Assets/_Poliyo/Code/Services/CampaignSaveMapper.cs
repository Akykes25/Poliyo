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
            RequireWeeklyMeetings = runtime.RequireWeeklyMeetings,
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

    public static ElectionResultSaveData CreateElectionResultData(CampaignElectionResult electionResult)
    {
        if (electionResult == null) return null;

        var candidateVotes = new List<CandidateVoteSaveData>(electionResult.Tally.CandidateVotes.Count);
        foreach (KeyValuePair<string, decimal> candidateVote in electionResult.Tally.CandidateVotes)
        {
            candidateVotes.Add(new CandidateVoteSaveData
            {
                CandidateId = candidateVote.Key,
                Votes = candidateVote.Value,
            });
        }

        return new ElectionResultSaveData
        {
            Tally = new ElectionTallySaveData
            {
                CandidateVotes = candidateVotes.ToArray(),
                ParticipatingWeight = electionResult.Tally.ParticipatingWeight,
                ValidVotes = electionResult.Tally.ValidVotes,
                BlankVotes = electionResult.Tally.BlankVotes,
                UndecidedVotes = electionResult.Tally.UndecidedVotes,
            },
            Outcome = new ElectionOutcomeSaveData
            {
                WinnerId = electionResult.Outcome.WinnerId,
                RunoffFirstId = electionResult.Outcome.RunoffFirstId,
                RunoffSecondId = electionResult.Outcome.RunoffSecondId,
            },
        };
    }

    public static ElectionResultSaveData CreateRunoffResultData(CampaignElectionResult runoffResult)
    {
        return CreateElectionResultData(runoffResult);
    }

    public static CampaignElectionResult RestoreElectionResult(CampaignSaveData saveData)
    {
        Validate(saveData);
        if (saveData.ElectionResult == null) return null;

        return RestoreElectionResultData(saveData.ElectionResult);
    }

    public static CampaignElectionResult RestoreRunoffResult(CampaignSaveData saveData)
    {
        Validate(saveData);
        if (saveData.RunoffResult == null) return null;

        return RestoreElectionResultData(saveData.RunoffResult);
    }

    private static CampaignElectionResult RestoreElectionResultData(ElectionResultSaveData resultData)
    {
        ElectionTallySaveData tallyData = resultData.Tally;
        var tally = new ElectionTally();
        tally.AddParticipation(tallyData.ParticipatingWeight);
        foreach (CandidateVoteSaveData candidateVote in tallyData.CandidateVotes)
        {
            tally.AddCandidateVotes(candidateVote.CandidateId, candidateVote.Votes);
        }

        tally.AddBlankVotes(tallyData.BlankVotes);
        tally.AddUndecidedVotes(tallyData.UndecidedVotes);

        ElectionOutcomeSaveData outcomeData = resultData.Outcome;
        var outcome = new ElectionOutcome(outcomeData.WinnerId, outcomeData.RunoffFirstId, outcomeData.RunoffSecondId);
        return new CampaignElectionResult(tally, outcome);
    }

    public static SliceClosureSaveData CreateSliceClosureData(CampaignSliceClosureResult closure)
    {
        if (closure == null) return null;

        var candidateVotes = new List<CandidateVoteSaveData>(closure.Tally.CandidateVotes.Count);
        foreach (KeyValuePair<string, decimal> candidateVote in closure.Tally.CandidateVotes)
        {
            candidateVotes.Add(new CandidateVoteSaveData
            {
                CandidateId = candidateVote.Key,
                Votes = candidateVote.Value,
            });
        }

        var factors = new List<CampaignCausalFactorSaveData>(closure.DecisiveFactors.Count);
        foreach (CampaignCausalFactor factor in closure.DecisiveFactors)
        {
            factors.Add(new CampaignCausalFactorSaveData
            {
                Category = factor.Category.ToString(),
                SourceId = factor.SourceId,
                EffectId = factor.EffectId,
                Magnitude = factor.Magnitude,
                Occurrences = factor.Occurrences,
            });
        }

        return new SliceClosureSaveData
        {
            Day = closure.Day,
            Tally = new ElectionTallySaveData
            {
                CandidateVotes = candidateVotes.ToArray(),
                ParticipatingWeight = closure.Tally.ParticipatingWeight,
                ValidVotes = closure.Tally.ValidVotes,
                BlankVotes = closure.Tally.BlankVotes,
                UndecidedVotes = closure.Tally.UndecidedVotes,
            },
            Outcome = new ElectionOutcomeSaveData
            {
                WinnerId = closure.Outcome.WinnerId,
                RunoffFirstId = closure.Outcome.RunoffFirstId,
                RunoffSecondId = closure.Outcome.RunoffSecondId,
            },
            DecisiveFactors = factors.ToArray(),
            DecisionCount = closure.DecisionCount,
        };
    }

    public static CampaignSliceClosureResult RestoreSliceClosure(CampaignSaveData saveData)
    {
        Validate(saveData);
        if (saveData.SliceClosureResult == null) return null;

        SliceClosureSaveData closureData = saveData.SliceClosureResult;
        var tally = RestoreTally(closureData.Tally);
        var outcome = new ElectionOutcome(
            closureData.Outcome.WinnerId,
            closureData.Outcome.RunoffFirstId,
            closureData.Outcome.RunoffSecondId);
        var factors = new List<CampaignCausalFactor>(closureData.DecisiveFactors.Length);
        foreach (CampaignCausalFactorSaveData factorData in closureData.DecisiveFactors)
        {
            if (factorData == null || !Enum.TryParse(factorData.Category, out CauseCategory category) ||
                !Enum.IsDefined(typeof(CauseCategory), category))
            {
                throw new InvalidOperationException("The save contains an invalid slice causal factor.");
            }

            factors.Add(new CampaignCausalFactor(
                category,
                factorData.SourceId,
                factorData.EffectId,
                factorData.Magnitude,
                factorData.Occurrences));
        }

        return new CampaignSliceClosureResult(
            closureData.Day,
            tally,
            outcome,
            factors,
            closureData.DecisionCount);
    }

    private static ElectionTally RestoreTally(ElectionTallySaveData tallyData)
    {
        if (tallyData == null || tallyData.CandidateVotes == null)
        {
            throw new InvalidOperationException("The save contains an incomplete slice tally.");
        }

        var tally = new ElectionTally();
        tally.AddParticipation(tallyData.ParticipatingWeight);
        foreach (CandidateVoteSaveData candidateVote in tallyData.CandidateVotes)
        {
            if (candidateVote == null) throw new InvalidOperationException("The save contains an invalid slice vote total.");
            tally.AddCandidateVotes(candidateVote.CandidateId, candidateVote.Votes);
        }

        tally.AddBlankVotes(tallyData.BlankVotes);
        tally.AddUndecidedVotes(tallyData.UndecidedVotes);
        return tally;
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

        if (saveData.Electorate == null || saveData.TeamMembers == null || saveData.NewsItems == null ||
            saveData.Relationships == null || saveData.Promises == null || saveData.DecisionRecords == null || saveData.CauseRecords == null ||
            saveData.DeferredConsequences == null)
        {
            throw new InvalidOperationException("The save contains a missing collection.");
        }

        if ((saveData.ElectionResult != null || saveData.RunoffResult != null) &&
            saveData.CurrentDay != CampaignCalendar.TotalCampaignDays)
        {
            throw new InvalidOperationException("The save contains an election result before election day.");
        }

        if (saveData.SelectedJurisdictionId != null && string.IsNullOrWhiteSpace(saveData.SelectedJurisdictionId))
        {
            throw new InvalidOperationException("The save contains an invalid selected jurisdiction.");
        }

        if (saveData.ElectionRulesVersion < CampaignSaveData.LegacyElectionRulesVersion ||
            saveData.ElectionRulesVersion > CampaignSaveData.CurrentElectionRulesVersion)
        {
            throw new NotSupportedException($"Election rules version {saveData.ElectionRulesVersion} is not supported.");
        }

        ValidateElectionResultData(saveData.ElectionResult);
        ValidateElectionResultData(saveData.RunoffResult);
        ValidateRunoffResultData(saveData.ElectionResult, saveData.RunoffResult);
        ValidateSliceClosureData(saveData.SliceClosureResult);
        if (saveData.LastWeeklyMeetingDay < 0 || saveData.LastWeeklyMeetingDay > saveData.CurrentDay)
        {
            throw new InvalidOperationException("The save contains an invalid weekly meeting day.");
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

        if (saveData.SchemaVersion == 4)
        {
            saveData.SchemaVersion = 5;
            saveData.ElectionRulesVersion = CampaignSaveData.LegacyElectionRulesVersion;
            saveData.ElectionResult = null;
        }

        if (saveData.SchemaVersion == 5)
        {
            saveData.SchemaVersion = 6;
            saveData.TeamSelectionCompleted = true;
            saveData.Relationships = Array.Empty<PoliticalRelationshipSaveData>();
            saveData.Promises = Array.Empty<PoliticalPromiseSaveData>();
            saveData.DecisionRecords = Array.Empty<CampaignDecisionRecordSaveData>();
            saveData.CauseRecords = Array.Empty<CauseRecordSaveData>();
            if (saveData.TeamMembers != null)
            {
                foreach (TeamMemberSaveData member in saveData.TeamMembers)
                {
                    if (member != null && string.IsNullOrWhiteSpace(member.ProfileId))
                    {
                        member.ProfileId = member.Id;
                    }
                }
            }
        }

        if (saveData.SchemaVersion == 6)
        {
            saveData.SchemaVersion = 7;
            saveData.RequireWeeklyMeetings = true;
            saveData.LastWeeklyMeetingDay = 0;
            saveData.CrisisResolved = false;
            saveData.DeferredConsequences = Array.Empty<DeferredConsequenceSaveData>();
            saveData.SliceClosureResult = null;
        }

        if (saveData.SchemaVersion == 7)
        {
            saveData.SchemaVersion = 8;
            saveData.RunoffResult = null;
        }
    }

    public static PoliticalRelationshipSaveData[] CreateRelationshipData(IEnumerable<PoliticalRelationship> relationships)
    {
        if (relationships == null) return Array.Empty<PoliticalRelationshipSaveData>();
        var data = new List<PoliticalRelationshipSaveData>();
        foreach (PoliticalRelationship relationship in relationships)
        {
            if (relationship == null) throw new ArgumentException("A relationship is required.", nameof(relationships));
            data.Add(new PoliticalRelationshipSaveData
            {
                ActorId = relationship.ActorId,
                Trust = relationship.Trust,
                Affinity = relationship.Affinity,
                Obligation = relationship.Obligation,
                Grievance = relationship.Grievance,
            });
        }

        return data.ToArray();
    }

    public static PoliticalPromiseSaveData[] CreatePromiseData(IEnumerable<PoliticalPromise> promises)
    {
        if (promises == null) return Array.Empty<PoliticalPromiseSaveData>();
        var data = new List<PoliticalPromiseSaveData>();
        foreach (PoliticalPromise promise in promises)
        {
            if (promise == null) throw new ArgumentException("A promise is required.", nameof(promises));
            data.Add(new PoliticalPromiseSaveData
            {
                Id = promise.Id,
                CounterpartId = promise.CounterpartId,
                Description = promise.Description,
                CreatedDay = promise.CreatedDay,
            });
        }

        return data.ToArray();
    }

    public static CampaignDecisionRecordSaveData[] CreateDecisionRecordData(IEnumerable<CampaignDecisionRecord> records)
    {
        if (records == null) return Array.Empty<CampaignDecisionRecordSaveData>();
        var data = new List<CampaignDecisionRecordSaveData>();
        foreach (CampaignDecisionRecord record in records)
        {
            if (record == null) throw new ArgumentException("A decision record is required.", nameof(records));
            var options = new List<string>(record.SelectedOptionIds);
            data.Add(new CampaignDecisionRecordSaveData
            {
                Id = record.Id,
                DecisionId = record.DecisionId,
                Day = record.Day,
                Activity = record.Activity.ToString(),
                ActorId = record.ActorId,
                Cost = record.Cost,
                ContextId = record.ContextId,
                SelectedOptionIds = options.ToArray(),
            });
        }

        return data.ToArray();
    }

    public static CauseRecordSaveData[] CreateCauseData(IEnumerable<CauseRecord> causes)
    {
        if (causes == null) return Array.Empty<CauseRecordSaveData>();
        var data = new List<CauseRecordSaveData>();
        foreach (CauseRecord cause in causes)
        {
            if (cause == null) throw new ArgumentException("A cause is required.", nameof(causes));
            data.Add(new CauseRecordSaveData
            {
                Day = cause.Day,
                Category = cause.Category.ToString(),
                SourceId = cause.SourceId,
                TargetId = cause.TargetId,
                EffectId = cause.EffectId,
                Magnitude = cause.Magnitude,
            });
        }

        return data.ToArray();
    }

    public static DeferredConsequenceSaveData[] CreateDeferredConsequenceData(IEnumerable<CampaignDeferredConsequence> consequences)
    {
        if (consequences == null) return Array.Empty<DeferredConsequenceSaveData>();

        var data = new List<DeferredConsequenceSaveData>();
        foreach (CampaignDeferredConsequence consequence in consequences)
        {
            if (consequence == null) throw new ArgumentException("A deferred consequence is required.", nameof(consequences));
            data.Add(new DeferredConsequenceSaveData
            {
                Id = consequence.Id,
                DueDay = consequence.DueDay,
                SourceId = consequence.SourceId,
                TargetId = consequence.TargetId,
                EffectId = consequence.EffectId,
                CandidateId = consequence.CandidateId,
                Metric = consequence.Metric.ToString(),
                Magnitude = consequence.Magnitude,
                Category = consequence.Category.ToString(),
            });
        }

        return data.ToArray();
    }

    public static IReadOnlyList<CampaignDeferredConsequence> RestoreDeferredConsequences(CampaignSaveData saveData)
    {
        Validate(saveData);
        var consequences = new List<CampaignDeferredConsequence>();
        foreach (DeferredConsequenceSaveData data in saveData.DeferredConsequences)
        {
            if (data == null || !Enum.TryParse(data.Metric, out ElectoralMetric metric) ||
                !Enum.IsDefined(typeof(ElectoralMetric), metric) || !Enum.TryParse(data.Category, out CauseCategory category) ||
                !Enum.IsDefined(typeof(CauseCategory), category))
            {
                throw new InvalidOperationException("The save contains an invalid deferred consequence.");
            }

            consequences.Add(new CampaignDeferredConsequence(
                data.Id,
                data.DueDay,
                data.SourceId,
                data.TargetId,
                data.EffectId,
                data.CandidateId,
                metric,
                data.Magnitude,
                category));
        }

        return consequences;
    }

    public static IReadOnlyList<PoliticalRelationship> RestoreRelationships(CampaignSaveData saveData)
    {
        Validate(saveData);
        var relationships = new List<PoliticalRelationship>();
        foreach (PoliticalRelationshipSaveData data in saveData.Relationships)
        {
            if (data == null) throw new InvalidOperationException("The save contains an invalid relationship.");
            relationships.Add(new PoliticalRelationship(data.ActorId, data.Trust, data.Affinity, data.Obligation, data.Grievance));
        }

        return relationships;
    }

    public static IReadOnlyList<PoliticalPromise> RestorePromises(CampaignSaveData saveData)
    {
        Validate(saveData);
        var promises = new List<PoliticalPromise>();
        foreach (PoliticalPromiseSaveData data in saveData.Promises)
        {
            if (data == null) throw new InvalidOperationException("The save contains an invalid promise.");
            promises.Add(new PoliticalPromise(data.Id, data.CounterpartId, data.Description, data.CreatedDay));
        }

        return promises;
    }

    public static IReadOnlyList<CampaignDecisionRecord> RestoreDecisionRecords(CampaignSaveData saveData)
    {
        Validate(saveData);
        var records = new List<CampaignDecisionRecord>();
        foreach (CampaignDecisionRecordSaveData data in saveData.DecisionRecords)
        {
            if (data == null || !Enum.TryParse(data.Activity, out CampaignActivity activity) || !Enum.IsDefined(typeof(CampaignActivity), activity))
            {
                throw new InvalidOperationException("The save contains an invalid decision record.");
            }

            records.Add(new CampaignDecisionRecord(data.Id, data.DecisionId, data.Day, activity, data.ActorId, data.Cost, data.SelectedOptionIds, data.ContextId));
        }

        return records;
    }

    public static IReadOnlyList<CauseRecord> RestoreCauses(CampaignSaveData saveData)
    {
        Validate(saveData);
        var causes = new List<CauseRecord>();
        foreach (CauseRecordSaveData data in saveData.CauseRecords)
        {
            if (data == null || !Enum.TryParse(data.Category, out CauseCategory category) || !Enum.IsDefined(typeof(CauseCategory), category))
            {
                throw new InvalidOperationException("The save contains an invalid cause record.");
            }

            causes.Add(new CauseRecord(data.Day, category, data.SourceId, data.TargetId, data.EffectId, data.Magnitude));
        }

        return causes;
    }

    private static void ValidateElectionResultData(ElectionResultSaveData electionResult)
    {
        if (electionResult == null) return;
        if (electionResult.Tally == null || electionResult.Outcome == null)
        {
            throw new InvalidOperationException("The save contains an incomplete election result.");
        }

        ElectionTallySaveData tally = electionResult.Tally;
        if (tally.CandidateVotes == null)
        {
            throw new InvalidOperationException("The save contains a missing election vote collection.");
        }

        if (tally.ParticipatingWeight < 0m || tally.ValidVotes < 0m || tally.BlankVotes < 0m || tally.UndecidedVotes < 0m)
        {
            throw new InvalidOperationException("The save contains invalid election totals.");
        }

        var candidateIds = new HashSet<string>(StringComparer.Ordinal);
        decimal calculatedValidVotes = 0m;
        foreach (CandidateVoteSaveData candidateVote in tally.CandidateVotes)
        {
            if (candidateVote == null || string.IsNullOrWhiteSpace(candidateVote.CandidateId) || candidateVote.Votes < 0m)
            {
                throw new InvalidOperationException("The save contains an invalid candidate vote total.");
            }

            if (!candidateIds.Add(candidateVote.CandidateId))
            {
                throw new InvalidOperationException("The save contains duplicate candidate vote totals.");
            }

            calculatedValidVotes += candidateVote.Votes;
        }

        if (candidateIds.Count < 2)
        {
            throw new InvalidOperationException("The save contains too few candidates for an election result.");
        }

        if (calculatedValidVotes != tally.ValidVotes)
        {
            throw new InvalidOperationException("The save contains an inconsistent valid vote total.");
        }

        if (tally.ValidVotes + tally.BlankVotes + tally.UndecidedVotes != tally.ParticipatingWeight)
        {
            throw new InvalidOperationException("The save contains an inconsistent election participation total.");
        }

        ElectionOutcomeSaveData outcome = electionResult.Outcome;
        ValidateOptionalCandidateId(outcome.WinnerId);
        ValidateOptionalCandidateId(outcome.RunoffFirstId);
        ValidateOptionalCandidateId(outcome.RunoffSecondId);

        if (outcome.WinnerId != null)
        {
            if (outcome.RunoffFirstId != null || outcome.RunoffSecondId != null || !candidateIds.Contains(outcome.WinnerId))
            {
                throw new InvalidOperationException("The save contains an invalid first-round election outcome.");
            }

            return;
        }

        if (outcome.RunoffFirstId == null || outcome.RunoffSecondId == null ||
            outcome.RunoffFirstId == outcome.RunoffSecondId ||
            !candidateIds.Contains(outcome.RunoffFirstId) || !candidateIds.Contains(outcome.RunoffSecondId))
        {
            throw new InvalidOperationException("The save contains an invalid runoff election outcome.");
        }
    }

    private static void ValidateRunoffResultData(
        ElectionResultSaveData firstRoundResult,
        ElectionResultSaveData runoffResult)
    {
        if (runoffResult == null) return;
        if (firstRoundResult == null || firstRoundResult.Outcome == null || firstRoundResult.Outcome.WinnerId != null)
        {
            throw new InvalidOperationException("The save contains a runoff result without a pending first-round runoff.");
        }

        if (runoffResult.Outcome == null || string.IsNullOrWhiteSpace(runoffResult.Outcome.WinnerId) ||
            runoffResult.Outcome.RunoffFirstId != null || runoffResult.Outcome.RunoffSecondId != null ||
            runoffResult.Tally == null || runoffResult.Tally.CandidateVotes == null ||
            runoffResult.Tally.CandidateVotes.Length != 2)
        {
            throw new InvalidOperationException("The save contains an invalid runoff result.");
        }

        var finalistIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (CandidateVoteSaveData candidateVote in runoffResult.Tally.CandidateVotes)
        {
            if (candidateVote == null || !finalistIds.Add(candidateVote.CandidateId))
            {
                throw new InvalidOperationException("The save contains duplicate runoff finalists.");
            }
        }

        if (!finalistIds.Contains(firstRoundResult.Outcome.RunoffFirstId) ||
            !finalistIds.Contains(firstRoundResult.Outcome.RunoffSecondId) ||
            !finalistIds.Contains(runoffResult.Outcome.WinnerId))
        {
            throw new InvalidOperationException("The save contains runoff candidates that do not match the first round.");
        }
    }

    private static void ValidateSliceClosureData(SliceClosureSaveData closure)
    {
        if (closure == null) return;
        if (closure.Day < CampaignSliceClosureResolver.RequiredDecisionDay || closure.Day > CampaignCalendar.TotalCampaignDays)
        {
            throw new InvalidOperationException("The save contains an invalid slice closure day.");
        }

        if (closure.DecisionCount < 1 || closure.DecisiveFactors == null)
        {
            throw new InvalidOperationException("The save contains an incomplete slice closure.");
        }

        ValidateElectionResultData(new ElectionResultSaveData
        {
            Tally = closure.Tally,
            Outcome = closure.Outcome,
        });

        foreach (CampaignCausalFactorSaveData factor in closure.DecisiveFactors)
        {
            if (factor == null || string.IsNullOrWhiteSpace(factor.SourceId) || string.IsNullOrWhiteSpace(factor.EffectId) || factor.Occurrences < 1)
            {
                throw new InvalidOperationException("The save contains an invalid slice causal factor.");
            }

            if (!Enum.TryParse(factor.Category, out CauseCategory category) || !Enum.IsDefined(typeof(CauseCategory), category))
            {
                throw new InvalidOperationException("The save contains an invalid slice causal category.");
            }
        }
    }

    private static void ValidateOptionalCandidateId(string candidateId)
    {
        if (candidateId != null && string.IsNullOrWhiteSpace(candidateId))
        {
            throw new InvalidOperationException("The save contains an invalid election candidate id.");
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
