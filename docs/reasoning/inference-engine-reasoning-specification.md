# Especificación de Razonamiento del Inference Engine

| Campo                | Valor                                    |
| -------------------- | ---------------------------------------- |
| Documento            | Inference Engine Reasoning Specification |
| Capability           | Capability-002 — Inference Engine        |
| Estado               | **Proposed**                             |
| Tipo                 | Especificación normativa de razonamiento |
| Owner propuesto      | Engineering Platform                     |
| Última actualización | 4 de agosto de 2026                      |

---

## 1. Propósito

Este documento define el modelo oficial de razonamiento de Capability-002. Su
objetivo es establecer cuándo una unidad epistemológica nace, qué condiciones
mantienen su validez, cómo puede relacionarse con otras unidades y cuándo debe
limitarse, descartarse o producir una abstención.

La especificación convierte el vocabulario de EIP en reglas normativas.
Evidence, Claim, Hypothesis, Finding, Rule, Scope, Confidence, Uncertainty,
Contradiction, Traceability, Abstention y Evaluation tienen aquí significados
precisos y relaciones verificables.

El Inference Engine no crea conocimiento nuevo. Explicita inferencias que pueden
derivarse del conocimiento disponible y gobernado, sin exceder la Evidence ni
ocultar sus límites.

## 2. Autoridad

Esta especificación deriva de:

1. Product Vision v1.1;
2. Architecture v1.0 y sus ADR vigentes;
3. Capability-002 — Inference Engine;
4. Local Context Contract;
5. Inference Report Contract.

Los contratos gobiernan los límites de entrada y salida. Capability-002 gobierna
el propósito y el modelo de dominio. Esta especificación es la **fuente
normativa única** de las Laws, los Invariants y el comportamiento del
razonamiento que ocurre entre ambos límites. Los resúmenes contenidos en
Capability-002 y en el Inference Report Contract no los redefinen.

Ante una contradicción, prevalece el documento Accepted de mayor autoridad. Esta
especificación no amplía entradas, salidas, autoridad ni responsabilidades.

## 3. Alcance

Esta especificación define:

- principios epistemológicos;
- ciclos de vida de Evidence, Claim, Hypothesis y Finding;
- condiciones para combinar Claims;
- significado y efecto de Confidence y Uncertainty;
- tratamiento de Contradictions;
- condiciones de Abstention;
- leyes e invariantes del razonamiento;
- criterios de Evaluation;
- condiciones de conformidad.

El alcance comienza con una entrada válida y termina con unidades aptas para el
Inference Report Contract. Presentar resultados, formular Recommendations, tomar
Decisions y ejecutar acciones permanecen fuera.

## 4. Vocabulario normativo

En este documento:

- **debe** expresa una condición obligatoria;
- **no debe** expresa una prohibición;
- **puede** expresa una posibilidad permitida bajo las condiciones indicadas;
- **válido** significa que una unidad satisface todas sus leyes e invariantes;
- **inválido** significa que al menos una condición esencial fue violada;
- **descartado** significa que una unidad candidata no puede avanzar;
- **limitado** significa que una unidad válida conserva restricciones
  explícitas;
- **publicable** significa que una unidad puede formar parte de un reporte
  válido;
- **abstención** significa la decisión epistemológica de no producir una
  inferencia que excedería el respaldo disponible.

Validez no equivale a verdad universal. Una unidad válida está correctamente
derivada dentro de su Evidence, Rules y alcance.

## 5. Principios epistemológicos

### 5.1 Primacía de Evidence

Toda inferencia comienza en Evidence. Ninguna Rule, repetición, plausibilidad o
preferencia sustituye una observación presente en la entrada.

### 5.2 Separación de niveles

Evidence, Claim, Hypothesis y Finding representan niveles distintos. Una unidad
no puede presentarse como otra para obtener mayor autoridad.

### 5.3 Derivación acotada

Una unidad derivada nunca puede afirmar más de lo que permiten sus unidades de
soporte, su Rule y su alcance conjunto.

### 5.4 Trazabilidad obligatoria

Toda relación conserva referencias completas hasta Evidence. Una inferencia sin
cadena reconstruible no es una inferencia válida.

### 5.5 Incertidumbre explícita

Todo límite material se representa. Ocultar una ausencia, ambigüedad o
contradicción invalida la unidad afectada.

### 5.6 Contradicción preservada

Las posiciones incompatibles no se eliminan ni se reconcilian sin respaldo. La
coherencia aparente nunca tiene prioridad sobre la Evidence.

### 5.7 Abstención legítima

No producir una inferencia es un resultado correcto cuando las condiciones no
alcanzan. Cantidad de Findings no es una medida de validez.

### 5.8 Confidence fundamentada

Confidence expresa grado de respaldo, no certeza. Siempre incluye fundamento,
limitaciones y dimensiones relevantes.

### 5.9 Inmutabilidad epistemológica

Las unidades aceptadas no cambian silenciosamente. Una revisión crea una nueva
unidad o un nuevo estado explícito y conserva la relación con aquello que
reemplaza.

### 5.10 Responsabilidad humana

El motor produce inferencias. Las personas conservan autoridad sobre toda
decisión posterior.

## 6. Estructura del razonamiento

La cadena normativa es:

```text
Evidence
    ↓
Claim
    ↓
Hypothesis
    ↓
Finding
```

