# Capability-002 — Incremento 0 — Arquitectura vacía

## Objetivo

Demostrar que el Inference Engine puede validar `local-context.json`, fijar una
identidad de ejecución y recorrer un pipeline vacío sin producir inferencias ni
consultar fuentes externas.

## Entrada

- `local-context.json`, conforme al Local Context Contract versión 1.

## Comando del usuario

Desde el checkout correspondiente al Pull Request:

```bash
export EIP_GITHUB_REPOSITORIES="org/repo"
dotnet run --project src/Eip.Cli -- review https://github.com/org/repo/pull/123
```

## Resultado esperado

```text
output/<pack-id>/
├── manifest.json
├── readmes.json
├── readme-contents.json
├── readme-metadata.json
├── readme-ranking.json
├── local-context.json
└── inference-execution.json
```

`inference-execution.json` contiene:

- `execution_id` determinista;
- `input_pack_id`;
- `rule_set_id` igual a `capability-002-empty-rules-v1`;
- `status` igual a `no_inferences`;
- Input Boundary completado y etapas futuras no implementadas;
- contadores de Evidence, Claims, Hypotheses, Findings y Abstentions en cero.

El registro de ejemplo está en
[increment-00-inference-execution.json](examples/increment-00-inference-execution.json).

## Demo ejecutable

La demo controlada ejecuta el pipeline sobre un Local Context válido y verifica
identidad, Rule Set, orden de etapas, estado y contadores:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~InferencePipelineTests.ExecutesEmptyPipelineWithoutProducingInferences'
```

Resultado esperado:

```text
Passed: 1, Failed: 0, Skipped: 0
```

## Reglas demostradas

- Input Boundary lee únicamente `local-context.json` y rechaza entradas
  incompatibles;
- `execution_id` depende de la identidad canónica del contexto admitido, el
  contrato soportado y el Rule Set vacío;
- el orden de las etapas es estable;
- la ejecución termina antes de Claim Processing;
- no existen Evidence de dominio, Claims, Hypotheses, Findings ni Abstentions;
- no existe acceso HTTP ni lectura de archivos originales del repositorio;
- los artefactos del Context Retrieval Pipeline permanecen byte por byte
  intactos;
- ejecuciones repetidas producen exactamente los mismos bytes;
- una falla no publica una salida parcial.

## Limitaciones

- `inference-execution.json` es un registro técnico temporal, no el Inference
  Report;
- el Rule Set está vacío y sólo posee identidad estable;
- los documentos del Local Context se validan, pero todavía no se transforman en
  Evidence de dominio;
- Claim Processing, Hypothesis Processing, Finding Processing y Report Builder
  no están implementados;
- no existen Confidence, Uncertainty, Contradiction, Traceability de unidades
  derivadas ni Evaluation.

## Decisión de continuar

**Listo para revisión independiente.** El Incremento 0 demuestra una frontera de
entrada y una ejecución vacía, determinista y observable. No autoriza el
Incremento 1.
