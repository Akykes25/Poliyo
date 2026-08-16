using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poliyo.Content
{
[CreateAssetMenu(menuName = "Poliyo/Data/Campaign Catalog", fileName = "CampaignContentDefinition")]
public sealed class CampaignContentDefinition : ScriptableObject
{
    [SerializeField] private LocalityDefinition[] _localities = Array.Empty<LocalityDefinition>();
    [SerializeField] private CampaignTeamCandidateDefinition[] _teamCandidates = Array.Empty<CampaignTeamCandidateDefinition>();
    [SerializeField] private CampaignDecisionScenarioDefinition[] _decisionScenarios = Array.Empty<CampaignDecisionScenarioDefinition>();

    public LocalityDefinition[] Localities => _localities;
    public CampaignTeamCandidateDefinition[] TeamCandidates => _teamCandidates == null || _teamCandidates.Length == 0
        ? CampaignContentDefaults.TeamCandidates
        : _teamCandidates;
    public CampaignDecisionScenarioDefinition[] DecisionScenarios => _decisionScenarios == null || _decisionScenarios.Length == 0
        ? CampaignContentDefaults.DecisionScenarios
        : _decisionScenarios;

    public void Configure(LocalityDefinition[] localities)
    {
        if (localities == null) throw new ArgumentNullException(nameof(localities));
        _localities = (LocalityDefinition[])localities.Clone();
    }

    public void Configure(
        LocalityDefinition[] localities,
        CampaignTeamCandidateDefinition[] teamCandidates,
        CampaignDecisionScenarioDefinition[] decisionScenarios)
    {
        Configure(localities);
        if (teamCandidates == null) throw new ArgumentNullException(nameof(teamCandidates));
        if (decisionScenarios == null) throw new ArgumentNullException(nameof(decisionScenarios));
        _teamCandidates = (CampaignTeamCandidateDefinition[])teamCandidates.Clone();
        _decisionScenarios = (CampaignDecisionScenarioDefinition[])decisionScenarios.Clone();
    }

    public CampaignTeamCandidateDefinition[] GetCandidatesForRole(string roleId)
    {
        var result = new List<CampaignTeamCandidateDefinition>();
        foreach (CampaignTeamCandidateDefinition candidate in TeamCandidates)
        {
            if (candidate != null && candidate.RoleId == roleId)
            {
                result.Add(candidate);
            }
        }

        return result.ToArray();
    }

    public CampaignDecisionScenarioDefinition GetDecisionScenario(CampaignActivityId activity, string scenarioId = null)
    {
        string activityId = activity.ToString();
        foreach (CampaignDecisionScenarioDefinition scenario in DecisionScenarios)
        {
            if (scenario == null || !string.Equals(scenario.ActivityId, activityId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(scenarioId) || scenario.Id == scenarioId)
            {
                return scenario;
            }
        }

        return null;
    }
}

public enum CampaignActivityId
{
    Rally,
    Interview,
    Negotiation,
}

[Serializable]
public sealed class CampaignTeamCandidateDefinition
{
    public string Id;
    public string RoleId;
    public string DisplayName;
    public string Initials;
    public string Ideology;
    public string Trajectory;
    public string Experience;
    public string PublicRelationships;
    public string NarrativeClue;
    [HideInInspector] public string HiddenProfileSignal;
}

[Serializable]
public sealed class CampaignDecisionScenarioDefinition
{
    public string Id;
    public string ActivityId;
    public string ActorId;
    public string ActorDisplayName;
    [TextArea(2, 5)] public string Context;
    [TextArea(2, 4)] public string KnownSignal;
    public string TargetMode = "National";
    // Unity serializes float but not decimal. The presentation boundary rounds
    // these authoring values before creating simulation decimals.
    public float BaseCost;
    public CampaignDecisionStepDefinition[] Steps = Array.Empty<CampaignDecisionStepDefinition>();
}

[Serializable]
public sealed class CampaignDecisionStepDefinition
{
    [TextArea(2, 4)] public string Prompt;
    public CampaignDecisionOptionDefinition[] Options = Array.Empty<CampaignDecisionOptionDefinition>();
}

[Serializable]
public sealed class CampaignDecisionOptionDefinition
{
    public string Id;
    public string Label;
    [TextArea(2, 4)] public string Description;
    public string RiskLabel;
    public string ImmediateReaction;
    public float CostModifier;
    public string MetricId = "Trust";
    public float ImpactDelta;
    public float Reach = 0.8f;
    public float Relevance = 0.8f;
    public float Compatibility = 0.8f;
    public float Credibility = 0.8f;
    public float MediaFraming = 0.8f;
    public float Novelty = 0.8f;
    public float TrustDelta;
    public float AffinityDelta;
    public float ObligationDelta;
    public float GrievanceDelta;
    public string PromiseId;
    public string PromiseDescription;
}

internal static class CampaignContentDefaults
{
    public static CampaignTeamCandidateDefinition[] TeamCandidates => CreateCandidates();
    public static CampaignDecisionScenarioDefinition[] DecisionScenarios => CreateScenarios();

    private static CampaignTeamCandidateDefinition[] CreateCandidates()
    {
        var candidates = new List<CampaignTeamCandidateDefinition>();
        string[] roles =
        {
            "vicepresidencia", "jefatura-campana", "jefatura-prensa", "voceria",
            "coordinacion-territorial", "legal-contable", "consultoria-politica", "jefatura-operaciones"
        };
        string[] roleNames =
        {
            "Vicepresidencia", "Jefatura de campaña", "Jefatura de prensa", "Vocería",
            "Coordinación territorial", "Responsable legal / contable", "Consultoría política", "Jefatura de Operaciones"
        };
        string[][] names =
        {
            new[] { "Mara Vico", "Tobías Leiva", "Inés del Puerto" },
            new[] { "Bruno Salvatierra", "Lola Ferreyra", "Nico Cuervo" },
            new[] { "Vera Montalvo", "Paz Roldán", "Ciro Bértola" },
            new[] { "Ema Quiroga", "Ramiro Sosa", "Luz Benítez" },
            new[] { "Tano Gaitán", "Malena Sur", "Roque Vidal" },
            new[] { "Clara Rivas", "Fede Arce", "Julia Cazón" },
            new[] { "Simón Ledesma", "Micaela Borda", "Hugo Neri" },
            new[] { "Nora Cid", "Bautista Paz", "Yamila Costa" },
        };
        string[] ideologies = { "desarrollismo pragmático", "liberalismo municipal", "federalismo social" };
        for (var roleIndex = 0; roleIndex < roles.Length; roleIndex++)
        {
            for (var candidateIndex = 0; candidateIndex < 3; candidateIndex++)
            {
                string name = names[roleIndex][candidateIndex];
                candidates.Add(new CampaignTeamCandidateDefinition
                {
                    Id = roles[roleIndex] + "-perfil-" + (candidateIndex + 1),
                    RoleId = roles[roleIndex],
                    DisplayName = name,
                    Initials = GetInitials(name),
                    Ideology = ideologies[candidateIndex],
                    Trajectory = roleNames[roleIndex] + " con recorrido en la campaña local.",
                    Experience = candidateIndex == 0 ? "Experiencia pública comprobable" : candidateIndex == 1 ? "Red territorial en expansión" : "Perfil técnico y discreto",
                    PublicRelationships = candidateIndex == 2 ? "Relaciones públicas todavía opacas" : "Vínculos visibles con dos referentes provinciales",
                    NarrativeClue = GetClue(roleIndex, candidateIndex),
                    HiddenProfileSignal = candidateIndex == 1 ? "ambición moderada; lealtad contextual" : "rasgo por descubrir",
                });
            }
        }

        return candidates.ToArray();
    }

    private static CampaignDecisionScenarioDefinition[] CreateScenarios()
    {
        return new[]
        {
            CreateRally(),
            CreateInterview("nt-fidel", "NT", "Fidel", "El noticiero nacional pide una frase que pueda titular sin contexto."),
            CreateInterview("nacion-lujan", "La Nación", "Luján", "La cronista llega con una carpeta de promesas incumplidas."),
            CreateInterview("5c-silvester", "5C", "Silvester", "El canal comunitario prioriza el costo de vida y la cercanía."),
            CreateInterview("cadena-samu", "Cadena", "Samu", "La señal de cable quiere una respuesta rápida y compartible."),
            CreateInterview("n48-neiman", "N48", "Neiman", "La entrevista nocturna ofrece tiempo, pero no indulgencia."),
            CreateNegotiation("rival-contr", "Contr", "La rivalidad ofrece un acuerdo territorial si cedés una bandera.", "reconciliación pública"),
            CreateNegotiation("aliada-juana", "Juana Funes", "Una dirigente aliada pide información y una promesa verificable.", "apoyo territorial"),
            CreateNegotiation("medio-cadena", "Cadena", "Un medio propone cobertura favorable a cambio de acceso.", "entrevista exclusiva"),
        };
    }

    private static CampaignDecisionScenarioDefinition CreateRally()
    {
        return new CampaignDecisionScenarioDefinition
        {
            Id = "rally-local",
            ActivityId = "Rally",
            ActorId = "publico-local",
            ActorDisplayName = "Asamblea local",
            Context = "La campaña puede ocupar una plaza, pero la convocatoria y el ánimo del barrio son señales incompletas.",
            KnownSignal = "Creemos que la preocupación dominante es el trabajo y que la estructura local puede sostener un cierre breve.",
            TargetMode = "Locality",
            BaseCost = 80f,
            Steps = new[]
            {
                CreateStep("¿Cómo abrís el acto?", new[]
                {
                    Option("barrio", "Nombrar problemas del barrio", "Conectar con una demanda concreta.", "riesgo bajo", "La plaza reconoce el diagnóstico, aunque exige una salida." , "Trust", 1.4m),
                    Option("bandera", "Poner la identidad de campaña al frente", "Ordenar a la militancia alrededor de una consigna.", "riesgo medio", "La militancia responde; parte del público queda mirando.", "VotingIntention", 1.7m),
                    Option("rival", "Responderle al rival", "Abrir con una crítica que puede marcar agenda.", "riesgo alto", "El rival contesta en redes y la plaza se divide.", "Rejection", 0.9m, grievance: 2m),
                }),
                CreateStep("¿Qué propuesta dejás instalada?", new[]
                {
                    Option("empleo", "Plan de empleo local", "Promesa concreta, con costo político si no se explica.", "riesgo medio", "Los sindicatos piden detalles; la cobertura mejora.", "VotingIntention", 2.1m, obligation: 2m, promise: "empleo-local", promiseDescription: "Presentar el plan de empleo local."),
                    Option("servicios", "Servicios que se puedan medir", "Priorizar una meta pequeña y visible.", "riesgo bajo", "La estructura puede repetir la meta puerta a puerta.", "Trust", 1.8m),
                    Option("futuro", "Un horizonte nacional", "Elevar el tono con una visión de largo plazo.", "riesgo alto", "La multitud aplaude; el cierre queda menos concreto.", "Participation", 1.4m),
                }),
                CreateStep("¿Cómo cerrás?", new[]
                {
                    Option("escucha", "Abrir preguntas", "Aceptar una última ronda de vecinos.", "riesgo medio", "Una pregunta difícil se vuelve aprendizaje para la campaña.", "Trust", 1.3m),
                    Option("unidad", "Cierre de unidad", "Evitar el conflicto y cuidar el tono.", "riesgo bajo", "La salida es prolija y suma confianza moderada.", "Trust", 1.1m),
                    Option("viral", "Frase para el recorte", "Buscar una escena que viaje por redes.", "riesgo alto", "El recorte circula, pero también su interpretación más hostil.", "Rejection", 1.2m, grievance: 1m),
                }),
            },
        };
    }

    private static CampaignDecisionScenarioDefinition CreateInterview(string id, string media, string journalist, string context)
    {
        return new CampaignDecisionScenarioDefinition
        {
            Id = id,
            ActivityId = "Interview",
            ActorId = id,
            ActorDisplayName = media + " · " + journalist,
            Context = context,
            KnownSignal = "El medio puede recortar la respuesta y el sesgo del periodista es una pista, no una certeza.",
            TargetMode = "National",
            BaseCost = 0f,
            Steps = new[]
            {
                CreateStep("La primera pregunta pide una definición incómoda.", new[]
                {
                    Option("directa", "Responder con una definición", "Nombrar la posición sin esconder el costo.", "riesgo medio", "El periodista toma nota y repregunta con precisión.", "Trust", 1.5m),
                    Option("puente", "Llevarla a una propuesta", "Convertir el ataque en agenda.", "riesgo bajo", "La entrevista se vuelve más programática.", "VotingIntention", 1.2m),
                    Option("evasiva", "Pedir contexto antes de responder", "Ganar segundos y observar el encuadre.", "riesgo alto", "El periodista marca la demora como noticia.", "Rejection", 0.8m, grievance: 1m),
                }),
                CreateStep("El periodista muestra una filtración.", new[]
                {
                    Option("confirmar", "Confirmar lo comprobable", "Admitir lo que ya está publicado.", "riesgo medio", "La respuesta parece honesta; la filtración gana volumen.", "Trust", 1.7m),
                    Option("desmentir", "Desmentir y pedir evidencia", "Defender el margen de maniobra.", "riesgo alto", "La sala espera la segunda fuente.", "Rejection", 0.7m),
                    Option("cuidar", "Cuidar la investigación", "No revelar datos que todavía no están verificados.", "riesgo bajo", "El medio conserva el tema para una próxima edición.", "Trust", 1.1m),
                }),
                CreateStep("Te ofrecen una última frase.", new[]
                {
                    Option("barrio", "Hablarle a quien todavía duda", "Cerrar con una promesa acotada.", "riesgo bajo", "La frase se entiende fuera del estudio.", "VotingIntention", 1.5m),
                    Option("equipo", "Reconocer al equipo", "Mostrar que la campaña no depende de una sola voz.", "riesgo medio", "El equipo gana visibilidad y responsabilidad.", "Trust", 1.2m),
                    Option("ataque", "Dejar un ataque final", "Forzar al rival a responder.", "riesgo alto", "La noticia es el ataque, no la propuesta.", "Rejection", 1.1m, grievance: 2m),
                }),
            },
        };
    }

    private static CampaignDecisionScenarioDefinition CreateNegotiation(string id, string actor, string context, string promiseLabel)
    {
        return new CampaignDecisionScenarioDefinition
        {
            Id = id,
            ActivityId = "Negotiation",
            ActorId = id,
            ActorDisplayName = actor,
            Context = context,
            KnownSignal = "No hay amistad automática: pesan acuerdos previos, humillaciones, favores y competencia territorial.",
            TargetMode = "National",
            BaseCost = 20f,
            Steps = new[]
            {
                CreateStep("¿Qué ponés sobre la mesa?", new[]
                {
                    Option("info", "Compartir información acotada", "Entregar una pieza útil sin regalar toda la carpeta.", "riesgo medio", "La contraparte valora el gesto y pide reciprocidad.", "Trust", 1.1m, affinity: 2m),
                    Option("favor", "Ofrecer un favor", "Abrir una deuda que después puede exigirse.", "riesgo alto", "El acuerdo avanza, pero queda una obligación.", "VotingIntention", 1.4m, obligation: 4m, promise: "favor-politico", promiseDescription: promiseLabel),
                    Option("nada", "No conceder todavía", "Probar la firmeza de la contraparte.", "riesgo bajo", "La reunión continúa con menos entusiasmo.", "Trust", 0.5m, grievance: 1m),
                }),
                CreateStep("La contraparte responde con una condición.", new[]
                {
                    Option("aceptar", "Aceptar con límites", "Registrar qué se entrega y cuándo.", "riesgo medio", "La contraparte firma el entendimiento preliminar.", "Trust", 1.8m, obligation: 2m, promise: "acuerdo-limite", promiseDescription: "Cumplir el límite acordado."),
                    Option("contra", "Contraofertar", "Bajar el costo político antes de cerrar.", "riesgo medio", "La negociación se alarga, pero mejora el margen.", "VotingIntention", 1.3m, affinity: 1m),
                    Option("rechazar", "Rechazar la condición", "Cuidar una línea roja explícita.", "riesgo alto", "La contraparte filtra el rechazo como señal de debilidad.", "Rejection", 0.9m, grievance: 4m),
                }),
                CreateStep("¿Cómo dejás la relación?", new[]
                {
                    Option("publico", "Anunciar un entendimiento", "Poner una fotografía política en circulación.", "riesgo alto", "Los aliados preguntan qué se prometió realmente.", "Trust", 1.6m, affinity: 2m),
                    Option("privado", "Cerrar sin comunicado", "Conservar ambigüedad y margen.", "riesgo bajo", "El acuerdo queda disponible para trabajar en silencio.", "Trust", 1.0m),
                    Option("cortar", "Cortar la reunión", "No hipotecar el día por un acuerdo malo.", "riesgo medio", "La contraparte se retira molesta y conserva memoria del desplante.", "Rejection", 0.8m, grievance: 3m),
                }),
            },
        };
    }

    private static CampaignDecisionStepDefinition CreateStep(string prompt, CampaignDecisionOptionDefinition[] options)
    {
        return new CampaignDecisionStepDefinition { Prompt = prompt, Options = options };
    }

    private static CampaignDecisionOptionDefinition Option(
        string id, string label, string description, string risk, string reaction, string metric, decimal delta,
        decimal trust = 0m, decimal affinity = 0m, decimal obligation = 0m, decimal grievance = 0m,
        string promise = null, string promiseDescription = null)
    {
        return new CampaignDecisionOptionDefinition
        {
            Id = id,
            Label = label,
            Description = description,
            RiskLabel = risk,
            ImmediateReaction = reaction,
            MetricId = metric,
            ImpactDelta = (float)delta,
            TrustDelta = (float)trust,
            AffinityDelta = (float)affinity,
            ObligationDelta = (float)obligation,
            GrievanceDelta = (float)grievance,
            PromiseId = promise,
            PromiseDescription = promiseDescription,
        };
    }

    private static string GetInitials(string name)
    {
        string[] parts = name.Split(' ');
        return (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant();
    }

    private static string GetClue(int roleIndex, int candidateIndex)
    {
        string[] clues = { "Pide tiempo antes de opinar.", "Conoce a demasiada gente en la provincia.", "Tiene una pregunta preparada para cada crisis." };
        return clues[(roleIndex + candidateIndex) % clues.Length];
    }
}
}