Rules autorizan transiciones entre niveles. Confidence y Uncertainty califican
las unidades derivadas. Contradictions pueden aparecer en cualquier relación.
Traceability conecta todo resultado con Evidence. Abstention detiene una
transición cuando sus condiciones no se cumplen.

La cadena puede ramificarse, pero nunca omitir un nivel:

- una Evidence puede respaldar varios Claims;
- un Claim puede referenciar varias Evidence;
- una Hypothesis puede relacionar varios Claims;
- un Claim puede participar en varias Hypotheses;
- un Finding puede relacionar varias Hypotheses;
- una Hypothesis puede contribuir a varios Findings si el alcance permanece
  explícito.

### 6.1 Scope

**Scope** representa la frontera conceptual dentro de la cual una unidad puede
considerarse válida. Delimita a qué porción de la entrada, condiciones,
relaciones y período observable se aplica una afirmación.

Scope proviene exclusivamente de límites explícitos presentes en Evidence, de
las restricciones de las Rules vigentes y de las unidades que participan en una
derivación. Nunca se amplía mediante una suposición.

Scope limita:

- qué puede afirmar un Claim;
- qué Claims pueden relacionarse;
- dónde puede verificarse o falsarse una Hypothesis;
- hasta dónde aplica un Finding;
- qué significa una ausencia o Contradiction;
- qué Confidence puede sostenerse.

Dos Scope son compatibles cuando sus fronteras coinciden o se superponen de
forma explícita y la relación propuesta continúa siendo válida dentro de esa
intersección. También pueden ser compatibles si una Rule permite relacionar un
Scope contenido en otro sin generalizar el resultado.

Dejan de ser compatibles cuando difieren en una condición esencial, describen
contextos mutuamente excluyentes, no poseen intersección demostrable o sólo
pueden combinarse eliminando una limitación material. Compartir vocabulario no
demuestra compatibilidad.

Scope es parte del significado de Claim, Hypothesis y Finding. Cambiarlo cambia
la unidad conceptual.

### 6.2 Identidad conceptual

La identidad conceptual determina cuándo dos unidades representan la misma
inferencia, sin prescribir una representación física.

Dos **Claims** son conceptualmente el mismo Claim sólo cuando conservan la misma
afirmación atómica, Scope, Evidence de soporte y relación autorizada por la
misma Rule vigente. Igual texto con distinto soporte o Scope representa Claims
distintos.

Dos **Hypotheses** son conceptualmente la misma Hypothesis sólo cuando proponen
la misma relación, dentro del mismo Scope, sobre el mismo conjunto de Claims y
bajo la misma Rule. Cambiar una dependencia, condición de verificación o
condición de falsación crea otra Hypothesis.

Dos **Findings** son conceptualmente el mismo Finding sólo cuando conservan la
misma categoría, formulación, Scope, Hypotheses, Traceability, Confidence y
Uncertainty materiales. Una variación en soporte o límites crea otro Finding,
aunque su texto visible coincida.

La identidad nunca se deduce sólo del texto. Depende del significado, alcance y
cadena de derivación completos.

## 7. Ciclo de vida de Evidence

### 7.1 Nacimiento

Evidence nace para una ejecución cuando una observación contenida en la entrada
válida queda identificada de manera inequívoca y puede referenciarse sin alterar
su contenido.

Una afirmación externa a la entrada no puede nacer como Evidence. Una Rule no
puede producir Evidence.

### 7.2 Admisión

Evidence es admitida cuando:

- pertenece a la entrada identificada;
- su ubicación puede reconstruirse;
- conserva el contenido observado;
- su estado de disponibilidad es explícito;
- no fue generada por una inferencia de la misma ejecución.

Admitir Evidence no significa declararla verdadera, vigente, suficiente ni libre
de contradicción.

### 7.3 Estados conceptuales

Evidence puede estar:

- **disponible:** la observación puede consultarse en la entrada;
- **limitada:** existe, pero su alcance o legibilidad está restringido;
- **ausente:** la entrada declara que la observación requerida no está
  disponible;
- **en conflicto:** otra Evidence sostiene una observación incompatible;
- **referenciada:** participa en al menos una unidad derivada;
- **no utilizada:** fue elegible, pero ninguna Rule válida requirió utilizarla.

Estos estados no modifican Evidence. Describen cómo participa en la ejecución.

### 7.4 Inmutabilidad

Una Evidence nunca cambia. No se corrige, resume, completa, reescribe ni
fusiona. Si una observación diferente aparece, constituye otra Evidence o una
nueva entrada.

Una ubicación o contenido alterados rompen identidad. Los Claims dependientes no
pueden conservarse como si su Evidence fuera la misma.

### 7.5 Fin de vigencia dentro de una ejecución

Evidence deja de ser utilizable cuando:

- su referencia ya no puede resolverse;
- se descubre que no pertenece a la entrada identificada;
- su identidad es ambigua;
- fue alterada después de ser admitida;
- una condición contractual esencial resulta inválida.

Una contradicción no elimina Evidence. Sólo limita lo que puede derivarse de
ella.

## 8. Ciclo de vida de Claim

### 8.1 Candidato a Claim

Una afirmación es candidata cuando una Rule vigente permite derivarla de una o
más Evidence admitidas. Antes de satisfacer todas las condiciones sigue siendo
una posibilidad, no un Claim.

