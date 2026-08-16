using System;
using System.Collections.Generic;
using Poliyo.Content;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Projects the initial eight-role roster choice without exposing hidden team stats.</summary>
public sealed class TeamSelectionScreenPresenter : MonoBehaviour
{
    [SerializeField] private string[] _roleIds = Array.Empty<string>();
    [SerializeField] private Button[] _roleButtons = Array.Empty<Button>();
    [SerializeField] private Button[] _candidateButtons = Array.Empty<Button>();
    [SerializeField] private TMP_Text _roleLabel;
    [SerializeField] private TMP_Text _progressLabel;
    [SerializeField] private TMP_Text _candidateName;
    [SerializeField] private TMP_Text _candidateRole;
    [SerializeField] private TMP_Text _candidateTrajectory;
    [SerializeField] private TMP_Text _candidateIdeology;
    [SerializeField] private TMP_Text _candidateExperience;
    [SerializeField] private TMP_Text _candidateRelationships;
    [SerializeField] private TMP_Text _candidateClue;
    [SerializeField] private TMP_Text _statusLabel;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private UiSceneNavigation _navigator;

    private readonly Dictionary<string, string> _selectedProfiles = new Dictionary<string, string>();
    private CampaignGameSessionHost _host;
    private string _selectedRoleId;

    public void Configure(
        string[] roleIds,
        Button[] roleButtons,
        Button[] candidateButtons,
        TMP_Text roleLabel,
        TMP_Text progressLabel,
        TMP_Text candidateName,
        TMP_Text candidateRole,
        TMP_Text candidateTrajectory,
        TMP_Text candidateIdeology,
        TMP_Text candidateExperience,
        TMP_Text candidateRelationships,
        TMP_Text candidateClue,
        TMP_Text statusLabel,
        Button confirmButton,
        UiSceneNavigation navigator)
    {
        _roleIds = roleIds ?? throw new ArgumentNullException(nameof(roleIds));
        _roleButtons = roleButtons ?? throw new ArgumentNullException(nameof(roleButtons));
        _candidateButtons = candidateButtons ?? throw new ArgumentNullException(nameof(candidateButtons));
        _roleLabel = roleLabel;
        _progressLabel = progressLabel;
        _candidateName = candidateName;
        _candidateRole = candidateRole;
        _candidateTrajectory = candidateTrajectory;
        _candidateIdeology = candidateIdeology;
        _candidateExperience = candidateExperience;
        _candidateRelationships = candidateRelationships;
        _candidateClue = candidateClue;
        _statusLabel = statusLabel;
        _confirmButton = confirmButton;
        _navigator = navigator;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("Team selection requires a campaign session host.");
        if (_roleIds.Length == 0 || _roleButtons.Length != _roleIds.Length || _candidateButtons.Length == 0)
        {
            throw new InvalidOperationException("TeamSelectionScreenPresenter requires eight roles, role buttons and candidate buttons.");
        }

        foreach (CampaignTeamMember member in _host.Session.Team.Members.Values)
        {
            if (member != null && !string.IsNullOrWhiteSpace(member.ProfileId))
            {
                _selectedProfiles[member.RoleId] = member.ProfileId;
            }
        }

        _selectedRoleId = _roleIds[0];
        Refresh();
        FocusButton(FindCandidateButton(0));
    }

    public void SelectRole(string roleId)
    {
        if (!Contains(_roleIds, roleId)) return;
        _selectedRoleId = roleId;
        Refresh();
        FocusButton(FindCandidateButton(0));
    }

    public void SelectCandidate(string profileId)
    {
        if (_host == null || string.IsNullOrWhiteSpace(_selectedRoleId)) return;
        CampaignTeamCandidateDefinition candidate = FindCandidate(_selectedRoleId, profileId);
        if (candidate == null) return;

        try
        {
            _host.SelectTeamMember(_selectedRoleId, candidate.Id);
            _selectedProfiles[_selectedRoleId] = candidate.Id;
            _statusLabel.text = candidate.DisplayName + " quedó reservado para " + GetRoleName(_selectedRoleId) + ".";
        }
        catch (InvalidOperationException exception)
        {
            _statusLabel.text = exception.Message;
        }

        Refresh();
    }

