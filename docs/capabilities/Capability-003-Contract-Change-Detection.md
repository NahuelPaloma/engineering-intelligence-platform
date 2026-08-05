# Capability-003 — Contract Change Detection

| Campo                | Valor                                               |
| -------------------- | --------------------------------------------------- |
| Identificador        | Capability-003                                      |
| Nombre               | Contract Change Detection                           |
| Estado               | **Accepted**                                        |
| Tipo                 | Capability transversal de detección y clasificación |
| Owner propuesto      | Engineering Platform                                |
| Audiencia            | Product, Architecture, Engineering y Risk           |
| Última actualización | 5 de agosto de 2026                                 |

---

## 1. Propósito y autoridad

Este documento define el dominio oficial de **Contract Change Detection** para
la Engineering Intelligence Platform.

Su propósito es determinar, exclusivamente a partir de Evidence autorizada:

1. qué archivos modificados son candidatos a Engineering Contract;
2. qué tipo de contrato representa cada candidato cuando existe respaldo
   suficiente;
3. si la Evidence disponible alcanza para realizar esa detección y dejar el
   candidato preparado para un análisis posterior.

Capability-003 no determina qué cambió dentro del contenido, si el cambio rompe
compatibilidad, qué impacto tiene ni qué debe hacerse. Detectar un contrato
modificado es una capacidad anterior y distinta de analizar su semántica.

Las fuentes normativas son, en este orden:

1. Product Vision v1.1, Accepted;
2. Architecture v1.0, Accepted;
3. ADR-001 a ADR-013 vigentes;
4. ADR-013 — Local Context Contract, Accepted;
5. Local Context Contract, versión conceptual 1, Accepted;
6. Capability-002 — Inference Engine, Accepted;
7. Inference Report Contract, versión conceptual 1, Accepted;
8. Inference Engine Reasoning Specification, Accepted;
9. TD-002 — Inference Engine, Accepted;
10. TD-003 — Inference Report, Accepted.

No existe actualmente un documento canónico denominado Capability-001. En este
documento, **Capability-001** identifica al Context Retrieval Pipeline ya
implementado y gobernado por ADR-013, Local Context Contract y sus demos. Esta
referencia no crea ni reconstruye una especificación ausente.

Ante una contradicción prevalece la fuente Accepted de mayor autoridad.
Capability-003 no modifica el modelo epistemológico de Capability-002 ni sus
contratos.

## 2. Contexto

Context Retrieval ya transporta `modified_files` desde Evidence autorizada del
cambio hasta `local-context.json`. Cada elemento conserva únicamente:

- `path` relativo al repositorio;
- `change_status` demostrado;
- procedencia mínima del cambio analizado.

La colección distingue Evidence disponible de Evidence no proporcionada. Su
presencia no crea Claims automáticamente y su ausencia no significa que ningún
archivo haya sido modificado.

Capability-002 puede admitir esa Evidence, aplicar Rules gobernadas y producir
inferencias trazables. Sin embargo, sus Rules actuales sólo reconocen
disponibilidad documental. EIP todavía no posee un dominio canónico para
identificar Engineering Contracts entre los archivos modificados.

Capability-003 define ese dominio sin anticipar parsing, análisis de
compatibilidad ni conocimiento específico de un formato.

## 3. Problema

Un archivo modificado no es automáticamente un contrato. Una extensión, un
nombre convencional o una ubicación pueden aportar señal, pero ninguna señal
aislada autoriza a atribuir contenido, importancia o impacto que no fue
observado.

Sin una capability explícita aparecen riesgos materiales:

- todo archivo declarativo se presenta como contrato;
- un path ambiguo se clasifica con certeza artificial;
- “archivo modificado” se confunde con “semántica modificada”;
- “contrato modificado” se confunde con “breaking change”;
- la ausencia de contenido se oculta y se promete análisis inexistente;
- cada producto crea taxonomías incompatibles;
- clasificación y decisión quedan mezcladas;
- una heurística de un formato invade el núcleo transversal;
- se pierde trazabilidad hasta `modified_files`;
- se consulta el repositorio para completar Evidence faltante.

El problema a resolver es:

> Identificar de forma trazable qué archivos modificados son candidatos a
> Engineering Contract, clasificarlos sólo cuando la Evidence lo permite y
> declarar explícitamente si existe información suficiente para continuar.

## 4. Outcome esperado

El outcome primario es:

> **Permitir que capabilities autorizadas conozcan qué Engineering Contracts
> pudieron detectarse en un cambio y cuáles requieren más Evidence antes de ser
> analizados.**

Outcomes secundarios:

- crear un vocabulario compartido para detección de contratos;
- separar detección, clasificación y suficiencia;
- evitar que clasificación tecnológica implique interpretación funcional;
- hacer visible Evidence no proporcionada, ambigua o insuficiente;
- conservar la relación exacta con el archivo modificado observado;
- permitir evaluación independiente por tipo de contrato;
- preparar una frontera reutilizable para análisis posteriores.

El éxito no se mide por cantidad de archivos clasificados. Se mide por
trazabilidad, precisión de detección, abstención correcta, consistencia y
ausencia de conclusiones no respaldadas.

## 5. Alcance

Capability-003:

1. considera exclusivamente archivos presentes en la Evidence autorizada de
   `modified_files`;
2. distingue Evidence disponible de Evidence no proporcionada;
3. identifica candidatos a Engineering Contract;
4. asigna un Contract Type únicamente cuando existe Evidence explícita y una
   Rule gobernada que autoriza la clasificación;
5. conserva candidatos ambiguos o desconocidos sin forzar una categoría;
6. evalúa si la Evidence disponible es suficiente para la detección y para
   habilitar un análisis posterior;