### 8.2 Nacimiento de un Claim

Un Claim nace únicamente cuando:

1. expresa una afirmación atómica y evaluable;
2. referencia al menos una Evidence válida;
3. una Rule vigente autoriza la derivación;
4. la afirmación no excede el alcance conjunto de la Evidence;
5. distingue observación de interpretación;
6. declara Confidence y Uncertainty iniciales;
7. puede confirmarse, limitarse o refutarse revisando sus referencias;
8. posee identidad inequívoca dentro de la ejecución.

Si falta una condición, no existe un Claim válido. La unidad puede conservarse
como candidata descartada exclusivamente para Evaluation, pero nunca participa
en Hypotheses ni forma parte del Inference Report.

### 8.3 Estados de Claim

Un Claim puede estar:

- **candidato:** todavía no satisface todas las condiciones;
- **válido:** cumple las leyes y puede participar en razonamiento;
- **limitado:** es válido dentro de restricciones explícitas;
- **contradicho:** existe Evidence o un Claim incompatible en el mismo alcance;
- **invalidado:** perdió una condición esencial;
- **descartado:** no alcanzó validez o dejó de ser admisible;
- **referenciado:** participa en una Hypothesis;
- **publicable:** conserva validez y trazabilidad para un reporte.

Contradicho no significa automáticamente invalidado. Un Claim contradicho puede
seguir siendo una representación válida de una posición respaldada.

### 8.4 Pérdida de validez

Un Claim deja de ser válido cuando:

- alguna Evidence requerida deja de ser válida;
- una referencia esencial se rompe;
- la Rule aplicada no estaba vigente o no aplicaba;
- la afirmación excede la Evidence;
- mezcla más de una afirmación inseparable y no evaluable;
- cambia de significado sin crear una nueva identidad;
- omite Uncertainty material;
- presenta una interpretación como observación directa;
- depende de una suposición no respaldada;
- su alcance ya no puede determinarse.

La aparición de Evidence contraria sólo invalida el Claim cuando demuestra que
la derivación era imposible bajo el mismo alcance y la Rule no permite conservar
posiciones alternativas. En los demás casos, el Claim permanece contradicho y
limitado.

### 8.5 Combinación de Claims

Dos o más Claims pueden combinarse en una Hypothesis cuando:

- todos son válidos;
- sus alcances son compatibles o la diferencia está declarada;
- la relación está permitida por una Rule vigente;
- sus términos no cambian de significado entre Claims;
- la combinación preserva toda Evidence y Uncertainty;
- una contradicción existente queda explícita;
- el conjunto permite una proposición verificable y falsable.

Claims no pueden combinarse cuando:

- pertenecen a entradas distintas sin una relación contractual explícita;
- sus alcances son incompatibles y esa incompatibilidad determina el resultado;
- una unidad requerida está invalidada;
- la combinación oculta una contradicción;
- sólo comparten vocabulario, pero no una relación respaldada;
- la Rule exige Evidence ausente;
- producirían una generalización mayor que su soporte conjunto.

Combinar Claims no los fusiona ni altera. Crea una Hypothesis que los
referencia.

### 8.6 Descarte de un Claim

Un candidato a Claim debe descartarse cuando:

- no referencia Evidence válida;
- no expresa una afirmación atómica y evaluable;
- ninguna Rule vigente permite su derivación;
- excede Scope;
- depende de una suposición;
- no puede declarar Confidence o Uncertainty completas;
- confunde observación con interpretación;
- su identidad o Traceability son ambiguas.

Un Claim antes válido debe descartarse cuando queda invalidado y no puede
formularse una nueva unidad limitada con soporte suficiente. El candidato
descartado conserva su motivo y puede ser examinado por Evaluation. Nunca es una
unidad válida, nunca respalda otra unidad y nunca aparece en el Inference
Report.

## 9. Ciclo de vida de Hypothesis

### 9.1 Candidata a Hypothesis

Una relación propuesta es candidata cuando una Rule permite relacionar uno o más
Claims válidos. Todavía no es una Hypothesis válida hasta demostrar
verificabilidad, falsabilidad, alcance y trazabilidad.

### 9.2 Nacimiento de una Hypothesis

Una Hypothesis nace cuando:

1. referencia todos los Claims que utiliza;
2. los Claims son válidos o limitados de forma explícita;
3. propone una relación clara y acotada;
4. identifica qué permitiría verificarla;
5. identifica qué podría falsarla;
6. una Rule vigente permite la relación;
7. conserva Claims favorables, contrarios y faltantes;
8. declara Confidence y Uncertainty;
9. no se presenta como hecho ni decisión.

Una proposición no verificable o no falsable no nace como Hypothesis.

### 9.3 Estados de Hypothesis

Una Hypothesis puede estar:

- **candidata:** pendiente de satisfacer condiciones;
- **respaldada:** Claims válidos ofrecen soporte suficiente según la Rule;
- **limitada:** conserva soporte, pero su alcance está restringido;
- **contradicha:** Claims válidos sostienen relaciones incompatibles;
- **insuficiente:** faltan Claims requeridos;
- **invalidada:** una condición esencial dejó de cumplirse;
- **descartada:** no puede participar en un Finding;
- **referenciada:** participa en un Finding;
- **publicable:** puede aparecer en un reporte con sus límites.

