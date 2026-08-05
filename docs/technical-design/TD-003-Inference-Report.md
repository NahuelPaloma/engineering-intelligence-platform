# TD-003 — Inference Report

| Campo                | Valor                                     |
| -------------------- | ----------------------------------------- |
| Identificador        | TD-003                                    |
| Nombre               | Inference Report                          |
| Estado               | **Proposed**                              |
| Capability           | Capability-002 — Inference Engine         |
| Tipo                 | Diseño técnico                            |
| Owner propuesto      | Engineering Platform                      |
| Audiencia            | Architecture, Engineering, Product y Risk |
| Última actualización | 4 de agosto de 2026                       |

---

## 1. Propósito y autoridad

Este documento define el comportamiento técnico del **Inference Report
Builder**, último módulo del camino de producción de Capability-002. Su
propósito es consolidar razonamiento ya validado en un resultado conforme al
Inference Report Contract, sin crear, completar ni reinterpretar unidades
epistemológicas.

Las fuentes normativas son, en este orden:

1. Product Vision v1.1, Accepted;
2. Architecture v1.0 y sus ADR vigentes, Accepted;
3. ADR-013 — Local Context Contract, Accepted;
4. Local Context Contract, versión 1, Accepted;
5. Capability-002 — Inference Engine, Accepted;
6. Inference Report Contract, versión conceptual 1, Accepted;
7. Inference Engine Reasoning Specification, Accepted;
8. TD-002 — Inference Engine, Accepted.

Ante una contradicción prevalece la fuente de mayor autoridad. La Reasoning
Specification gobierna leyes, invariantes y ciclos de vida. El Inference Report
Contract gobierna el límite de salida. TD-002 gobierna la arquitectura interna y
la autoridad de sus módulos. TD-003 no redefine ninguna de esas decisiones.

## 2. Alcance y límites

El diseño comienza cuando las etapas de Evidence, Claim, Hypothesis y Finding
han terminado y Validation ha emitido sus decisiones. Termina cuando Inference
Report Builder entrega a Validation un reporte candidato y ésta autoriza su
publicación como completo o incompleto.

TD-003 define exclusivamente:

- admisión de resultados internos ya cerrados;
- consolidación sin transformación epistemológica;
- preservación de unidades, relaciones y estados;
- cálculo técnico de cobertura y completitud a partir de hechos de ejecución;
- validación final y publicación única del resultado contractual.

Quedan fuera:

- producción o revisión de Evidence, Claims, Hypotheses y Findings;
- detección o resolución de Contradictions;
- cálculo o propagación de Confidence y Uncertainty;
- decisión de Abstention;
- Recommendations, Decisions y acciones;
- representación física, serialización, persistencia, transporte y API;
- presentación para una capability consumidora.

El registro técnico temporal de ejecución no es el Inference Report y no se
convierte en contrato por contener información similar.

## 3. Responsabilidad exacta

Inference Report Builder posee una sola responsabilidad:

> Consolidar resultados válidos y estados epistemológicos explícitos de una
> única ejecución en un reporte candidato completo, autocontenido y trazable,
> para que Validation determine si puede publicarse conforme al contrato.

Consolidar significa seleccionar exclusivamente unidades autorizadas, conservar
su identidad y relaciones, establecer un orden estable y declarar cobertura y
estado de ejecución. No significa resumir, fusionar, priorizar ni redactar
conclusiones nuevas.

Inference Report Builder no es una etapa adicional de razonamiento. Ninguna
unidad nace en este módulo.

## 4. Entradas

Inference Report Builder recibe un resultado de ejecución cerrado que pertenece
a una sola entrada contractual e incluye:

- identidad inequívoca de `local-context.json` y de la ejecución;
- versión contractual soportada;
- identidad del conjunto de Rules y políticas fijadas;
- Evidence admitida;
- Claims válidos y publicables;
- Hypotheses válidas y publicables;
- Findings válidos y publicables;
- Abstentions publicables, locales, parciales o totales;
- Contradictions explícitas y sus relaciones;
- Confidence y Uncertainty asociadas a cada unidad;
- Scope de cada unidad y estado;
- cobertura declarada por las etapas;
- degradaciones conocidas;
- estado terminal de cada etapa requerida.

