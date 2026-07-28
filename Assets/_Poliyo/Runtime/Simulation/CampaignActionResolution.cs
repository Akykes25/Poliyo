using System.Collections.Generic;

namespace Poliyo.Simulation
{
public sealed class CampaignActionResolution
{
    public CampaignActionResolution(bool wasPaid, IReadOnlyList<CauseRecord> causes)
    {
        WasPaid = wasPaid;
        Causes = causes;
    }

    public bool WasPaid { get; }
    public IReadOnlyList<CauseRecord> Causes { get; }
}
}