Respaldada nunca significa verdadera. Contradicha nunca significa que sus Claims
deban eliminarse.

### 9.4 Descarte de una Hypothesis

Una Hypothesis debe descartarse cuando:

- no es verificable;
- no es falsable;
- carece de Claims válidos;
- depende de un Claim invalidado esencial;
- la Rule no aplica o fue retirada;
- excede el alcance de los Claims;
- requiere Evidence que no existe y la Rule la declara obligatoria;
- su trazabilidad está rota;
- oculta una contradicción material;
- sólo puede sostenerse reemplazando ausencia con suposición;
- selecciona una posición entre alternativas equivalentes sin precedencia;
- no puede expresar su Uncertainty sin negar su propia proposición.

Descartar una Hypothesis no elimina los Claims ni Evidence que la originaron. El
descarte y su motivo permanecen evaluables.

### 9.5 Contradicción e invalidez

Una contradicción invalida una Hypothesis cuando afecta la relación central de
tal manera que no puede formularse una proposición acotada sin elegir
arbitrariamente una posición.

No la invalida automáticamente cuando:

- sólo limita una condición secundaria;
- permite formular alternativas explícitas;
- la propia Hypothesis describe la existencia del conflicto;
- existe precedencia respaldada por Evidence;
- puede reducirse el alcance sin cambiar la proposición central.

En esos casos la Hypothesis permanece limitada o contradicha y debe reflejarlo
en Confidence y Uncertainty.

## 10. Ciclo de vida de Finding

### 10.1 Candidato a Finding

Un resultado es candidato cuando una o más Hypotheses válidas permiten formular
conocimiento derivado útil para un consumidor sin proponer una acción.

### 10.2 Nacimiento de un Finding

Un Finding nace cuando:

1. referencia al menos una Hypothesis válida;
2. todas sus Hypotheses conservan Claims y Evidence trazables;
3. expresa una sola unidad consumible y acotada;
4. declara categoría conceptual;
5. incluye Confidence completa;
6. hace explícita toda Uncertainty material;
7. conserva contradicciones y ausencias relevantes;
8. contiene preguntas abiertas cuando falta verificación;
9. declara límites de aplicabilidad;
10. no contiene Recommendation, Decision, aprobación ni rechazo.

Un texto útil pero sin cadena completa no nace como Finding.

### 10.3 Estados de Finding

Un Finding puede estar:

- **candidato:** pendiente de validación integral;
- **válido:** cumple todas las leyes;
- **limitado:** es válido bajo restricciones explícitas;
- **bloqueado:** Uncertainty material impide publicarlo;
- **invalidado:** perdió una dependencia o condición esencial;
- **descartado:** no puede incorporarse al reporte;
- **publicable:** está listo para formar parte de un reporte válido;
- **publicado:** forma parte de un reporte identificado.

Publicado no significa aceptado por una persona ni confirmado fuera de la
Evidence.

### 10.4 Pérdida de validez

Un Finding deja de ser válido cuando:

- una Hypothesis esencial queda invalidada;
- se rompe cualquier tramo de trazabilidad;
- Confidence carece de fundamento o limitaciones;
- Uncertainty material fue omitida;
- una contradicción fue eliminada sin respaldo;
- excede el alcance de sus Hypotheses;
- cambia de Finding a Recommendation o Decision;
- mezcla resultados de entradas diferentes sin conservar identidad;
- ya no puede distinguirse qué parte está respaldada y cuál permanece abierta.

### 10.5 Finding limitado

Un Finding limitado puede publicarse cuando su afirmación continúa siendo válida
dentro de un alcance reducido y el consumidor puede comprender exactamente:

- qué se sostiene;
- qué no se sostiene;
- qué Confidence corresponde;
- qué Uncertainty permanece;
- qué pregunta podría reducirla.

Si esa frontera no puede expresarse sin inducir una certeza mayor, el Finding se
bloquea y corresponde Abstention.

### 10.6 Descarte de un Finding

Un candidato a Finding debe descartarse cuando:

- no referencia Hypotheses válidas;
- su Traceability está incompleta;
- excede Scope;
- omite Uncertainty o Contradictions materiales;
- Confidence es insuficiente;
- no puede distinguir conocimiento derivado de una acción posible;
- contiene Recommendation, Decision, aprobación o rechazo;
- sólo puede sostenerse modificando o inventando Evidence.

Un Finding antes válido debe descartarse cuando pierde una dependencia esencial
y no puede nacer otro Finding limitado que cumpla todas las Laws. El candidato
descartado puede conservarse únicamente para Evaluation; nunca constituye un
Finding válido ni forma parte del Inference Report.

## 11. Confidence

Confidence es un Value Object conceptual que expresa grado de respaldo. Está
compuesto inseparablemente por:

- **nivel:** fuerte, moderado, débil o insuficiente;
- **fundamento:** razón trazable del nivel;
- **limitaciones:** condiciones que impiden mayor respaldo;
- **dimensiones:** aspectos evaluados por separado.

Las dimensiones mínimas son:

- relación directa entre soporte y afirmación;
- cobertura de Evidence requerida;
- consistencia entre unidades;
- integridad de Traceability;
- restricciones de las Rules aplicadas.

