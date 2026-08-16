namespace Poliyo.Simulation
{
public sealed class ElectionOutcome
{
    public ElectionOutcome(string winnerId, string runoffFirstId, string runoffSecondId)
    {
        WinnerId = winnerId;
        RunoffFirstId = runoffFirstId;
        RunoffSecondId = runoffSecondId;
    }

    public string WinnerId { get; }
    public string RunoffFirstId { get; }
    public string RunoffSecondId { get; }
    public bool RequiresRunoff => WinnerId == null;
}

}
