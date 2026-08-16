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

    [Header("Campaign testing")]
    [SerializeField, Range(1, CampaignCalendar.TotalCampaignDays)]
    [Tooltip("Only new campaigns are simulated up to this day. Loaded autosaves always keep their saved day.")]
    private int _newCampaignStartDay = 1;

    private JsonCampaignSaveRepository _saveRepository;
    private CampaignSimulationSession _session;
    private string _selectedJurisdictionId;

    public static CampaignGameSessionHost Current { get; private set; }

    public event Action StateChanged;

    public CampaignSimulationSession Session => _session ?? throw new InvalidOperationException("No active campaign session exists.");
    public CampaignContentDefinition ContentCatalog => _contentCatalog;
    public string SelectedJurisdictionId => _selectedJurisdictionId;
    public bool HasAutosave => _saveRepository != null && _saveRepository.Exists(AutosaveSlotId);
    public bool IsInitialTeamSelectionComplete => Session.TeamSelectionCompleted;

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

        CampaignSaveData savedCampaign = _saveRepository.Load(AutosaveSlotId);
        _session = CreateSession(savedCampaign);
        _selectedJurisdictionId = IsKnownJurisdiction(savedCampaign.SelectedJurisdictionId)
            ? savedCampaign.SelectedJurisdictionId
            : null;
        NotifyStateChanged();
    }

    public CampaignActionResolution ResolveAction(CampaignActivity activity)
    {
        CampaignActionResolution resolution = Session.ResolveAction(CreateActionDefinition(activity), SelectActionTargets());
        SaveAutosave();
        NotifyStateChanged();
        return resolution;
    }

    public CampaignDecisionResolution ResolveDecision(
        CampaignActivity activity,
        string scenarioId,
        IReadOnlyList<string> selectedOptionIds,
        string localityId = null)
    {
        if (selectedOptionIds == null) throw new ArgumentNullException(nameof(selectedOptionIds));
        CampaignDecisionScenarioDefinition scenario = GetDecisionScenario(activity, scenarioId);
        if (scenario == null)
        {
            throw new InvalidOperationException("The requested decision scenario does not exist in the campaign catalog.");
        }

        if (scenario.Steps == null || scenario.Steps.Length == 0)
        {
            throw new InvalidOperationException("The requested decision scenario has no authored steps.");
        }

        if (selectedOptionIds.Count != scenario.Steps.Length)
        {
            throw new ArgumentException("A decision must select exactly one option for every step.", nameof(selectedOptionIds));
        }

        if (string.Equals(scenario.TargetMode, "Locality", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(localityId) || !IsKnownLocality(localityId))
            {
                throw new InvalidOperationException("A locality is required for this political act.");
            }
        }

        var impacts = new List<ElectoralImpact>();
        var relationshipChanges = new List<PoliticalRelationshipChange>();
        var promises = new List<CampaignPromiseDefinition>();
        decimal cost = ToSimulationDecimal(scenario.BaseCost);
        for (var stepIndex = 0; stepIndex < scenario.Steps.Length; stepIndex++)
        {
            CampaignDecisionOptionDefinition option = FindOption(scenario.Steps[stepIndex], selectedOptionIds[stepIndex]);
            if (option == null)
            {
                throw new ArgumentException("A selected decision option does not belong to the scenario.", nameof(selectedOptionIds));
            }

            cost += ToSimulationDecimal(option.CostModifier);
            impacts.Add(CreateImpact(scenario, option, stepIndex));
            decimal trustDelta = ToSimulationDecimal(option.TrustDelta);
            decimal affinityDelta = ToSimulationDecimal(option.AffinityDelta);
            decimal obligationDelta = ToSimulationDecimal(option.ObligationDelta);
            decimal grievanceDelta = ToSimulationDecimal(option.GrievanceDelta);
            if (trustDelta != 0m || affinityDelta != 0m || obligationDelta != 0m || grievanceDelta != 0m)
            {
                relationshipChanges.Add(new PoliticalRelationshipChange(
                    scenario.ActorId,
                    trustDelta,
                    affinityDelta,
                    obligationDelta,
                    grievanceDelta));
            }

            if (!string.IsNullOrWhiteSpace(option.PromiseId))
            {
                promises.Add(new CampaignPromiseDefinition(
                    option.PromiseId + "-" + stepIndex + "-day-" + Session.Runtime.State.Calendar.CurrentDay,
                    scenario.ActorId,
                    string.IsNullOrWhiteSpace(option.PromiseDescription) ? option.Label : option.PromiseDescription));
            }
        }

        var plan = new CampaignDecisionPlan(
            scenario.Id,
            activity,
            scenario.ActorId,
            Math.Max(0m, cost),
            impacts,
            selectedOptionIds,
            relationshipChanges,
            promises);
        CampaignDecisionResolution resolution = Session.ResolveDecision(plan, SelectDecisionTargets(scenario, localityId));
        if (resolution.WasResolved)
        {
            SaveAutosave();
        }

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

    public void AssignTeamTask(string memberId, DelegatedTaskType taskType, string targetId)
    {
        Session.AssignTeamTask(memberId, taskType, targetId);
        SaveAutosave();
        NotifyStateChanged();
    }

    public void SelectTeamMember(string roleId, string profileId)
    {
        Session.SelectTeamMember(roleId, profileId);
        SaveAutosave();
        NotifyStateChanged();
    }

    public void FinalizeTeamSelection()
    {
        Session.FinalizeTeamSelection();
        SaveAutosave();
        NotifyStateChanged();
    }

    public CampaignTeamCandidateDefinition[] GetTeamCandidates(string roleId)
    {
        return _contentCatalog == null ? Array.Empty<CampaignTeamCandidateDefinition>() : _contentCatalog.GetCandidatesForRole(roleId);
    }

    public CampaignDecisionScenarioDefinition[] GetDecisionScenarios(CampaignActivity activity)
    {
        var scenarios = new List<CampaignDecisionScenarioDefinition>();
        if (_contentCatalog == null) return scenarios.ToArray();
        string activityId = activity.ToString();
        foreach (CampaignDecisionScenarioDefinition scenario in _contentCatalog.DecisionScenarios)
        {
            if (scenario != null && string.Equals(scenario.ActivityId, activityId, StringComparison.OrdinalIgnoreCase))
            {
                scenarios.Add(scenario);
            }
        }

        return scenarios.ToArray();
    }

    public CampaignDecisionScenarioDefinition GetDecisionScenario(CampaignActivity activity, string scenarioId = null)
    {
        if (_contentCatalog == null) return null;
        return _contentCatalog.GetDecisionScenario((CampaignActivityId)activity, scenarioId);
    }

    public void SelectJurisdiction(string jurisdictionId)
    {
        string selection = string.IsNullOrWhiteSpace(jurisdictionId) ? null : jurisdictionId;
        if (selection != null && !IsKnownJurisdiction(selection))
        {
            throw new ArgumentException("The selected jurisdiction does not exist in the campaign catalog.", nameof(jurisdictionId));
        }

        _selectedJurisdictionId = selection;
        SaveAutosave();
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
        int electionRulesVersion = savedCampaign == null
            ? CampaignSaveData.CurrentElectionRulesVersion
            : savedCampaign.ElectionRulesVersion;
        var session = new CampaignSimulationSession(
            runtime,
            electorate,
            CreateTeam(),
            new NewsMemory(),
            electionRulesVersion,
            savedCampaign == null ? false : savedCampaign.TeamSelectionCompleted);
        if (savedCampaign != null)
        {
            session.RestoreActivityLimits(savedCampaign);
            session.RestoreTeam(savedCampaign);
            session.RestoreNews(savedCampaign);
            session.RestorePoliticalMemory(savedCampaign);
            session.RestoreElectionResult(savedCampaign);
        }
        else
        {
            AdvanceNewCampaignToConfiguredDay(session);
        }

        return session;
    }

    private void AdvanceNewCampaignToConfiguredDay(CampaignSimulationSession session)
    {
        int targetDay = Mathf.Clamp(_newCampaignStartDay, 1, CampaignCalendar.TotalCampaignDays);
        while (session.Runtime.State.Calendar.CurrentDay < targetDay)
        {
            session.AdvanceDay();
        }
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

    private IReadOnlyList<MicroElector> SelectDecisionTargets(CampaignDecisionScenarioDefinition scenario, string localityId)
    {
        if (!string.Equals(scenario.TargetMode, "Locality", StringComparison.OrdinalIgnoreCase))
        {
            return Session.Electorate;
        }

        var targets = new List<MicroElector>();
        foreach (MicroElector elector in Session.Electorate)
        {
            if (elector.LocalityId == localityId)
            {
                targets.Add(elector);
            }
        }

        if (targets.Count == 0)
        {
            throw new InvalidOperationException("The selected locality has no electorate data.");
        }

        return targets;
    }

    private void SaveAutosave()
    {
        CampaignSaveData saveData = Session.CreateSaveData();
        saveData.SelectedJurisdictionId = _selectedJurisdictionId;
        _saveRepository.Save(AutosaveSlotId, saveData);
    }

    private bool IsKnownJurisdiction(string jurisdictionId)
    {
        if (string.IsNullOrWhiteSpace(jurisdictionId) || _contentCatalog == null)
        {
            return false;
        }

        foreach (LocalityDefinition locality in _contentCatalog.Localities)
        {
            if (locality != null && locality.JurisdictionId == jurisdictionId)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsKnownLocality(string localityId)
    {
        if (string.IsNullOrWhiteSpace(localityId) || _contentCatalog == null) return false;
        foreach (LocalityDefinition locality in _contentCatalog.Localities)
        {
            if (locality != null && locality.Id == localityId) return true;
        }

        return false;
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
        return new CampaignTeam(new CampaignTeamMember[0]);
    }

    private static CampaignDecisionOptionDefinition FindOption(CampaignDecisionStepDefinition step, string optionId)
    {
        if (step == null || step.Options == null) return null;
        foreach (CampaignDecisionOptionDefinition option in step.Options)
        {
            if (option != null && option.Id == optionId) return option;
        }

        return null;
    }

    private static ElectoralImpact CreateImpact(CampaignDecisionScenarioDefinition scenario, CampaignDecisionOptionDefinition option, int stepIndex)
    {
        if (!Enum.TryParse(option.MetricId, true, out ElectoralMetric metric) || !Enum.IsDefined(typeof(ElectoralMetric), metric))
        {
            throw new InvalidOperationException("A decision option references an invalid electoral metric.");
        }

        return new ElectoralImpact(
            scenario.Id + "-step-" + stepIndex,
            CampaignCandidateIds.Player,
            metric,
            ToSimulationDecimal(option.ImpactDelta),
            ToSimulationDecimal(option.Reach),
            ToSimulationDecimal(option.Relevance),
            ToSimulationDecimal(option.Compatibility),
            ToSimulationDecimal(option.Credibility),
            ToSimulationDecimal(option.MediaFraming),
            ToSimulationDecimal(option.Novelty));
    }

    private static decimal ToSimulationDecimal(float value)
    {
        return decimal.Round((decimal)value, 4, MidpointRounding.AwayFromZero);
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