### 11.1 Asignación

Confidence se asigna a Claims, Hypotheses y Findings de acuerdo con su soporte
propio. No se hereda automáticamente desde una unidad anterior.

La Confidence de una unidad derivada nunca puede ser más fuerte que lo permitido
por su dependencia esencial más débil, salvo que Evidence adicional e
independiente reduzca explícitamente esa limitación.

### 11.2 Composición

Múltiples unidades con el mismo contenido no elevan Confidence por cantidad. El
respaldo adicional sólo cuenta cuando aporta Evidence pertinente, distinguible y
no meramente duplicada.

Una contradicción material reduce Confidence o impide la unidad. La decisión
depende de si la proposición puede permanecer válida dentro de un alcance
explícito.

### 11.3 Nivel insuficiente

Confidence insuficiente significa que no existe respaldo para publicar la
inferencia propuesta. La unidad puede conservarse como candidata descartada para
Evaluation, pero no aparece como Claim, Hypothesis o Finding válido.

Confidence insuficiente no se convierte en débil mediante lenguaje cauteloso.

## 12. Uncertainty

Uncertainty expresa un límite material del razonamiento. Debe ser específica,
trazable y asociada a las unidades que afecta.

### 12.1 Orígenes

Uncertainty puede originarse en:

- Evidence ausente;
- Evidence limitada o no legible;
- cobertura parcial;
- ambigüedad de términos o alcance;
- Claims contradictorios;
- relación indirecta;
- Rule con precondiciones parcialmente satisfechas;
- imposibilidad de verificar o falsar una Hypothesis;
- referencia o identidad incompleta;
- alternativas igualmente respaldadas.

No toda Uncertainty proviene de una Contradiction: también puede surgir de
ausencia, cobertura parcial, ambigüedad o límites de una Rule. Toda
Contradiction, en cambio, genera Uncertainty para cada unidad cuya validez,
Confidence o Scope pueda verse afectada.

### 12.2 Obligaciones

Toda Uncertainty debe declarar:

- su origen;
- las unidades afectadas;
- el efecto sobre Confidence;
- el efecto sobre alcance;
- qué Evidence permitiría reducirla, si puede señalarse;
- la pregunta abierta resultante.

### 12.3 Uncertainty que impide un Finding

Uncertainty impide producir un Finding cuando:

- falta Evidence obligatoria para la Rule central;
- no puede identificarse el alcance de la afirmación;
- existe una contradicción irresoluble sobre la proposición central;
- la cadena de trazabilidad es incompleta;
- la Confidence resultante es insuficiente;
- no puede formularse el Finding sin introducir una suposición;
- las alternativas tienen respaldo equivalente y el Finding elegiría una;
- las preguntas abiertas constituyen la totalidad de la supuesta conclusión;
- una limitación no puede comunicarse con claridad al consumidor.

Uncertainty no impide un Finding cuando éste puede expresar válidamente la
existencia de la ausencia, ambigüedad o contradicción sin resolverla.

## 13. Contradictions

Contradiction es una relación explícita entre unidades materialmente
incompatibles dentro de un alcance comparable.

### 13.1 Detección conceptual

Existe Contradiction cuando dos o más Evidence o Claims:

- afirman y niegan la misma proposición;
- asignan valores incompatibles a la misma condición;
- establecen relaciones mutuamente excluyentes;
- declaran estados que no pueden coexistir en el mismo alcance;
- sostienen precedencias incompatibles.

Una diferencia de vocabulario, granularidad, tiempo o alcance no es
Contradiction hasta demostrar comparabilidad.

### 13.2 Preservación

Toda Contradiction debe conservar:

- las unidades participantes;
- la Evidence de cada posición;
- el alcance compartido;
- las diferencias de alcance relevantes;
- las Hypotheses y Findings afectados;
- su efecto sobre Confidence y Uncertainty.

Nunca se elimina una posición para simplificar el resultado.

### 13.3 Efecto

Una Contradiction puede:

- limitar un Claim;
- impedir combinar Claims;
- debilitar o descartar una Hypothesis;
- bloquear un Finding;
- originar un Finding que describa el conflicto;
- producir Abstention para una proposición específica;
- volver incompleto el reporte respecto de una pregunta.

La Contradiction invalida una inferencia cuando la inferencia sólo puede existir
si una posición se trata como verdadera sin precedencia respaldada.

Contradiction produce Uncertainty; Uncertainty no produce por sí sola una
Contradiction. Esta dirección preserva el conflicto como causa observable y la
incertidumbre como su efecto sobre el razonamiento.

## 14. Abstention

Abstention es un resultado epistemológico explícito. Indica que el motor no
puede producir una unidad válida bajo las condiciones disponibles.

### 14.1 Alcance

La abstención puede ser:

- **local:** impide un Claim, Hypothesis o Finding específico;
- **parcial:** impide una parte del alcance del reporte;
- **total:** impide producir Findings válidos para la ejecución.

Una abstención local no obliga a detener inferencias independientes.

### 14.2 Condiciones obligatorias

El motor debe abstenerse cuando:

- no existe Evidence requerida;
- la entrada no permite una referencia trazable;
- una Rule aplicable no satisface sus precondiciones;
- la derivación requiere inventar o completar Evidence;
- una Hypothesis no es verificable o falsable;
- una Contradiction material no admite una formulación limitada;
- Confidence es insuficiente;
- Uncertainty impide delimitar una afirmación;
- producir una unidad ocultaría una ausencia;
- el resultado implicaría Recommendation, Decision, aprobación o rechazo;
- cualquier ley del razonamiento sería violada.

### 14.3 Expresión

Toda Abstention debe explicar:

- qué unidad no pudo producirse;
- qué condición falló;
- qué Evidence estaba disponible;
- qué Evidence faltó, si puede identificarse;
- qué Contradiction o Uncertainty intervino;
- qué alcance restante continúa siendo válido.

Abstention no debe redactarse como una inferencia negativa. “No hay Evidence
suficiente para afirmar X” no equivale a “X es falso”.

## 15. Traceability

Traceability permite reconstruir el razonamiento completo desde un Finding hasta
Evidence.

### 15.1 Cadena mínima

```text
Finding
  ↓
Hypothesis
  ↓
Claim
  ↓
Evidence
```

Cada transición también referencia la Rule que la autorizó. Confidence y
Uncertainty se asocian a la unidad que califican, no a un texto global.

### 15.2 Propiedades

Traceability debe ser:

- **completa:** contiene todos los niveles obligatorios;
- **bidireccional:** permite recorrer soporte y dependencias;
- **inequívoca:** cada identidad refiere una sola unidad;
- **cerrada:** toda cadena termina en Evidence de la entrada;
- **estable:** una unidad publicada no cambia sus referencias;
- **explicable:** cada transición declara su fundamento;
- **auditable:** puede evaluarse sin reconstrucción implícita.

### 15.3 Ruptura

Una referencia rota invalida la unidad dependiente y todas las unidades que
dependen esencialmente de ella. La invalidación se propaga hacia Findings, nunca
hacia Evidence independiente.

Una cadena rota no puede repararse con una nueva afirmación. Requiere restaurar
la referencia mediante una nueva unidad válida o abstenerse.

## 16. Laws of Reasoning

Las siguientes leyes son obligatorias y universales para Capability-002.

### Ley 1 — Inmutabilidad de Evidence

Una Evidence nunca cambia dentro de una ejecución.

### Ley 2 — Origen autorizado

Toda Evidence proviene exclusivamente de la entrada contractual válida.

### Ley 3 — Claim respaldado

Todo Claim referencia al menos una Evidence válida.

### Ley 4 — Derivación autorizada

Toda unidad derivada identifica una Rule vigente que permite su relación.

### Ley 5 — Hypothesis respaldada

Toda Hypothesis referencia uno o más Claims válidos.

### Ley 6 — Finding respaldado

Todo Finding referencia una o más Hypotheses válidas.

### Ley 7 — Cadena completa

Toda inferencia es trazable hasta Evidence sin saltos epistemológicos.

### Ley 8 — No fabricación

El motor nunca inventa, completa ni reemplaza Evidence ausente.

### Ley 9 — Incertidumbre explícita

Toda Uncertainty material se declara en las unidades afectadas.

### Ley 10 — Contradicción preservada

El motor nunca elimina, oculta ni resuelve una Contradiction sin respaldo.

### Ley 11 — Ausencia limitada

Ausencia de Evidence no prueba inexistencia fuera del alcance observado.

### Ley 12 — Confidence fundamentada

Toda Confidence contiene nivel, fundamento, limitaciones y dimensiones.

### Ley 13 — Confidence no es certeza

Ningún nivel de Confidence equivale a verdad, probabilidad, prioridad o
severidad.

### Ley 14 — No elevación por repetición

Repetir una afirmación o duplicar Evidence no aumenta respaldo.

### Ley 15 — Correlación limitada

Una relación observada no se presenta como causalidad sin Evidence y Rule que la
sostengan.

### Ley 16 — Alcance no expansivo

Una unidad derivada nunca excede el alcance conjunto de sus soportes.

### Ley 17 — Abstención permitida

El motor puede abstenerse y debe hacerlo cuando producir una inferencia violaría
otra ley.

### Ley 18 — Abstención explicada

Toda Abstention declara condición fallida, alcance y Evidence disponible.

### Ley 19 — Nunca Recommendations

Capability-002 nunca produce Recommendations.

### Ley 20 — Nunca Decisions

Capability-002 nunca produce Decisions, aprobaciones ni rechazos.

### Ley 21 — No acción

Un Finding describe conocimiento derivado y nunca ordena una acción.

### Ley 22 — Rule no es Evidence

Una Rule autoriza relaciones, pero nunca demuestra un hecho.

### Ley 23 — Gobierno externo de Rules

El motor consume Rules vigentes y nunca las crea, modifica ni aprueba.

### Ley 24 — Validez local

La validez de una inferencia se limita a su entrada, Rules y alcance declarados.

### Ley 25 — Propagación de invalidez

La invalidez de una dependencia esencial invalida toda unidad derivada que no
pueda sostenerse sin ella.

### Ley 26 — Identidad preservada

Un cambio de significado, soporte o alcance crea una nueva unidad; nunca altera
silenciosamente la anterior.

### Ley 27 — Evaluación separada

Una ejecución no se evalúa, corrige ni aprueba a sí misma.

### Ley 28 — Decisión humana

Toda decisión posterior permanece bajo responsabilidad humana y fuera del motor.

