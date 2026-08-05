# TD-004 — Contract Change Detection

| Campo                | Valor                                      |
| -------------------- | ------------------------------------------ |
| Documento            | TD-004                                     |
| Capability           | Capability-003 — Contract Change Detection |
| Estado               | **Accepted**                               |
| Tipo                 | Technical Design                           |
| Owner propuesto      | Engineering Platform                       |
| Audiencia            | Architecture, Engineering y Quality        |
| Última actualización | 5 de agosto de 2026                        |

---

## 1. Propósito y autoridad

Este documento define el diseño técnico oficial para implementar Capability-003
sin modificar su dominio ni debilitar sus contratos.

Transforma en una arquitectura implementable:

1. Capability-003 — Contract Change Detection;
2. Contract Change Detection Contract;
3. Contract Change Detection Reasoning Specification.

También respeta como límites Accepted:

- Product Vision v1.1;
- Architecture v1.0;
- ADR-013 — Local Context Contract;
- Local Context Contract;
- Capability-002 — Inference Engine;
- Inference Report Contract;
- Inference Engine Reasoning Specification;
- TD-002 — Inference Engine;
- TD-003 — Inference Report.

Este documento decide arquitectura técnica. No redefine Modified File Evidence,
Contract Candidate, Contract Detection, Contract Type, Classification Basis,
Detection Sufficiency, Analysis Readiness, Scope, Confidence, Uncertainty,
Traceability, Coverage, Abstention ni Contradiction.

Ante una contradicción prevalece la fuente normativa de mayor autoridad.

## 2. Objetivos del diseño

El diseño debe permitir:

1. ejecutar Contract Change Detection como extensión declarativa de
   Capability-002;
2. reutilizar Input Boundary, Rule Runtime, procesamiento epistemológico,
   Validation, Traceability, Confidence, Uncertainty e Inference Report Builder;
3. admitir exclusivamente Modified File Evidence ya presente en
   `local-context.json`;
4. fijar Rules y taxonomía por ejecución;
5. producir Candidates, Detections, Sufficiency, Readiness y Coverage
   deterministas;
6. preservar abstención y contradicción;
7. publicar el futuro Contract Change Report sin reinterpretar Findings;
8. evaluar calidad con Golden Dataset independiente;
9. mantener una única CLI y los deployables actuales;
10. introducir sólo componentes justificados por Capability-003.

El diseño optimiza primero por corrección, trazabilidad y simplicidad. Volumen,
latencia y generalización se optimizan después de medir.

## 3. No objetivos técnicos

TD-004 no diseña ni autoriza:

- lectura de archivos;
- acceso al checkout o repositorio;
- consultas a GitHub u otros providers;
- diff, patch o comparación base/head;
- parsing de contratos;
- interpretación de contenido;
- validación sintáctica de OpenAPI, AsyncAPI, GraphQL u otro formato;
- breaking change detection;
- cálculo de compatibilidad;
- severidad, riesgo, criticidad o impacto;
- Recommendations, Decisions o acciones;
- nuevas fuentes de Evidence;
- modificación de VS-001;
- modificación de contratos existentes;
- un segundo Inference Engine;
- una plataforma general de plugins;
- carga dinámica de ensamblados;
- un lenguaje o DSL de Rules;
- base de datos, cache distribuida, colas o servicios;
- nuevos proyectos, procesos o deployables por defecto.

## 4. Decisión arquitectónica principal

Capability-003 será un **plugin declarativo in-process** del Inference Engine.

En TD-004, “plugin” significa exclusivamente:

- un descriptor inmutable de capability;
- un execution profile identificado y versionado;
- un conjunto explícito de Rules registradas;
- una taxonomía explícita y versionada;
- validaciones de dominio;
- una proyección contractual pura;
- casos de Evaluation asociados.

No significa:

- descubrimiento en runtime;
- instalación externa;
- carga dinámica;
- aislamiento en proceso separado;
- marketplace;
- protocolo general;
- infraestructura para futuras capabilities.

El plugin se registra explícitamente dentro de la aplicación actual. El Rule
Runtime existente ejecuta sus Rules y los módulos de Capability-002 producen y
validan Evidence, Claims, Hypotheses y Findings.

Esta decisión evita duplicar el motor y mantiene el conocimiento de dominio
fuera de su núcleo epistemológico.

## 5. Responsabilidades

### 5.1 Capability-003 plugin

Debe:

- declarar identidad y versión;
- declarar contrato de entrada soportado;
- registrar taxonomía y Rules de detección;
- seleccionar únicamente Modified File Evidence;
- declarar validaciones específicas de dominio;
- declarar orden de evaluación;
- proyectar Findings válidos al Contract Change Detection Contract;
- aportar casos de Evaluation.

Nunca debe recuperar Evidence, decidir publicación genérica del Inference Report
ni cambiar leyes de Capability-002.

### 5.2 Inference Engine

Continúa siendo responsable de:

- Input Boundary;
- identidad de ejecución;
- Rule Runtime;
- Claim, Hypothesis y Finding Processing;
- Validation;
- Traceability;
- Confidence y Uncertainty;
- Contradiction y Abstention;
- Inference Report.

Capability-003 consume estos mecanismos; no los reimplementa.

### 5.3 Contract Change Report boundary

Debe:

- seleccionar únicamente unidades válidas de Capability-003;
- preservar estado, Scope, Confidence, Uncertainty y referencias;
- reconciliar Coverage;
- validar el contrato especializado;
- publicar de forma atómica;
- no crear nuevas inferencias.

### 5.4 Evaluation

Debe operar fuera del camino de producción, comparar salidas con expectativas
gobernadas y nunca modificar Evidence, Rules ni resultados.

## 6. Arquitectura general

```text
local-context.json
        ↓
Existing Input Boundary
        ↓
Admitted Modified File Evidence
        ↓
Capability-003 Execution Profile
        ├── Rule Registry
        ├── Taxonomy Registry
        └── Domain Validation Policy
        ↓
Existing Inference Pipeline
Evidence → Claim → Hypothesis → Finding
        ↓
Existing Inference Report Builder + Validation
        ↓
Validated inference-report
        ↓
Contract Change Report Projection
        ↓
Contract Change Report Validation
        ↓
contract-change-report
```

