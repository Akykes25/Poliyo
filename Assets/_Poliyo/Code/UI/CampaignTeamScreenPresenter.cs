using System;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Connects the campaign team screen to the persistent session without placing task rules in the view.</summary>
public sealed class CampaignTeamScreenPresenter : MonoBehaviour
{
    [SerializeField] private string[] _memberIds = Array.Empty<string>();
    [SerializeField] private Button[] _memberButtons = Array.Empty<Button>();
    [SerializeField] private TMP_Text[] _availabilityLabels = Array.Empty<TMP_Text>();
    [SerializeField] private TMP_Text _roleLabel;
    [SerializeField] private TMP_Text _assignmentLabel;
    [SerializeField] private TMP_Text _statusLabel;
    [SerializeField] private Button _territorialCampaignButton;
    [SerializeField] private Button _investigationButton;
    [SerializeField] private Button _mediaStatementButton;

    private CampaignGameSessionHost _host;
    private string _selectedMemberId;

    public void Configure(
        string[] memberIds,
        Button[] memberButtons,
        TMP_Text[] availabilityLabels,
        TMP_Text roleLabel,
        TMP_Text assignmentLabel,
        TMP_Text statusLabel,
        Button territorialCampaignButton,
        Button investigationButton,
        Button mediaStatementButton)
    {
        _memberIds = memberIds ?? throw new ArgumentNullException(nameof(memberIds));
        _memberButtons = memberButtons ?? throw new ArgumentNullException(nameof(memberButtons));
        _availabilityLabels = availabilityLabels ?? throw new ArgumentNullException(nameof(availabilityLabels));
        _roleLabel = roleLabel;
        _assignmentLabel = assignmentLabel;
        _statusLabel = statusLabel;
        _territorialCampaignButton = territorialCampaignButton;
        _investigationButton = investigationButton;
        _mediaStatementButton = mediaStatementButton;
        ValidateConfiguration();
    }

    private void Start()
    {
        ValidateConfiguration();
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("Campaign team requires a CampaignGameSessionHost.");
        _host.StateChanged += Refresh;
        _selectedMemberId = _memberIds.Length == 0 ? null : _memberIds[0];
        Refresh();
    }

    private void OnDestroy()
    {
        if (_host != null)
        {
            _host.StateChanged -= Refresh;
        }
    }

    public void SelectMember(string memberId)
    {
        if (!_host.Session.Team.Members.ContainsKey(memberId))
        {
            throw new ArgumentException("The selected campaign team member does not exist.", nameof(memberId));
        }

        _selectedMemberId = memberId;
        Refresh();
    }

    public void AssignTerritorialCampaign()
    {
        Assign(DelegatedTaskType.TerritorialCampaign, GetTerritorialTarget());
    }

    public void AssignInvestigation()
    {
        Assign(DelegatedTaskType.Investigation, GetTerritorialTarget());
    }

    public void AssignMediaStatement()
    {
        Assign(DelegatedTaskType.MediaStatement, "nacional");
    }

    private void Assign(DelegatedTaskType taskType, string targetId)
    {
        try
        {
            _host.AssignTeamTask(_selectedMemberId, taskType, targetId);
            _statusLabel.text = $"Tarea asignada: {GetTaskName(taskType)}. Se resolverá al cerrar la jornada.";
        }
        catch (InvalidOperationException exception)
        {
            _statusLabel.text = exception.Message;
        }
    }