7. produce resultados descriptivos, trazables y reproducibles;
8. permite abstención local, parcial o total;
9. declara cobertura y limitaciones.

El alcance termina al identificar el candidato, su tipo cuando puede sostenerse
y su suficiencia. No comienza el análisis del contenido funcional.

## 6. Responsabilidades

Capability-003 es responsable de:

- mantener el significado oficial de Engineering Contract para esta capability;
- gobernar la taxonomía conceptual de Contract Types;
- definir las condiciones observables de detección y clasificación;
- exigir trazabilidad hasta Modified File Evidence;
- distinguir clasificación confirmada, ambigua y no determinada;
- declarar si la Evidence fue proporcionada;
- declarar si la Evidence alcanza para continuar;
- conservar el `change_status` sin reinterpretarlo;
- abstenerse cuando la clasificación excedería el respaldo;
- permitir evaluación por casos positivos, negativos y ambiguos;
- mantener la decisión humana fuera del resultado.

No es responsable de recuperar, parsear, comparar, aprobar ni presentar
contratos.

## 7. Límites

### 7.1 Límite con Context Retrieval

Capability-001 recupera y autoriza Evidence. Capability-003 no consulta el Git
Provider, el repositorio, el checkout ni artefactos intermedios. No reconstruye
`modified_files` ni completa su ausencia.

### 7.2 Límite con Capability-002

Capability-002 conserva las leyes epistemológicas y ejecuta razonamiento sobre
Evidence admitida. Capability-003 aporta el dominio declarativo de detección,
pero no redefine Evidence, Claim, Hypothesis, Finding, Confidence, Uncertainty,
Traceability, Contradiction ni Abstention.

La autoridad para crear, aprobar, versionar o retirar conocimiento declarativo
de Contract Change Detection permanece fuera del Inference Engine. El motor no
se convierte en owner del dominio.

### 7.3 Límite con análisis posteriores

Una capability posterior podrá analizar contenido, comparar versiones o
clasificar compatibilidad. Capability-003 no anticipa esas responsabilidades.

```text
Contract Change Detection
          ↓
identifica candidato + tipo + suficiencia
          ↓
futura capability de análisis
          ↓
compatibilidad, impacto o breaking change
```

Detectar no autoriza el siguiente paso; sólo produce Evidence derivada que un
consumidor podrá evaluar bajo su propio contrato.

### 7.4 Límite con productos consumidores

Architecture Review, Incident Intelligence, Security, Observability, API
Governance y otros consumidores pueden seleccionar o presentar detecciones. No
pueden atribuirles severidad, criticidad o incompatibilidad como si esas
propiedades hubieran sido producidas por Capability-003.

## 8. Anti-Goals

Capability-003 explícitamente no:

- compara versiones de un contrato;
- detecta breaking changes;
- determina compatibilidad backward o forward;
- interpreta operaciones, endpoints, mensajes, campos o recursos;
- interpreta semántica funcional;
- parsea formatos específicos;
- valida sintaxis de un formato;
- determina que un contrato es válido;
- determina consumidores o dependencias;
- calcula impacto, riesgo, severidad o criticidad;
- recomienda acciones;
- aprueba ni rechaza cambios;
- revisa código;
- inspecciona diffs o patches;
- lee contenido de archivos;
- consulta repositorios ni proveedores;
- modifica Evidence;
- genera comentarios, Markdown o UI;
- ejecuta acciones;
- crea una plataforma general de contratos;
- aprende automáticamente de resultados o feedback.

## 9. Vocabulario oficial

### 9.1 Engineering Contract

Un **Engineering Contract** es un artefacto declarativo, consumible por una
persona o sistema, que expresa una forma esperada de interacción, estructura,
configuración o comportamiento entre componentes, herramientas o límites de
ingeniería.

Esta definición no declara que todo artefacto declarativo sea un contrato. Un
archivo sólo participa como Engineering Contract cuando Evidence y una Rule
vigente permiten sostener esa clasificación.

### 9.2 Modified File Evidence

**Modified File Evidence** es la observación inmutable transportada por
`local-context.json` que demuestra que un path participó del cambio analizado
con un `change_status` y una procedencia determinados.

Demuestra modificación reportada del archivo. No demuestra que:

- su contenido esté disponible;
- su sintaxis sea válida;
- represente realmente el tipo sugerido por su path;
- su semántica haya cambiado;
- el cambio sea compatible o incompatible.

### 9.3 Contract Candidate

Un **Contract Candidate** es un archivo modificado para el cual existe Evidence
que justifica evaluar si representa un Engineering Contract.

Ser candidato no equivale a ser contrato confirmado. La candidatura conserva la
señal que la originó, sus límites y la Rule aplicable.

### 9.4 Detected Contract

Un **Detected Contract** es un Contract Candidate cuyo Contract Type puede
sostenerse dentro del alcance disponible mediante Evidence y una Rule vigente.

“Detected” significa clasificado para esta ejecución. No significa parseado,
válido, vigente, compatible, crítico ni completo.

### 9.5 Contract Type

**Contract Type** es una categoría tecnológica descriptiva que identifica la
clase de artefacto declarativo representada por un candidato.

Contract Type no representa dominio de negocio, versión, severidad ni
compatibilidad. La taxonomía puede evolucionar de forma gobernada sin cambiar
las unidades epistemológicas de Capability-002.

### 9.6 Change Status

**Change Status** conserva el hecho reportado por Context Retrieval:

- `added`;
- `modified`;
- `deleted`;
- `renamed`.

No describe una diferencia semántica. Por ejemplo, `modified` significa que la
fuente reportó modificación del archivo, no que una operación contractual haya
cambiado.

