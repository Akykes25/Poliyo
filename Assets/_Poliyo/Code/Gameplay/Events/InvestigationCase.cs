using System;

namespace Poliyo.Simulation
{
public enum InvestigationMethod { Journalist, Infiltrator, LegalAudit, PoliticalContact, OperationsChief }

public sealed class InvestigationCase
{
    public InvestigationCase(string id, string targetId, InvestigationMethod method, int startedDay)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("An investigation requires id and target.");
        if (startedDay < 1 || startedDay > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(startedDay));
        Id = id; TargetId = targetId; Method = method; StartedDay = startedDay;
    }

    public string Id { get; }
    public string TargetId { get; }
    public InvestigationMethod Method { get; }
    public int StartedDay { get; }
    public EvidenceQuality Evidence { get; private set; }
    public bool IsResolved { get; private set; }

    public void Resolve(EvidenceQuality evidence)
    {
        if (IsResolved) throw new InvalidOperationException("An investigation can only resolve once.");
        Evidence = evidence;
        IsResolved = true;
    }
}

}