    private void Refresh()
    {
        if (_host == null)
        {
            return;
        }

        for (var index = 0; index < _memberIds.Length; index++)
        {
            if (!_host.Session.Team.Members.TryGetValue(_memberIds[index], out CampaignTeamMember listedMember))
            {
                _availabilityLabels[index].text = "SELECCIÓN INICIAL PENDIENTE";
                _memberButtons[index].interactable = false;
                continue;
            }

            _availabilityLabels[index].text = listedMember.IsAvailable
                ? "DISPONIBLE"
                : "EN TAREA · " + GetTaskName(listedMember.CurrentAssignment.TaskType).ToUpperInvariant();
            _memberButtons[index].interactable = _memberIds[index] != _selectedMemberId;
        }

        if (string.IsNullOrWhiteSpace(_selectedMemberId) ||
            !_host.Session.Team.Members.TryGetValue(_selectedMemberId, out CampaignTeamMember member))
        {
            SetActionsInteractable(false);
            _roleLabel.text = "Seleccioná un integrante";
            _assignmentLabel.text = _host.Session.TeamSelectionCompleted
                ? "Elegí un rol para revisar su disponibilidad y delegar una tarea."
                : "La selección inicial todavía no está confirmada. Volvé a la escena de elección del equipo.";
            return;
        }

        _roleLabel.text = GetRoleName(member.RoleId);
        _statusLabel.text = member.IsAvailable
            ? "ESTADO · DISPONIBLE"
            : "ESTADO · EN TAREA";
        _assignmentLabel.text = member.IsAvailable
            ? "Puede asumir una tarea paralela durante la jornada actual."
            : $"Asignación actual: {GetTaskName(member.CurrentAssignment.TaskType)} · {FormatTarget(member.CurrentAssignment.TargetId)}";
        bool canAssign = member.IsAvailable && _host.Session.CanAssignTeamTask;
        SetActionsInteractable(canAssign);
    }

    private string GetTerritorialTarget()
    {
        return string.IsNullOrWhiteSpace(_host.SelectedJurisdictionId)
            ? "nacional"
            : _host.SelectedJurisdictionId;
    }

    private void SetActionsInteractable(bool interactable)
    {
        _territorialCampaignButton.interactable = interactable;
        _investigationButton.interactable = interactable;
        _mediaStatementButton.interactable = interactable;
    }

    private void ValidateConfiguration()
    {
        if (_memberIds == null || _memberButtons == null || _availabilityLabels == null ||
            _memberIds.Length == 0 || _memberIds.Length != _memberButtons.Length || _memberIds.Length != _availabilityLabels.Length)
        {
            throw new InvalidOperationException("CampaignTeamScreenPresenter requires matching member, button and status arrays.");
        }

        if (_roleLabel == null || _assignmentLabel == null || _statusLabel == null ||
            _territorialCampaignButton == null || _investigationButton == null || _mediaStatementButton == null)
        {
            throw new InvalidOperationException("CampaignTeamScreenPresenter is missing required UI references.");
        }
    }

    private static string GetTaskName(DelegatedTaskType taskType)
    {
        switch (taskType)
        {
            case DelegatedTaskType.PoliticalContact: return "Contacto político";
            case DelegatedTaskType.Investigation: return "Investigación";
            case DelegatedTaskType.MediaStatement: return "Declaración en medios";
            case DelegatedTaskType.CrisisAnalysis: return "Análisis de crisis";
            case DelegatedTaskType.Fundraising: return "Recaudación";
            case DelegatedTaskType.TerritorialCampaign: return "Campaña territorial";
            case DelegatedTaskType.AffiliateRecruitment: return "Captación de afiliados";
            default: return taskType.ToString();
        }
    }

    private static string GetRoleName(string roleId)
    {
        switch (roleId)
        {
            case "vicepresidencia": return "Vicepresidencia";
            case "jefatura-campana": return "Jefatura de campaña";
            case "jefatura-prensa": return "Jefatura de prensa";
            case "voceria": return "Vocería";
            case "coordinacion-territorial": return "Coordinación territorial";
            case "legal-contable": return "Responsable legal / contable";
            case "consultoria-politica": return "Consultoría política";
            case "jefatura-operaciones": return "Jefatura de Operaciones";
            default: return roleId;
        }
    }

    private static string FormatTarget(string targetId)
    {
        return targetId == "nacional" ? "alcance nacional" : targetId.Replace('-', ' ');
    }
}
}
