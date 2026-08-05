# Contract Change Detection Reasoning Specification

| Campo                | Valor                                               |
| -------------------- | --------------------------------------------------- |
| Documento            | Contract Change Detection Reasoning Specification   |
| Capability           | Capability-003 — Contract Change Detection          |
| Estado               | **Proposed**                                        |
| Tipo                 | Especificación normativa de razonamiento de dominio |
| Owner propuesto      | Engineering Platform                                |
| Última actualización | 4 de agosto de 2026                                 |

---

## 1. Propósito

Este documento define el modelo oficial propuesto de razonamiento de
Capability-003. Establece cuándo nace cada unidad de Contract Change Detection,
qué condiciones mantienen su validez, cómo se relaciona con otras unidades y
cuándo debe limitarse, descartarse o producir Abstention.

Es la **autoridad normativa única** para el razonamiento específico de
Capability-003.

La especificación no redefine el modelo epistemológico de Capability-002.
Modified File Evidence, Contract Candidate y Contract Detection participan en la
cadena Evidence → Claim → Hypothesis → Finding y deben respetar todas las leyes
Accepted del Inference Engine.

Capability-003 sólo razona sobre Evidence suficiente para detectar la existencia
observada de un candidato y su Contract Type. Nunca interpreta el contenido
funcional del archivo.

## 2. Autoridad

Esta especificación deriva de:

1. Product Vision v1.1;
2. Architecture v1.0 y sus ADR vigentes;
3. ADR-013 — Local Context Contract;
4. Local Context Contract, versión conceptual 1;
5. Capability-002 — Inference Engine;
6. Inference Report Contract, versión conceptual 1;
7. Inference Engine Reasoning Specification;
8. Capability-003 — Contract Change Detection;
9. Contract Change Detection Contract, versión conceptual 1 Proposed.

Los contratos gobiernan las fronteras. Capability-003 gobierna propósito,
vocabulario y límites. La Inference Engine Reasoning Specification continúa
siendo autoridad sobre Evidence, Claim, Hypothesis, Finding, Rule, Confidence,
Uncertainty, Traceability, Contradiction y Abstention como conceptos
transversales.

Este documento especializa esas leyes para Contract Change Detection. No puede
debilitarlas. Ante una contradicción prevalece la fuente Accepted de mayor
autoridad.

## 3. Alcance

Esta especificación define:

- admisión y estados de Modified File Evidence;
- nacimiento, validez e identidad de Contract Candidate;
- nacimiento, validez e identidad de Contract Detection;
- asignación y límites de Contract Type;
- conformación y validez de Classification Basis;
- cálculo conceptual de Detection Sufficiency;
- cálculo conceptual de Analysis Readiness;
- compatibilidad de Scope;
- propagación de Confidence y Uncertainty;
- Traceability y Coverage;
- Contradiction y Abstention;
- Laws, Invariants y prohibiciones;
- determinismo;
- Definition of Correctness.

El alcance comienza con una entrada válida que conserva la disponibilidad de
Modified File Evidence y termina con unidades aptas para el Contract Change
Detection Contract.

Quedan fuera parsing, análisis funcional, comparación, compatibilidad, breaking
changes, severidad, Recommendations, Decisions y acciones.

## 4. Vocabulario normativo

En este documento:

- **debe** expresa una obligación;
- **no debe** expresa una prohibición;
- **puede** expresa una posibilidad permitida bajo condiciones explícitas;
- **válido** significa que una unidad cumple todas sus leyes e invariantes;
- **inválido** significa que una condición esencial fue violada;
- **candidato** significa que una unidad todavía debe atravesar validación;
- **descartado** significa que un candidato no puede avanzar;
- **limitado** significa que una unidad válida conserva restricciones;
- **publicable** significa que una unidad puede integrar un resultado válido;
- **disponible** significa que la Evidence fue transportada y admitida;
- **no proporcionado** significa que el productor anterior no transportó la
  Evidence requerida;
- **suficiente** significa que las precondiciones declaradas se satisfacen;
- **Abstention** significa no producir una inferencia que excedería el respaldo.

Validez no equivale a verdad universal. Detection válida significa derivación
correcta dentro de Evidence, Rules y Scope, no confirmación mediante parsing.

## 5. Principios epistemológicos

### 5.1 Primacía de Modified File Evidence

Toda detección comienza en un elemento admitido de `modified_files`. Un nombre,
taxonomía, expectativa o Rule no sustituye Evidence.

### 5.2 Detección antes que análisis

La capability sólo identifica candidatura, tipo y suficiencia. No interpreta qué
declara el contrato ni qué cambió en su semántica.

### 5.3 Clasificación acotada

Contract Type nunca afirma más que la Classification Basis y Scope permiten.

### 5.4 Ausencia inequívoca

`not_provided` y `available` con colección vacía son estados distintos. Ausencia
de Evidence nunca prueba ausencia de contratos.

### 5.5 Trazabilidad obligatoria

Toda Detection permite reconstruir Candidate, unidades de razonamiento y
Modified File Evidence.

### 5.6 Suficiencia explícita

Detection Sufficiency y Analysis Readiness se expresan por separado. Ninguna se
oculta detrás de Confidence.

### 5.7 Incertidumbre explícita

Unknown, Ambiguous, Evidence parcial y tipos fuera de cobertura conservan sus
límites sin clasificación probable.

### 5.8 Contradicción preservada

Clasificaciones incompatibles respaldadas no se resuelven sin precedencia
observable.

### 5.9 Abstención legítima

No producir una Detection es correcto cuando las condiciones no alcanzan.

### 5.10 Núcleo neutral al formato

El modelo no cambia entre OpenAPI, Terraform, GraphQL u otro Contract Type. La
variabilidad pertenece a conocimiento declarativo gobernado.

### 5.11 Reproducibilidad

Misma entrada canónica y mismas Rules producen el mismo resultado conceptual.

### 5.12 Responsabilidad humana

La capability detecta. Una capability posterior o persona decide si realiza otro
análisis.

## 6. Estructura del razonamiento

La cadena específica es:

```text
Modified File Evidence
          ↓
Candidate Claims
          ↓
Classification Hypotheses
          ↓
Detection Findings
          ↓
Contract Detection
```

Contract Candidate es el objeto de evaluación respaldado por Claims. Contract
Detection es la vista contractual de Findings válidos. Ninguno reemplaza los
niveles epistemológicos de Capability-002.

