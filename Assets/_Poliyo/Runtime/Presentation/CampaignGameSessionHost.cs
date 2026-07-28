using System;
using System.Collections.Generic;
using Poliyo.Application;
using Poliyo.Content;
using Poliyo.Core;
using Poliyo.Simulation;
using UnityEngine;

namespace Poliyo.Presentation
{
/// <summary>
/// Persistent Unity composition root for one campaign. Simulation remains scene-independent and the host owns autosave boundaries.
/// </summary>
public sealed class CampaignGameSessionHost : MonoBehaviour
{
    private const string AutosaveSlotId = "vertical-slice-autosave";

    [SerializeField] private CampaignContentDefinition _contentCatalog;
    [SerializeField] private ulong _seed = 20260725UL;
    [SerializeField] private float _initialFunds = 1200f;

    private JsonCampaignSaveRepository _saveRepository;
    private CampaignSimulationSession _session;
    private string _selectedJurisdictionId;

    public static CampaignGameSessionHost Current { get; private set; }

    public event Action StateChanged;

    public CampaignSimulationSession Session => _session ?? throw new InvalidOperationException("No active campaign session exists.");
    public CampaignContentDefinition ContentCatalog => _contentCatalog;
    public string SelectedJurisdictionId => _selectedJurisdictionId;

    public void Configure(CampaignContentDefinition contentCatalog, ulong seed, float initialFunds)
    {
        _contentCatalog = contentCatalog;
        _seed = seed;
        _initialFunds = initialFunds;
    }

    private void Awake()
    {
        if (Current != null && Current != this)
        {
            Destroy(gameObject);
            return;
        }

        Current = this;
        DontDestroyOnLoad(gameObject);
        _saveRepository = new JsonCampaignSaveRepository();
        EnsureSession();
    }

    private void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }
    }

    public void StartNewCampaign()
    {
        _session = CreateSession(null);
        _selectedJurisdictionId = null;
        SaveAutosave();
        NotifyStateChanged();
    }

    public void LoadAutosave()
    {
        if (!_saveRepository.Exists(AutosaveSlotId))
        {
            StartNewCampaign();
            return;
        }

        _session = CreateSession(_saveRepository.Load(AutosaveSlotId));
        _selectedJurisdictionId = null;
        NotifyStateChanged();
    }

    public CampaignActionResolution ResolveAction(CampaignActivity activity)
    {
        CampaignActionResolution resolution = Session.ResolveAction(CreateActionDefinition(activity), SelectActionTargets());
        SaveAutosave();
        NotifyStateChanged();
        return resolution;
    }

    public CampaignDayAdvanceResult AdvanceDay()
    {
        CampaignDayAdvanceResult result = Session.AdvanceDay();
        SaveAutosave();
        NotifyStateChanged();
        return result;
    }

    public void SelectJurisdiction(string jurisdictionId)
    {
        _selectedJurisdictionId = string.IsNullOrWhiteSpace(jurisdictionId) ? null : jurisdictionId;
        NotifyStateChanged();
    }

    public IReadOnlyList<LocalityDefinition> GetSelectedJurisdictionLocalities()
    {
        var localities = new List<LocalityDefinition>();
        if (_contentCatalog == null || string.IsNullOrWhiteSpace(_selectedJurisdictionId))
        {
            return localities;
        }

        foreach (LocalityDefinition locality in _contentCatalog.Localities)
        {
            if (locality != null && locality.JurisdictionId == _selectedJurisdictionId)
            {
                localities.Add(locality);
            }
        }

        return localities;
    }

    private void EnsureSession()
    {
        if (_session != null)
        {
            return;
        }

        _session = CreateSession(null);
        _selectedJurisdictionId = null;
    }

    private CampaignSimulationSession CreateSession(CampaignSaveData savedCampaign)
    {
        if (_contentCatalog == null)
        {
            throw new InvalidOperationException("CampaignGameSessionHost requires a content catalog.");
        }

        IReadOnlyList<MicroElector> electorate = savedCampaign != null && savedCampaign.Electorate.Length > 0
            ? CampaignSaveMapper.RestoreElectorate(savedCampaign)
            : CreateElectorate();
        CampaignRuntime runtime = savedCampaign != null
            ? CampaignRuntime.Restore(savedCampaign, CreateCommitments())
            : CreateRuntime();
        var session = new CampaignSimulationSession(runtime, electorate, CreateTeam(), new NewsMemory());
        if (savedCampaign != null)
        {
            session.RestoreActivityLimits(savedCampaign);
            session.RestoreTeam(savedCampaign);
            session.RestoreNews(savedCampaign);
        }

        return session;
    }

    private CampaignRuntime CreateRuntime()
    {
        var runtime = new CampaignRuntime(new CampaignSeed(_seed), (decimal)_initialFunds, CreateCommitments());
        runtime.StartCampaign();
        return runtime;
    }

    private IReadOnlyList<MicroElector> CreateElectorate()
    {
        var seeds = new List<LocalityElectorateSeed>(_contentCatalog.Localities.Length);
        foreach (LocalityDefinition locality in _contentCatalog.Localities)
        {
            if (locality != null)
            {
                seeds.Add(new LocalityElectorateSeed(locality.Id, locality.PopulationWeight));
            }
        }

        return VerticalSliceElectorateFactory.Create(new CampaignSeed(_seed), seeds);
    }

    private IReadOnlyList<MicroElector> SelectActionTargets()
    {
        if (string.IsNullOrWhiteSpace(_selectedJurisdictionId))
        {
            return Session.Electorate;
        }

        var targets = new List<MicroElector>();
        foreach (MicroElector elector in Session.Electorate)
        {
            foreach (LocalityDefinition locality in GetSelectedJurisdictionLocalities())
            {
                if (elector.LocalityId == locality.Id)
                {
                    targets.Add(elector);
                    break;
                }
            }
        }

        return targets.Count > 0 ? targets : Session.Electorate;
    }

    private void SaveAutosave()
    {
        _saveRepository.Save(AutosaveSlotId, Session.CreateSaveData());
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
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