### 9.7 Classification Basis

**Classification Basis** es la Evidence mínima y las condiciones declarativas
que respaldan la asignación de un Contract Type.

Debe permitir reconstruir qué observación fue utilizada. Nunca se expresa como
una explicación plausible sin referencia.

### 9.8 Detection Sufficiency

**Detection Sufficiency** expresa si la Evidence alcanza para determinar
candidatura y tipo dentro del alcance de Capability-003.

No expresa calidad del contrato ni preparación operativa.

### 9.9 Analysis Readiness

**Analysis Readiness** expresa si la Evidence declarada sería suficiente para
entregar el candidato a una futura capability de análisis.

Capability-003 sólo declara suficiencia o faltantes. No realiza el análisis ni
prescribe cómo obtener la Evidence faltante.

### 9.10 Unknown y Ambiguous

**Unknown** significa que no existe respaldo suficiente para asignar un Contract
Type.

**Ambiguous** significa que la Evidence disponible respalda más de una
clasificación incompatible y no existe precedencia observable.

Ninguno equivale a error. Ambos pueden producir abstención trazable.

### 9.11 Contract Change Detection

**Contract Change Detection** es la determinación acotada de que Modified File
Evidence corresponde a un Contract Candidate o Detected Contract.

No significa detección de cambio semántico ni detección de incompatibilidad.

## 10. Entidades conceptuales

### 10.1 Contract Candidate

Representa el objeto de evaluación de la capability. Debe conservar:

- identidad dentro de una ejecución;
- referencia inequívoca a Modified File Evidence;
- path observado;
- Change Status observado;
- procedencia;
- Scope;
- Classification Basis disponible;
- tipos candidatos, si existen;
- Uncertainty y estado de suficiencia.

Un candidato nunca agrega contenido de archivo ni afirmaciones funcionales.

### 10.2 Contract Type Classification

Representa la clasificación descriptiva de un candidato. Debe contener
conceptualmente:

- Contract Type asignado o ausencia explícita;
- Evidence utilizada;
- Rule que autorizó la clasificación;
- Confidence fundamentada;
- Uncertainty;
- alternativas incompatibles cuando exista ambigüedad;
- Scope y límites de aplicabilidad.

Una clasificación no puede existir sin Candidate y Evidence.

### 10.3 Sufficiency Assessment

Representa la evaluación declarativa de disponibilidad y cobertura. Distingue:

- Evidence de `modified_files` disponible;
- Evidence de `modified_files` no proporcionada;
- Evidence válida pero insuficiente para detectar candidatura;
- candidatura detectable pero tipo no determinable;
- tipo detectable pero Evidence insuficiente para análisis posterior;
- Evidence suficiente para entregar al siguiente límite.

Sufficiency Assessment nunca completa la Evidence faltante.

### 10.4 Contract Detection

Representa un resultado de detección válido y consumible. Relaciona:

- Contract Candidate;
- Contract Type Classification;
- Sufficiency Assessment;
- Traceability completa;
- Confidence;
- Uncertainty;
- preguntas abiertas;
- límites de aplicabilidad.

El futuro contrato de salida decidirá cómo se expone. Este documento no define
su estructura física.

### 10.5 Detection Set

Representa el conjunto ordenado de resultados para una única ejecución y una
única entrada identificable. Conserva:

- cobertura sobre Modified File Evidence;
- detecciones válidas;
- candidatos no clasificados;
- abstenciones;
- contradicciones;
- identidad de entrada y Rules vigentes.

No agrega resultados de revisiones diferentes ni modifica su orden sin una regla
explícita.

## 11. Tipos de contratos soportados

La taxonomía objetivo debe poder representar, sin incorporar parsing ni
semántica específica:

| Familia conceptual           | Contract Types iniciales contemplados |
| ---------------------------- | ------------------------------------- |
| API e interacción            | OpenAPI, AsyncAPI, GraphQL            |
| Schema y serialización       | Avro, Protobuf, JSON Schema           |
| Infraestructura declarativa  | Terraform, Kubernetes CRD             |
| Automatización y composición | GitHub Actions, Docker Compose        |

“Contemplado” significa que la taxonomía puede expresar el tipo y evaluarlo con
casos gobernados. No significa que este documento autorice una Rule específica,
un parser ni detección por contenido.

El núcleo de Capability-003 no contiene lógica intrínseca para ninguno de estos
tipos. Cada clasificación exige Evidence observable y conocimiento declarativo
versionado. Incorporar un nuevo Contract Type no debe cambiar el significado de
Candidate, Detection Sufficiency ni Traceability.

### 11.1 Agnosticismo del dominio

La capability es agnóstica de dominios de negocio y productos consumidores. Un
contrato de pagos y uno de logística siguen el mismo modelo. También es
agnóstica de la semántica interna de cada tecnología: conocer el nombre
“OpenAPI” no implica comprender paths, operations o responses.

### 11.2 Tipos fuera de alcance inicial

Permanecen fuera del alcance:

- contratos inferidos exclusivamente desde código imperativo;
- acuerdos humanos sin representación declarativa identificable;
- documentación narrativa sin señal contractual explícita;
- binarios o artefactos cuyo tipo requiere ejecutar herramientas externas;
- contratos existentes sólo fuera de `local-context.json`;
- formatos propietarios sin taxonomía y casos gobernados;
- contratos cuya identificación requiere contenido no transportado;
- contratos generados durante la ejecución;
- dependencias, consumers y providers inferidos del ecosistema.

Un tipo fuera de alcance no se clasifica como “no contrato”. Se registra como no
evaluable o queda fuera de la cobertura declarada.