Classification Basis autoriza la relación entre Evidence y tipo. Detection
Sufficiency y Analysis Readiness califican lo que puede sostenerse. Confidence y
Uncertainty acompañan cada unidad derivada. Scope limita toda afirmación.
Coverage describe cuánto de la Evidence elegible fue evaluado. Contradiction y
Abstention preservan conflictos y límites.

## 7. Identidad conceptual general

La identidad conceptual depende del significado completo, no del texto visible.

Toda identidad debe incluir conceptualmente:

- identidad de la entrada canónica;
- identidad y versión de Rules vigentes;
- Scope;
- unidades de soporte;
- afirmación o clasificación;
- estado material;
- Confidence y Uncertainty cuando forman parte del significado.

Cambiar Evidence, Rule, Scope, tipo, estado o soporte crea una nueva unidad. Una
unidad publicada nunca cambia silenciosamente.

El orden contractual de colecciones se conserva. El orden físico de propiedades
o formato de serialización no cambia identidad cuando el contrato lo considera
irrelevante.

## 8. Ciclo de vida de Modified File Evidence

### 8.1 Nacimiento

Modified File Evidence nace para una ejecución cuando un elemento válido de
`modified_files` es admitido desde Local Context Contract y puede identificarse
sin alterar:

- path;
- Change Status;
- provenance;
- posición contractual;
- identidad de entrada.

Capability-003 no crea esta Evidence. La recibe de Context Retrieval a través
del límite gobernado.

### 8.2 Disponibilidad

La colección posee uno de dos estados:

- **available:** el campo fue proporcionado, incluso si contiene cero elementos;
- **not_provided:** el campo no fue transportado por el productor.

Un elemento individual sólo puede nacer cuando la colección es available.

### 8.3 Admisión

Un elemento es admitido cuando:

- pertenece a la entrada identificada;
- path es válido y relativo;
- Change Status pertenece a la taxonomía contractual;
- provenance es íntegra y coherente con la entrada;
- identidad no colisiona con otro elemento;
- el orden puede preservarse.

Admisión no demuestra que el archivo sea contrato ni que exista fuera de la
Evidence.

### 8.4 Estados conceptuales

Modified File Evidence puede estar:

- **available:** observación válida y utilizable;
- **referenced:** respalda al menos un Candidate o unidad derivada;
- **evaluated:** fue considerada por todas las Rules aplicables;
- **not_applicable:** ninguna Rule vigente aplica;
- **outside_coverage:** no existe conocimiento gobernado para evaluarla;
- **in_conflict:** participa en una Contradiction;
- **not_used:** ninguna derivación válida la utiliza.

Estos estados no modifican la Evidence.

### 8.5 Identidad

Dos Modified File Evidence son la misma observación sólo cuando comparten
entrada, path, Change Status, provenance y posición contractual equivalente.

Mismo path con distinto Change Status o provenance representa Evidence distinta
o una entrada inválida según el contrato productor.

### 8.6 Inmutabilidad

Path, Change Status y provenance nunca cambian durante una ejecución. Normalizar
nuevamente, resolver el path o completar provenance está prohibido.

### 8.7 Pérdida de validez

Evidence pierde admisibilidad cuando se descubre que viola el contrato de
entrada, su identidad es ambigua o su referencia pertenece a otra ejecución.

La pérdida invalida unidades dependientes, pero no altera Evidence
independiente.

### 8.8 Fin del ciclo

El ciclo termina como evaluated, not_applicable, outside_coverage o invalid. No
puede quedar Evidence elegible sin estado al declarar Coverage completa.

## 9. Ciclo de vida de Contract Candidate

### 9.1 Candidato preliminar

Un candidato preliminar aparece cuando una Rule vigente encuentra una señal
observable que podría justificar evaluar un archivo como Engineering Contract.

Todavía no es Contract Candidate válido ni Contract Detection.

### 9.2 Nacimiento

Contract Candidate nace cuando:

- referencia exactamente una Modified File Evidence primaria;
- una Rule vigente autoriza la candidatura;
- la señal observable está presente;
- Scope es explícito y compatible con Evidence;
- identidad es inequívoca;
- Confidence inicial está fundamentada;
- Uncertainty material está declarada;
- no afirma Contract Type sin respaldo.

### 9.3 Estados

Puede estar:

- **identified:** candidatura válida pendiente de clasificación;
- **classified:** Contract Type respaldado;
- **unknown:** no existe soporte suficiente para asignar tipo;
- **ambiguous:** existen tipos incompatibles respaldados;
- **outside_coverage:** ninguna Rule gobernada cubre la señal;
- **abstained:** una condición impide continuar;
- **invalid:** viola una ley esencial;
- **discarded:** un preliminar no alcanzó validez.

### 9.4 Identidad

Dos Candidates son conceptualmente el mismo sólo cuando comparten:

- Modified File Evidence primaria;
- señal de candidatura;
- Rule y versión;
- Scope;
- estado material;
- Classification Basis aplicable.

Igual path con distinta Rule o Scope produce Candidates distintos.

### 9.5 Validez

Candidate permanece válido mientras su Evidence, Rule, Scope, Confidence,
Uncertainty y referencias sean válidos.

### 9.6 Pérdida de validez

Pierde validez cuando:

- Evidence deja de ser admisible;
- Rule no estaba vigente;
- señal no existe;
- Scope fue ampliado;
- identidad colisiona;
- Classification Basis fue fabricada;
- Uncertainty material fue ocultada;
- incorpora parsing, breaking change, severidad o acción.

### 9.7 Descarte

Un preliminar se descarta cuando:

- no existe Evidence primaria;
- la Rule no aplica;
- la señal es insuficiente incluso para candidatura;
- requeriría leer contenido;
- el Scope no puede delimitarse;
- producirlo violaría una prohibición.

El descarte conserva causa para Evaluation, pero no se publica como Candidate.

### 9.8 Combinación

Candidates no se combinan por compartir path, extensión o Type. Una relación
compuesta requiere Rule explícita y Scope compatible. La Evidence primaria de
cada Candidate permanece distinguible.

### 9.9 Fin del ciclo

El ciclo termina en classified, unknown, ambiguous, outside_coverage, abstained,
invalid o discarded. Ningún Candidate queda provisional en un reporte completo.

## 10. Ciclo de vida de Contract Detection