Cada entrada debe haber sido aceptada previamente por Validation. Los candidatos
descartados pertenecen exclusivamente a Evaluation Harness y no son entrada de
Inference Report Builder.

La ausencia de Findings no impide construir un reporte cuando existe una
Abstention total íntegra y explicada.

## 5. Salida

La salida de Inference Report Builder es un **reporte candidato conceptual**.
Debe permitir que Validation determine, sin consultar fuentes externas:

- a qué entrada y ejecución corresponde;
- qué Rules y condiciones gobernaron el razonamiento;
- qué Findings pudieron sostenerse;
- cómo se reconstruye cada cadena hasta Evidence;
- qué Confidence y Uncertainty limitan cada inferencia;
- qué Contradictions y Abstentions permanecen abiertas;
- qué alcance fue procesado;
- si el resultado es completo, incompleto o inválido y por qué.

Un reporte candidato no es publicable por sí mismo. Sólo Validation puede
autorizar su estado contractual final. Un resultado inválido se rechaza y nunca
cruza el límite del Inference Report Contract.

Este documento no define la representación física de la salida.

## 6. Autoridad y separación de responsabilidades

### 6.1 Inference Report Builder

Inference Report Builder:

- consolida unidades válidas y estados explícitos;
- conserva identidades, relaciones, orden y Scope;
- calcula cobertura y propone completitud desde resultados declarados;
- produce un reporte candidato;
- no decide validez final ni reabre decisiones anteriores.

Inference Report Builder es funcionalmente puro: la misma entrada cerrada
produce el mismo reporte candidato, sin estado interno observable. No conserva
memoria entre ejecuciones ni depende de tiempo, fuentes externas o resultados no
declarados.

### 6.2 Validation

Validation es la única autoridad que:

- acepta un reporte como completo;
- acepta un reporte como incompleto pero válido;
- rechaza un reporte inválido;
- impide publicación parcial o inconsistente.

Validation no completa información ni crea unidades durante esta decisión.

### 6.3 Módulos anteriores

Finding Processing, Confidence, Uncertainty, Traceability y Rule Runtime
entregan resultados cerrados dentro de sus responsabilidades. Inference Report
Builder no suplanta ninguno de ellos.

### 6.4 Evaluation Harness

Evaluation Harness observa el reporte publicado y los registros técnicos,
incluidos los descartes. No altera el reporte ni interviene en su publicación.

## 7. Consolidación de Findings

Inference Report Builder incorpora únicamente Findings válidos y publicables.
Para cada Finding debe:

- preservar identidad, categoría, formulación y Scope;
- conservar Hypotheses y Evidence referenciadas;
- conservar Confidence, Uncertainty, preguntas abiertas y límites;
- mantener Contradictions relacionadas;
- preservar el orden estable determinado antes de la publicación.

No debe:

- fusionar Findings similares;
- deduplicarlos por coincidencia textual;
- resumirlos;
- asignar prioridad, severidad o impacto;
- elegir qué Finding es más importante para un consumidor;
- convertirlos en Recommendations o Decisions;
- publicar un candidato descartado.

Dos Findings con igual formulación y distinta identidad, Scope o trazabilidad
permanecen como unidades distintas.

## 8. Preservación de trazabilidad

El reporte debe cerrar toda cadena obligatoria:

```text
Finding
  ↓
Hypothesis
  ↓
Claim
  ↓
Evidence
  ↓
local-context.json
```

Inference Report Builder conserva los vínculos existentes y construye una vista
navegable del conjunto. No infiere referencias ausentes ni reemplaza una unidad
por otra.

Antes de entregar el candidato debe comprobar que:

- cada referencia resuelve una única unidad de la misma ejecución;
- ninguna cadena omite un nivel obligatorio;
- toda cadena termina en Evidence de la entrada identificada;
- no existen referencias cruzadas entre ejecuciones;
- las relaciones de apoyo, limitación y contradicción permanecen distinguibles;
- ninguna unidad excede su Scope declarado.

