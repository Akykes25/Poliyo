using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Stable identifiers for the eight roles required by the initial campaign team.</summary>
public static class CampaignTeamRoleIds
{
    public const string VicePresident = "vicepresidencia";
    public const string CampaignChief = "jefatura-campana";
    public const string PressChief = "jefatura-prensa";
    public const string Spokesperson = "voceria";
    public const string TerritorialCoordinator = "coordinacion-territorial";
    public const string LegalAndAccounting = "legal-contable";
    public const string PoliticalConsultant = "consultoria-politica";
    public const string OperationsChief = "jefatura-operaciones";

    private static readonly string[] RequiredRoles =
    {
        VicePresident,
        CampaignChief,
        PressChief,
        Spokesperson,
        TerritorialCoordinator,
        LegalAndAccounting,
        PoliticalConsultant,
        OperationsChief,
    };

    public static IReadOnlyList<string> All => RequiredRoles;
}
}