### 10.1 Candidata a Detection

Nace una candidata cuando Claims e Hypotheses válidas sostienen una
clasificación o un estado explícito de no resolución para un Candidate.

### 10.2 Nacimiento

Contract Detection nace cuando:

- Candidate es válido;
- Findings de soporte son válidos;
- Contract Type o ausencia explícita es coherente;
- Classification Basis está íntegra;
- Detection Sufficiency está determinada;
- Analysis Readiness está determinada o declarada unknown;
- Scope no excede sus soportes;
- Confidence y Uncertainty son propias;
- Traceability está completa;
- Coverage puede reconciliarla;
- ninguna prohibición fue violada.

### 10.3 Estados

Puede estar:

- **detected:** existe Type respaldado;
- **unknown:** Candidate válido sin Type suficiente;
- **ambiguous:** tipos incompatibles conservados;
- **outside_coverage:** conocimiento vigente no cubre el caso;
- **abstained:** no puede sostenerse una clasificación válida;
- **invalid:** viola una condición esencial;
- **discarded:** candidata que no alcanzó validez.

### 10.4 Identidad

Dos Detections son la misma sólo cuando conservan:

- Candidate;
- estado;
- Contract Type o alternativas;
- Classification Basis;
- Detection Sufficiency;
- Analysis Readiness;
- Scope;
- Findings y Traceability;
- Confidence y Uncertainty materiales.

Cambiar cualquiera de esos elementos crea otra Detection.

### 10.5 Pérdida de validez

Pierde validez cuando una dependencia esencial se invalida, una referencia se
rompe, el Type deja de estar respaldado, Sufficiency contradice Evidence,
Readiness excede sus precondiciones o aparece Uncertainty material omitida.

### 10.6 Descarte

Una candidata se descarta si no puede alcanzar trazabilidad completa, mezcla
ejecuciones, duplica otra Detection sin significado distinto o contiene una
afirmación prohibida.

### 10.7 Detection limitada

Puede ser válida y limitada cuando Type está respaldado pero Readiness es
not_ready, Coverage es parcial o existe Uncertainty que no invalida la
clasificación.

Limitada no significa incompleta estructuralmente.

### 10.8 Fin del ciclo

Toda candidata termina en estado válido, invalid o discarded. Un reporte no
publica candidatas provisionales.

## 11. Ciclo de vida de Contract Type

### 11.1 Elegibilidad

Un Type es elegible cuando pertenece a la taxonomía gobernada y una Rule vigente
declara señales observables para él.

Enumerarlo en Capability-003 no basta para asignarlo.

### 11.2 Asignación

Contract Type se asigna cuando:

- Candidate es válido;
- Classification Basis satisface la Rule;
- Scope es compatible;
- no existe alternativa incompatible con igual respaldo sin precedencia;
- Confidence alcanza el mínimo declarado;
- Uncertainty permite una clasificación limitada pero válida.

### 11.3 Estados

Un Type puede estar:

- **assigned:** clasificación válida;
- **candidate:** alternativa todavía no validada;
- **conflicted:** alternativa dentro de Contradiction;
- **unsupported:** fuera de taxonomía vigente;
- **rejected:** no satisface la Rule.

Unknown, Ambiguous y outside_coverage son estados de Detection, no Contract
Types.

### 11.4 Identidad

La identidad del Type incluye su categoría y versión taxonómica. Dos etiquetas
iguales bajo taxonomías incompatibles no son el mismo Type gobernado.

### 11.5 Descarte

Una asignación candidata se descarta si la señal falta, la Rule no aplica, Type
está fuera de cobertura o asignarlo requeriría interpretar contenido.

### 11.6 Prohibiciones

Type nunca implica sintaxis válida, contenido, versión del formato,
compatibilidad, severidad, criticidad ni acción.

## 12. Ciclo de vida de Classification Basis

### 12.1 Nacimiento

Classification Basis nace cuando se relacionan explícitamente:

- Evidence observada;
- señal de candidatura o clasificación;
- Rule y versión;
- taxonomía vigente;
- Scope;
- límites conocidos.

### 12.2 Estados

Puede estar:

- **complete:** contiene todos los soportes requeridos;
- **partial:** permite candidatura, pero no Type definitivo;
- **conflicted:** soportes o Rules sostienen alternativas incompatibles;
- **insufficient:** no autoriza clasificación;
- **invalid:** contiene referencias rotas o inventadas.

### 12.3 Identidad

Dos Basis son iguales sólo cuando comparten Evidence, señal, Rule, taxonomía,
Scope y límites. Igual explicación con soporte diferente representa otra Basis.

### 12.4 Validez

Permanece válida mientras todas sus referencias y relaciones estén vigentes. Una
Rule no cuenta como Evidence; sólo autoriza la relación.

### 12.5 Descarte

Se descarta como soporte publicable cuando falta Evidence, la señal no es
observable, la Rule no aplica o la relación sólo puede sostenerse leyendo
contenido.

### 12.6 Fin del ciclo

Termina complete, partial, conflicted, insufficient, invalid o discarded. Una
Detection detected exige Basis complete.

## 13. Ciclo de vida de Detection Sufficiency

### 13.1 Nacimiento

Detection Sufficiency nace cuando la Evidence elegible y las precondiciones de
las Rules aplicables fueron consideradas para un Candidate o ejecución.

### 13.2 Estados

- **sufficient:** todas las condiciones de detección se satisfacen;
- **partial:** puede sostenerse parte del resultado;
- **insufficient:** una condición esencial no se satisface;
- **not_provided:** la Evidence requerida no fue transportada;
- **invalid:** el estado contradice Evidence o Coverage.

### 13.3 Identidad

Depende de unidad calificada, Evidence considerada, precondiciones, Rule, Scope
y faltantes. Cambiar un faltante material crea otra Sufficiency.

### 13.4 Transiciones permitidas

Dentro de una evaluación inmutable no cambia silenciosamente. Evidence adicional
exige nueva entrada y nueva unidad. Una validación puede pasar una candidata a
estado válido o invalid, no completar faltantes.

### 13.5 Descarte

Una Sufficiency candidata se descarta si no identifica unidad, Evidence,
precondiciones o Scope.

### 13.6 Reglas

`not_provided` nunca se transforma en insufficient ni en colección vacía.

`partial` no autoriza una Detection detected si falta una condición esencial de
Type.