Una referencia rota invalida la unidad dependiente. Si la ruptura alcanza una
garantía estructural del reporte, el candidato completo se rechaza; no se repara
ni se publica parcialmente.

## 9. Confidence y Uncertainty

Inference Report Builder preserva Confidence y Uncertainty exactamente como
fueron validadas para Claims, Hypotheses y Findings.

Para Confidence debe conservar inseparablemente:

- nivel cualitativo;
- fundamento;
- limitaciones;
- dimensiones relevantes.

No recalcula niveles, no los promedia y no eleva respaldo por cantidad de
unidades.

Para Uncertainty debe conservar:

- origen;
- unidades afectadas;
- efecto sobre Confidence y Scope;
- Evidence que podría reducirla, cuando haya sido declarada;
- preguntas abiertas.

No elimina Uncertainty por existir un Finding válido ni crea una cláusula
general que sustituya incertidumbres específicas. Si una Uncertainty material no
puede vincularse con las unidades afectadas, el reporte candidato no puede
publicarse como válido.

## 10. Abstention

Inference Report Builder incorpora Abstention como unidad conceptual distinta de
Finding. Puede recibir Abstentions:

- locales, que impiden una unidad específica;
- parciales, que impiden cubrir parte del Scope;
- totales, que impiden producir Findings válidos.

Cada Abstention debe conservar identidad, condición incumplida, Evidence
disponible, Evidence faltante identificable, Contradiction o Uncertainty
involucrada y Scope restante válido.

Inference Report Builder no decide abstenerse, no convierte descartes técnicos
en Abstentions y no redacta una inferencia negativa. Una Abstention total válida
puede coexistir con cero Findings y producir un reporte completo respecto de su
alcance si toda la entrada fue procesada y la abstención está íntegramente
explicada.

## 11. Contradiction

Inference Report Builder incorpora Contradictions ya detectadas y validadas.
Debe preservar:

- todas las posiciones respaldadas;
- Evidence de cada posición;
- Scope comparable y diferencias materiales;
- Claims, Hypotheses y Findings afectados;
- efectos declarados sobre Confidence y Uncertainty;
- cualquier Abstention relacionada.

No decide precedencia, no elimina posiciones y no presenta la contradicción como
error técnico. Una Contradiction puede volver un reporte incompleto respecto de
una pregunta o contribuir a una Abstention; esas decisiones deben llegar
explícitas desde Validation.

## 12. Completitud, incompletitud e invalidez

### 12.1 Completo

Inference Report Builder propone estado completo cuando:

- toda Evidence elegible fue procesada bajo las Rules vigentes;
- todas las etapas requeridas terminaron;
- todas las unidades publicables satisfacen sus invariantes;
- toda trazabilidad está cerrada;
- Confidence, Uncertainty, ausencia, Contradiction y Abstention están
  declaradas;
- ninguna degradación impidió cubrir el alcance previsto.

Completo no significa verdadero, exhaustivo ni libre de incertidumbre.

### 12.2 Incompleto pero válido

Propone estado incompleto cuando una limitación conocida impidió cubrir parte
del alcance, pero cada unidad publicada permanece íntegra. Debe conservar qué
parte no se cubrió, la causa, las unidades potencialmente afectadas, las
garantías que continúan vigentes y aquello que no puede concluirse.

La incompletitud no relaja invariantes ni permite omitir una incertidumbre.

### 12.3 Inválido

El candidato es inválido cuando no puede cumplir una garantía esencial, entre
otras causas:

- entrada o ejecución no identificables;
- mezcla de ejecuciones;
- referencia rota o ambigua;
- unidad publicada sin soporte obligatorio;
- Confidence incompleta o insuficiente para una unidad publicada;
- Uncertainty o Contradiction material ocultas;
- estado de etapa incompatible con la cobertura declarada;
- contenido expresamente excluido por el contrato.

