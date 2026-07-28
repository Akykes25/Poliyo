using System;
using System.Collections.Generic;
using Poliyo.Application;
using Poliyo.Content;
using Poliyo.Core;
using Poliyo.Simulation;
using UnityEngine;

namespace Poliyo.Presentation
{
/// <summary>Vertical-slice composition root. It owns Unity references and creates a pure campaign session.</summary>
public sealed class CampaignSliceBootstrap : MonoBehaviour
{
    private const string AutosaveSlotId = "vertical-slice-autosave";

    [SerializeField] private CampaignCanvasController _canvasController;
    [SerializeField] private CampaignContentDefinition _contentCatalog;
    [SerializeField] private ulong _seed = 20260725UL;
    [SerializeField] private float _initialFunds = 1200f;
    [SerializeField] private bool _loadAutosave;

    private IReadOnlyList<MonthlyCommitment> _commitments;
    private ICampaignSaveRepository _saveRepository;
    private CampaignSimulationSession _session;

    private void Awake()
    {
        RuntimeUiInputBootstrap.Ensure();
    }

    private void Start()
    {
        _commitments = CreateCommitments();
        _saveRepository = new JsonCampaignSaveRepository();
        CampaignSaveData savedCampaign = _loadAutosave && _saveRepository.Exists(AutosaveSlotId)
            ? _saveRepository.Load(AutosaveSlotId)
            : null;
        IReadOnlyList<MicroElector> electorate = savedCampaign != null && savedCampaign.Electorate.Length > 0
            ? CampaignSaveMapper.RestoreElectorate(savedCampaign)
            : CreateElectorate();
        CampaignRuntime runtime = savedCampaign != null
            ? CampaignRuntime.Restore(savedCampaign, _commitments)
            : CreateNewCampaign();
        _session = new CampaignSimulationSession(runtime, electorate, CreateTeam(), new NewsMemory());
        if (savedCampaign != null)
        {
            _session.RestoreActivityLimits(savedCampaign);
            _session.RestoreTeam(savedCampaign);
            _session.RestoreNews(savedCampaign);
        }

        if (_canvasController == null)
        {
            _canvasController = GetComponent<CampaignCanvasController>();
        }

        _canvasController.Bind(
            _session.Runtime,
            _session.Team,
            _contentCatalog,
            _session.Electorate,
            _session.News,
            AdvanceDay,
            ResolveAction,
            AssignTeamTask);
    }

    private void OnApplicationQuit()
    {
        SaveCampaign();
    }

    public void SaveCampaign()
    {
        if (_session == null || _saveRepository == null)
        {
            return;
        }

        _saveRepository.Save(AutosaveSlotId, _session.CreateSaveData());
    }

    private CampaignActionResolution ResolveAction(CampaignActivity activity, string localityId)
    {
        if (_session.Electorate.Count == 0)
        {
            throw new InvalidOperationException("No hay electorado territorial disponible para aplicar esta accion.");
        }

        IReadOnlyList<MicroElector> targets = SelectActionTargets(localityId);
        return _session.ResolveAction(CreateActionDefinition(activity), targets);
    }

    private void AssignTeamTask(string memberId, DelegatedTaskType taskType, string targetId)
    {
        _session.AssignTeamTask(memberId, taskType, targetId);
        SaveCampaign();
    }

    private IReadOnlyList<MicroElector> SelectActionTargets(string localityId)
    {
        if (string.IsNullOrWhiteSpace(localityId))
        {
            return _session.Electorate;
        }

        var targets = new List<MicroElector>();
        foreach (MicroElector elector in _session.Electorate)
        {
            if (elector.LocalityId == localityId)
            {
                targets.Add(elector);
            }
        }

        return targets.Count > 0 ? targets : _session.Electorate;
    }

    private static CampaignActionDefinition CreateActionDefinition(CampaignActivity activity)
    {
        switch (activity)
        {
            case CampaignActivity.Rally:
                return new CampaignActionDefinition("rally", activity, 80m, new ElectoralImpact("rally", CampaignCandidateIds.Player, ElectoralMetric.VotingIntention, 4m, 0.55m, 0.8m, 0.8m, 0.7m, 1m, 1m));
            case CampaignActivity.Interview:
                return new CampaignActionDefinition("interview", activity, 35m, new ElectoralImpact("interview", CampaignCandidateIds.Player, ElectoralMetric.Trust, 3m, 0.45m, 0.7m, 0.8m, 0.8m, 1m, 1m));
            case CampaignActivity.Negotiation:
                return new CampaignActionDefinition("negotiation", activity, 20m, new ElectoralImpact("negotiation", CampaignCandidateIds.Player, ElectoralMetric.Rejection, -2m, 0.3m, 0.6m, 0.8m, 0.9m, 1m, 1m));
            default:
                throw new ArgumentOutOfRangeException(nameof(activity));
        }
    }

    private CampaignDayAdvanceResult AdvanceDay()
    {
        CampaignDayAdvanceResult result = _session.AdvanceDay();
        if (result.ElectionResult != null)
        {
            SaveCampaign();
        }

        return result;
    }

    private IReadOnlyList<MicroElector> CreateElectorate()
    {
        if (_contentCatalog == null)
        {
            Debug.LogWarning("CampaignSlice has no content catalog assigned; electoral resolution is unavailable.", this);
            return new List<MicroElector>();
        }

        var localities = new List<LocalityElectorateSeed>(_contentCatalog.Localities.Length);
        foreach (LocalityDefinition locality in _contentCatalog.Localities)
        {
            if (locality != null)
            {
                localities.Add(new LocalityElectorateSeed(locality.Id, locality.PopulationWeight));
            }
        }

        return VerticalSliceElectorateFactory.Create(new CampaignSeed(_seed), localities);
    }

    private CampaignRuntime CreateNewCampaign()
    {
        var runtime = new CampaignRuntime(new CampaignSeed(_seed), (decimal)_initialFunds, _commitments);
        runtime.StartCampaign();
        return runtime;
    }

    private static CampaignTeam CreateTeam()
    {
        return new CampaignTeam(new[]
        {
            new CampaignTeamMember("vicepresidencia", "vicepresidencia"),
            new CampaignTeamMember("jefatura-campana", "jefatura-campana"),
            new CampaignTeamMember("jefatura-prensa", "jefatura-prensa"),
            new CampaignTeamMember("voceria", "voceria"),
            new CampaignTeamMember("coordinacion-territorial", "coordinacion-territorial"),
            new CampaignTeamMember("legal-contable", "legal-contable"),
            new CampaignTeamMember("consultoria-politica", "consultoria-politica"),
            new CampaignTeamMember("jefatura-operaciones", "jefatura-operaciones"),
        });
    }

    private static IReadOnlyList<MonthlyCommitment> CreateCommitments()
    {
        return new List<MonthlyCommitment>
        {
            new MonthlyCommitment("inversor-inicial", MonthlyCommitmentType.Income, 500m),
            new MonthlyCommitment("sede", MonthlyCommitmentType.Expense, 200m),
            new MonthlyCommitment("estructura", MonthlyCommitmentType.Expense, 150m),
        };
    }
}
}