# Capability-002 — Incremento 1 — Evidence → Claim

## Objetivo

Demostrar la primera transición de razonamiento de EIP: admitir Evidence para un
documento legible y producir un Claim descriptivo, atómico y trazable mediante
una única Rule fija.

## Entrada

- `local-context.json`, conforme al Local Context Contract versión 1;
- un documento legible;
- un documento con error de lectura declarado.

El Inference Engine no consulta ninguna otra fuente ni artefacto.

## Comando del usuario

Desde el checkout correspondiente al Pull Request:

```bash
export EIP_GITHUB_REPOSITORIES="org/repo"
dotnet run --project src/Eip.Cli -- review https://github.com/org/repo/pull/123
```

## Resultado esperado

```text
local-context.json
        ↓
Evidence admitida
        ↓
document-availability-claim v1
        ↓
Claim válido
        ↓
inference-execution.json
```

El artefacto contiene:

- `rule_set_id` igual a `capability-002-document-availability-rules-v1`;
- estado `claims_produced`;
- Claim Processing completado;
- una Evidence y un Claim para el documento legible;
- un candidato descartado para el documento no legible;
- cero Hypotheses, Findings y Abstentions.

El resultado completo está en
[increment-01-inference-execution.json](examples/increment-01-inference-execution.json).

## Demo ejecutable

La demo controlada verifica que un documento legible produce un Claim y que un
documento no legible se descarta sin degradar el resultado:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~InferencePipelineTests.DiscardsUnreadableDocumentWithoutDegradingValidClaims'
```

Resultado esperado:

```text
Passed: 1, Failed: 0, Skipped: 0
```

## Evidencia observable

Para el documento legible:

```text
Claim
  evidence_ids[0]
        ↓
Evidence.evidence_id
  provenance.contract = local-context-v1
  document_path = docs/payments/README.md
```

El Claim conserva:

- statement exacto y determinista;
- Scope limitado al documento;
- Rule y versión;
- Confidence `strong`, con fundamento, limitaciones y cinco dimensiones;
- Uncertainty vacía porque no existe una limitación material sobre la afirmación
  de disponibilidad.

## Reglas demostradas

- la Rule sólo afirma disponibilidad del documento;
- el contenido no se copia ni interpreta en el registro técnico;
- Evidence y Claim poseen identidades deterministas;
- cada Claim referencia una Evidence existente con el mismo Scope;
- Evidence, Claims y descartes usan orden ordinal por path;
- un documento no legible produce `document_not_readable` y ningún Claim;
- un descarte local no degrada toda la ejecución;
- la entrada y los artefactos anteriores permanecen byte por byte intactos;
- la misma entrada produce exactamente los mismos bytes;
- el nuevo Rule Set cambia `execution_id` respecto del Incremento 0.

## Limitaciones

- existe una sola Rule fija, descriptiva y no configurable;
- el Claim sólo afirma disponibilidad, no contenido, calidad, autoridad,
  vigencia ni relevancia;
- no existe composición avanzada de Scope, Confidence o Uncertainty;
- `inference-execution.json` no es un Inference Report;
- no existen Hypotheses, Findings, Contradiction, Abstention de reporte ni
  Evaluation Harness.

## Decisión de continuar

**Listo para revisión independiente.** El Incremento 1 demuestra Evidence →
Claim sin incorporar ninguna capacidad del Incremento 2.