El flujo permanece dentro de la CLI y del proceso actual.

### 6.1 Fronteras lógicas

Se agregan namespaces o carpetas lógicas, no proyectos físicos:

- perfil de Capability-003;
- Rules de detección;
- taxonomía;
- validación de dominio;
- proyección contractual;
- Evaluation.

Separar físicamente requeriría una razón demostrable de dependencia, seguridad,
despliegue o testing. Ninguna existe actualmente.

### 6.2 Dependencias permitidas

```text
Capability-003 plugin
        ↓
Inference Engine extension boundary
        ↓
Existing Capability-002 modules
```

El motor no depende del plugin concreto. El execution profile conoce los
contratos internos estables del motor; el plugin aporta datos declarativos y
evaluadores acotados.

Contract Change Report Projection depende del Inference Report validado y del
contrato de Capability-003. No depende de Local Context ni de fuentes.

## 7. Pipeline interno

El pipeline técnico es:

```text
Profile Resolution
        ↓
Input Admission
        ↓
Modified File Evidence Selection
        ↓
Candidate Rule Evaluation
        ↓
Classification Rule Evaluation
        ↓
Sufficiency Evaluation
        ↓
Readiness Evaluation
        ↓
Reasoning Validation
        ↓
Inference Report
        ↓
Detection Projection
        ↓
Contract Validation
        ↓
Atomic Publication
```

Cada etapa recibe datos inmutables y produce candidatos o decisiones explícitas.
Ninguna etapa consulta estado externo.

## 8. Etapas del pipeline

### 8.1 Profile Resolution

**Entrada:** identificador explícito de Capability-003 y registros compilados.

**Salida:** execution profile cerrado con identidad, versión, Rules, taxonomía,
orden y políticas.

**Responsabilidad:** seleccionar una única configuración gobernada antes de
admitir Evidence.

**Nunca:** descubre plugins, descarga Rules, fusiona perfiles ni usa defaults
ambiguos.

### 8.2 Input Admission

**Entrada:** `local-context.json`.

**Salida:** resultado existente de Input Boundary con identidad canónica,
disponibilidad y Modified File Evidence.

**Responsabilidad:** reutilizar validación y canonicalización existentes.

**Nunca:** modifica Input Boundary, relee manifest ni corrige Evidence.

### 8.3 Modified File Evidence Selection

**Entrada:** colección admitida y estado available/not_provided.

**Salida:** Scope elegible ordenado o condición de Abstention total.

**Responsabilidad:** seleccionar únicamente Modified File Evidence para este
perfil.

**Nunca:** lee documentos, paths físicos o contenido.

### 8.4 Candidate Rule Evaluation

**Entrada:** Evidence elegible, Candidate Rules y taxonomía fijadas.

**Salida:** Candidate Claims, descartes y estados no aplicables.

**Responsabilidad:** evaluar señales declaradas de candidatura.

**Nunca:** asigna Type sin Classification Basis ni publica Claims.

### 8.5 Classification Rule Evaluation

**Entrada:** Candidates válidos, Classification Rules y Taxonomy Registry.

**Salida:** Classification Hypotheses candidatas, Unknown, Ambiguous o
outside_coverage.

**Responsabilidad:** proponer Types respaldados y conservar alternativas.

**Nunca:** parsea archivos, elige precedencia implícita ni valida formato.

### 8.6 Sufficiency Evaluation

**Entrada:** Evidence, Candidates, Basis y precondiciones de detección.

**Salida:** Detection Sufficiency candidata por unidad y ejecución.

**Responsabilidad:** distinguir sufficient, partial, insufficient y
not_provided.

**Nunca:** completa Evidence faltante ni calcula Readiness por implicación.

### 8.7 Readiness Evaluation

**Entrada:** Detection candidata y precondiciones versionadas de entrega.

**Salida:** ready, not_ready, unknown o not_applicable.

**Responsabilidad:** evaluar sólo condiciones declaradas para el siguiente
límite.

**Nunca:** conoce Capability-004, ejecuta análisis ni promete éxito.

### 8.8 Reasoning Validation

**Entrada:** candidatos epistemológicos y resultados de dominio.

**Salida:** unidades válidas, descartes, Contradictions y Abstentions.

**Responsabilidad:** reutilizar Validation existente y agregar verificaciones de
las Laws de Capability-003.

**Nunca:** repara referencias, relaja Rules ni fabrica estados.

### 8.9 Inference Report

**Entrada:** Findings y estados ya validados.

**Salida:** Inference Report validado.

**Responsabilidad:** usar TD-003 sin cambios conceptuales.

**Nunca:** agrega semántica de Capability-003 dentro del Builder genérico.

### 8.10 Detection Projection

**Entrada:** Inference Report validado y execution profile fijado.

**Salida:** candidato a Contract Change Report.

**Responsabilidad:** proyectar, no inferir; componer Candidates, Detections,
Sufficiency, Readiness y Coverage desde unidades ya validadas.

**Nunca:** recibe descartes; re-deriva Evidence, Claims, Hypotheses o Findings;
crea Candidates o Detections mediante razonamiento nuevo; recalcula Confidence,
Uncertainty, Scope, Sufficiency o Readiness; ni consulta Local Context.

### 8.11 Contract Validation

**Entrada:** candidato a Contract Change Report.

**Salida:** complete, incomplete, invalid o fallo de construcción.

**Responsabilidad:** verificar Contract Change Detection Contract.

**Nunca:** transforma invalid en Abstention ni autoriza resultados prohibidos.

### 8.12 Atomic Publication

**Entrada:** reporte autorizado.

**Salida:** un artefacto completo o ninguna salida nueva.

**Responsabilidad:** reutilizar el patrón de escritura temporal y reemplazo
atómico de la CLI.

**Nunca:** deja salida parcial ni modifica Inference Report.

## 9. Componentes

### 9.1 Capability Plugin Descriptor

Descriptor inmutable que reúne identidad, versión, contrato soportado, execution
profile, Rules, taxonomía, validaciones y Evaluation suite.

Justificación: evita parámetros dispersos y fija condiciones antes de ejecutar.

### 9.2 Execution Profile Resolver

