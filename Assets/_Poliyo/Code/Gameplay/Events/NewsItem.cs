using System;

namespace Poliyo.Simulation
{
public enum EvidenceQuality { Rumor, Indication, Proof }

public sealed class NewsItem
{
    public NewsItem(
        string id,
        int day,
        string topicId,
        string sourceId,
        string affectedCandidateId,
        EvidenceQuality evidence,
        decimal reach,
        decimal framing,
        decimal currentIntensity = 1m)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(topicId) || string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("News requires id, topic and source.");
        if (day < 1 || day > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(day));
        if (reach < 0m || reach > 1m || framing < -1m || framing > 1m || currentIntensity < 0m || currentIntensity > 1m) throw new ArgumentOutOfRangeException(nameof(currentIntensity));
        Id = id;
        Day = day;
        TopicId = topicId;
        SourceId = sourceId;
        AffectedCandidateId = affectedCandidateId;
        Evidence = evidence;
        Reach = reach;
        Framing = framing;
        CurrentIntensity = currentIntensity;
    }

    public string Id { get; }
    public int Day { get; }
    public string TopicId { get; }
    public string SourceId { get; }
    public string AffectedCandidateId { get; }
    public EvidenceQuality Evidence { get; }
    public decimal Reach { get; }
    public decimal Framing { get; }
    public decimal CurrentIntensity { get; private set; }

    public void AgeOneDay() => CurrentIntensity *= 0.90m;
    public void Reactivate(decimal factor) => CurrentIntensity = Math.Min(1m, CurrentIntensity + factor);
}
}