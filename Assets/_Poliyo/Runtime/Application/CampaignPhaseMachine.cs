using System;

namespace Poliyo.Application
{
/// <summary>Explicitly limits campaign flow transitions; presentation cannot skip strategic states.</summary>
public sealed class CampaignPhaseMachine
{
    public CampaignPhaseMachine()
        : this(CampaignPhase.Creation)
    {
    }

    private CampaignPhaseMachine(CampaignPhase current)
    {
        Current = current;
    }

    public CampaignPhase Current { get; private set; }

    public static CampaignPhaseMachine Restore(CampaignPhase current)
    {
        if (!Enum.IsDefined(typeof(CampaignPhase), current))
        {
            throw new ArgumentOutOfRangeException(nameof(current));
        }

        return new CampaignPhaseMachine(current);
    }

    public void MoveTo(CampaignPhase next)
    {
        if (!IsAllowed(Current, next)) throw new InvalidOperationException($"Cannot transition from {Current} to {next}.");
        Current = next;
    }

    private static bool IsAllowed(CampaignPhase current, CampaignPhase next)
    {
        switch (current)
        {
            case CampaignPhase.Creation: return next == CampaignPhase.WeeklyMeeting;
            case CampaignPhase.WeeklyMeeting: return next == CampaignPhase.Planning;
            case CampaignPhase.Planning: return next == CampaignPhase.DailyResolution || next == CampaignPhase.DecisionScene;
            case CampaignPhase.DecisionScene: return next == CampaignPhase.DailyResolution;
            case CampaignPhase.DailyResolution: return next == CampaignPhase.WeeklyMeeting || next == CampaignPhase.Planning || next == CampaignPhase.ElectoralFog || next == CampaignPhase.ElectoralBan;
            case CampaignPhase.ElectoralFog: return next == CampaignPhase.DailyResolution || next == CampaignPhase.WeeklyMeeting || next == CampaignPhase.ElectoralBan;
            case CampaignPhase.ElectoralBan: return next == CampaignPhase.ElectionDay;
            case CampaignPhase.ElectionDay: return next == CampaignPhase.Scrutiny;
            case CampaignPhase.Scrutiny: return next == CampaignPhase.Runoff || next == CampaignPhase.Finished;
            case CampaignPhase.Runoff: return next == CampaignPhase.WeeklyMeeting || next == CampaignPhase.ElectoralFog || next == CampaignPhase.ElectoralBan;
            default: return false;
        }
    }
}
}