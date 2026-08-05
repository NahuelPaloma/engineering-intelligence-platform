# Capability-003 — Incremento 1 — Modified File Evidence Admission

## Objetivo

Demostrar que el profile de Capability-003 admite exclusivamente Modified File
Evidence ya validada por Input Boundary, preserva su disponibilidad y calcula
Coverage técnica sin ejecutar Candidate Rules.

## Flujo

```text
local-context.json
        ↓
Input Boundary compartido
        ↓
Capability-003 profile
        ↓
Modified File Evidence Admission
        ↓
Validation de Coverage y Abstention
        ↓
0 Candidates · 0 Types · 0 Detections · 0 Contract Reports
```

## Demo ejecutable

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ExecutionProfileTests|FullyQualifiedName~ModifiedFileEvidenceAdmissionTests'
```

Resultado esperado:

```text
Passed: 13, Failed: 0, Skipped: 0
```

## Caso A — `not_provided`

Cuando `modified_files` no fue transportado:

- availability permanece `not_provided`;
- Coverage es `unknown`;
- la causa es `modified_files_not_provided`;
- Validation autoriza una Abstention total;
- no se afirma que existan cero archivos o contratos.

Ejemplo:
[increment-01-not-provided.json](examples/increment-01-not-provided.json).

## Caso B — `available` vacío

Cuando el campo fue proporcionado con cero elementos:

- availability permanece `available`;
- Coverage es `complete`;
- todos los Scope están vacíos;
- no existe causa de degradación;
- no se produce Abstention.

Ejemplo:
[increment-01-available-empty.json](examples/increment-01-available-empty.json).

## Caso C — `available` con elementos

Cada elemento conserva identidad, posición contractual, path, `change_status`,
provenance y Scope. Como todavía no existen Candidate Rules:

- Coverage es `partial`;
- `processed_scope` está vacío;
- `uncovered_scope` contiene exactamente todos los elementos en orden;
- la causa es `no_candidate_rules_registered`;
- ningún elemento se presenta como Contract Candidate ni Contract Type.

Ejemplo:
[increment-01-available-files.json](examples/increment-01-available-files.json).

## Identidad

Para profiles nuevos, `execution_id` deriva de una canonicalización estructurada
de:

```text
canonical_context_identity
contract_id
plugin_id
plugin_version
profile_id
profile_version
rule_set_id
taxonomy_id
taxonomy_version
```

El tuple Accepted de Capability-002 conserva su fórmula histórica y sus bytes.
Cambiar plugin, versión de plugin, profile, Rule Set, taxonomía o entrada cambia
la identidad de los profiles nuevos.

## Garantías demostradas

- existe un único Input Boundary, Pipeline y Rule Runtime;
- el registry es un `FrozenDictionary` con exactamente dos profiles;
- no se relee `local-context.json` después de Input Boundary;
- no se lee `manifest.json`, el repositorio ni los paths admitidos;
- no existe acceso HTTP;
- Validation reconcilia Coverage y autoriza Abstention;
- ejecuciones repetidas producen los mismos bytes;
- Capability-002 conserva su forma, identidades y comportamiento Accepted;
- errores de entrada fallan sin publicar salida parcial;
- no se genera `contract-change-report.json`.

## Limitaciones

- no existen Candidate Rules;
- no existen Contract Candidates, Contract Types ni Contract Detections;
- la Coverage `partial` sólo expresa Evidence pendiente de futuras Rules;
- no se usa `outside_coverage` porque todavía no existe taxonomía de dominio;
- no existe Detection Sufficiency por Candidate ni Analysis Readiness;
- no existe parsing, compatibilidad, breaking change, severidad, Recommendation
  ni Decision.

## Decisión de continuar

**Listo para revisión independiente.** El incremento demuestra admisión y
Coverage sin autorizar el Incremento 2.
