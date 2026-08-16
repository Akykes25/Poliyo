using System;
using Poliyo.Simulation;

namespace Poliyo.Presentation
{
/// <summary>Centralizes player-facing labels for national electoral indicators.</summary>
public static class ElectoralMetricDisplay
{
    public static bool ShouldHideEstimates(CampaignCalendar calendar)
    {
        if (calendar == null)
        {
            throw new ArgumentNullException(nameof(calendar));
        }

        return calendar.IsElectoralFogActive;
    }

    public static string FormatNational(ElectoralMetric metric, decimal value, bool estimatesHidden)
    {
        switch (metric)
        {
            case ElectoralMetric.Trust:
                return estimatesHidden
                    ? "Confianza: señal cualitativa reservada"
                    : $"Confianza: {value:0.0}";
            case ElectoralMetric.VotingIntention:
                return estimatesHidden
                    ? "Intención de voto: estimación no disponible"
                    : $"Intención de voto: {value:0.0}";
            case ElectoralMetric.Rejection:
                return estimatesHidden
                    ? "Rechazo: estimación no disponible"
                    : $"Rechazo: {value:0.0}";
            case ElectoralMetric.Participation:
                return estimatesHidden
                    ? "Participación: estimación no disponible"
                    : $"Participación: {value:0.0}";
            default:
                throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unsupported electoral metric.");
        }
    }
}
}