Un reporte inválido se rechaza de forma cerrada. No existe una versión parcial
publicable de un candidato estructural o epistemológicamente inválido.

## 13. Determinismo y reproducibilidad

Para la misma combinación de entrada, versión contractual, Rules, políticas y
resultados validados, Inference Report Builder debe producir el mismo resultado
conceptual y el mismo orden.

El diseño exige:

1. una sola identidad de entrada y ejecución;
2. conjunto de resultados cerrado antes de consolidar;
3. orden total y estable para unidades y relaciones;
4. desempates definidos por identidades conceptuales estables;
5. ausencia de tiempo, estado externo o preferencias del consumidor;
6. cálculo de cobertura basado sólo en estados declarados;
7. publicación única después de Validation;
8. identidad del reporte derivada exclusivamente de componentes conceptuales
   gobernados, cuando esa identidad física sea diseñada.

La pureza funcional es una invariante del módulo: ninguna ejecución anterior ni
estado interno puede afectar el candidato producido.

Reproducibilidad exige poder reconstruir qué entrada, Rules, políticas y
decisiones de Validation produjeron el resultado, sin consultar las fuentes de
origen.

## 14. Validaciones y rechazos

Inference Report Builder verifica antes de entregar el candidato:

- pertenencia a una única ejecución;
- estados terminales coherentes de las etapas;
- unidades permitidas y publicables;
- unicidad de identidades;
- cierre y navegabilidad de referencias;
- Scope no expansivo;
- presencia íntegra de Confidence y Uncertainty;
- asociación de Contradictions y Abstentions;
- coherencia entre cobertura y completitud propuesta;
- ausencia de contenido prohibido.

Rechaza como entrada de consolidación:

- candidatos descartados;
- unidades provisionales, inválidas o de otra ejecución;
- referencias implícitas o ambiguas;
- datos obtenidos fuera de `local-context.json`;
- texto específico para una capability consumidora;
- Recommendations, Decisions, aprobaciones, rechazos o acciones;
- comentarios humanos y contenido de presentación.

Estas comprobaciones no reemplazan la validación final. Inference Report Builder
detecta que no puede construir un candidato íntegro; Validation conserva la
autoridad de aceptar o rechazar el resultado contractual.

## 15. Manejo de errores y degradación

Detienen la construcción y prohíben publicar:

- identidad de entrada o ejecución inválida;
- conjunto de Rules o políticas no fijado;
- resultados internos no cerrados;
- referencia cruzada entre ejecuciones;
- trazabilidad esencial rota;
- inconsistencia entre estados y cobertura;
- imposibilidad de garantizar publicación íntegra.

Producen un candidato incompleto sólo las degradaciones ya declaradas que
permiten conservar todas las garantías de las unidades publicadas.

Producen Abstention únicamente las condiciones epistemológicas decididas antes
de Inference Report Builder. Un error técnico del módulo no se representa como
Abstention.

Los errores deben ser seguros, estables y libres de Evidence sensible, rutas
internas o detalles de implementación. Una falla no publica un reporte anterior
como si correspondiera a la ejecución actual ni deja un resultado parcial.

## 16. Observabilidad

Sin incorporar contenido sensible, el módulo debe permitir observar:

- ejecuciones recibidas, consolidadas, aceptadas y rechazadas;
- cantidad de unidades por nivel y estado;
- referencias verificadas y fallidas;
- cobertura declarada y estado de completitud;
- Abstentions por alcance y condición normalizada;
- Contradictions preservadas;
- causas normalizadas de rechazo o incompletitud;
- estabilidad del resultado ante ejecuciones repetidas;
- tiempo de consolidación separado del tiempo de razonamiento.

Las métricas no incluyen el contenido completo de Evidence ni convierten
cantidad de Findings en medida de calidad.

## 17. Estrategia de pruebas

### 17.1 Unit

- selección exclusiva de unidades publicables;
- orden estable y desempates;
- preservación exacta de cada unidad;
- cálculo de cobertura y propuesta de completitud;
- rechazo de categorías de entrada prohibidas.