## 12. Rule Model

Capability-003 adopta el significado normativo de Rule definido por
Capability-002 y la Reasoning Specification. Este documento define obligaciones
de dominio, no Rules ejecutables.

Una futura Rule de detección debe declarar como mínimo:

- identidad y versión;
- Contract Type al que aplica;
- precondiciones observables;
- Modified File Evidence requerida;
- señal que permite candidatura;
- señal que permite clasificación;
- señales incompatibles o ambiguas;
- Scope;
- Confidence máxima justificable;
- Uncertainty inevitable;
- condiciones de no aplicabilidad;
- condiciones de abstención;
- casos positivos, negativos y de frontera.

Una Rule de Capability-003 puede producir únicamente conocimiento descriptivo
sobre candidatura, tipo y suficiencia. Nunca puede producir:

- contenido no observado;
- una afirmación de compatibilidad;
- un breaking change;
- severidad o criticidad;
- Recommendation o Decision;
- una clasificación basada sólo en preferencia;
- Evidence nueva.

Las Rules son gobernadas externamente, versionadas y evaluadas. Capability-003
es owner del significado de dominio; Capability-002 no crea ni aprueba esas
Rules.

## 13. Evidence necesaria

### 13.1 Evidence mínima disponible actualmente

La única Evidence actualmente autorizada para este dominio es la colección
`modified_files` del Local Context Contract:

- path relativo validado;
- Change Status;
- provenance;
- estado inequívoco de disponibilidad.

Esta Evidence permite detectar sólo aquello que pueda sostenerse sin leer el
archivo. No contiene diff, patch, contenido, lenguaje, clasificación ni
estructura contractual.

### 13.2 Evidence de clasificación

Una clasificación válida requiere que la señal observable indicada por la Rule
esté presente en la Evidence admitida. Una convención de path puede respaldar
una clasificación limitada cuando la Rule y Evaluation demuestran esa relación.
Nunca prueba sintaxis, contenido ni validez funcional.

### 13.3 Evidence para análisis posterior

La Evidence mínima actual puede resultar insuficiente para una futura capability
que compare contenido o estructura. Capability-003 debe declarar ese faltante;
no puede leer el archivo para resolverlo.

Ampliar Local Context Contract con contenido contractual, identidades de
versiones u otras observaciones requiere evolución gobernada previa. Este
documento no autoriza esa evolución.

### 13.4 Evidence no proporcionada

Cuando `modified_files` es `not_provided`:

- no se infiere una colección vacía;
- no se afirma que no cambiaron contratos;
- no nacen candidatos;
- la cobertura sobre archivos modificados es desconocida;
- corresponde una abstención total para esta capability.

### 13.5 Colección disponible y vacía

Cuando `modified_files` es `available` y la colección está vacía, existe
Evidence de que el productor entregó cero archivos modificados soportados para
esa revisión. Capability-003 puede producir un resultado completo con cero
detecciones, sin convertirlo en una afirmación universal sobre otras fuentes.

## 14. Relación con modified_files

Cada Contract Candidate debe referenciar exactamente un elemento de
`modified_files`. Una detección compuesta puede relacionar varios candidatos
sólo si una futura Rule lo autoriza y conserva cada referencia.

Capability-003:

- preserva path, Change Status, provenance y orden contractual;
- no deduplica nuevamente Evidence ya admitida;
- no normaliza estados con otra taxonomía;
- no interpreta `renamed` como compatibilidad o continuidad semántica;
- no interpreta `deleted` como breaking change;
- no interpreta `added` como compatibilidad;
- no interpreta `modified` como cambio funcional;
- no resuelve paths contra un filesystem;
- no usa provenance como credencial ni autorización de acceso.

El Scope de toda detección queda limitado a la revisión, repositorio, archivo y
Evidence identificados por la entrada.

## 15. Relación con Evidence, Claims, Hypotheses y Findings

Capability-003 no crea un modelo epistemológico alternativo.

### 15.1 Evidence

Evidence conserva la observación de `modified_files` y su disponibilidad. La
candidatura, clasificación y suficiencia no son Evidence de entrada: son
conocimiento derivado.

### 15.2 Claims

Claims admisibles pueden afirmar únicamente hechos descriptivos derivados, por
ejemplo que un archivo es candidato, que una señal de clasificación está
presente o que Evidence requerida no fue proporcionada.

Un Claim nunca puede afirmar compatibilidad, impacto o breaking change dentro de
Capability-003.

### 15.3 Hypotheses

Hypotheses pueden relacionar Claims compatibles para proponer una clasificación
verificable y falsable. Deben expresar qué Evidence adicional confirmaría o
refutaría el tipo cuando la clasificación es limitada.

No se utiliza una Hypothesis para ocultar ambigüedad ni para convertir una
convención de path en certeza.

### 15.4 Findings

Findings consumibles pueden expresar únicamente:

- candidato a contrato detectado;
- Contract Type respaldado;
- Contract Type ambiguo o no determinado;
- Detection Sufficiency;
- Analysis Readiness;
- ausencia o limitación de Evidence;
- abstención y preguntas abiertas.

Nunca expresan breaking change, severidad, riesgo, Recommendation, aprobación o
rechazo.

### 15.5 Trazabilidad mínima

Toda detección debe reconstruir:

```text
Contract Detection
        ↓
Finding
        ↓
Hypothesis
        ↓
Claim
        ↓
Modified File Evidence
```

El futuro contrato decidirá si Contract Detection es una vista del Finding o una
unidad de salida separada. No puede romper ni ocultar la cadena normativa de
Capability-002.

## 16. Qué consume

Conceptualmente, Capability-003 requiere:

- una entrada Local Context Contract válida y admitida;
- la disponibilidad y colección de `modified_files`;
- identidad canónica de la ejecución;
- Rules de detección gobernadas y versionadas;
- las leyes, invariantes y controles de Capability-002.

No consume:

- `manifest.json`;
- GitHub ni otro proveedor;
- repositorio o checkout;
- diff o patch;
- contenido de contratos;
- archivos intermedios de Context Retrieval;
- comentarios humanos;
- resultados de una capability posterior.

La representación técnica exacta de esta relación se decidirá en TD-004. Este
documento no establece nuevas entradas físicas para Capability-002.

## 17. Qué produce

Capability-003 produce conceptualmente un resultado trazable de Contract Change
Detection que permite conocer:

- identidad y cobertura de la ejecución;
- detecciones válidas;
- Contract Candidates no clasificados;
- Contract Types respaldados;
- Detection Sufficiency y Analysis Readiness;
- Confidence y Uncertainty;
- Evidence utilizada;
- preguntas abiertas;
- Contradictions y Abstentions.

El nombre conceptual previsto para su futuro límite es `contract-change-report`.
Su contrato, versión, completitud y compatibilidad se definirán en un documento
posterior. Este documento no define JSON, schema, serialización ni transporte.

## 18. Pipeline conceptual

```text
Local Context Contract
        ↓
Modified File Evidence Admission
        ↓
Contract Candidate Detection
        ↓
Contract Type Classification
        ↓
Detection Sufficiency Assessment
        ↓
Validation
        ↓
Contract Detection Findings
        ↓
contract-change-report
```

### 18.1 Modified File Evidence Admission

Preserva disponibilidad, identidad, orden, path, Change Status y provenance. No
accede a fuentes ni interpreta contenido.

### 18.2 Contract Candidate Detection

Evalúa si existe respaldo para considerar un archivo como candidato. No fuerza
clasificación ni excluye silenciosamente casos ambiguos.

### 18.3 Contract Type Classification

Relaciona Evidence y conocimiento declarativo gobernado para asignar un tipo
cuando corresponde. Unknown y Ambiguous son resultados legítimos.

### 18.4 Detection Sufficiency Assessment

Determina qué pudo establecerse y qué Evidence falta para continuar. No obtiene
esa Evidence ni diseña la capability posterior.

### 18.5 Validation

Comprueba leyes, Scope, Traceability, Confidence, Uncertainty y límites de
dominio. Rechaza toda unidad que exceda candidatura, tipo o suficiencia.

### 18.6 Contract Detection Findings

Expone conocimiento descriptivo validado. No agrega interpretación de producto.

### 18.7 Contract Change Report

Consolida el resultado conceptual sin modificar las unidades. Su contrato queda
postergado.

## 19. Invariantes

En toda ejecución deben cumplirse simultáneamente:

1. toda Evidence proviene de la entrada contractual admitida;
2. toda detección referencia Modified File Evidence existente;
3. ausencia de `modified_files` nunca equivale a colección vacía;
4. colección vacía nunca se convierte en ausencia desconocida;
5. un archivo modificado no es automáticamente un contrato;
6. un Contract Candidate no es automáticamente un Detected Contract;
7. todo Contract Type requiere Classification Basis trazable;
8. Unknown no se reemplaza por el tipo más probable;
9. Ambiguous conserva todas las alternativas respaldadas;
10. Change Status nunca se interpreta como cambio semántico;
11. ninguna detección afirma compatibilidad o incompatibilidad;
12. ninguna detección afirma severidad, riesgo ni criticidad;
13. Detection Sufficiency no implica validez del contrato;
14. Analysis Readiness no implica resultado del análisis;
15. toda inferencia conserva Confidence y Uncertainty propias;
16. toda cadena termina en Evidence de la misma ejecución;
17. ninguna Rule actúa como Evidence;
18. ninguna unidad modifica la entrada;
19. ningún resultado contiene Recommendation o Decision;
20. toda abstención explica condición, alcance y Evidence disponible;
21. resultados de entradas diferentes no se mezclan;
22. mismo contexto canónico y mismas Rules producen el mismo resultado
    conceptual.

La violación de un invariante invalida la unidad afectada o exige abstención. No
se relaja el dominio para aumentar detecciones.

## 20. Principios

### 20.1 Evidence Before Classification

La clasificación comienza en Evidence admitida. Una taxonomía o una expectativa
no demuestra que un archivo pertenezca a un tipo.

### 20.2 Detection Before Analysis

Primero se determina qué puede analizarse. La interpretación funcional y la
comparación pertenecen a capabilities posteriores.

### 20.3 Explicit Sufficiency

La capability declara qué Evidence posee y qué le falta. No confunde detección
exitosa con preparación para un análisis más profundo.

### 20.4 Traceability by Design

Cada candidato y clasificación nace con referencias a Modified File Evidence y a
la Rule que autorizó la derivación.

### 20.5 Format-Neutral Core

El modelo permanece estable entre OpenAPI, Terraform o cualquier otro Contract
Type. La variabilidad de cada tecnología vive en conocimiento declarativo
gobernado.

### 20.6 Domain Agnostic

La capability no conoce dominios de negocio ni necesidades de productos
consumidores.

### 20.7 Abstention over Guessing

Unknown, Ambiguous y Abstention son preferibles a una clasificación sin
respaldo.

### 20.8 Human Decision

El resultado informa. Una persona o capability autorizada decide si debe
realizarse un análisis posterior.

### 20.9 Reproducibility

Toda diferencia material debe atribuirse a Evidence, taxonomía o Rules
versionadas.

### 20.10 Minimal Evidence