`sufficient` no implica Analysis Readiness.

## 14. Ciclo de vida de Analysis Readiness

### 14.1 Nacimiento

Analysis Readiness nace cuando una Detection válida puede compararse con
precondiciones declaradas para entregar el resultado a una capability posterior.

No requiere conocer el dominio ni la implementación de Capability-004.

### 14.2 Estados

- **ready:** todas las precondiciones declaradas están satisfechas;
- **not_ready:** existe un faltante conocido;
- **unknown:** no puede determinarse con el contrato disponible;
- **not_applicable:** no existe Detection apta;
- **invalid:** contradice Evidence o precondiciones.

### 14.3 Identidad

Depende de Detection, conjunto versionado de precondiciones, Evidence, Scope y
faltantes. Cambiar precondiciones crea otra Readiness.

### 14.4 Transiciones permitidas

No cambia durante una ejecución. Una capability posterior puede producir su
propia evaluación, pero no modifica retroactivamente esta unidad.

### 14.5 Descarte

Se descarta si no existe Detection válida, salvo que el estado correcto sea
not_applicable, o si pretende evaluar precondiciones no declaradas.

### 14.6 Prohibiciones

`ready` nunca significa contenido válido, análisis exitoso, compatibilidad ni
autorización para actuar.

## 15. Ciclo de vida de Scope

### 15.1 Nacimiento

Scope nace de límites explícitos de entrada, Evidence, Rules, taxonomía y unidad
calificada.

### 15.2 Componentes conceptuales

Puede incluir:

- entrada y ejecución;
- repositorio y revisión como procedencia;
- path;
- Change Status;
- Rule y taxonomía;
- cobertura;
- Candidate o Detection.

Estos componentes no conceden acceso.

### 15.3 Identidad

Dos Scope son iguales sólo cuando todas sus fronteras materiales coinciden.

### 15.4 Compatibilidad

Dos Scope son compatibles cuando:

- pertenecen a la misma entrada y ejecución;
- sus paths coinciden o una Rule autoriza relación explícita;
- sus límites taxonómicos no son incompatibles;
- existe intersección demostrable;
- la unidad derivada queda limitada a esa intersección.

### 15.5 Incompatibilidad

Son incompatibles cuando:

- pertenecen a ejecuciones distintas;
- describen paths distintos sin relación autorizada;
- dependen de Rules o taxonomías mutuamente excluyentes;
- no existe intersección demostrable;
- combinarlos elimina una limitación material.

Compartir Type, extensión o nombre no demuestra compatibilidad.

### 15.6 Propagación

La unidad derivada recibe como máximo la intersección de los Scope de soporte y
las restricciones de la Rule. Nunca se amplía.

### 15.7 Descarte

Un Scope candidato se descarta si no puede delimitarse o depende de una
suposición.

## 16. Ciclo de vida de Confidence

### 16.1 Nacimiento

Confidence nace con toda unidad derivada válida. Incluye nivel, fundamento,
limitaciones y dimensiones relevantes.

### 16.2 Dimensiones mínimas

Para Capability-003 deben considerarse separadamente:

- directness de Evidence;
- fuerza de la señal de candidatura;
- fuerza de Classification Basis;
- cobertura de precondiciones;
- consistencia de Rules;
- integridad de Traceability;
- compatibilidad de Scope;
- suficiencia de Evidence.

### 16.3 Identidad

Confidence cambia si cambia nivel, fundamento, limitación o dimensión material.
No es una etiqueta aislada.

### 16.4 Propagación

Confidence de una unidad derivada:

- nunca supera el soporte más débil esencial;
- nunca aumenta por repetición o duplicación;
- disminuye ante Basis indirecta, Coverage parcial o Contradiction material;
- puede permanecer fuerte con Uncertainty no esencial, si el fundamento lo
  explica;
- se vuelve insufficient cuando falta una condición esencial.

### 16.5 Separación

Candidate, Type, Sufficiency y Readiness poseen Confidence propias cuando
corresponde. Confidence fuerte de Type no se copia a Readiness.

### 16.6 Pérdida de validez

Confidence es inválida si carece de fundamento, oculta limitaciones, se expresa
como probabilidad no calibrada o representa severidad.

## 17. Ciclo de vida de Uncertainty

### 17.1 Nacimiento

Uncertainty nace cuando una ausencia, ambigüedad, conflicto, cobertura parcial o
límite de Rule afecta lo que puede sostenerse.

### 17.2 Orígenes

- Evidence not_provided;
- señal indirecta;
- Type desconocido;
- alternativas ambiguas;
- formato fuera de cobertura;
- falta de contenido para confirmar clasificación;
- Scope incompatible;
- Rule no aplicable o contradictoria;
- Detection Sufficiency partial o insufficient;
- Analysis Readiness not_ready o unknown;
- Traceability limitada;
- Coverage parcial o desconocida.

### 17.3 Identidad

Depende de condición, unidad afectada, Evidence, Scope, efecto y faltante. Dos
textos iguales sobre unidades distintas son Uncertainty distintas.

### 17.4 Propagación

Una Uncertainty se propaga a toda unidad cuya validez, Confidence, Scope,
Sufficiency o Readiness dependa de resolverla.

No se propaga a unidades independientes.

### 17.5 Reducción

Sólo Evidence adicional en una nueva entrada y una Rule válida pueden reducir
una Uncertainty material. No se elimina por plausibilidad ni decisión del
consumidor.

### 17.6 Bloqueo

Impide una Detection detected cuando no puede asignarse Type sin elegir una
suposición, delimitar Scope o preservar Traceability.

### 17.7 Fin del ciclo

Permanece explicitada, produce Abstention o deja de aplicar en una nueva unidad
respaldada. Nunca se borra retroactivamente.

## 18. Ciclo de vida de Traceability

### 18.1 Nacimiento

Traceability nace con la primera relación Candidate Claim → Modified File
Evidence y se extiende en cada transición.

### 18.2 Cadena mínima

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

Cada transición identifica Rule y Scope.

### 18.3 Estados

Puede estar:

- **complete:** todas las referencias existen;
- **limited:** cadena íntegra con límites explícitos;
- **broken:** al menos una referencia falta o es ambigua;
- **invalid:** mezcla entradas o contiene soporte inventado.

### 18.4 Identidad

La identidad depende del grafo completo de unidades y relaciones. Cambiar un
vínculo esencial crea otra Traceability y otra Detection.