### 17.2 Integration

- flujo completo desde una ejecución cerrada hasta Validation final;
- reporte con Findings y trazabilidad completa;
- reporte con cero Findings y Abstention total válida;
- reporte incompleto con garantías preservadas;
- falla cerrada sin publicación parcial.

### 17.3 Reasoning

- leyes de cadena completa, Scope no expansivo y no fabricación;
- Confidence fundamentada y Uncertainty explícita;
- Contradiction preservada sin precedencia inventada;
- Abstention distinguible de Finding;
- ausencia total de Recommendations y Decisions.

### 17.4 Regression

- misma entrada y condiciones producen el mismo resultado;
- cambios de Rules o políticas cambian identidad de ejecución;
- referencias y orden no varían accidentalmente;
- un reporte antes válido no pierde garantías por cambios internos compatibles.

### 17.5 Conjuntos controlados

Los casos gobernados deben incluir:

- uno y varios Findings independientes;
- Findings con texto coincidente e identidad distinta;
- Uncertainty sin Contradiction;
- Contradiction con todas sus posiciones;
- Abstention local, parcial y total;
- reporte completo con cero Findings;
- reporte incompleto pero válido;
- referencia rota, mezcla de ejecuciones y contenido prohibido;
- degradación técnica que impide publicar.

## 18. Definition of Done

TD-003 se considera implementado cuando existe evidencia repetible de que:

- Inference Report Builder consume exclusivamente resultados internos cerrados y
  validados de una ejecución;
- no crea ni modifica Evidence, Claims, Hypotheses o Findings;
- no recibe ni publica candidatos descartados;
- preserva íntegramente Traceability, Scope, Confidence y Uncertainty;
- incorpora Contradictions y Abstentions sin reinterpretarlas;
- distingue completo, incompleto e inválido conforme al contrato;
- un reporte completo puede contener cero Findings cuando corresponde;
- Validation es la única autoridad de publicación;
- resultados inválidos fallan de forma cerrada y sin salida parcial;
- la misma entrada y condiciones producen el mismo resultado;
- no existen Recommendations, Decisions ni conocimiento de dominio consumidor;
- cada incremento termina con una demo observable y repetible;
- la suite de conformidad cubre casos completos, incompletos, contradictorios y
  de abstención.

## 19. Roadmap incremental

Este roadmap no altera la secuencia Accepted de TD-002. El tratamiento
transversal de Confidence, Uncertainty, Contradiction y Abstention del
Incremento 4 es prerrequisito para publicar el Inference Report del
Incremento 5.

### Paso 0 — Frontera vacía de Inference Report Builder

**Capacidad:** admitir una ejecución cerrada sin publicar todavía un reporte.

**Demo:** una ejecución válida alcanza Inference Report Builder, éste declara
sus entradas y termina antes de crear un candidato. Ninguna unidad cambia.

### Paso 1 — Consolidación íntegra de Findings

**Capacidad:** construir un candidato con Findings válidos y su cadena completa,
sin todavía autorizar publicación contractual.

**Demo:** uno o más Findings conservan identidad, orden, Scope, Confidence,
Uncertainty y trazabilidad byte por byte respecto de las unidades de entrada.

### Paso 2 — Abstention y Contradiction

**Capacidad:** incorporar estados ya producidos por el Incremento 4 de TD-002,
sin detectarlos ni propagarlos dentro de Inference Report Builder.

**Demo:** una Contradiction conserva todas sus posiciones y una ejecución con
Abstention total produce un candidato con cero Findings.

### Paso 3 — Cobertura y completitud

**Capacidad:** proponer estado completo o incompleto desde cobertura y
degradaciones declaradas.

**Demo:** dos ejecuciones controladas producen candidatos distinguibles: uno
completo y otro incompleto con alcance y causas explícitas.

### Paso 4 — Validación y publicación contractual

**Capacidad:** someter el candidato a Validation y publicar una única salida
conforme al Inference Report Contract.

**Demo:** un caso completo y uno incompleto se publican; una referencia rota se
rechaza sin salida parcial. La misma entrada produce el mismo resultado.