Selecciona por identificador explícito un descriptor registrado.

Justificación: el motor actual posee un Rule Set fijo; Capability-003 necesita
seleccionar otro conjunto sin duplicar pipeline.

No es router general ni sistema de plugins.

### 9.3 Inference Engine Extension Boundary

#### Responsabilidad

Permite que Capability-002 ejecute un execution profile externo, cerrado e
inmutable, sin incorporar conocimiento del dominio de Capability-003.

Recibe conceptualmente:

- execution profile identity;
- Rule Set identity;
- Rule descriptors;
- orden de evaluación;
- Domain Rule Adapters;
- políticas de Validation;
- referencias a taxonomía únicamente mediante identidades declarativas;
- límites y versiones soportadas.

Expone al pipeline existente únicamente capacidades genéricas para:

- enumerar Rules;
- evaluar aplicabilidad;
- construir candidatos epistemológicos;
- ejecutar Validation;
- producir Inference Report.

#### Prohibiciones

El boundary nunca:

- conoce Contract Type;
- conoce Contract Candidate;
- conoce Detection Sufficiency;
- conoce Analysis Readiness;
- depende de Capability-003;
- contiene Rules concretas;
- resuelve taxonomías;
- carga plugins dinámicamente;
- crea un segundo pipeline;
- modifica Contracts, Laws o responsabilidades de Capability-002;
- decide publicación.

#### Justificación

TD-002 ya define conceptualmente al Rule Runtime como consumidor del conjunto
gobernado de Rules vigente para la ejecución. El Rule Set fijo de la
implementación actual es una primera implementación concreta, no una restricción
normativa permanente.

Hacer paramétrico ese punto es una evolución interna compatible: no cambia
contratos, no cambia el modelo epistemológico y no introduce conocimiento de
dominio dentro del motor.

#### Forma mínima

El boundary es in-process, explícito, inmutable por ejecución y registrado en
código. No usa reflexión, descubrimiento, carga dinámica ni infraestructura
adicional.

### 9.4 Rule Registry

Registro inmutable de Rule descriptors de Capability-003.

Justificación: reproducibilidad exige conocer identidad, versión, orden y
aplicabilidad de cada Rule.

### 9.5 Taxonomy Registry

Registro inmutable de Contract Types y relaciones permitidas.

Justificación: Type no puede ser string libre ni conocimiento implícito en una
Rule.

### 9.6 Domain Rule Adapter

Traduce el resultado declarativo de una Rule al candidato epistemológico que el
Rule Runtime existente procesa.

Justificación: mantiene semántica de dominio fuera de Claim/Hypothesis/Finding
Processing sin crear un segundo motor.

### 9.7 Domain Validation Policy

Agrupa verificaciones específicas de Capability-003 que complementan, sin
reemplazar, Validation existente.

Justificación: las prohibiciones de Type, Sufficiency y Readiness no pertenecen
al núcleo agnóstico de Capability-002.

### 9.8 Detection Projection

Compone el candidato contractual desde Inference Report validado.

Justificación: Capability-003 es productor único de su contrato y no puede
delegar al consumidor la reconstrucción.

### 9.9 Contract Change Report Validation

Decide conformidad y publicación del reporte especializado.

Justificación: un Inference Report válido no garantiza por sí solo Coverage,
Sufficiency y estados específicos del contrato de Capability-003.

### 9.10 Evaluation Harness Extension

Ejecuta Golden Dataset y métricas de Capability-003 usando el arnés existente.

Justificación: precisión de candidatura y clasificación requieren expectativas
de dominio separadas del runtime productor.

## 10. Interfaces internas

Las interfaces son límites lógicos, no APIs públicas ni protocolos.

### 10.1 Plugin Descriptor boundary

Expone:

- plugin ID y versión;
- execution profile ID;
- contract ID soportado;
- Rule Set ID;
- taxonomy ID y versión;
- registros inmutables;
- políticas de Validation;
- suite de Evaluation.

### 10.2 Inference Engine Extension Boundary

Recibe un execution profile validado y ofrece al pipeline existente:

- Rule Set cerrado;
- orden total de Rules;
- adaptadores declarativos;
- políticas de validación de dominio;
- identidad de profile, taxonomía y Rules;
- resultados normalizados de aplicabilidad.

El motor conserva autoridad epistemológica y el plugin conserva conocimiento de
dominio. El boundary no publica unidades ni reconstruye un segundo Rule Runtime.

### 10.3 Rule Registry boundary

Permite únicamente:

- enumerar Rules en orden estable;
- resolver Rule por identidad exacta;
- verificar unicidad y versión;
- obtener aplicabilidad declarada.

No permite mutación durante una ejecución.

### 10.4 Taxonomy Registry boundary

Permite:

- resolver Type por identidad;
- comprobar si está soportado;
- conocer versión taxonómica;
- detectar identidades duplicadas;
- validar alternativas incompatibles declaradas.

No clasifica Evidence.

### 10.5 Domain Rule Evaluation boundary

Recibe una Evidence, Rule, taxonomía y Scope inmutables. Devuelve únicamente:

- applicable con candidato declarativo;
- not_applicable con razón normalizada;
- insufficient con faltante;
- conflicted con alternativas respaldadas;
- invalid_rule.

No devuelve unidades publicadas.

### 10.6 Projection boundary

Recibe Inference Report validado y perfil exacto. Devuelve:

- candidato estructuralmente íntegro; o
- fallo técnico de construcción normalizado.

No declara estado contractual final.

### 10.7 Contract Validation boundary

Recibe candidato íntegro. Devuelve:

- publicación complete;
- publicación incomplete;
- rechazo invalid.

Validation conserva autoridad final.

### 10.8 Evaluation boundary

Recibe fixture, perfil y expectativas. Devuelve observaciones de conformidad y
métricas. No modifica producción.

## 11. Modelos internos

Los modelos internos reflejan conceptos normativos sin convertirse en nuevos
conceptos de dominio.

### 11.1 Admitted Modified File

Reutiliza el modelo ya admitido por Input Boundary:

- path;
- Change Status;
- provenance;
- orden;
- disponibilidad de colección;
- identidad de entrada.

### 11.2 Capability Execution Profile