### 18.5 Propagación

Cada unidad hereda únicamente referencias explícitas de sus soportes. No hereda
Evidence de Candidates no relacionados.

### 18.6 Ruptura

Una referencia rota invalida la unidad dependiente y propaga invalidez hasta la
Detection. No invalida Evidence independiente.

### 18.7 Reparación

No se repara dentro de la misma unidad. Requiere una nueva derivación válida o
Abstention.

## 19. Ciclo de vida de Coverage

### 19.1 Nacimiento

Coverage nace al establecer el conjunto de Modified File Evidence elegible y las
Rules vigentes para la ejecución.

### 19.2 Estados

- **complete:** toda Evidence elegible alcanzó estado explícito;
- **partial:** parte no pudo evaluarse con causa conocida;
- **unknown:** Evidence requerida fue not_provided;
- **invalid:** conteos, identidades o estados no pueden reconciliarse.

### 19.3 Identidad

Depende del conjunto ordenado elegible, unidades evaluadas, estados finales,
Rules y Scope. Cambiar un elemento material crea otra Coverage.

### 19.4 Reglas de contabilización

Cada Modified File Evidence elegible debe terminar exactamente una vez como:

- Candidate evaluado;
- not_applicable;
- outside_coverage;
- abstained;
- invalid con falla de ejecución.

Una Evidence puede respaldar varias inferencias, pero no se cuenta varias veces
como alcance procesado.

### 19.5 Colección vacía

Available con cero elementos permite Coverage complete con cero Candidates.

### 19.6 Evidence no proporcionada

Not_provided exige Coverage unknown y Abstention total para Capability-003.

### 19.7 Pérdida de validez

Coverage es inválida si omite Evidence, duplica alcance, confunde unknown con
cero o declara complete con unidades provisionales.

## 20. Ciclo de vida de Abstention

### 20.1 Candidata

Surge cuando producir una unidad violaría una precondición, Law o Invariant.

### 20.2 Nacimiento

Abstention nace cuando:

- identifica la unidad impedida;
- declara condición incumplida;
- referencia Evidence disponible;
- identifica Evidence faltante cuando es posible;
- conserva Scope afectado y restante;
- relaciona Confidence, Uncertainty o Contradiction;
- formula una pregunta abierta sin prescribir acción.

### 20.3 Alcance

- **local:** Candidate, Type o Detection específica;
- **partial:** parte de Coverage;
- **total:** ninguna Detection válida puede producirse para la capability.

### 20.4 Identidad

Depende de unidad impedida, condición, Evidence, Scope, faltante y tipo de
alcance. Cambiar cualquiera crea otra Abstention.

### 20.5 Condiciones obligatorias

Debe existir ante:

- `modified_files` not_provided;
- Evidence insuficiente para candidatura;
- Type que requeriría contenido no disponible;
- clasificación Ambiguous sin formulación limitada válida;
- Scope incompatible;
- Confidence insufficient;
- Traceability rota;
- resultado que implicaría parsing, compatibilidad, breaking change, severidad,
  Recommendation o Decision.

### 20.6 Validez

Una Abstention válida no afirma que el contrato no existe ni que una proposición
sea falsa.

### 20.7 Descarte

Se descarta si no posee condición observable, oculta Evidence disponible, se
redacta como inferencia negativa o prescribe una acción.

### 20.8 Relación con reporte

Abstention no es un estado de reporte separado. Un reporte complete o incomplete
puede conservar Abstention, incluso total, si Coverage e integridad son
coherentes.

## 21. Ciclo de vida de Contradiction

### 21.1 Candidata

Surge cuando dos o más posiciones respaldadas parecen incompatibles dentro de un
Scope comparable.

### 21.2 Nacimiento

Contradiction nace cuando:

- existen al menos dos posiciones explícitas;
- cada posición posee Evidence y derivación válida;
- Scope es compatible;
- las posiciones no pueden ser verdaderas simultáneamente dentro de ese Scope;
- no existe precedencia respaldada que resuelva el conflicto;
- efecto sobre unidades dependientes puede identificarse.

### 21.3 Casos admisibles

- dos Types incompatibles para el mismo Candidate;
- Rules vigentes incompatibles;
- Sufficiency incompatibles sobre la misma Evidence;
- Coverage declarada complete frente a alcance omitido.

### 21.4 No contradicciones

No constituyen Contradiction:

- Evidence ausente;
- Type desconocido;
- falta de una Rule;
- dos Candidates de Scope distinto;
- diferencia de formato textual;
- Confidence distintas sin posiciones incompatibles.

### 21.5 Identidad

Depende de posiciones, Evidence, Rules, Scope y relación de incompatibilidad.

### 21.6 Efecto

Contradiction:

- conserva todas las posiciones;
- genera Uncertainty;
- limita Confidence;
- bloquea unidades que requieren elegir precedencia;
- permite unidades independientes;
- puede producir Abstention local o parcial.

### 21.7 Resolución

No se resuelve por orden, preferencia ni mayoría. Requiere Evidence o
precedencia gobernada en una nueva derivación. La Contradiction original
permanece inmutable.

### 21.8 Descarte

Una candidata se descarta si los Scope no son comparables, una posición carece
de respaldo o no existe incompatibilidad material.

## 22. Compatibilidad y combinación de Scope

### 22.1 Regla general

Sólo pueden combinarse unidades de la misma entrada, con Scope compatible y una
Rule que autorice la relación.

### 22.2 Intersección

El Scope resultante es la intersección demostrable más las restricciones de la
Rule. No puede conservar una frontera más amplia que sus soportes.

### 22.3 Candidates del mismo archivo

Pueden relacionarse si representan señales distintas sobre la misma Evidence y
la Rule explica la relación. No se fusionan automáticamente.

### 22.4 Candidates de archivos distintos

No se combinan para afirmar un único contrato salvo Rule explícita y Evidence de
relación. Compartir directorio, extensión o Type es insuficiente.

### 22.5 Taxonomías

Unidades producidas bajo versiones taxonómicas distintas son incompatibles salvo
regla de compatibilidad gobernada.

### 22.6 Ejecuciones

Unidades de ejecuciones distintas nunca se combinan dentro de una Detection. Una
capability agregadora futura debe conservar identidades separadas.

## 23. Reglas de propagación de Confidence

### 23.1 Candidate