Sólo se transporta y utiliza la Evidence necesaria. La posibilidad futura de
analizar contenido no autoriza recuperarlo ahora.

## 21. Degradación

### 21.1 Entrada inválida

Un Local Context Contract inválido detiene la ejecución. No se publica un
resultado parcial como si fuera válido.

### 21.2 Evidence no proporcionada

`modified_files: not_provided` produce abstención total de Capability-003. La
ejecución puede continuar para otras Rules independientes de Capability-002,
pero no existe cobertura de Contract Change Detection.

### 21.3 Evidence disponible y vacía

Produce cero candidatos con cobertura completa dentro del alcance declarado. No
requiere abstención por ausencia.

### 21.4 Candidato no clasificable

El candidato se conserva como Unknown con Uncertainty explícita o produce una
abstención local, según permita la futura Rule y el contrato de salida.

### 21.5 Clasificación ambigua

Se preservan alternativas incompatibles y su Evidence. No se elige precedencia
sin respaldo.

### 21.6 Tipo no soportado

Se declara fuera de cobertura o no evaluable. No se clasifica como “no
contrato”.

### 21.7 Evidence insuficiente para análisis posterior

La detección puede seguir siendo válida y declarar Analysis Readiness
insuficiente. Esa limitación no invalida lo que sí fue detectado.

### 21.8 Falla de una Rule

Una Rule inválida, contradictoria o no identificable no produce detecciones. Las
Rules independientes pueden continuar si conservan Scope y trazabilidad.

## 22. Abstención

Capability-003 debe abstenerse cuando:

- `modified_files` no fue proporcionado;
- no existe Evidence para determinar candidatura;
- asignar un Contract Type exigiría inventar contenido;
- dos tipos incompatibles poseen igual respaldo sin precedencia;
- la Rule aplicable no satisface sus precondiciones;
- el tipo requerido está fuera de cobertura;
- una referencia de Traceability está rota;
- Confidence resulta insuficiente;
- Uncertainty impide delimitar Scope;
- el resultado implicaría breaking change, severidad, Recommendation o Decision.

La abstención puede ser:

- local para un Candidate o clasificación;
- parcial para parte de `modified_files`;
- total cuando no existe Evidence de archivos modificados o ninguna detección
  puede sostenerse.

Toda abstención declara la condición incumplida, Evidence disponible, Evidence
faltante cuando puede nombrarse, Scope restante y pregunta abierta.

## 23. Métricas

Las métricas evalúan la calidad de detección, no una tecnología ni el desempeño
individual de personas.

### 23.1 Trazabilidad

- proporción de detecciones con cadena completa hasta Modified File Evidence;
- referencias rotas o ambiguas;
- clasificaciones sin Classification Basis;
- detecciones que mezclan ejecuciones.

El objetivo normativo para referencias rotas y clasificaciones sin Evidence es
cero.

### 23.2 Precisión de candidatura

- proporción de Contract Candidates confirmados por evaluación autorizada;
- tasa de archivos no contractuales clasificados como candidatos;
- tasa de contratos elegibles omitidos;
- precisión por familia y Contract Type.

### 23.3 Precisión de clasificación

- proporción de Contract Types correctamente respaldados;
- confusiones entre tipos;
- clasificaciones forzadas en casos Unknown o Ambiguous;
- consistencia bajo Evidence equivalente.

### 23.4 Suficiencia explícita

- proporción de resultados que distinguen detección y Analysis Readiness;
- tasa de Evidence ausente presentada incorrectamente como colección vacía;
- tasa de candidatos sin faltantes explícitos cuando no pueden analizarse;
- calidad de preguntas abiertas.

### 23.5 Abstención

- tasa de abstención correcta en casos insuficientes;
- abstenciones innecesarias en casos detectables;
- clasificaciones fabricadas que debieron abstenerse;
- distribución de abstención local, parcial y total.

### 23.6 Cobertura

- proporción de Modified File Evidence evaluada;
- proporción clasificada, Unknown, Ambiguous y fuera de cobertura;
- Contract Types cubiertos por casos gobernados;
- Evidence no procesada con causa explícita.

### 23.7 Reproducibilidad

- resultados equivalentes para mismo contexto canónico y mismas Rules;
- diferencias atribuibles a cambios versionados;
- estabilidad del orden contractual;
- detecciones cuya identidad no puede reconstruirse.

## 24. Evaluation

Evaluation ocurre fuera del pipeline productor y no modifica Evidence, Rules ni
resultados.

### 24.1 Golden Dataset conceptual

El conjunto debe contener paths y procedencia sanitizados, con clasificación
esperada establecida por evaluadores autorizados antes de ejecutar la
capability.

Debe cubrir:

- un caso positivo y uno negativo por Contract Type;
- convenciones de path inequívocas y ambiguas;
- extensiones compartidas por distintos tipos;
- nombres sin extensión;
- archivos no contractuales con extensiones similares;
- `added`, `modified`, `deleted` y `renamed`;
- colección presente y vacía;
- colección no proporcionada;
- tipos desconocidos y fuera de cobertura;
- duplicados rechazados por el contrato de entrada;
- provenance diferente;
- ejecuciones repetidas;
- Rules aplicables, no aplicables y contradictorias.

### 24.2 Evaluadores

Los evaluadores deben conocer las convenciones tecnológicas representadas, pero
no pueden corregir retroactivamente una ejecución ni convertir su juicio en
Evidence de la misma entrada.

### 24.3 Criterios

Evaluation revisa:

- validez de Candidate;
- corrección de Contract Type;
- respaldo de Classification Basis;
- suficiencia declarada;
- Confidence y Uncertainty;
- corrección de Abstention;
- integridad de Traceability;
- cumplimiento de anti-goals;
- reproducibilidad y cobertura.

