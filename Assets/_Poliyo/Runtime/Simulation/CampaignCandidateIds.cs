using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Stable identifiers for the five MVP candidacies. Visible names remain content, not simulation keys.</summary>
public static class CampaignCandidateIds
{
    public const string Player = "player";
    public const string Liberales = "liberales";
    public const string Contr = "contr";
    public const string Zurditos = "zurditos";
    public const string Federales = "federales";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Player,
        Liberales,
        Contr,
        Zurditos,
        Federales,
    };
}
}
