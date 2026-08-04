# Capability-002 — Incremento 2 — Claim → Hypothesis

## Objetivo

Demostrar el nacimiento determinista de una Hypothesis provisional a partir de
un único Claim válido, sin interpretar texto, combinar Scopes ni producir
Findings.

## Entrada

- `local-context.json`, conforme al Local Context Contract versión 1.

La ejecución genera Evidence y Claims reales mediante el Incremento 1. No usa
Claims sintéticos ni consulta otras fuentes.

## Regla fija

```text
rule_id: available-document-context-hypothesis
rule_version: 1
rule_set_id: capability-002-document-context-rules-v1
```

La Rule aplica únicamente a un Claim `valid` producido por
`document-availability-claim` versión 1, con Evidence existente y Scope íntegro.

## Resultado esperado

```text
local-context.json
        ↓
Evidence
        ↓
Claim
        ↓
Hypothesis unaria
        ↓
0 Findings
```

La Hypothesis expresa exactamente:

```text
The available document at '<relative-path>' may provide context for this execution.
```

Conserva el mismo Scope del Claim y referencia exactamente un `claim_id`.

El artefacto completo está en
[increment-02-inference-execution.json](examples/increment-02-inference-execution.json).

## Demo ejecutable

La demo controlada recorre el pipeline completo y verifica Hypothesis, Scope,
Confidence, Uncertainty, verificabilidad, falsabilidad y cero Findings:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~InferencePipelineTests.ProducesAtomicTraceableClaimForReadableDocument'
```

Resultado esperado:

```text
Passed: 1, Failed: 0, Skipped: 0
```

## Evidencia observable

```text
Hypothesis.claim_ids[0]
        ↓
Claim.claim_id
  evidence_ids[0]
        ↓
Evidence.evidence_id
  provenance.contract = local-context-v1
```

La Hypothesis contiene:

- Confidence propia con nivel `moderate`;
- Uncertainty obligatoria sobre uso contextual y aspectos no evaluados;
- condición explícita de verificación;
- condición explícita de falsación;
- estado `valid` decidido únicamente por Validation.

## Reglas demostradas

- cada Hypothesis utiliza exactamente un Claim real;
- Hypothesis y Claim conservan exactamente el mismo Scope documental;
- la Rule rechaza Claims de otra Rule, con trazabilidad rota o Scope
  inconsistente;
- la expresión `may provide context` permanece provisional;
- no se consulta Evidence adicional para verificar o falsar la Hypothesis;
- Hypotheses y descartes poseen orden estable;
- ejecuciones repetidas producen exactamente los mismos bytes;
- `execution_id` cambia al fijar el nuevo Rule Set;
- no se combinan Claims ni Scopes.

## Limitaciones

- existe una sola Rule de Hypothesis, fija y no configurable;
- no se determina si el documento aporta contexto útil;
- no se evalúan contenido, autoridad, vigencia, calidad ni relevancia;
- la combinación de múltiples Claims queda postergada;
- no existe propagación avanzada de Confidence o Uncertainty;
- no existen Findings, Contradiction transversal, Inference Report ni Evaluation
  Harness.

## Decisión de continuar

**Listo para revisión independiente.** El Incremento 2 demuestra Claim →
Hypothesis mediante una transición unaria real. No autoriza el Incremento 3.