Cada paso agrega una sola capacidad observable. Ningún paso autoriza definir
representación física, transporte o API fuera de una decisión posterior.

## 20. Riesgos y trade-offs

| Riesgo                                           | Impacto                                | Tratamiento inicial                                   |
| ------------------------------------------------ | -------------------------------------- | ----------------------------------------------------- |
| Inference Report Builder reinterpreta resultados | Nuevo razonamiento fuera de Validation | Preservación exacta y pruebas de inmutabilidad        |
| Trazabilidad voluminosa                          | Reporte difícil de consumir            | Medir tamaño sin eliminar vínculos normativos         |
| Completitud confundida con exhaustividad         | Confianza indebida del consumidor      | Estados y garantías explícitos                        |
| Descartes filtrados al reporte                   | Violación contractual                  | Entrada tipada por estado y pruebas negativas         |
| Contradiction simplificada                       | Pérdida de posiciones respaldadas      | Preservar relaciones y rechazar precedencia implícita |
| Abstention usada como error técnico              | Semántica epistemológica incorrecta    | Separar fallas del módulo de estados de razonamiento  |
| Orden accidental                                 | Pérdida de reproducibilidad            | Orden total estable y regresión                       |
| Contenido sensible en observabilidad             | Exposición adicional                   | Identidades y motivos normalizados, no Evidence       |
| Acoplamiento a primer consumidor                 | Contrato deja de ser transversal       | Vocabulario agnóstico y presentación externa          |

El principal trade-off es conservar detalle suficiente para trazabilidad sin
optimizar prematuramente el tamaño o la experiencia de consumo. La integridad
contractual tiene prioridad sobre concisión.

## 21. Decisiones tomadas

- Inference Report Builder consolida; no razona.
- Inference Report Builder es funcionalmente puro y no posee estado interno
  observable.
- Validation conserva autoridad exclusiva sobre publicación.
- Sólo unidades válidas y estados publicables entran a Inference Report Builder.
- Los descartes permanecen exclusivamente en Evaluation Harness.
- Findings conservan identidad y no se fusionan por texto.
- Confidence y Uncertainty se preservan, no se recalculan.
- Contradiction y Abstention llegan ya decididas por etapas anteriores.
- Un reporte completo puede contener cero Findings.
- Una falla técnica de Inference Report Builder nunca se expresa como
  Abstention.
- Inference Report y registro técnico de ejecución son artefactos
  conceptualmente distintos.

## 22. Decisiones explícitamente postergadas

Se postergan:

- formato físico y nombre del artefacto;
- estructura de campos y schema;
- serialización y mecanismo de versionado físico;
- persistencia, retención y almacenamiento;
- transporte, API y estrategia de entrega;
- empaquetado y despliegue;
- concurrencia y paralelismo;
- límites cuantitativos de tamaño y rendimiento;
- estrategia de lectura parcial por consumidores;
- presentación, Markdown y UI;
- taxonomías específicas de capabilities consumidoras.

Estas decisiones requieren evidencia de implementación y del primer consumidor.
No forman parte de TD-003.

## 23. Limitaciones y preguntas abiertas

Este diseño depende de que el Incremento 4 de TD-002 entregue Contradiction,
Confidence, Uncertainty y Abstention de forma transversal y validada. TD-003 no
rellena esa ausencia ni autoriza adelantarlos dentro de Inference Report
Builder.

Permanecen abiertas para decisiones posteriores:

- qué mecanismo físico identificará versión y compatibilidad;
- qué tamaño real tendrá la trazabilidad con múltiples unidades;
- qué necesidades de consumo justificarán acceso parcial sin debilitar el
  contrato;
- qué política operativa gobernará transición entre versiones físicas;
- qué objetivos cuantitativos serán necesarios después de medir el primer
  consumidor.

## 24. Historial del documento

| Fecha      | Cambio                      | Estado   |
| ---------- | --------------------------- | -------- |
| 2026-08-04 | Propuesta inicial de TD-003 | Proposed |