Representa las condiciones cerradas de ejecución:

- plugin y versión;
- contrato de entrada;
- Rule Set;
- taxonomía;
- políticas;
- orden;
- límites.

### 11.3 Rule Descriptor

Representa identidad, versión, etapa, precondiciones, Evidence requerida,
resultado permitido, Scope, Confidence máxima, Uncertainty y condiciones de
Abstention.

No contiene Evidence ni autoridad de publicación.

### 11.4 Taxonomy Entry

Representa identidad estable de Contract Type, versión taxonómica, estado de
soporte y relaciones de incompatibilidad declaradas.

No contiene parser, schema ni semántica funcional.

### 11.5 Candidate Projection State

Representa identified, classified, unknown, ambiguous, outside_coverage o
abstained con referencias a Findings y Evidence.

### 11.6 Detection Projection State

Representa detected, unknown, ambiguous, outside_coverage o abstained con Type,
Basis, Sufficiency, Readiness y Scope.

### 11.7 Classification Basis View

Conserva referencias a Evidence, Rule, taxonomía, señal y límites. No duplica
contenido.

### 11.8 Sufficiency State

Representa sufficient, partial, insufficient o not_provided con precondiciones
observadas y faltantes.

### 11.9 Readiness State

Representa ready, not_ready, unknown o not_applicable con precondiciones
versionadas.

### 11.10 Detection Coverage

Reconcilia Evidence elegible, evaluada, Candidates, Detections, estados no
aplicables, outside_coverage y Abstentions.

### 11.11 Contract Change Report Candidate

Agrupa exclusivamente unidades publicables, identidad, Coverage y estado de
ejecución. No incluye descartes.

## 12. Runtime

### 12.1 Decisión

Capability-003 se ejecutará en **.NET 9 o superior compatible dentro de la línea
seleccionada para el piloto VS-001**, usando la aplicación `Eip.Cli` existente.

La decisión selecciona runtime para esta implementación y no declara el lenguaje
universal de EIP.

### 12.2 Justificación

- Capability-002 ya está implementada en ese runtime;
- Input Boundary y modelos epistemológicos existen en el mismo proceso;
- reutilizar evita serialización y deployables nuevos;
- testing, analizadores y formato ya están configurados;
- no existe necesidad demostrada de aislamiento físico.

### 12.3 Modelo de ejecución

- proceso único;
- ejecución solicitada y finita;
- sin background services;
- sin estado global mutable;
- sin red;
- sin filesystem fuera de lectura del artefacto de entrada y escritura de
  salida;
- cancelación propagada;
- memoria acotada por entrada y resultados.

## 13. Rule Engine

### 13.1 Decisión

No se construirá un Rule Engine nuevo. Capability-003 reutilizará el **Rule
Runtime** y las etapas Claim/Hypothesis/Finding de Capability-002.

“Rule Engine de Capability-003” denomina la composición de:

- execution profile;
- Rule Registry;
- Domain Rule Adapter;
- Rule Runtime existente;
- Validation existente y específica.

### 13.2 Forma inicial

Las Rules serán definiciones inmutables incluidas en código y registradas
explícitamente. No habrá DSL, scripts, configuración remota ni interpretación
dinámica.

Esta forma es la mínima capaz de demostrar identidad, versionado y evaluación.

### 13.3 Orden

El profile fija orden total por:

1. etapa normativa;
2. prioridad declarativa sólo cuando la normativa la justifica;
3. Rule ID ordinal;
4. versión exacta.

Prioridad no resuelve Contradictions epistemológicas. Sólo ordena evaluación.

### 13.4 Aplicabilidad

Cada Rule devuelve estado explícito. No lanzar una inferencia no equivale a
not_applicable; errores técnicos, insuficiencia y conflicto permanecen
distintos.

### 13.5 Restricciones

Rule Engine no puede:

- crear Evidence;
- leer contenido;
- parsear;
- consultar fuentes;
- cambiar taxonomía;
- aprobar Rules;
- producir breaking changes o severidad;
- publicar unidades sin Validation.

## 14. Rule Registry

### 14.1 Decisión

Registro in-process, inmutable y explícito por execution profile.

### 14.2 Validaciones al inicio

- IDs no vacíos y únicos;
- versiones válidas;
- etapa reconocida;
- Type referenciado existe en taxonomía;
- precondiciones completas;
- resultado permitido por dominio;
- Confidence máxima declarada;
- Uncertainty y Abstention declaradas;
- orden total sin colisiones;
- ausencia de categorías prohibidas.

Un registro inválido impide iniciar el perfil. No produce Abstention, porque es
falla de configuración gobernada.

### 14.3 Lifecycle

El registro se construye una vez por ejecución o desde una instancia inmutable
verificada. Nunca se modifica durante el pipeline.

### 14.4 Versionado

Rule Set ID participa de execution ID. Cambiar Rule, versión, orden material o
precondición exige nuevo Rule Set ID.

### 14.5 No plataforma general

No existe administración dinámica, interfaz de usuario, catálogo remoto,
persistencia ni hot reload.

## 15. Taxonomy Registry

### 15.1 Decisión

Registro in-process, inmutable, cerrado por ejecución y específico de
Capability-003.

### 15.2 Contenido permitido

- Type ID estable;
- nombre canónico;
- versión taxonómica;
- estado supported/deprecated cuando se autorice;
- familia descriptiva;
- relaciones explícitas de incompatibilidad entre Types.

### 15.3 Contenido prohibido

- parsers;
- schemas;
- ejemplos de contenido tratados como verdad;
- severidad;
- reglas de breaking change;
- recomendaciones;
- accesos a herramientas.

### 15.4 Validación

- identidad única;
- versión exacta;
- familia conocida por el profile;
- relaciones simétricas cuando corresponda;
- ninguna referencia huérfana;
- orden ordinal estable.

### 15.5 Evolución

Agregar Type puede ser aditivo si no redefine existentes. Cambiar significado o
relación material exige nueva versión taxonómica y Rule Set compatible.

## 16. Validation Pipeline

Validation ocurre en cada frontera y no sólo al final.

### 16.1 Profile Validation

Verifica descriptor, Rule Registry, Taxonomy Registry y compatibilidad mutua.