Su Confidence no supera directness de Modified File Evidence y fuerza de la
señal de candidatura.

### 23.2 Contract Type

No supera Candidate, Classification Basis ni Rule de clasificación.

### 23.3 Detection Sufficiency

No supera la cobertura de precondiciones observadas. Un faltante esencial obliga
insufficient.

### 23.4 Analysis Readiness

Se calcula independientemente sobre precondiciones declaradas. No hereda nivel
de Contract Type.

### 23.5 Contract Detection

No supera la Confidence más débil de Candidate, Type, Sufficiency, Findings y
Traceability esenciales.

### 23.6 Contradiction

Reduce Confidence sólo de unidades que dependen de resolver el conflicto.

### 23.7 Repetición

Duplicar una convención o inferencia no eleva Confidence.

### 23.8 Coverage

Coverage parcial limita Confidence de afirmaciones de completitud, no
necesariamente la clasificación local de una Evidence procesada.

## 24. Reglas de propagación de Uncertainty

### 24.1 Propagación esencial

Toda Uncertainty que afecte una condición esencial se propaga a Candidate, Type,
Sufficiency, Readiness y Detection dependientes.

### 24.2 Propagación localizada

Una limitación de un archivo no afecta Detections de otros archivos sin
dependencia.

### 24.3 Evidence no proporcionada

Se propaga a Coverage unknown, Detection Sufficiency not_provided y Abstention
total.

### 24.4 Type ambiguo

Se propaga a Detection ambiguous y bloquea detected, salvo que una formulación
limitada no elija un Type.

### 24.5 Readiness

Readiness unknown no invalida una Detection local válida; limita lo que el
consumidor puede concluir sobre el siguiente paso.

### 24.6 No eliminación

Ninguna unidad derivada elimina Uncertainty sin Evidence y Rule que expliquen
por qué dejó de aplicar.

## 25. Reglas de determinismo

Una ejecución determinista requiere:

- identidad canónica de entrada;
- versión contractual soportada;
- disponibilidad de Modified File Evidence;
- orden contractual preservado;
- taxonomía versionada;
- Rules identificadas y versionadas;
- Scope explícito;
- reglas de orden de resultados declaradas;
- ausencia de reloj, aleatoriedad o estado externo no gobernado.

Misma entrada conceptual, taxonomía y Rules deben producir:

- mismos Candidates;
- mismos estados;
- mismos Types;
- misma Sufficiency y Readiness;
- mismas Contradictions y Abstentions;
- misma Coverage;
- mismas identidades conceptuales;
- mismo orden contractual.

Una diferencia material sólo es válida si puede atribuirse a entrada, taxonomía
o Rules diferentes.

## 26. Laws of Contract Change Detection

Las siguientes leyes son obligatorias y se suman a las Laws de Capability-002.

### Ley 1 — Evidence autorizada

Toda detección comienza exclusivamente en Modified File Evidence admitida.

### Ley 2 — Evidence inmutable

Path, Change Status y provenance nunca cambian durante la ejecución.

### Ley 3 — Ausencia distinguida

Not_provided nunca equivale a available vacío.

### Ley 4 — Vacío observado

Available vacío permite cero Candidates con Coverage completa dentro del Scope.

### Ley 5 — Candidatura respaldada

Todo Contract Candidate referencia Modified File Evidence y Rule vigente.

### Ley 6 — Candidate no es Detection

Ser candidato nunca prueba Contract Type.

### Ley 7 — Type respaldado

Todo Contract Type asignado posee Classification Basis completa.

### Ley 8 — Unknown preservado

Ausencia de tipo respaldado nunca se reemplaza por una categoría probable.

### Ley 9 — Ambigüedad preservada

Types incompatibles respaldados permanecen visibles sin precedencia inventada.

### Ley 10 — Change Status literal

Added, modified, deleted y renamed describen hechos de fuente, no cambios
semánticos.

### Ley 11 — No contenido

Capability-003 nunca lee, interpreta, resume ni reconstruye contenido del
contrato.

### Ley 12 — No parsing

Capability-003 nunca parsea ni valida sintaxis de un formato.

### Ley 13 — No breaking changes

Ninguna unidad afirma presencia o ausencia de breaking changes.

### Ley 14 — No compatibilidad

Ninguna unidad calcula compatibilidad backward, forward ni bilateral.

### Ley 15 — No severidad

Ninguna unidad asigna severidad, criticidad, prioridad, impacto o riesgo.

### Ley 16 — Sufficiency separada

Detection Sufficiency no equivale a Analysis Readiness.

### Ley 17 — Readiness limitada

Ready nunca garantiza análisis exitoso ni autoriza ejecutarlo.

### Ley 18 — Scope no expansivo

Toda unidad derivada permanece dentro de la intersección de sus soportes.

### Ley 19 — Confidence fundamentada

Toda Confidence contiene nivel, fundamento, limitaciones y dimensiones.

### Ley 20 — Confidence no es certeza

Confidence nunca representa verdad, probabilidad, severidad ni prioridad.

### Ley 21 — Uncertainty explícita

Toda limitación material se conserva en las unidades afectadas.

### Ley 22 — Contradiction preservada

Posiciones incompatibles no se resuelven sin precedencia respaldada.

### Ley 23 — Coverage reconciliable

Toda Evidence elegible alcanza exactamente un estado de cobertura.

### Ley 24 — Abstention obligatoria

La capability se abstiene cuando producir una unidad violaría otra ley.

### Ley 25 — Abstention no es negación

No detectar nunca equivale a afirmar que no existe contrato.

### Ley 26 — Traceability completa

Toda Detection termina en Modified File Evidence de la misma entrada.

### Ley 27 — Rule no es Evidence

Una Rule autoriza relaciones, pero nunca demuestra tipo ni existencia.

### Ley 28 — Identidad preservada

Cambiar soporte, Scope, Type, estado o límite crea una nueva unidad.

### Ley 29 — Determinismo

Mismas condiciones gobernadas producen el mismo resultado conceptual.

### Ley 30 — Nunca Recommendations

Capability-003 nunca recomienda acciones.

### Ley 31 — Nunca Decisions

Capability-003 nunca aprueba, rechaza ni toma decisiones.

### Ley 32 — No acción

Ningún resultado ejecuta ni autoriza una acción.

### Ley 33 — Evaluación separada

Una ejecución no se evalúa ni corrige a sí misma.