### 24.4 Umbrales

Los umbrales cuantitativos se fijan antes de promover la capability. No se
ajustan retroactivamente para aceptar un resultado. Deben priorizar evitar
clasificaciones falsas y breaking changes inventados sobre maximizar cantidad de
detecciones.

## 25. Riesgos y trade-offs

| Riesgo o trade-off                       | Consecuencia                            | Respuesta requerida                                 |
| ---------------------------------------- | --------------------------------------- | --------------------------------------------------- |
| Path tratado como prueba absoluta        | Clasificación incorrecta                | Confidence limitada y Evaluation negativa           |
| Definición demasiado amplia de contrato  | Todo archivo parece contractual         | Candidatura gobernada y casos negativos             |
| Taxonomía demasiado específica           | Núcleo acoplado a formatos              | Contract Type externo al modelo estable             |
| Taxonomía demasiado genérica             | Resultado poco útil                     | Familias y tipos explícitos sin parsing             |
| Ausencia tratada como vacío              | Falsa cobertura                         | Disponibilidad preservada y abstención total        |
| `modified` tratado como semántica        | Breaking change inventado               | Change Status sólo como hecho de fuente             |
| Unknown forzado a una categoría          | Falsa certeza                           | Unknown y Ambiguous de primera clase                |
| Exceso de abstención                     | Baja utilidad                           | Medir cobertura sin debilitar respaldo              |
| Baja abstención                          | Clasificaciones fabricadas              | Casos adversariales y umbrales previos              |
| Tipo soportado confundido con analizable | Consumidor inicia análisis sin Evidence | Analysis Readiness separada                         |
| Rule específica invade el motor          | Capability-002 pierde agnosticismo      | Gobierno externo y núcleo format-neutral            |
| Consumidor atribuye severidad            | Decisión sin autoridad                  | Contrato de salida y límites explícitos             |
| Crecimiento prematuro de fuentes         | Complejidad y permisos innecesarios     | Evolución contractual sólo por necesidad demostrada |

El trade-off rector es aceptar menos clasificaciones a cambio de que cada
detección sea trazable y no prometa análisis que todavía no puede realizarse.

## 26. Definition of Done

Capability-003 se considera terminada sólo cuando cumple simultáneamente:

### 26.1 Dominio y límites

- propósito, outcome, owner y consumidores están aceptados;
- Candidate, Contract Type, Classification Basis, Detection Sufficiency y
  Analysis Readiness poseen definiciones no ambiguas;
- detección permanece separada de parsing, comparación y breaking changes;
- tipos soportados y fuera de cobertura están explícitos;
- la capability demuestra agnosticismo de dominio y formato en su núcleo;
- ningún resultado contiene Recommendation ni Decision.

### 26.2 Contratos

- la Evidence proviene exclusivamente del Local Context Contract vigente;
- ausencia y colección vacía permanecen distinguibles;
- existe un Contract Change Report Contract Accepted;
- productor y consumidores comprenden completitud e incompletitud;
- toda evolución incompatible sigue gobernanza mediante ADR;
- identidades y compatibilidad pueden verificarse.

### 26.3 Razonamiento

- existe una Reasoning Specification Accepted para Capability-003;
- toda detección conserva la cadena hasta Modified File Evidence;
- Rules iniciales están gobernadas, versionadas y evaluadas;
- Unknown, Ambiguous y Abstention poseen comportamiento verificable;
- Confidence y Uncertainty cumplen la normativa de Capability-002;
- ninguna Rule produce compatibilidad, severidad ni breaking change;
- mismo contexto canónico y Rules producen resultados reproducibles.

### 26.4 Calidad

- no existen clasificaciones publicadas sin Evidence;
- no existen referencias rotas;
- se alcanzan umbrales preacordados de precisión, trazabilidad, suficiencia,
  abstención, cobertura y reproducibilidad;
- casos negativos no son promovidos como contratos;
- tipos ambiguos no reciben precedencia inventada;
- contextos sin `modified_files` producen abstención correcta;
- no existe acceso a fuentes externas durante la ejecución.

### 26.5 Outcome

- al menos dos familias de Contract Type demuestran el modelo transversal;
- al menos un consumidor autorizado puede utilizar detecciones sin releer la
  fuente;
- evaluadores confirman que el resultado reduce trabajo de localización sin
  crear falsa certeza;
- existe una decisión explícita de aceptar, iterar o retirar basada en
  evidencia;
- el outcome supera la mera clasificación de extensiones.

Implementar código o producir una demo convincente no satisface por sí solo esta
Definition of Done.

## 27. Roadmap documental y de validación

### Etapa 0 — Dominio

Aceptar propósito, vocabulario, entidades, límites, tipos iniciales y Definition
of Done de Capability-003.

**Demo observable:** revisión de casos de frontera que separa archivo
modificado, Contract Candidate, Detected Contract y breaking change.

### Etapa 1 — Contract Change Report Contract

Definir el límite conceptual estable entre Capability-003 y sus consumidores.

**Demo observable:** dos consumidores interpretan el mismo resultado sin acceder
a la fuente ni ampliar garantías.

### Etapa 2 — Reasoning Specification

Definir nacimiento, validez, combinación, descarte y abstención de las unidades
de Contract Change Detection.

**Demo observable:** recorrido completo desde Modified File Evidence hasta una
detección válida y un caso abstendido.

### Etapa 3 — TD-004

Diseñar la arquitectura técnica mínima sin elegir infraestructura anticipada.

**Demo observable:** pipeline técnico y fronteras verificables sobre entradas
controladas.