### 16.2 Input Validation

Reutiliza Input Boundary sin cambios contractuales.

### 16.3 Candidate Validation

Verifica Evidence primaria, Rule, señal, Scope, Confidence, Uncertainty e
identidad.

### 16.4 Classification Validation

Verifica Type, Basis, taxonomía, alternativas, Scope y ausencia de
interpretación funcional.

### 16.5 Sufficiency Validation

Reconcilia estado con Evidence, precondiciones y faltantes.

### 16.6 Readiness Validation

Reconcilia estado con precondiciones versionadas y prohíbe promesas de análisis.

### 16.7 Epistemic Validation

Reutiliza validadores de Claim, Hypothesis, Finding, Traceability, Confidence,
Uncertainty, Contradiction y Abstention.

### 16.8 Report Candidate Validation

Verifica que proyección no incluya descartes, referencias rotas ni unidades de
otro perfil.

### 16.9 Contract Validation

Es la única autoridad que clasifica complete, incomplete o invalid y autoriza
publicación especializada.

## 17. Evaluation Pipeline

```text
Versioned Fixture
        ↓
Profile Validation
        ↓
Production Pipeline Execution
        ↓
Observed Contract Change Report
        ↓
Expectation Comparison
        ↓
Metrics + Deviations
```

### 17.1 Independencia

Evaluation usa el pipeline de producción como caja observable. No inserta
resultados esperados durante razonamiento.

### 17.2 Entradas

- Local Context fixtures sanitizados;
- profile exacto;
- taxonomía y Rules versionadas;
- expectativas gobernadas;
- categoría de caso y Scope.

### 17.3 Salidas

- conformidad por unidad;
- métricas agregadas;
- diferencias normalizadas;
- fallas de Laws e Invariants;
- evidencia de reproducibilidad.

### 17.4 Separación

Una falla de Evaluation no modifica el reporte. Impide promoción o registra
regresión.

### 17.5 Sin plataforma adicional

El pipeline se ejecuta dentro del proyecto de tests existente. No requiere
servicio, base de datos ni dashboard.

## 18. Error Model

### 18.1 Errores sistémicos que detienen

- plugin ID desconocido;
- profile inválido;
- Rule Registry inválido;
- Taxonomy Registry inválido;
- entrada incompatible;
- identidad ambigua;
- referencia cruzada de ejecución;
- imposibilidad de canonicalizar;
- fallo de escritura atómica;
- cancelación.

No publican reporte parcial.

### 18.2 Condiciones epistemológicas que producen Abstention

- Modified File Evidence not_provided;
- Candidate sin señal suficiente;
- Type que requeriría contenido;
- Basis insufficient;
- Confidence insufficient;
- Scope incompatible;
- Contradiction no resoluble;
- Traceability no construible cuando la limitación misma es trazable;
- resultado prohibido por dominio.

### 18.3 Degradación válida

- Candidate unknown;
- clasificación ambiguous;
- Type outside_coverage;
- Detection Sufficiency partial;
- Analysis Readiness not_ready o unknown;
- Coverage parcial;
- Detection válida limitada.

### 18.4 Fallo de construcción

Detection Projection puede declarar que no puede construir candidato íntegro.
Entrega causa normalizada a Validation, no publica y no declara un reporte
invalid existente.

### 18.5 Resultado invalid

Cuando existe candidato íntegro, Contract Validation puede clasificarlo invalid
y rechazar publicación.

### 18.6 Mensajes

Errores externos son genéricos y no contienen paths absolutos, contenido,
credenciales, stack traces ni detalles de Rules sensibles. Registros internos
usan IDs y códigos normalizados.

## 19. Determinismo

### 19.1 Identidad de ejecución

Execution ID conserva la convención de identidad gobernada por Capability-002 y
agrega términos explícitos para todo elemento material de la ejecución:

```text
execution_id = hash(
  canonicalize(
    canonical_context_identity,
    contract_id,
    plugin_id,
    plugin_version,
    execution_profile_id,
    execution_profile_version,
    rule_set_id,
    taxonomy_id,
    taxonomy_version
  )
)
```

La canonicalización estructurada usa términos separados, orden fijo y
delimitación inequívoca. No depende de codificar identidad o versión de plugin,
profile o taxonomía dentro del nombre del Rule Set.

Cualquier cambio material de plugin, execution profile, Rule Set, taxonomía o
entrada produce una identidad distinta. No se agregan timestamps ni GUID ni se
define aquí una representación física o un algoritmo adicional a la convención
de hash Accepted.

### 19.2 Orden

Se preserva orden contractual de Modified File Evidence. Dentro de una Evidence,
Rules se evalúan según orden total del profile. Salidas se ordenan por:

1. posición de Evidence primaria;
2. etapa;
3. Rule ID ordinal;
4. identidad de unidad ordinal.

No se ordena por Confidence, Type o texto visible.

### 19.3 Canonicalización

Se reutiliza identidad canónica de Input Boundary. Modelos derivados usan
representación canónica de campos normativos en orden fijo y colecciones según
orden contractual.

### 19.4 Estado externo

No participa reloj, cultura, locale, filesystem, red, orden de hash maps ni
concurrencia no determinista.

### 19.5 Repetibilidad

Mismo contexto, profile, Rules y taxonomía producen mismos IDs, orden, estados y
bytes publicados.

## 20. Performance

### 20.1 Perfil esperado

La entrada está acotada por archivos modificados de una revisión. El trabajo
inicial depende de:

- `F`: cantidad de Modified File Evidence;
- `R`: cantidad de Rules del profile;
- `T`: cantidad de Types registrados;
- `U`: unidades epistemológicas producidas.

### 20.2 Complejidad inicial

- validación de Evidence: `O(F)`;
- evaluación directa de Rules: como máximo `O(F × R)`;
- resolución de Type por ID: `O(1)` esperado mediante índice inmutable;
- reconciliación de Coverage: `O(F + U)`;
- proyección y validación: `O(U)`.

No se introduce optimización antes de medir. El Rule Set inicial debe ser
pequeño y explícito.

### 20.3 Memoria

El pipeline mantiene la entrada admitida y unidades de ejecución en memoria,
como Capability-002. No duplica contenido porque `modified_files` no lo posee.

