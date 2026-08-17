using System;
using System.Collections.Generic;
using System.Text;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Projects the persisted first-round, runoff or slice-closure readout into a real result screen.</summary>
public sealed class CampaignElectionResultScreenPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleLabel;
    [SerializeField] private TMP_Text _phaseLabel;
    [SerializeField] private TMP_Text _outcomeLabel;
    [SerializeField] private TMP_Text _tallyLabel;
    [SerializeField] private TMP_Text _causesLabel;
    [SerializeField] private TMP_Text _statusLabel;
    [SerializeField] private Button _returnButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private UiSceneNavigation _navigator;

    private CampaignGameSessionHost _host;
    private bool _returnToCampaignAllowed;

    public void Configure(
        TMP_Text titleLabel,
        TMP_Text phaseLabel,
        TMP_Text outcomeLabel,
        TMP_Text tallyLabel,
        TMP_Text causesLabel,
        TMP_Text statusLabel,
        Button returnButton,
        Button menuButton,
        UiSceneNavigation navigator)
    {
        _titleLabel = titleLabel;
        _phaseLabel = phaseLabel;
        _outcomeLabel = outcomeLabel;
        _tallyLabel = tallyLabel;
        _causesLabel = causesLabel;
        _statusLabel = statusLabel;
        _returnButton = returnButton;
        _menuButton = menuButton;
        _navigator = navigator;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("El escrutinio requiere una campaña activa.");
        _host.StateChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (_host != null)
        {
            _host.StateChanged -= Refresh;
        }
    }

    public void ReturnToCampaign()
    {
        if (!_returnToCampaignAllowed)
        {
            ReturnToMenu();
            return;
        }

        if (_navigator != null) _navigator.OpenCampaignDashboard();
        else SceneManager.LoadScene("CampaignSlice");
    }

    public void ReturnToMenu()
    {
        if (_navigator != null) _navigator.OpenScene("MainMenu");
        else SceneManager.LoadScene("MainMenu");
    }

    private void Refresh()
    {
        if (_host == null) return;

        CampaignElectionResult election = _host.Session.ElectionResult;
        CampaignElectionResult runoff = _host.Session.RunoffResult;
        CampaignSliceClosureResult closure = _host.Session.SliceClosureResult;
        SetText(_phaseLabel, "Día " + _host.Session.Runtime.State.Calendar.CurrentDay + " · " + GetPhaseName(_host.Session.Runtime.PhaseMachine.Current));

        if (runoff != null)
        {
            bool playerWon = runoff.Outcome.WinnerId == CampaignCandidateIds.Player;
            SetText(_titleLabel, playerWon ? "Victoria" : "Fin de campaña");
            SetText(_outcomeLabel, FormatRunoffOutcome(runoff));
            SetText(_tallyLabel, FormatTally(runoff.Tally, _host.Session.ActiveElectionCandidateIds));
            SetText(_causesLabel, FormatCampaignSummary(election, runoff));
            SetText(_statusLabel, playerWon
                ? "La candidatura ganó el balotaje. Este resumen queda disponible desde el autosave."
                : "La candidatura perdió el balotaje. La campaña terminó y el resumen queda disponible desde el autosave.");
            ConfigureNavigation(false, true);
            return;
        }

        if (election != null)
        {
            bool playerReachedRunoff = _host.Session.PlayerReachedRunoff;
            bool playerWonFirstRound = !election.Outcome.RequiresRunoff && election.Outcome.WinnerId == CampaignCandidateIds.Player;
            bool playerEliminated = election.Outcome.RequiresRunoff && !playerReachedRunoff;
            SetText(_titleLabel, !election.Outcome.RequiresRunoff || playerEliminated
                ? (playerWonFirstRound ? "Victoria" : "Fin de campaña")
                : "Balotaje");
            SetText(_outcomeLabel, playerEliminated ? FormatEliminationOutcome(election) : FormatElectionOutcome(election));
            SetText(_tallyLabel, FormatTally(election.Tally, CampaignCandidateIds.All));
            SetText(_causesLabel, FormatCampaignSummary(election, null));
            SetText(_statusLabel, election.Outcome.RequiresRunoff && playerReachedRunoff
                ? "Tu candidatura entró al balotaje. Volvé a la mesa: ahora sólo quedan tu partido y el rival."
                : "La campaña terminó para tu candidatura. Revisá el resumen y los factores que explican el desenlace.");
            ConfigureNavigation(
                election.Outcome.RequiresRunoff && playerReachedRunoff,
                !election.Outcome.RequiresRunoff || playerEliminated);
            return;
        }

        if (closure != null)
        {
            SetText(_titleLabel, "Cierre del vertical slice");
            SetText(_outcomeLabel, FormatClosureOutcome(closure));
            SetText(_tallyLabel, FormatTally(closure.Tally, CampaignCandidateIds.All));
            SetText(_causesLabel, FormatCampaignSummary(null, null) + "\n\nFactores decisivos:\n" + FormatClosureCauses(closure));
            SetText(_statusLabel, "El corte de prueba terminó. Los factores causales quedan disponibles para revisión.");
            ConfigureNavigation(true, false);
            return;
        }

        SetText(_titleLabel, "Resultado pendiente");
        SetText(_outcomeLabel, "Todavía no hay un escrutinio resuelto.");
        SetText(_tallyLabel, "La campaña aún no produjo un resultado final.");
        SetText(_causesLabel, "Volvé al tablero para continuar la campaña.");
        SetText(_statusLabel, "No se puede mostrar una lectura de elección sin un resultado persistido.");
        ConfigureNavigation(true, false);
    }

    private static string FormatElectionOutcome(CampaignElectionResult result)
    {
        decimal playerShare = result.Tally.GetValidVoteShare(CampaignCandidateIds.Player);
        if (!result.Outcome.RequiresRunoff)
        {
            return result.Outcome.WinnerId == CampaignCandidateIds.Player
                ? $"Ganaste la primera vuelta con {playerShare:0.0}% de los votos válidos."
                : $"Ganó {GetCandidateName(result.Outcome.WinnerId)}. Tu candidatura obtuvo {playerShare:0.0}%.";
        }

        return $"Balotaje confirmado · {GetCandidateName(result.Outcome.RunoffFirstId)} vs. {GetCandidateName(result.Outcome.RunoffSecondId)}. Tu candidatura obtuvo {playerShare:0.0}%.";
    }

    private static string FormatEliminationOutcome(CampaignElectionResult result)
    {
        decimal playerShare = result.Tally.GetValidVoteShare(CampaignCandidateIds.Player);
        return $"Fin de campaña: tu candidatura no alcanzó el balotaje. Pasan {GetCandidateName(result.Outcome.RunoffFirstId)} y {GetCandidateName(result.Outcome.RunoffSecondId)}; obtuviste {playerShare:0.0}% de los votos válidos.";
    }

    private static string FormatRunoffOutcome(CampaignElectionResult result)
    {
        decimal winnerShare = result.Tally.GetValidVoteShare(result.Outcome.WinnerId);
        return result.Outcome.WinnerId == CampaignCandidateIds.Player
            ? $"Victoria: ganaste el balotaje con {winnerShare:0.0}% de los votos válidos."
            : $"Fin de campaña: ganó {GetCandidateName(result.Outcome.WinnerId)} con {winnerShare:0.0}% de los votos válidos.";
    }

    private static string FormatClosureOutcome(CampaignSliceClosureResult result)
    {
        decimal playerShare = result.Tally.GetValidVoteShare(CampaignCandidateIds.Player);
        string outcome = result.Outcome.RequiresRunoff
            ? $"Balotaje de prueba · {GetCandidateName(result.Outcome.RunoffFirstId)} vs. {GetCandidateName(result.Outcome.RunoffSecondId)}."
            : $"Ganó {GetCandidateName(result.Outcome.WinnerId)}.";
        return $"{outcome} Tu candidatura obtuvo {playerShare:0.0}% en el corte del día {result.Day}.";
    }

    private static string FormatTally(ElectionTally tally, IReadOnlyList<string> candidateIds)
    {
        ElectionVoteCounts normalized = ElectionVoteCountNormalizer.Normalize(tally, candidateIds);
        var lines = new List<string>();
        foreach (string candidateId in candidateIds)
        {
            lines.Add($"{GetCandidateName(candidateId)} · {tally.GetValidVoteShare(candidateId):0.0}% · {normalized.GetCandidateVotes(candidateId):N0} votos");
        }

        lines.Add($"Participantes · {normalized.ParticipatingVotes:N0}  |  Votos válidos · {normalized.ValidVotes:N0}  |  Blancos · {normalized.BlankVotes:N0}  |  Indecisos · {normalized.UndecidedVotes:N0}");
        return string.Join("\n", lines.ToArray());
    }

    private string FormatCampaignSummary(CampaignElectionResult firstRound, CampaignElectionResult runoff)
    {
        var builder = new StringBuilder();
        builder.AppendLine("RESUMEN DE CAMPAÑA");
        builder.Append("Duración: día ").Append(_host.Session.Runtime.State.Calendar.CurrentDay)
            .Append(" · fondos finales: $").Append(_host.Session.Runtime.Economy.Funds.ToString("0"))
            .AppendLine();
        builder.Append("Decisiones: ").Append(_host.Session.DecisionRecords.Count)
            .Append(" · noticias y consecuencias: ").Append(_host.Session.News.Items.Count)
            .Append(" · promesas: ").Append(_host.Session.Promises.Count)
            .Append(" · consecuencias diferidas pendientes: ").Append(_host.Session.DeferredConsequences.Count)
            .AppendLine();

        if (firstRound != null)
        {
            builder.Append("Primera vuelta: ").Append(firstRound.Outcome.RequiresRunoff
                ? "hubo balotaje"
                : "hubo ganador directo").AppendLine();
        }

        if (runoff != null)
        {
            builder.Append("Balotaje: ganador ").Append(GetCandidateName(runoff.Outcome.WinnerId)).AppendLine();
        }

        AppendRecentDecisions(builder);
        AppendRecentNews(builder);
        builder.Append("Factores registrados: ").Append(_host.Session.Runtime.State.CauseRecords.Count);
        return builder.ToString();
    }

    private void AppendRecentDecisions(StringBuilder builder)
    {
        if (_host.Session.DecisionRecords.Count == 0) return;

        builder.AppendLine("Hitos de la mesa:");
        int firstIndex = Math.Max(0, _host.Session.DecisionRecords.Count - 4);
        for (int index = firstIndex; index < _host.Session.DecisionRecords.Count; index++)
        {
            CampaignDecisionRecord record = _host.Session.DecisionRecords[index];
            builder.Append("· Día ").Append(record.Day).Append(" · ").Append(record.Activity).AppendLine();
        }
    }

    private void AppendRecentNews(StringBuilder builder)
    {
        if (_host.Session.News.Items.Count == 0) return;

        builder.AppendLine("Últimas señales públicas:");
        int firstIndex = Math.Max(0, _host.Session.News.Items.Count - 4);
        for (int index = firstIndex; index < _host.Session.News.Items.Count; index++)
        {
            NewsItem item = _host.Session.News.Items[index];
            builder.Append("· Día ").Append(item.Day).Append(" · ").Append(item.TopicId).AppendLine();
        }
    }

    private static string FormatClosureCauses(CampaignSliceClosureResult result)
    {
        if (result.DecisiveFactors == null || result.DecisiveFactors.Count == 0)
        {
            return "No se registraron factores decisivos en este corte.";
        }

        var lines = new List<string>();
        foreach (CampaignCausalFactor factor in result.DecisiveFactors)
        {
            lines.Add($"{factor.Category} · {factor.SourceId} · {factor.EffectId} · {factor.Magnitude:0.0} ({factor.Occurrences}x)");
        }

        return string.Join("\n", lines.ToArray());
    }

    private void ConfigureNavigation(bool allowCampaignReturn, bool terminal)
    {
        _returnToCampaignAllowed = allowCampaignReturn;
        if (_returnButton != null)
        {
            _returnButton.interactable = allowCampaignReturn;
            SetButtonLabel(_returnButton, allowCampaignReturn ? "Ir a la mesa" : terminal ? "Campaña terminada" : "Revisar mesa");
        }
    }

    private static void SetButtonLabel(Button button, string value)
    {
        if (button == null) return;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = value;
    }

    private static string FormatElectionCauses(IReadOnlyList<CauseRecord> causes)
    {
        var lines = new List<string>();
        foreach (CauseRecord cause in causes)
        {
            if (cause == null || cause.Category != CauseCategory.Election) continue;
            lines.Add($"Día {cause.Day} · {GetCandidateName(cause.TargetId)} · {cause.EffectId} · {cause.Magnitude:0.0}");
        }

        return lines.Count == 0 ? "El escrutinio no tiene factores causales registrados." : string.Join("\n", lines.ToArray());
    }

    /* Kept as a small compatibility helper for older scene bindings. */
    private string FormatElectionCauses()
    {
        return FormatElectionCauses(_host.Session.Runtime.State.CauseRecords);
    }

    /* Replaced by the campaign summary for new result screens. */
    private static string FormatTallyLegacy(ElectionTally tally)
    {
        return FormatTally(tally, CampaignCandidateIds.All);
    }

    private static string GetCandidateName(string candidateId)
    {
        switch (candidateId)
        {
            case CampaignCandidateIds.Player: return "Tu candidatura";
            case CampaignCandidateIds.Liberales: return "Liberales";
            case CampaignCandidateIds.Contr: return "CONTR";
            case CampaignCandidateIds.Zurditos: return "Zurditos";
            case CampaignCandidateIds.Federales: return "Federales";
            default: return "Candidatura rival";
        }
    }

    private static string GetPhaseName(Poliyo.Application.CampaignPhase phase)
    {
        switch (phase)
        {
            case Poliyo.Application.CampaignPhase.Scrutiny: return "Escrutinio";
            case Poliyo.Application.CampaignPhase.Runoff: return "Balotaje";
            case Poliyo.Application.CampaignPhase.Finished: return "Campaña finalizada";
            case Poliyo.Application.CampaignPhase.SliceClosure: return "Cierre del slice";
            default: return phase.ToString();
        }
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null) label.text = value;
    }
}
}