### Ley 29 — Rule descriptiva

Una Rule únicamente puede producir conocimiento descriptivo. Nunca puede
producir conocimiento prescriptivo, Recommendations ni Decisions.

## 17. Invariants

En todo punto del razonamiento deben cumplirse simultáneamente:

1. la entrada es única e identificable;
2. Evidence permanece inmutable;
3. toda unidad posee identidad inequívoca;
4. toda dependencia existe y es compatible;
5. toda derivación está autorizada por una Rule vigente;
6. no hay Claim sin Evidence;
7. no hay Hypothesis sin Claim;
8. no hay Finding sin Hypothesis;
9. no hay referencia fuera de la entrada;
10. Confidence está completa;
11. Uncertainty material está explícita;
12. Contradictions permanecen visibles;
13. ausencia no se transforma en negación;
14. ningún texto no confiable cambia las Rules;
15. ninguna unidad derivada modifica sus soportes;
16. no existen Recommendations ni Decisions;
17. una unidad inválida no es publicable;
18. Abstention conserva explicación y alcance;
19. cada Finding permite reconstrucción completa;
20. el reporte conserva el estado real de completitud.

La violación de un invariant obliga a invalidar la unidad afectada o a producir
Abstention. Nunca se degrada silenciosamente la exigencia.

## 18. Reglas de propagación

### 18.1 Propagación de soporte

El soporte se propaga sólo mediante referencias explícitas. Una Hypothesis no
hereda Evidence que sus Claims no referencian. Un Finding no hereda Claims que
sus Hypotheses no incluyen.

### 18.2 Propagación de limitaciones

Toda limitación esencial se propaga hacia las unidades dependientes. Puede
reducirse únicamente cuando Evidence adicional y una Rule válida explican por
qué ya no afecta la unidad derivada.

### 18.3 Propagación de Contradiction

Una Contradiction se propaga a toda Hypothesis o Finding cuya proposición
dependa de elegir una de las posiciones. No afecta inferencias independientes.

### 18.4 Propagación de invalidez

Cuando una unidad queda invalidada:

1. se identifican sus dependientes directos;
2. cada dependiente se reevalúa sin la unidad;
3. si conserva soporte suficiente, nace una nueva versión limitada;
4. si pierde una condición esencial, queda invalidado;
5. la invalidación continúa hasta los Findings afectados;
6. las unidades independientes permanecen intactas.

### 18.5 No propagación de autoridad

La autoridad de una Evidence no convierte a Claims o Findings en fuentes
autoritativas. Las unidades derivadas conservan naturaleza inferencial.

## 19. Evaluation

Evaluation determina si el razonamiento cumple esta especificación y si resulta
útil dentro de los límites declarados. Ocurre después de producir el reporte y
fuera de la cadena Evidence → Claim → Hypothesis → Finding.

### 19.1 Independencia

Evaluation:

- no modifica el reporte evaluado;
- no crea Evidence para la misma ejecución;
- no cambia Rules;
- no convierte feedback en conocimiento;
- no aprueba sus propios criterios;
- no altera retroactivamente Confidence;
- no oculta errores para preservar un resultado.

### 19.2 Objetos evaluados

Evaluation revisa:

- admisión e inmutabilidad de Evidence;
- validez y atomicidad de Claims;
- verificabilidad y falsabilidad de Hypotheses;
- conformidad de Findings;
- aplicación de Rules vigentes;
- fundamento de Confidence;
- completitud de Uncertainty;
- preservación de Contradictions;
- corrección de Abstentions;
- integridad de Traceability;
- cumplimiento de Laws e Invariants;
- completitud real del reporte.

### 19.3 Casos mínimos

El conjunto de evaluación debe incluir:

- Evidence suficiente;
- Evidence parcial, ausente y no legible;
- Claims atómicos y afirmaciones compuestas;
- Claims compatibles e incompatibles;
- Hypotheses verificables y no verificables;
- Hypotheses falsables y no falsables;
- Contradictions secundarias y centrales;
- Confidence fuerte, moderada, débil e insuficiente;
- Uncertainty que limita y que bloquea;
- Abstention local, parcial y total;
- referencias completas y rotas;
- Rules aplicables y no aplicables;
- Findings válidos, limitados e inválidos;
- repetición sobre condiciones equivalentes;
- dominios distintos para verificar neutralidad conceptual.

### 19.4 Métricas normativas

Evaluation mide al menos:

- Claims sin Evidence;
- Hypotheses sin Claims válidos;
- Findings sin Hypotheses válidas;
- referencias rotas;
- Contradictions ocultas;
- Uncertainty material omitida;
- Confidence sin fundamento;
- inferencias que exceden alcance;
- Abstentions correctas e incorrectas;
- resultados incompatibles bajo condiciones equivalentes;
- capacidad de reconstruir cada Finding;
- cobertura sin fabricación.

El valor normativo esperado para Claims sin Evidence, referencias rotas,
Contradictions ocultas y Decisions producidas es cero.

### 19.5 Resultado de Evaluation

Evaluation puede determinar:

- **conforme:** todas las leyes aplicables se cumplen;
- **conforme con limitaciones:** no existen violaciones, pero la cobertura o
  utilidad está limitada;