### 20.4 Límites

Se deben medir cantidad de Evidence, Rules, Candidates, unidades y bytes de
salida. Los límites cuantitativos se fijan después del baseline.

### 20.5 Paralelismo

La primera implementación es secuencial. Paralelizar podría alterar orden y no
está justificado por datos.

## 21. Escalabilidad

### 21.1 Estrategia inicial

Escala dentro del proceso actual y por ejecución independiente. No existe estado
compartido entre ejecuciones.

### 21.2 Escala vertical

Si crece `F` o `R`, primero se optimizan índices inmutables, preselección de
Rules por metadatos ya admitidos y asignaciones. No se agrega distribución.

### 21.3 Escala horizontal

Ejecuciones independientes podrían ejecutarse en procesos separados en un
entorno futuro sin cambiar contratos. TD-004 no diseña ese hosting.

### 21.4 Gates de evolución

Una separación física requiere evidencia de:

- latencia o memoria incompatibles;
- frontera de seguridad diferente;
- ciclo de despliegue independiente sostenido;
- blast radius material;
- ownership separado;
- continuidad operativa distinta.

Sin uno de estos gates, permanece modular in-process.

## 22. Observabilidad

### 22.1 Identidad y configuración

- execution ID;
- plugin/profile ID y versión;
- contract ID;
- Rule Set ID;
- taxonomy ID y versión.

### 22.2 Volumen

- Modified File Evidence available/not_provided;
- Evidence elegible;
- Candidates por estado;
- Detections por estado;
- Claims, Hypotheses y Findings;
- Contradictions y Abstentions;
- Coverage.

### 22.3 Calidad técnica

- duración por etapa;
- descartes por código normalizado;
- fallas de validación;
- fallos de construcción;
- tamaño de resultados;
- reproducciones divergentes.

### 22.4 Calidad epistemológica

- Types sin Basis;
- referencias rotas;
- unknown/ambiguous/outside_coverage;
- Sufficiency y Readiness por estado;
- Confidence y Uncertainty;
- Abstention correcta según Evaluation.

### 22.5 Minimización

Observabilidad no registra contenido, paths completos cuando no sean necesarios,
credenciales ni Evidence sensible. Prefiere IDs, conteos y motivos normalizados.

### 22.6 No telemetría de personas

No mide desempeño individual ni utiliza autoría del Pull Request.

## 23. Testing Strategy

### 23.1 Unit

Prueba aisladamente:

- descriptor y profile;
- Rule Registry;
- Taxonomy Registry;
- aplicabilidad de cada Rule;
- Domain Rule Adapter;
- validaciones de Candidate, Type, Sufficiency y Readiness;
- orden e identidad;
- proyección pura;
- códigos de error.

### 23.2 Integration

Prueba:

- Input Boundary → plugin → Inference Pipeline;
- Inference Report → Detection Projection;
- Validation → publicación atómica;
- contextos available, empty y not_provided;
- coexistencia del profile actual y Capability-003 sin contaminación.

### 23.3 Reasoning

Verifica todas las Laws e Invariants con casos positivos, negativos, unknown,
ambiguous, Contradiction y Abstention.

### 23.4 Contract

Verifica complete, incomplete, invalid, referencias, Coverage, estados y
responsabilidades del productor.

### 23.5 Regression

Garantiza que:

- Capability-002 actual conserva comportamiento;
- inference-report actual conserva forma y semántica;
- VS-001 no cambia;
- execution IDs sólo cambian ante profile, Rules, taxonomía o entrada material;
- no aparecen nuevas fuentes ni accesos.

### 23.6 Determinism

- ejecución repetida produce mismos bytes;
- property order irrelevante no altera identidad;
- orden contractual sí se preserva;
- orden de registro no declarado no afecta resultado;
- culturas y zonas horarias no afectan salida.

### 23.7 Security

- paths hostiles permanecen datos, no acceso;
- contenido inyectado en strings no cambia Rules;
- IDs inválidos fallan cerrados;
- errores no filtran paths absolutos ni stack traces;
- ninguna prueba necesita red.

## 24. Golden Dataset

### 24.1 Propósito

Demostrar precisión, límites, abstención y neutralidad de formato antes de
promover la capability.

### 24.2 Forma

Fixtures Local Context sanitizados y expectativas conceptuales versionadas. Cada
caso declara:

- identidad;
- categoría;
- profile, Rules y taxonomía;
- Evidence esperada;
- Candidates y descartes esperados;
- Types esperados;
- Sufficiency y Readiness;
- Confidence y Uncertainty;
- Coverage;
- Contradictions y Abstentions;
- estado contractual.

### 24.3 Casos mínimos

- not_provided;
- available vacío;
- archivo no contractual;
- candidato clasificable;
- candidate unknown;
- candidate ambiguous;
- Type outside_coverage;
- added, modified, deleted y renamed;
- Scope compatible e incompatible;
- Basis complete, partial e insufficient;
- Readiness ready, not_ready, unknown y not_applicable;
- Coverage complete, partial e invalid;
- Contradiction válida y falsa;
- Abstention local, partial y total;
- repetición determinista;
- Rule Registry o taxonomía inválidos.

### 24.4 Representatividad

Debe incluir al menos dos familias de Contract Types para demostrar que el
nucleo no está acoplado a APIs. Los casos no autorizan parsing.

### 24.5 Gobierno

Expectativas se fijan antes de ejecutar y son revisadas por evaluadores
autorizados. Cambiar una expectativa requiere motivo y versión.

### 24.6 Datos

No se versionan repositorios, secretos ni contenido real no autorizado. Paths y
provenance se sanitizan.

## 25. Casos adversariales

El conjunto adversarial debe cubrir:

- path que aparenta ser OpenAPI sin respaldo suficiente;
- extensión compartida por varios Types;
- nombre con mayúsculas, Unicode o segmentos engañosos;
- path que contiene texto parecido a instrucciones;
- Type ID desconocido;
- Rule duplicada o versión colisionada;
- taxonomía con referencia huérfana;
- alternativas incompatibles sin precedencia;
- not_provided presentado como vacío;
- Collection vacía presentada como desconocida;
- `deleted` presentado como breaking change;
- `renamed` presentado como continuidad compatible;
- `modified` presentado como cambio semántico;
- Candidate sin Evidence;
- Type sin Basis;
- Readiness heredada de Confidence;
- Scope ampliado a otro archivo;
- referencia a otra ejecución;
- Coverage con duplicados u omisiones;
- descarte filtrado al reporte;
- resultado con severidad o Recommendation;
- intento de leer filesystem o red;
- orden no determinista;
- error que intenta filtrar path absoluto.

El resultado esperado es rechazo, degradación o Abstention conforme al caso,
nunca una inferencia conveniente.

## 26. Extensibilidad

### 26.1 Contract Types

Se agregan mediante nueva Taxonomy Entry, Rules y casos de Evaluation. No cambia
el pipeline si el modelo normativo permanece.

### 26.2 Rules

Se agregan al registro explícito con nueva identidad o versión. No existe carga
dinámica.

### 26.3 Precondiciones de Readiness

Pueden evolucionar versionadas. No modifican Detections anteriores.

### 26.4 Validaciones

Pueden fortalecerse si sólo rechazan resultados ya inválidos según normativa.
Cambiar significado exige evolución documental.

### 26.5 Contract output

La representación física podrá evolucionar después de una decisión específica.
TD-004 sólo exige preservar significado.

### 26.6 Nuevas Evidence

No son punto de extensión interno. Requieren evolución gobernada de contratos y
arquitectura antes de llegar al plugin.

### 26.7 Capability-004

El consumidor puede evolucionar independientemente si respeta Contract Change
Detection Contract. No puede insertar análisis dentro del plugin.

## 27. Riesgos

| Riesgo                                  | Impacto                              | Mitigación inicial                             |
| --------------------------------------- | ------------------------------------ | ---------------------------------------------- |
| “Plugin” se convierte en plataforma     | Infraestructura anticipada           | Registro explícito e in-process                |
| Segundo Rule Engine                     | Divergencia epistemológica           | Reutilizar Rule Runtime existente              |
| Rules esconden parsing                  | Violación de alcance                 | Modelo declarativo y tests adversariales       |
| Type como string libre                  | Taxonomía inconsistente              | Registry versionado                            |
| Orden de Rules altera resultado         | No reproducibilidad                  | Orden total e identidad del Rule Set           |
| Profile contamina Rules actuales        | Regresión de Capability-002          | Selección explícita y tests de coexistencia    |
| Projection vuelve a razonar             | Doble autoridad                      | Proyección pura y Validation separada          |
| Sufficiency se confunde con Readiness   | Promesa de análisis inexistente      | Modelos, validadores y métricas separados      |
| Path se trata como prueba absoluta      | Falsos positivos                     | Confidence limitada y Golden Dataset           |
| Taxonomía crece sin evaluación          | Cobertura aparente                   | Entry + Rules + casos obligatorios             |
| Observabilidad filtra Evidence          | Exposición indebida                  | IDs y conteos minimizados                      |
| O(F × R) crece                          | Latencia                             | Medir antes de indexar                         |
| Salida especializada duplica inferencia | Inconsistencia                       | Referencias al Inference Report, no recreación |
| Contract report parcial                 | Consumidor recibe estado incoherente | Construcción y publicación atómicas            |

## 28. Trade-offs

### 28.1 Registro en código versus configuración dinámica

Se elige registro en código. Reduce flexibilidad operativa, pero maximiza
versionado, revisión y determinismo sin crear plataforma.

### 28.2 Pipeline secuencial versus paralelo

Se elige secuencial. Puede tener menor throughput, pero conserva orden simple y
es suficiente sin baseline contrario.

### 28.3 Proyección especializada versus consumidor reconstruyendo

Se elige proyección especializada. Agrega un componente, pero protege productor
único y evita que Capability-004 reconstruya semántica.

### 28.4 Taxonomía explícita versus strings libres

Se elige taxonomía explícita. Requiere gobierno, pero evita categorías
inconsistentes y permite compatibilidad.

### 28.5 Un solo deployable versus aislamiento

Se elige un solo deployable. Menor aislamiento, pero no existe diferencia de
seguridad, escala ni lifecycle que justifique separación.

### 28.6 Menos detecciones versus clasificación probable

Se elige Abstention. Reduce cantidad aparente de resultados, pero conserva
confianza y límites.

## 29. Roadmap incremental

Cada incremento agrega una sola capacidad y termina con demo ejecutable. Ninguno
autoriza anticipar el siguiente.

### Incremento 0 — Extension boundary, plugin vacío y coexistencia

**Capacidad:** implementar el extension boundary genérico; conservar el profile
actual de Capability-002; registrar el execution profile de Capability-003 con
Rule y Taxonomy Registries vacíos; y ejecutar ambos mediante el mismo pipeline,
sin Rules ni taxonomía de dominio.

**Demo:** el mismo Local Context ejecuta el profile actual y Capability-003 por
separado. Los profiles tienen identidades diferentes, no se contaminan,
Capability-002 conserva exactamente su comportamiento actual y Capability-003
produce cero Claims. La demo evidencia que existe un solo Rule Engine.

### Incremento 1 — Admisión y Coverage de Modified File Evidence

**Capacidad:** seleccionar Evidence, preservar available/not_provided y
reconciliar Scope sin Candidate Rules.

**Demo:** available vacío produce Coverage completa con cero Candidates;
not_provided produce Coverage unknown y Abstention total. No nacen Types.

### Incremento 2 — Primer Contract Candidate

**Capacidad:** ejecutar una única Candidate Rule gobernada para una convención
acotada, producir Claims válidos y descartar negativos.

**Demo:** un path controlado produce Candidate trazable; otro no aplicable no
produce Candidate. No se asigna Contract Type.

### Incremento 3 — Primera Contract Type Classification

**Capacidad:** registrar taxonomía mínima y una Classification Rule, formar
Hypothesis y Finding de Type con Basis, Confidence y Uncertainty.

**Demo:** un Candidate obtiene Type respaldado; otro queda unknown. No existe
parsing.

### Incremento 4 — Detection Sufficiency

