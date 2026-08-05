# Capability-003 — Incremento 0 — Extension Boundary y coexistencia

## Objetivo

Demostrar que Capability-002 y el profile vacío de Capability-003 pueden
ejecutarse mediante el mismo Inference Engine sin compartir Rules ni taxonomía.

## Entrada

- un `local-context.json` válido;
- selección explícita de uno de los dos execution profiles registrados.

## Flujo demostrado

```text
Capability-002 profile
        ↓
Inference Engine compartido
        ↓
Inference Report

Capability-003 empty profile
        ↓
Inference Engine compartido
        ↓
No Rules → No unidades → No Contract Report
```

## Demo ejecutable

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ExecutionProfileTests'
```

Resultado esperado:

```text
Passed: 7, Failed: 0, Skipped: 0
```

## Evidencia observable

- seleccionar explícitamente el profile de Capability-002 produce
  `inference-execution.json` e `inference-report.json` byte por byte iguales a
  los producidos por el comportamiento predeterminado;
- ambos profiles atraviesan `InferencePipeline`,
  `InferenceEngineExtensionBoundary`, el único `RuleRuntime`, Validation e
  Inference Report Builder;
- el profile de Capability-002 conserva sus tres Rule descriptors;
- el profile de Capability-003 contiene cero Rules;
- ambos profiles usan el mismo adapter de aislamiento y Validation;
- sus identidades de profile, Rule Set, taxonomía y ejecución son distintas;
- una unidad respaldada por una Rule ajena al profile seleccionado es rechazada;
- el profile vacío termina con cero Evidence derivada, Claims, Hypotheses,
  Findings, Contradictions y Abstentions;
- el profile vacío produce una ejecución válida y un Inference Report mediante
  el límite existente;
- no se genera `contract-change-report.json`.

## Compatibilidad de identidad

El tuple Accepted del profile actual de Capability-002 conserva su
`execution_id` histórico para mantener los artefactos existentes byte por byte.
Todo profile nuevo usa una identidad canónica que incorpora explícitamente
profile, Rule Set y taxonomía. Cambiar cualquiera de esos componentes cambia la
identidad.

## Limitaciones

- Capability-003 no contiene Rules;
- no existe Contract Candidate, Contract Type ni Detection;
- no existe Contract Change Report;
- el profile se selecciona mediante el límite interno; este incremento no agrega
  un comando público nuevo;
- el registro contiene exactamente dos profiles compilados y no admite carga
  dinámica;
- no existe parsing, compatibilidad, breaking change, Recommendation ni
  Decision.

## Decisión de continuar

**Listo para revisión independiente.** La demo prueba coexistencia y aislamiento
sin autorizar Candidate Rules ni el Incremento 1.