### Ley 34 — Consumidor no retroactivo

Un análisis posterior nunca modifica retroactivamente la Detection original.

## 27. Invariants

En todo punto deben cumplirse simultáneamente:

1. la entrada es única e identificable;
2. la disponibilidad de `modified_files` es inequívoca;
3. Evidence permanece inmutable;
4. toda unidad posee identidad no ambigua;
5. toda derivación identifica Rule vigente;
6. todo Candidate posee Evidence primaria;
7. toda Detection posee Candidate válido;
8. todo Type asignado posee Basis completa;
9. Unknown y Ambiguous no son Contract Types;
10. Scope es explícito y no expansivo;
11. unidades combinadas poseen Scope compatible;
12. Detection Sufficiency es explícita;
13. Analysis Readiness es explícita o not_applicable;
14. Sufficiency y Readiness no se sustituyen;
15. Confidence está completa;
16. Uncertainty material está explícita;
17. Contradictions permanecen visibles;
18. Coverage reconcilia toda Evidence elegible;
19. Traceability está cerrada sobre la entrada;
20. referencias rotas invalidan dependientes;
21. estados provisionales no se publican;
22. Abstention conserva condición y Scope;
23. ausencia no se convierte en negación;
24. Change Status no se convierte en semántica;
25. no existe contenido interpretado;
26. no existe parsing;
27. no existen breaking changes;
28. no existe cálculo de compatibilidad;
29. no existe severidad, riesgo ni prioridad;
30. no existen Recommendations ni Decisions;
31. ninguna unidad modifica sus soportes;
32. mismo input y Rules producen mismo resultado;
33. el reporte conserva completitud real.

Violar un Invariant obliga a invalidar la unidad o producir Abstention. Nunca se
relaja una exigencia para aumentar detecciones.

## 28. Prohibiciones normativas

Capability-003 no debe:

- acceder al repositorio, checkout o provider;
- leer archivos o diffs;
- interpretar contenido funcional;
- parsear OpenAPI, AsyncAPI, GraphQL ni otro formato;
- validar sintaxis;
- comparar base y head;
- inferir endpoints, events, fields, resources o schemas;
- detectar breaking changes;
- calcular compatibilidad;
- calcular severidad, criticidad, riesgo o impacto;
- descubrir consumers o dependencias;
- completar Evidence faltante;
- elegir Type por mayoría o preferencia;
- usar Rule como Evidence;
- elevar Confidence por repetición;
- ocultar Unknown, Ambiguous o Contradiction;
- presentar not_provided como vacío;
- presentar Abstention como ausencia de contrato;
- recomendar acciones;
- aprobar ni rechazar;
- tomar Decisions;
- modificar resultados por feedback durante la ejecución.

Una unidad que viola una prohibición es inválida aunque parezca útil.

## 29. Propagación de invalidez

Cuando una unidad queda inválida:

1. se identifican dependientes directos;
2. cada dependiente se reevalúa sin esa unidad;
3. si conserva soporte suficiente, nace una nueva unidad limitada;
4. si pierde una condición esencial, queda invalidado;
5. la invalidación continúa hasta Contract Detection;
6. Coverage registra el alcance afectado;
7. puede nacer Abstention si la limitación es trazable;
8. unidades independientes permanecen intactas.

Invalidez nunca se propaga hacia Evidence independiente ni se corrige inventando
soporte.

## 30. Completitud del razonamiento

El razonamiento es completo cuando:

- la disponibilidad de Modified File Evidence fue considerada;
- toda Evidence elegible alcanzó estado explícito;
- todo candidato preliminar terminó válido o descartado;
- todo Candidate válido alcanzó estado final;
- toda Classification Basis alcanzó estado final;
- Sufficiency y Readiness están determinadas;
- Coverage es reconciliable;
- Contradictions y Abstentions están declaradas;
- Traceability está íntegra;
- Confidence y Uncertainty están completas;
- ninguna limitación conocida fue ocultada.

Completo no significa que existan Detections. Available vacío puede producir
razonamiento completo con cero Candidates. Not_provided produce Coverage
unknown, Abstention total y un reporte incomplete pero válido cuando todas las
limitaciones están correctamente preservadas.

El razonamiento es incompleto cuando parte del Scope no pudo evaluarse, pero las
unidades publicadas siguen siendo válidas. Es inválido cuando una unidad
publicada viola una Law o Invariant.

## 31. Traceability normativa

Toda revisión debe poder recorrer en ambos sentidos:

```text
Contract Detection
  ↕
Finding
  ↕
Hypothesis
  ↕
Claim
  ↕
Modified File Evidence
```

Además debe identificar:

- Classification Basis;
- Rule y versión en cada transición;
- Scope;
- Confidence;
- Uncertainty;
- Contradictions;
- Abstentions;
- Coverage;
- identidad de entrada y ejecución.

Una Detection cuyo Type puede verse pero cuyo soporte no puede reconstruirse no
es correcta.

## 32. Evaluation

Evaluation ocurre fuera de la cadena productora y no cambia el resultado.

### 32.1 Objetos evaluados

- admisión de Modified File Evidence;
- Candidate y descartes;
- Type y Classification Basis;
- Detection Sufficiency;
- Analysis Readiness;
- compatibilidad de Scope;
- Confidence y Uncertainty;
- Traceability y Coverage;
- Contradictions y Abstentions;
- determinismo;
- cumplimiento de Laws y prohibiciones.

### 32.2 Casos mínimos

- `modified_files` not_provided;
- available vacío;
- uno y varios archivos;
- Candidate positivo y negativo;
- Type respaldado, unknown y ambiguous;
- tipo outside_coverage;
- Basis complete, partial e insufficient;
- Sufficiency sufficient, partial, insufficient y not_provided;
- Readiness ready, not_ready, unknown y not_applicable;
- Scope compatible e incompatible;
- Confidence fuerte, moderada, débil e insufficient;
- Uncertainty limitante y bloqueante;
- Contradiction válida y candidata descartada;
- Abstention local, partial y total;
- Coverage complete, partial, unknown e invalid;
- referencia completa y rota;
- entradas y Rules repetidas;
- contenido que intenta inducir parsing o decisiones;
- casos de OpenAPI, AsyncAPI, GraphQL y familias no API sin interpretar
  contenido.

### 32.3 Métricas normativas

