# Capability-002 — Incremento 4 — Controles transversales

## Objetivo

Demostrar propagación determinista de Confidence y Uncertainty, preservación de
Contradiction, Abstention local, parcial y total, y cobertura explícita sin
crear un Inference Report.

## Flujo real

```text
local-context.json
        ↓
Evidence → Claim → Hypothesis → Finding
        ↓
Confidence + Uncertainty + Coverage
        ↓
inference-execution.json
        ↓
0 Inference Reports
```

El flujo real no fabrica Contradictions ni convierte documentos descartados en
Abstentions. Los casos transversales que todavía no nacen naturalmente se
demuestran mediante fixtures controlados con unidades válidas y Evidence
distintas.

El registro real de ejemplo está en
[increment-04-inference-execution.json](examples/increment-04-inference-execution.json).

## Demo ejecutable

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ReasoningControlsTests|FullyQualifiedName~InferencePipelineTests.ProducesAtomicTraceableClaimForReadableDocument'
```

Resultado esperado:

```text
Passed: 10, Failed: 0, Skipped: 0
```

## Evidencia observable

- Confidence nunca supera su soporte ni aumenta por repetición;
- Uncertainty material se hereda, conserva origen y no se duplica;
- una Contradiction controlada conserva posiciones, Evidence y Scope sin elegir
  precedencia;
- Scope no comparable descarta el candidato a Contradiction;
- Validation autoriza Abstention local, parcial y total;
- una Abstention total conserva cero Scope restante;
- un descarte rutinario permanece como descarte y no genera Abstention;
- cobertura real distingue Scope total, procesado y no cubierto;
- IDs y orden permanecen deterministas;
- el pipeline no accede a fuentes ni produce Inference Report.

## Limitaciones

- el flujo real actual no genera Contradictions porque sus Rules producen
  afirmaciones independientes y compatibles;
- el flujo real actual no requiere Abstention epistemológica; los tres alcances
  se validan mediante casos controlados;
- no existe representación transversal completa para dominios adicionales;
- `inference-execution.json` sigue siendo un registro técnico temporal;
- Inference Report Builder y publicación contractual quedan para Incremento 5.

## Decisión de continuar

**Listo para revisión independiente.** El Incremento 4 demuestra los controles
transversales sin anticipar ninguna parte del Incremento 5.
