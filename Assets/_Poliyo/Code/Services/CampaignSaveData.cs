using System;

namespace Poliyo.Application
{
/// <summary>Versioned DTO boundary for persistence; Unity scenes never own campaign data.</summary>
[Serializable]
public sealed class CampaignSaveData
{
    public const int CurrentSchemaVersion = 6;
    public const int LegacyElectionRulesVersion = 0;
    public const int CurrentElectionRulesVersion = 1;

    public int SchemaVersion = CurrentSchemaVersion;
    public ulong Seed;
    public int CurrentDay;
    public decimal Funds;
    public decimal UnpaidObligations;
    public string Phase;
    public int LastActionDay;
    public int ActivityWeek;
    public int PublicActivitiesThisWeek;
    public int ElectionRulesVersion = CurrentElectionRulesVersion;
    public string SelectedJurisdictionId;
    public bool TeamSelectionCompleted = true;
    public ElectorSaveData[] Electorate = Array.Empty<ElectorSaveData>();
    public TeamMemberSaveData[] TeamMembers = Array.Empty<TeamMemberSaveData>();
    public NewsItemSaveData[] NewsItems = Array.Empty<NewsItemSaveData>();
    public PoliticalRelationshipSaveData[] Relationships = Array.Empty<PoliticalRelationshipSaveData>();
    public PoliticalPromiseSaveData[] Promises = Array.Empty<PoliticalPromiseSaveData>();
    public CampaignDecisionRecordSaveData[] DecisionRecords = Array.Empty<CampaignDecisionRecordSaveData>();
    public CauseRecordSaveData[] CauseRecords = Array.Empty<CauseRecordSaveData>();
    public ElectionResultSaveData ElectionResult;
}

[Serializable]
public sealed class ElectorSaveData
{
    public string Id;
    public string LocalityId;
    public decimal ElectoralWeight;
    public decimal Participation;
    public decimal BlankVoteIntention;
    public decimal UndecidedIntention;
    public CandidateElectoralSaveData[] Candidates = Array.Empty<CandidateElectoralSaveData>();
}

[Serializable]
public sealed class CandidateElectoralSaveData
{
    public string CandidateId;
    public decimal Trust;
    public decimal VotingIntention;
    public decimal Rejection;
}

[Serializable]
public sealed class TeamMemberSaveData
{
    public string Id;
    public string RoleId;
    public string ProfileId;
    public DelegatedTaskSaveData Assignment;
}

[Serializable]
public sealed class PoliticalRelationshipSaveData
{
    public string ActorId;
    public decimal Trust;
    public decimal Affinity;
    public decimal Obligation;
    public decimal Grievance;
}

[Serializable]
public sealed class PoliticalPromiseSaveData
{
    public string Id;
    public string CounterpartId;
    public string Description;
    public int CreatedDay;
}

[Serializable]
public sealed class CampaignDecisionRecordSaveData
{
    public string Id;
    public string DecisionId;
    public int Day;
    public string Activity;
    public string ActorId;
    public decimal Cost;
    public string[] SelectedOptionIds = Array.Empty<string>();
}

[Serializable]
public sealed class CauseRecordSaveData
{
    public int Day;
    public string Category;
    public string SourceId;
    public string TargetId;
    public string EffectId;
    public decimal Magnitude;
}

[Serializable]
public sealed class DelegatedTaskSaveData
{
    public int Day;
    public string TaskType;
    public string TargetId;
}

[Serializable]
public sealed class NewsItemSaveData
{
    public string Id;
    public int Day;
    public string TopicId;
    public string SourceId;
    public string AffectedCandidateId;
    public string Evidence;
    public decimal Reach;
    public decimal Framing;
    public decimal CurrentIntensity;
}

[Serializable]
public sealed class ElectionResultSaveData
{
    public ElectionTallySaveData Tally;
    public ElectionOutcomeSaveData Outcome;
}

[Serializable]
public sealed class ElectionTallySaveData
{
    public CandidateVoteSaveData[] CandidateVotes = Array.Empty<CandidateVoteSaveData>();
    public decimal ParticipatingWeight;
    public decimal ValidVotes;
    public decimal BlankVotes;
    public decimal UndecidedVotes;
}

[Serializable]
public sealed class CandidateVoteSaveData
{
    public string CandidateId;
    public decimal Votes;
}

[Serializable]
public sealed class ElectionOutcomeSaveData
{
    public string WinnerId;
    public string RunoffFirstId;
    public string RunoffSecondId;
}
}