- Candidates sin Evidence: cero;
- Types sin Basis completa: cero;
- Detections con referencias rotas: cero;
- not_provided tratado como vacío: cero;
- breaking changes o compatibilidad producidos: cero;
- severidades, Recommendations o Decisions: cero;
- Contradictions ocultadas: cero;
- clasificaciones fabricadas que debieron abstenerse: cero;
- reproducibilidad bajo condiciones equivalentes;
- precisión de Candidate y Type;
- corrección de Sufficiency, Readiness, Coverage y Abstention.

### 32.4 Independencia

Evaluation no crea Evidence, no cambia Rules, no modifica Confidence, no repara
Traceability y no convierte feedback en verdad.

## 33. Definition of Correctness

Una ejecución de Capability-003 es **correcta** si y sólo si cumple
simultáneamente todas las condiciones siguientes.

### 33.1 Corrección de entrada

- la entrada y versión son soportadas;
- disponibilidad de Modified File Evidence está preservada;
- Evidence admitida es válida e inmutable;
- identidad canónica y orden contractual se conservan.

### 33.2 Corrección de derivación

- cada Candidate nace bajo todas sus precondiciones;
- cada Type posee Classification Basis completa;
- cada Detection deriva de unidades válidas;
- cada relación identifica Rule vigente;
- Scope nunca se amplía;
- ningún descarte se publica como unidad válida.

### 33.3 Corrección epistemológica

- Confidence está fundamentada;
- Uncertainty material está explícita;
- Contradictions se preservan;
- Abstentions aparecen cuando son obligatorias;
- absence no se convierte en negación;
- Traceability termina en Evidence.

### 33.4 Corrección de suficiencia

- Detection Sufficiency refleja precondiciones observadas;
- Analysis Readiness refleja únicamente precondiciones declaradas;
- ambos conceptos permanecen separados;
- Coverage reconcilia todo el alcance elegible.

### 33.5 Corrección de límites

- no se interpreta contenido;
- no se realiza parsing;
- no se detectan breaking changes;
- no se calcula compatibilidad;
- no se calcula severidad, riesgo ni impacto;
- no se producen Recommendations ni Decisions;
- no se accede a fuentes externas.

### 33.6 Corrección de salida

- todas las unidades publicadas son válidas;
- estados complete, incomplete e invalid se usan conforme al contrato;
- una Abstention total no se confunde con invalidez;
- identidad y orden son deterministas;
- el resultado satisface Contract Change Detection Contract.

Una ejecución que falla una sola condición no es correcta, aunque clasifique el
Type esperado.

## 34. Preguntas de conformidad

Toda revisión debe responder afirmativamente:

1. ¿La disponibilidad de `modified_files` fue preservada?
2. ¿Cada Evidence pertenece a la entrada?
3. ¿Cada Candidate posee Evidence y Rule?
4. ¿Cada Type posee Basis completa?
5. ¿Unknown y Ambiguous permanecen explícitos?
6. ¿Change Status se mantuvo literal?
7. ¿Sufficiency y Readiness están separadas?
8. ¿Los Scope combinados son compatibles?
9. ¿Confidence incluye fundamento y límites?
10. ¿Uncertainty material está visible?
11. ¿Toda Contradiction posee posiciones respaldadas?
12. ¿Toda Abstention explica condición y Scope?
13. ¿Coverage reconcilia Evidence elegible?
14. ¿Traceability termina en Modified File Evidence?
15. ¿Mismas condiciones producen mismo resultado?
16. ¿No se leyó ni interpretó contenido?
17. ¿No se realizó parsing?
18. ¿No se produjo breaking change ni compatibilidad?
19. ¿No existe severidad, Recommendation ni Decision?
20. ¿Capability-004 puede distinguir detección de análisis propio?

Una respuesta negativa implica no conformidad o Abstention hasta resolver la
condición.

## 35. Fronteras

Esta especificación no autoriza:

- nuevas entradas físicas;
- acceso directo a fuentes;
- contenido contractual;
- diff o comparación base/head;
- parsers;
- Rules concretas;
- taxonomía física;
- breaking change detection;
- cálculo de compatibilidad;
- severidad o riesgo;
- Recommendation, Decision o acción;
- integración con VS-001;
- implementación técnica;
- modificación de contratos Accepted;
- definición de Capability-004.

## 36. Gobernanza y evolución

La evolución debe preservar Capability-002 y Contract Change Detection Contract.

Un cambio es incompatible cuando:

- elimina una Law o Invariant;
- permite Candidate sin Evidence;
- permite Type sin Basis;
- fusiona Sufficiency y Readiness;
- debilita Scope o Traceability;
- presenta ausencia como vacío;
- oculta Unknown, Ambiguous o Contradiction;
- redefine Confidence como certeza;
- elimina Abstention;
- autoriza contenido, parsing, breaking changes o compatibilidad;
- permite severidad, Recommendation o Decision;
- cambia la cadena epistemológica;
- transfiere gobierno de Rules al motor.

Todo cambio incompatible requiere revisión coordinada de Capability-003,
Contract Change Detection Contract y documentos superiores. Si cambia una
frontera arquitectónica o contrato Accepted, requiere ADR.

Agregar un Contract Type no exige cambiar esta especificación cuando conserva el
modelo y sólo agrega conocimiento gobernado. Cambiar el significado de Contract
Type sí exige revisión normativa.

## 37. Criterio de aceptación

La especificación puede promoverse a **Accepted** cuando:

- Capability-003 y su contrato están Accepted o se promueven en una revisión
  coordinada;
- Capability Owner y Architecture Owner aceptan sus Laws;
- todos los ciclos de vida pueden evaluarse;
- Definition of Correctness no contiene ambigüedades;
- casos positivos, negativos, ambiguous y unknown son reproducibles;
- Sufficiency, Readiness y Coverage son consistentes;
- Contradiction y Abstention cumplen la normativa de Capability-002;
- Traceability es completa;
- ningún caso produce contenido interpretado, breaking change o severidad;
- no existen Recommendations ni Decisions;
- Capability-004 puede consumir el contrato sin reconstruir razonamiento.

Una implementación o clasificación correcta por casualidad no basta para aceptar
esta especificación.

## 38. Historial

| Fecha      | Cambio                                                                 | Estado   |
| ---------- | ---------------------------------------------------------------------- | -------- |
| 2026-08-04 | Propuesta inicial de Contract Change Detection Reasoning Specification | Proposed |