**Capacidad:** evaluar sufficient, partial e insufficient sin calcular
Readiness.

**Demo:** tres casos controlados distinguen estados y faltantes. Ningún estado
lee contenido.

### Incremento 5 — Analysis Readiness

**Capacidad:** evaluar precondiciones versionadas ready, not_ready, unknown y
not_applicable.

**Demo:** una Detection lista y otra no lista conservan exactamente la misma
clasificación; sólo cambia readiness respaldada.

### Incremento 6 — Unknown, Ambiguous y Contradiction

**Capacidad:** preservar alternativas, producir Contradiction y Abstention local
sin precedencia implícita.

**Demo:** dos Types incompatibles permanecen visibles; ninguna se selecciona.

### Incremento 7 — Contract Change Report

**Capacidad:** proyectar Inference Report validado, validar y publicar el
contrato especializado.

**Demo:** un reporte complete y uno incomplete se publican; uno invalid no deja
salida parcial. Misma entrada produce mismos bytes.

### Incremento 8 — Golden Dataset y Evaluation gate

**Capacidad:** ejecutar casos de varias familias, medir métricas normativas y
bloquear promoción ante regresión.

**Demo:** suite positiva, negativa y adversarial produce reporte de Evaluation
repetible sin modificar resultados de producción.

### Incremento 9 — Integración con Capability-004

Queda fuera de TD-004 hasta que Capability-004 sea diseñada. No forma parte de
la Definition of Done técnica inicial de implementación del plugin.

## 30. Definition of Done técnica

TD-004 se considera implementado cuando:

- existe el Inference Engine Extension Boundary genérico;
- existe un plugin declarativo in-process explícitamente registrado;
- Capability-002 mantiene su profile anterior sin cambios funcionales;
- Capability-003 usa el mismo pipeline mediante un profile distinto;
- Capability-002 no posee conocimiento de Contract Types;
- no existe segundo Rule Engine;
- Profile, Rule Registry y Taxonomy Registry son inmutables y versionados;
- profile, Rule Set y taxonomía participan explícitamente de Execution ID;
- Input Boundary sigue siendo única entrada;
- available y not_provided permanecen distintos;
- cada Candidate y Detection conserva cadena completa;
- Sufficiency y Readiness permanecen separadas;
- Unknown, Ambiguous, Contradiction y Abstention funcionan conforme a normativa;
- Contract Change Report Projection es pura;
- Validation controla publicación;
- salida es determinista y atómica;
- Golden Dataset cubre al menos dos familias;
- métricas normativas alcanzan umbrales preacordados;
- casos adversariales no logran parsing, acceso o decisiones;
- la suite completa de Capability-002 y VS-001 permanece verde;
- no existen nuevas fuentes, deployables ni infraestructura compartida;
- cada incremento posee demo observable;
- no existen breaking changes, compatibilidad, severidad, Recommendations ni
  Decisions en las salidas.

Completar clases o producir una clasificación correcta no satisface por sí solo
esta Definition of Done.

## 31. Decisiones técnicas tomadas

| ID        | Decisión                                                                                               | Estado   |
| --------- | ------------------------------------------------------------------------------------------------------ | -------- |
| TD004-D01 | Plugin declarativo in-process, no plataforma dinámica                                                  | Accepted |
| TD004-D02 | Reutilizar Rule Runtime y pipeline de Capability-002                                                   | Accepted |
| TD004-D03 | Execution profile explícito y versionado                                                               | Accepted |
| TD004-D04 | Rule Registry inmutable y registrado en código                                                         | Accepted |
| TD004-D05 | Taxonomy Registry inmutable y sin parsers                                                              | Accepted |
| TD004-D06 | .NET 9 compatible y única CLI                                                                          | Accepted |
| TD004-D07 | Procesamiento secuencial inicial                                                                       | Accepted |
| TD004-D08 | Validation en cada frontera                                                                            | Accepted |
| TD004-D09 | Proyección contractual pura desde Inference Report validado                                            | Accepted |
| TD004-D10 | Publicación atómica                                                                                    | Accepted |
| TD004-D11 | Evaluation fuera del camino productor                                                                  | Accepted |
| TD004-D12 | Ninguna nueva Evidence ni acceso a fuentes                                                             | Accepted |
| TD004-D13 | Capability-002 admite execution profiles mediante un extension boundary genérico, interno y compatible | Accepted |

TD004-D13 permite cambios internos compatibles en Capability-002, pero no le
permite incorporar conocimiento de Contract Types ni altera sus contratos, Laws,
invariantes o responsabilidades. Todo cambio incompatible continúa sujeto a
gobernanza formal.

## 32. Decisiones postergadas

- formato físico de Contract Change Report;
- schema y serialización;
- mecanismo físico de plugin más allá del registro explícito;
- configuración externa de Rules;
- DSL;
- carga dinámica;
- persistencia y cache;
- hosting y despliegue independiente;
- paralelismo;
- límites cuantitativos sin baseline;
- Contract Types del primer incremento;
- Rules concretas;
- contenido o parsing;
- comparación y breaking changes;
- Capability-004;
- integración con VS-001;
- UI, Markdown y feedback;
- infraestructura compartida.

## 33. Preguntas abiertas

- ¿Qué único Contract Type ofrece el caso inicial más inequívoco sin leer
  contenido?
- ¿Qué señal permite Candidate pero no Type?
- ¿Qué precondiciones mínimas de Readiness pueden declararse sin conocer
  Capability-004?
- ¿Qué tamaño real alcanzan Rule Set y taxonomía iniciales?
- ¿Qué umbrales de falsos positivos y Abstention deben fijarse?
- ¿Qué relación física mínima entre Inference Report y Contract Change Report
  evita duplicación excesiva?
- ¿Qué evidencia demostraría que el registro en código dejó de ser suficiente?
- ¿Qué volumen justificaría indexar Rules o paralelizar?

Estas preguntas no autorizan alcance adicional.

## 34. Historial

| Fecha      | Cambio                                                  | Estado   |
| ---------- | ------------------------------------------------------- | -------- |
| 2026-08-04 | Propuesta inicial de TD-004 — Contract Change Detection | Proposed |
| 2026-08-05 | Promoción formal coordinada                             | Accepted |