- **no conforme:** al menos una ley o invariant fue violado;
- **no evaluable:** faltan casos o referencias para emitir resultado.

El resultado de Evaluation no modifica automáticamente el estado de la
capability. Es evidencia para su gobierno.

## 20. Completitud del razonamiento

El razonamiento está completo cuando:

- toda Evidence elegible fue considerada por las Rules aplicables;
- cada candidato alcanzó un estado explícito;
- todas las unidades publicables cumplen sus leyes;
- descartes y Abstentions conservan motivos;
- Contradictions y ausencias están representadas;
- Traceability está íntegra;
- Confidence y Uncertainty están completas;
- ninguna limitación conocida fue ocultada.

Completo no significa exhaustivo ni cierto. Puede existir razonamiento completo
con cero Findings si todas las transiciones imposibles terminaron en Abstention
correctamente explicada.

El razonamiento está incompleto cuando parte del alcance no pudo evaluarse, pero
las unidades publicadas continúan siendo válidas. Está invalidado cuando una
unidad publicada viola una ley esencial.

## 21. Conformidad

Una ejecución es conforme con esta especificación cuando:

- respeta todas las Laws of Reasoning;
- mantiene todos los Invariants;
- cada ciclo de vida termina en un estado permitido;
- ningún candidato se publica antes de alcanzar validez;
- toda pérdida de validez se propaga correctamente;
- cada Contradiction conserva sus posiciones;
- Uncertainty bloquea Findings cuando corresponde;
- cada Abstention está justificada;
- Evaluation puede reconstruir las decisiones epistemológicas;
- la salida cumple el Inference Report Contract.

Una ejecución no conforme no puede presentarse como reporte válido, aunque sus
conclusiones parezcan razonables.

## 22. Fronteras

Esta especificación no autoriza:

- entradas distintas de `local-context.json`;
- fuentes adicionales durante el razonamiento;
- modificación de Evidence;
- creación o administración de Rules por el motor;
- omisión de un nivel epistemológico;
- Recommendation o Decision;
- aprobación o rechazo;
- presentación específica para un consumidor;
- aprendizaje automático desde Evaluation;
- ampliación silenciosa del alcance.

Una capability consumidora puede usar Findings bajo su propia autoridad, pero no
puede atribuir al Inference Engine aquello que esta especificación prohíbe.

## 23. Preguntas de conformidad

Toda revisión de razonamiento debe poder responder afirmativamente:

1. ¿Cada Evidence pertenece a la entrada y permanece intacta?
2. ¿Cada Claim nació sólo después de satisfacer todas sus condiciones?
3. ¿Cada Claim continúa dentro del alcance de su Evidence?
4. ¿Los Claims combinados tienen alcance compatible?
5. ¿Cada Hypothesis es verificable y falsable?
6. ¿Las Hypotheses descartadas conservan un motivo explícito?
7. ¿Toda Contradiction material está visible?
8. ¿La Contradiction invalida sólo las inferencias que realmente afecta?
9. ¿Toda Uncertainty material limita o bloquea correctamente?
10. ¿Cada Finding conserva Hypotheses, Claims y Evidence?
11. ¿Confidence incluye fundamento y limitaciones?
12. ¿Toda Abstention explica qué no pudo sostenerse?
13. ¿Las referencias son completas y cerradas?
14. ¿Las Rules aplicadas estaban vigentes y externamente gobernadas?
15. ¿El resultado evita Recommendations y Decisions?
16. ¿Una persona conserva la decisión final?

Una respuesta negativa implica no conformidad o Abstention hasta resolver la
condición.

## 24. Gobernanza y evolución

La evolución de esta especificación debe preservar el vocabulario y los límites
de Capability-002. Agregar una nueva unidad epistemológica requiere demostrar
que no duplica Evidence, Claim, Hypothesis o Finding y que resuelve una
necesidad transversal observada.

Un cambio es incompatible cuando:

- elimina una ley;
- permite una unidad sin soporte obligatorio;
- debilita Traceability;
- oculta Uncertainty o Contradictions;
- redefine Confidence como certeza;
- elimina Abstention;
- permite Recommendations o Decisions;
- transfiere gobierno de Rules al motor;
- cambia la cadena epistemológica mínima.

Todo cambio incompatible requiere revisión arquitectónica y actualización
coordinada de Capability-002 y del Inference Report Contract antes de entrar en
vigencia.

## 25. Criterio de aceptación

Esta especificación puede promoverse a **Accepted** cuando:

- Capability Owner y Architecture Owner aceptan sus leyes;
- todas las preguntas normativas poseen respuesta no ambigua;
- los ciclos de vida pueden evaluarse de forma independiente;
- un conjunto representativo demuestra combinación, descarte, contradicción y
  Abstention correctas;
- no existen Claims, Hypotheses o Findings sin trazabilidad completa;
- Confidence y Uncertainty se aplican consistentemente;
- Evaluation distingue conformidad, limitación y no conformidad;
- Inference Report Contract puede satisfacerse sin ampliar sus garantías;
- no se introducen conceptos específicos de un consumidor;
- el outcome supera la mera producción de inferencias plausibles.

## 26. Historial

| Fecha      | Cambio                                                 | Estado   |
| ---------- | ------------------------------------------------------ | -------- |
| 2026-08-04 | Propuesta inicial de la especificación de razonamiento | Proposed |