### Etapa 4 — Implementación incremental

Construir baby steps, cada uno con una única capacidad demostrable. Los
incrementos concretos serán definidos por TD-004 y no se anticipan aquí.

### Etapa 5 — Integración con un consumidor

Integrar sólo después de aceptar dominio, contrato, reasoning, diseño técnico y
outcome mínimo.

**Demo observable:** el consumidor presenta detecciones sin reconstruirlas ni
convertirlas en breaking changes.

## 28. Decisiones tomadas

| ID         | Decisión                                                       | Estado   |
| ---------- | -------------------------------------------------------------- | -------- |
| CAP003-D01 | La capability detecta candidatura, tipo y suficiencia          | Accepted |
| CAP003-D02 | Detectar modificación no implica cambio semántico              | Accepted |
| CAP003-D03 | Breaking Change Detection pertenece a una capability posterior | Accepted |
| CAP003-D04 | `modified_files` es la única Evidence autorizada actualmente   | Accepted |
| CAP003-D05 | Ausencia y colección vacía poseen significados distintos       | Accepted |
| CAP003-D06 | El núcleo es agnóstico de dominio y neutral al formato         | Accepted |
| CAP003-D07 | Las Rules específicas son externas, gobernadas y versionadas   | Accepted |
| CAP003-D08 | Unknown y Ambiguous son resultados legítimos                   | Accepted |
| CAP003-D09 | Detection Sufficiency y Analysis Readiness son distintas       | Accepted |
| CAP003-D10 | El futuro límite se denomina `contract-change-report`          | Accepted |
| CAP003-D11 | No existe acceso a fuentes ni lectura de contenido             | Accepted |
| CAP003-D12 | Ningún resultado contiene severidad, Recommendation o Decision | Accepted |

Estas decisiones quedaron Accepted mediante la promoción coordinada del
documento y sus dependencias normativas.

## 29. Decisiones postergadas

Este documento no decide:

- Contract Change Report Contract;
- estructura física o serialización del resultado;
- categorías físicas de Findings;
- Rules concretas de detección;
- señales exactas por Contract Type;
- estrategia de resolución entre convenciones;
- parsing o validación de formatos;
- recuperación futura de contenido;
- comparación base versus head;
- definición de breaking change;
- severidad, criticidad o riesgo;
- consumidores y dependencias;
- thresholds cuantitativos sin baseline;
- mecanismo de distribución o lifecycle de Rules;
- runtime, lenguaje, biblioteca o framework;
- persistencia, caché, transporte, hosting o UI;
- integración con VS-001;
- nueva infraestructura compartida.

Cada decisión futura exige una necesidad demostrada y no queda autorizada por
ser mencionada aquí.

## 30. Suposiciones

- Local Context Contract conserva Evidence autorizada e inmutable;
- `modified_files` contiene señal suficiente para evaluar al menos algunos
  candidatos mediante convenciones explícitas;
- las convenciones iniciales pueden establecerse y evaluarse sin leer contenido;
- evaluadores pueden construir casos positivos, negativos y ambiguos;
- futuros consumidores pueden respetar el límite entre detección y análisis;
- al menos dos familias tecnológicas permiten demostrar neutralidad del núcleo.

Si estas suposiciones no se sostienen, la capability debe abstenerse o promover
una evolución contractual gobernada; no debe ampliar acceso de forma implícita.

## 31. Preguntas abiertas

- ¿Qué Contract Types deben participar en el primer experimento sin intentar
  cubrir toda la taxonomía?
- ¿Qué señales de path son suficientemente explícitas para cada tipo?
- ¿Cuándo una señal permite Detected Contract y cuándo sólo Contract Candidate?
- ¿Cómo debe representar el futuro contrato Unknown y Ambiguous?
- ¿Qué diferencia contractual debe existir entre Detection Sufficiency y
  Analysis Readiness?
- ¿Qué Contract Types requieren contenido antes incluso de confirmar
  candidatura?
- ¿Qué umbral de falsos positivos es aceptable para un experimento limitado?
- ¿Qué consumidor validará primero la utilidad de la detección?
- ¿Qué Evidence mínima necesitará la capability posterior de análisis?
- ¿La taxonomía debe versionarse independientemente de las Rules?
- ¿Cómo se retiran convenciones que dejaron de ser confiables?
- ¿Qué señal demostraría que una familia no pertenece a esta capability?

Las preguntas abiertas no autorizan implementación ni evolución contractual.

## 32. Criterio de aceptación y evolución

Capability-003 puede promoverse a **Accepted** cuando:

- Architecture Owner acepta su frontera con Capability-001 y Capability-002;
- Capability Owner acepta vocabulario, tipos y anti-goals;
- al menos un consumidor valida el outcome esperado;
- Contract Candidate no puede confundirse con breaking change;
- Evidence actual y faltante están claramente separadas;
- invariantes, degradación y abstención son evaluables;
- las decisiones postergadas no se presentan como autorizadas;
- los riesgos abiertos poseen respuesta verificable.

Una evolución futura requiere revisar explícitamente:

- impacto sobre contratos Accepted;
- cambios de autoridad o acceso a fuentes;
- efecto sobre las leyes de Capability-002;
- compatibilidad con consumidores;
- estrategia de Evaluation;
- necesidad de ADR ante incompatibilidad o cambio arquitectónico.

## 33. Historial del documento

| Fecha      | Cambio                                                          | Estado   |
| ---------- | --------------------------------------------------------------- | -------- |
| 2026-08-04 | Propuesta inicial de Capability-003 — Contract Change Detection | Proposed |
| 2026-08-05 | Promoción formal coordinada                                     | Accepted |
