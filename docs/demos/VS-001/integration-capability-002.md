# VS-001 — Integración con Capability-002

## Objetivo

Demostrar por primera vez el flujo vertical completo desde una Review Request
hasta un Architecture Review Context Pack construido exclusivamente desde el
Inference Report oficial.

## Flujo

```text
Review Request
      ↓
Context Retrieval
      ↓
local-context.json
      ↓
Inference Engine
      ↓
inference-report.json
      ↓
Architecture Review Orchestrator
      ↓
context-pack.md
```

El artefacto `inference-execution.json` se conserva para diagnóstico técnico,
pero no participa como entrada funcional del Architecture Review Orchestrator.

## Demo ejecutable

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ReviewCommandTests.WritesMinimalEvidenceManifest|FullyQualifiedName~ArchitectureReviewOrchestratorTests'
```

Resultado esperado:

```text
Passed: 4, Failed: 0, Skipped: 0
```

## Evidencia observable

- Review Request produce todos los artefactos de Context Retrieval;
- Capability-002 consume únicamente `local-context.json`;
- `inference-report.json` contiene los Findings contractuales;
- Architecture Review Orchestrator lee únicamente el Inference Report;
- un archivo técnico deliberadamente inválido no afecta el Review;
- ejecuciones repetidas producen los mismos bytes;
- cero Findings produce un Context Pack válido con estado `insufficient`;
- el Context Pack no contiene Claims ni Hypotheses;
- el reviewer conserva explícitamente la decisión.

El resultado de ejemplo está en
[integration-context-pack.md](examples/integration-context-pack.md).

## Limitaciones

- las Rules actuales sólo producen Findings de disponibilidad potencial de
  contexto;
- todavía no existen Findings específicos de arquitectura, riesgos ni contratos;
- no existen Recommendations ni Decisions;
- el Context Pack inicial es compacto y no intenta completar todas las secciones
  futuras del diseño funcional;
- no se consulta ningún artefacto intermedio después de producir el Inference
  Report.

## Decisión de continuar

**Listo para revisión independiente.** La integración demuestra la separación
Context Retrieval → Inference Engine → Architecture Review Intelligence sin
duplicar razonamiento.