    public void SelectCandidateAtIndex(int candidateIndex)
    {
        if (_host == null || candidateIndex < 0) return;
        CampaignTeamCandidateDefinition[] candidates = _host.GetTeamCandidates(_selectedRoleId);
        if (candidateIndex >= candidates.Length || candidates[candidateIndex] == null) return;
        SelectCandidate(candidates[candidateIndex].Id);
        FocusButton(FindCandidateButton(candidateIndex));
    }

    public void ConfirmSelection()
    {
        if (_host == null) return;
        try
        {
            _host.FinalizeTeamSelection();
            _statusLabel.text = "Equipo confirmado. La campaña empieza con información incompleta, no con una planilla perfecta.";
            if (_navigator != null)
            {
                _navigator.OpenScene("CampaignSlice");
            }
        }
        catch (InvalidOperationException exception)
        {
            _statusLabel.text = exception.Message;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (_host == null) return;
        int selectedCount = _selectedProfiles.Count;
        _progressLabel.text = selectedCount + "/" + _roleIds.Length + " roles confirmados";
        _confirmButton.interactable = selectedCount == _roleIds.Length;
        for (var index = 0; index < _roleButtons.Length; index++)
        {
            _roleButtons[index].interactable = _roleIds[index] != _selectedRoleId;
        }

        CampaignTeamCandidateDefinition[] candidates = _host.GetTeamCandidates(_selectedRoleId);
        _roleLabel.text = GetRoleName(_selectedRoleId);
        for (var index = 0; index < _candidateButtons.Length; index++)
        {
            bool available = index < candidates.Length && candidates[index] != null;
            _candidateButtons[index].interactable = available;
            if (!available) continue;
            TMP_Text buttonLabel = _candidateButtons[index].GetComponentInChildren<TMP_Text>(true);
            if (buttonLabel != null)
            {
                buttonLabel.text = "[" + candidates[index].Initials + "] " + candidates[index].DisplayName + "\n" + candidates[index].Ideology;
            }
        }

        CampaignTeamCandidateDefinition selected = null;
        if (_selectedProfiles.TryGetValue(_selectedRoleId, out string profileId))
        {
            selected = FindCandidate(_selectedRoleId, profileId);
        }

        _candidateName.text = selected == null ? "Elegí una persona, no una estadística" : selected.DisplayName;
        _candidateRole.text = selected == null ? GetRoleName(_selectedRoleId) : GetRoleName(selected.RoleId);
        _candidateTrajectory.text = selected == null ? "Tres perfiles disponibles. La campaña sólo conoce sus señales públicas." : selected.Trajectory;
        _candidateIdeology.text = selected == null ? "Ideología declarada: pendiente" : "Ideología declarada · " + selected.Ideology;
        _candidateExperience.text = selected == null ? "Experiencia pública: pendiente" : "Experiencia · " + selected.Experience;
        _candidateRelationships.text = selected == null ? "Relaciones públicas: pendiente" : "Relaciones visibles · " + selected.PublicRelationships;
        _candidateClue.text = selected == null ? "Pista narrativa: compará trayectoria, tono y antecedentes antes de confirmar." : "Pista narrativa · " + selected.NarrativeClue;
    }

    private CampaignTeamCandidateDefinition FindCandidate(string roleId, string profileId)
    {
        foreach (CampaignTeamCandidateDefinition candidate in _host.GetTeamCandidates(roleId))
        {
            if (candidate != null && candidate.Id == profileId) return candidate;
        }

        return null;
    }

    private Button FindCandidateButton(int candidateIndex)
    {
        return candidateIndex >= 0 && candidateIndex < _candidateButtons.Length
            ? _candidateButtons[candidateIndex]
            : null;
    }

    private static void FocusButton(Button button)
    {
        if (button != null && button.gameObject.activeInHierarchy && button.interactable && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private static bool Contains(string[] values, string value)
    {
        foreach (string item in values) if (item == value) return true;
        return false;
    }

    public static string GetRoleName(string roleId)
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
}
}
