using System;

namespace Poliyo.Application
{
/// <summary>Versioned DTO boundary for persistence; Unity scenes never own campaign data.</summary>
[Serializable]
public sealed class CampaignSaveData
{
    public const int CurrentSchemaVersion = 4;

    public int SchemaVersion = CurrentSchemaVersion;
    public ulong Seed;
    public int CurrentDay;
    public decimal Funds;
    public decimal UnpaidObligations;
    public string Phase;
    public int LastActionDay;
    public int ActivityWeek;
    public int PublicActivitiesThisWeek;
    public ElectorSaveData[] Electorate = Array.Empty<ElectorSaveData>();
    public TeamMemberSaveData[] TeamMembers = Array.Empty<TeamMemberSaveData>();
    public NewsItemSaveData[] NewsItems = Array.Empty<NewsItemSaveData>();
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
    public DelegatedTaskSaveData Assignment;
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
}