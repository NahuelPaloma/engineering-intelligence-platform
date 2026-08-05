# Capability-002 — Incremento 3 — Hypothesis → Finding

## Objetivo

Demostrar el nacimiento determinista del primer Finding consumible a partir de
una Hypothesis válida, sin interpretar contenido ni producir Recommendations,
Decisions o un Inference Report.

## Entrada

- `local-context.json`, conforme al Local Context Contract versión 1.

La ejecución produce Evidence, Claim e Hypothesis reales mediante los
incrementos anteriores. No utiliza unidades sintéticas ni consulta otras
fuentes.

## Regla fija

```text
rule_id: available-document-context-finding
rule_version: 1
rule_set_id: capability-002-document-context-finding-rules-v1
```

La Rule aplica únicamente a una Hypothesis `valid` producida por
`available-document-context-hypothesis` versión 1, con exactamente un Claim y
una cadena íntegra hasta Evidence.

## Resultado esperado

```text
local-context.json
        ↓
Evidence
        ↓
Claim
        ↓
Hypothesis
        ↓
Finding
        ↓
0 Inference Reports
```

El Finding expresa exactamente:

```text
A document available at '<relative-path>' may provide context for this execution.
```

No establece relevancia, utilidad, autoridad, vigencia ni una acción. El
artefacto completo está en
[increment-03-inference-execution.json](examples/increment-03-inference-execution.json).

## Demo ejecutable

La demo controlada recorre el pipeline completo y verifica categoría, statement,
Scope, Confidence, Uncertainty, trazabilidad, preguntas abiertas, límites de
aplicabilidad y ausencia de Inference Reports:

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
Finding.hypothesis_ids[0]
        ↓
Hypothesis.hypothesis_id
  claim_ids[0]
        ↓
Claim.claim_id
  evidence_ids[0]
        ↓
Evidence.evidence_id
  provenance.contract = local-context-v1
```

Toda la cadena conserva exactamente el Scope del documento. El Finding contiene
Confidence y Uncertainty propias, una pregunta abierta no prescriptiva y límites
explícitos de aplicabilidad. Sólo Validation cambia su estado de `candidate` a
`valid`.

La misma ejecución descarta localmente el documento no legible
`docs/legacy/README.md` como candidato a Claim. El descarte no degrada el
Finding válido ni aparece dentro de `findings`.

## Limitaciones

- existe una sola Rule de Finding, fija y no configurable;
- cada Finding referencia exactamente una Hypothesis y una Evidence;
- no se determina si el documento aporta contexto útil;
- no se evalúan contenido, autoridad, vigencia, relevancia ni aplicabilidad;
- no existe propagación avanzada de Confidence o Uncertainty;
- no existen Contradiction transversal, Abstention de reporte, Inference Report
  ni Evaluation Harness completo;
- no existen Recommendations ni Decisions.

## Decisión de continuar

**Listo para revisión independiente.** El Incremento 3 demuestra Hypothesis →
Finding mediante una transición real, trazable y no prescriptiva. No autoriza el
Incremento 4.
