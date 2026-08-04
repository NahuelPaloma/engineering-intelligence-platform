# TD-002 — Inference Engine

| Campo                | Valor                                     |
| -------------------- | ----------------------------------------- |
| Identificador        | TD-002                                    |
| Nombre               | Inference Engine                          |
| Estado               | **Accepted**                              |
| Capability           | Capability-002 — Inference Engine         |
| Tipo                 | Diseño técnico                            |
| Owner propuesto      | Engineering Platform                      |
| Audiencia            | Architecture, Engineering, Product y Risk |
| Última actualización | 4 de agosto de 2026                       |

---

## 1. Propósito y autoridad

Este documento define la arquitectura técnica interna necesaria para ejecutar
Capability-002 sin violar su modelo de razonamiento. No redefine Evidence,
Claim, Hypothesis, Finding, Rule, Scope, Confidence, Uncertainty, Contradiction,
Abstention, Traceability ni Evaluation.

Las fuentes normativas son, en este orden:

1. Product Vision v1.1, Accepted;
2. Architecture v1.0 y sus ADR registrados, Accepted;
3. ADR-013 — Local Context Contract, Accepted;
4. Local Context Contract, versión 1, Accepted;
5. Capability-002 — Inference Engine, Accepted;
6. Inference Report Contract, versión conceptual 1, Accepted;
7. Inference Engine Reasoning Specification, Accepted.

Ante una contradicción prevalece la fuente de mayor autoridad. La Reasoning
Specification es la única fuente normativa de las leyes, invariantes y ciclos de
vida del razonamiento. Este diseño sólo asigna responsabilidades técnicas para
hacerlas cumplir.

## 2. Alcance y límites

El diseño comienza al recibir un `local-context.json` compatible y termina al
publicar un `inference-report` conceptualmente válido. Entre ambos límites
coordina transformaciones explícitas, valida cada transición y conserva la
cadena completa de respaldo.

TD-002 implementa únicamente `local-context.json`, la entrada actualmente
autorizada. La incorporación futura de nuevos Context Contracts requerirá
evolución arquitectónica previa y queda fuera del alcance de TD-002.

El motor:

- consume una entrada identificable e inmutable;
- aplica exclusivamente Rules vigentes y gobernadas externamente;
- produce unidades de razonamiento sólo cuando satisfacen la normativa;
- registra incertidumbre, contradicciones, descartes y abstenciones;
- no consulta fuentes de origen ni amplía el contexto recibido;
- no modifica Evidence;
- no produce Recommendations ni Decisions;
- no presenta resultados ni ejecuta acciones.

Los módulos definidos aquí son límites lógicos de responsabilidad. No implican
separación física, procesos independientes ni nuevos componentes compartidos.

## 3. Arquitectura interna

La arquitectura posee un camino de producción lineal y responsabilidades
transversales que lo controlan:

```text
Local Context Contract
          ↓
    Input Boundary
          ↓
       Evidence
          ↓
        Claims
          ↓
      Hypotheses
          ↓
       Findings
          ↓
    Report Builder
          ↓
Inference Report Contract

Controles transversales:
Rule Runtime · Validation · Traceability · Confidence · Uncertainty

Evaluación independiente:
Evaluation Harness ← resultados y registros observables
```

El Pipeline es el orquestador lógico de toda la secuencia. No aparece como una
etapa epistemológica porque no transforma unidades; coordina las cajas y los
controles representados. Cada etapa recibe unidades validadas de la etapa
anterior y entrega unidades candidatas junto con sus resultados de validación.
Ninguna etapa puede saltar un nivel de la cadena.

Validation opera transversalmente también sobre Input Boundary, en cada
transición y antes de publicar el reporte. Traceability acompaña cada unidad
desde su nacimiento. Confidence y Uncertainty se calculan y propagan donde la
normativa lo exige; no se agregan al final como anotaciones decorativas.

Evaluation observa ejecuciones ya producidas y conjuntos controlados. Está fuera
del camino de producción: no aporta Evidence, no modifica Rules durante una
ejecución y no altera retroactivamente un reporte.

## 4. Pipeline técnico

El pipeline conserva exactamente esta secuencia normativa:

```text
Evidence
    ↓
Claim
    ↓
Hypothesis
    ↓
Finding
    ↓
Inference Report
```

Las validaciones intermedias no crean niveles epistemológicos adicionales. Una
unidad candidata que no satisface las condiciones de nacimiento no entra en la
etapa siguiente. Su descarte puede conservarse para Evaluation, pero nunca se
publica como unidad válida.

Processing genera unidades candidatas y reúne su soporte. Rule Runtime determina
la aplicabilidad técnica de las Rules vigentes y entrega ese resultado, pero no
acepta ni publica unidades. Confidence y Uncertainty calculan y propagan sus
conceptos sin decidir el estado final. Validation es la única autoridad técnica
que decide aceptación, descarte, limitación, degradación, Abstention o detención
en cada frontera. Pipeline coordina la secuencia y aplica el resultado emitido
por Validation. Las listas **Nunca debe** expresan condiciones que Validation
debe hacer cumplir, no autoridades independientes de decisión. Validation no
deriva Claims, Hypotheses ni Findings.

### 4.1 Admisión de Evidence

**Entrada:** un `local-context.json` completo en bytes y una versión de contrato
soportada.

**Salida:** Evidence identificada, ordenada e inmutable; ausencias y errores de
recuperación ya declarados; identidad de ejecución.

**Responsabilidades:**

- validar compatibilidad, estructura, identidad y orden contractual;
- establecer la identidad estable de cada Evidence dentro de la ejecución;
- distinguir contenido observable de metadata y errores declarados;
- conservar exactamente la procedencia y los límites recibidos;
- identificar qué Evidence es elegible para las Rules vigentes.

**Nunca debe:**

- reparar una entrada inválida;
- completar valores ausentes;
- alterar, normalizar semánticamente o enriquecer Evidence;
- consultar el origen;
- tratar una Rule o una inferencia como Evidence.

### 4.2 Derivación de Claims

**Entrada:** Evidence elegible y Rules vigentes aplicables.

**Salida:** Claims válidos, atómicos y trazables; incertidumbres,
contradicciones o abstenciones locales cuando correspondan; candidatos
descartados para Evaluation.

**Responsabilidades:**

- evaluar precondiciones observables de cada Rule;
- derivar una única afirmación evaluable por Claim;
- registrar toda Evidence y Rule que habilitan la derivación;
- acotar Scope y Confidence al respaldo disponible;
- impedir que una afirmación exceda la Evidence combinada.

**Nunca debe:**

- generar un Claim sin Evidence;
- usar repetición como aumento automático de respaldo;
- convertir una interpretación en observación;
- ocultar Claims incompatibles;
- combinar afirmaciones para evitar la atomicidad.

### 4.3 Formación de Hypotheses

**Entrada:** Claims válidos con Scope, Confidence, Uncertainty y trazabilidad.

**Salida:** Hypotheses explícitas, verificables, falsables y provisionales;
relaciones de apoyo, limitación o contradicción; abstenciones cuando no exista
base válida.

**Responsabilidades:**

- combinar únicamente Claims con Scope compatible;
- declarar la relación propuesta y sus condiciones de verificación;
- identificar qué Evidence podría falsarla sin inventar una fuente;
- conservar Claims favorables, limitantes y contradictorios;
- propagar restricciones materiales de las unidades de soporte.

**Nunca debe:**

- presentar una Hypothesis como hecho;
- crear una Hypothesis sin Claims válidos;
- resolver una contradicción sin precedencia observable;
- eliminar límites para hacer compatibles dos Claims;
- introducir causalidad no respaldada.

### 4.4 Formación de Findings

**Entrada:** Hypotheses válidas junto con su cadena completa de Claims,
Evidence, Confidence, Uncertainty y Scope.

**Salida:** Findings consumibles y acotados, o Abstention cuando no pueda
sostenerse un Finding válido.

**Responsabilidades:**

- verificar que cada Finding posea categoría conceptual y formulación clara;
- incluir Confidence fundamentada, Uncertainty y preguntas abiertas;
- conservar todas las Hypotheses asociadas y su cadena de respaldo;
- expresar límites de aplicabilidad;
- impedir la publicación de una unidad con trazabilidad rota.

**Nunca debe:**

- convertir una Hypothesis en certeza;
- omitir contradicciones materiales;
- asignar prioridad, acción o veredicto;
- producir Recommendation o Decision;
- publicar un Finding para evitar una Abstention requerida.

### 4.5 Construcción del Inference Report

**Entrada:** Findings válidos, Abstentions publicables, Uncertainty y
Contradictions relacionadas, cobertura y estado completo de la ejecución.

**Salida:** un `inference-report` conceptualmente conforme, completo o
explícitamente incompleto.

**Responsabilidades:**

- vincular el reporte con una única entrada identificable;
- preservar el orden declarado y las identidades internas;
- cerrar toda referencia en Evidence de la entrada;
- declarar cobertura, incompletitud y causas de degradación;
- validar todas las garantías del Inference Report Contract antes de publicar.

**Nunca debe:**

- mezclar resultados de entradas diferentes;
- completar unidades faltantes con texto plausible;
- publicar un reporte parcialmente inconsistente;
- ocultar Abstention, ausencia de Evidence o contradicciones;
- recibir o publicar candidatos descartados, que permanecen exclusivamente como
  registros observables para Evaluation Harness;
- agregar contenido específico de una capability consumidora.

## 5. Módulos internos

### 5.1 Pipeline

Controla el ciclo de vida de una ejecución, el orden de las etapas, la entrega
de resultados validados y el cierre único de la ejecución. Coordina y aplica el
resultado emitido por Validation. No razona, no crea unidades epistemológicas y
no decide cómo aplicar una Rule.

### 5.2 Input Boundary

Admite exclusivamente el Local Context Contract soportado, fija la identidad de
la entrada y entrega Evidence inmutable. No accede a fuentes ni corrige datos.

### 5.3 Rule Runtime

Recibe el conjunto gobernado de Rules vigente para la ejecución, determina su
aplicabilidad técnica, entrega el resultado de esa evaluación y registra qué
versión participó en cada derivación. No acepta ni publica unidades. No crea,
modifica, aprueba ni retira Rules. Una Rule nunca aporta Evidence.

### 5.4 Claim Processing

Genera Claims candidatos, reúne su soporte y conserva sus descartes evaluables.
No decide su estado final, no relaciona Claims como Hypotheses ni produce
Findings.

### 5.5 Hypothesis Processing

Genera Hypotheses candidatas a partir de Claims con Scope compatible y reúne
soporte, falsabilidad, limitaciones y contradicciones. No decide su estado final
ni eleva relaciones provisionales a Findings sin validación.

### 5.6 Finding Processing

Genera Findings candidatos y reúne su soporte a partir de Hypotheses válidas. No
decide su estado final ni agrega propósito de producto, Recommendation, Decision
o presentación.

### 5.7 Validation

Aplica las condiciones contractuales, leyes e invariantes en cada frontera. Es
la única autoridad técnica que decide aceptación, descarte, limitación,
degradación, Abstention o detención. No deriva Claims, Hypotheses ni Findings,
repara referencias ni relaja una condición para mantener el flujo.

### 5.8 Traceability

Asigna y verifica identidades y vínculos desde Finding hasta Evidence. Detecta
referencias rotas, ciclos indebidos, cruces de ejecución y soporte fuera de
Scope. No infiere vínculos ausentes.

### 5.9 Confidence

Mantiene nivel cualitativo, fundamento, limitaciones y dimensiones de respaldo
para cada unidad que lo requiera. No representa verdad, probabilidad, prioridad
ni severidad, no oculta Uncertainty ni decide el estado final de una unidad.

### 5.10 Uncertainty

Registra y propaga ausencia, cobertura parcial, ambigüedad, contradicción y
límites, y entrega sus resultados a Validation. No decide el estado final de una
unidad ni resuelve vacíos mediante suposición.

### 5.11 Report Builder

Consolida exclusivamente unidades válidas y estados explícitos en el contrato de
salida. No reinterpreta el razonamiento ni crea unidades nuevas.

### 5.12 Evaluation Harness

Compara resultados observables con expectativas gobernadas, verifica leyes y
registra desviaciones. Opera de forma independiente del camino de producción. No
modifica un reporte, una Rule o la Evidence de la ejecución evaluada.

## 6. Puntos de extensión

Pueden evolucionar de manera independiente mientras preserven los contratos y la
Reasoning Specification:

- el conjunto versionado de Rules y sus ciclos externos de gobierno;
- la estrategia de aplicación de Rules dentro de cada transición;
- las políticas de validación que vuelven verificables las mismas invariantes;
- las dimensiones y fundamentos de Confidence admitidos por la normativa;
- el tratamiento explícito de nuevas causas de Uncertainty;
- los criterios de cobertura y los conjuntos de Evaluation;
- la representación futura del Inference Report, mediante evolución gobernada;
- los mecanismos de observación, sin alterar el resultado del razonamiento.

Una nueva dimensión de Confidence es compatible cuando es aditiva, está
explícitamente gobernada y no redefine ni debilita las dimensiones mínimas
Accepted. Cambiar el significado de una dimensión mínima, su composición
normativa o las garantías del contrato requiere evolución documental previa.

No son puntos de extensión:

- aceptar entradas distintas de `local-context.json`;
- omitir niveles de la cadena normativa;
- permitir Claims sin Evidence;
- producir Recommendations o Decisions;
- usar una capability consumidora para completar razonamiento interno;
- consultar fuentes para reducir incertidumbre.

Una extensión incompatible con los contratos Accepted o con las leyes del
razonamiento requiere gobernanza documental previa. No se habilita mediante una
variación interna.

## 7. Determinismo

El motor debe producir el mismo resultado conceptual ante la misma combinación
de entrada, versión contractual, Rules vigentes y políticas declaradas.

Para garantizarlo:

1. la identidad de entrada queda fijada antes de iniciar el pipeline;
2. Evidence permanece inmutable durante toda la ejecución;
3. el conjunto y versión de Rules quedan fijados para esa ejecución;
4. cada etapa posee orden de evaluación y desempate explícitos y estables;
5. toda unidad deriva únicamente de entradas declaradas;
6. no existe acceso tardío a fuentes ni estado externo no identificado;
7. las identidades derivadas se obtienen de información estable;
8. el orden de salida se define por reglas reproducibles;
9. todo descarte, degradación y Abstention registra una causa estable;
10. la repetición se compara mediante Evaluation y pruebas de regresión.

El requisito es determinismo conceptual: mismas unidades, relaciones, estados,
orden y fundamentos. La representación física del reporte se definirá en una
decisión posterior y deberá especificar entonces sus propias garantías.

## 8. Observabilidad

La observabilidad debe permitir reconstruir la ejecución sin exponer contenido
de Evidence innecesario. Como mínimo debe hacer visibles:

- identidad y resultado final de la ejecución;
- versiones de contratos y Rules utilizadas;
- inicio, fin y duración de cada etapa;
- cantidad de unidades candidatas, válidas y descartadas por etapa;
- cantidad y alcance de Abstentions;
- distribución de niveles de Confidence;
- cantidad y propagación de Uncertainty y Contradictions;
- referencias rotas o violaciones de invariantes detectadas;
- cobertura de Evidence elegible;
- estado completo, incompleto, degradado o inválido del reporte;
- causa normalizada de cada detención o degradación.

Los registros de operación no deben convertirse en Evidence, cambiar el
resultado ni revelar por defecto contenido, secretos o datos sensibles. Las
mediciones agregadas no deben evaluar desempeño individual de personas.

## 9. Manejo de errores

### 9.1 Errores que detienen el pipeline

Detienen la ejecución y prohíben publicar un reporte:

- entrada inválida, incompatible o con identidad ambigua;
- imposibilidad de preservar la inmutabilidad de Evidence;
- ausencia o identidad ambigua del conjunto de Rules requerido;
- violación sistémica de una ley o invariante;
- referencias cruzadas entre ejecuciones;
- imposibilidad de construir un reporte contractualmente válido;
- falla que impide conocer si el resultado es completo o íntegro.

La detención es fail-closed. Debe conservar una causa segura y verificable, sin
fabricar un resultado parcial.

### 9.2 Condiciones que producen Abstention

La sección 14, **Abstention**, de la Reasoning Specification define sus
condiciones normativas. Toda condición vigente o futura de esa fuente aplica
aunque TD-002 no la repita.

Técnicamente, Validation determina el alcance local, parcial o total, registra
la condición incumplida y emite una Abstention trazable para que Pipeline
aplique el resultado. Por ejemplo, de forma no exhaustiva, Evidence insuficiente
puede impedir una transición local y una contradicción material puede impedir
las unidades dependientes. Abstention es un resultado válido del razonamiento,
no una falla operativa.

### 9.3 Condiciones de degradación

Permiten continuar sólo cuando las unidades restantes conservan validez:

- Evidence declarada como no disponible por el contrato de entrada;
- cobertura parcial cuyo límite puede expresarse con precisión;
- una Rule no aplicable que no impide evaluar otras Rules;
- una contradicción limitada a una rama separable del razonamiento.

Toda degradación debe propagarse a Confidence, Uncertainty, Scope, completitud y
observabilidad donde corresponda. Nunca reduce las condiciones de validez.

Un descarte local correcto es parte normal del ciclo epistemológico y no
constituye por sí mismo degradación, error operativo ni reporte incompleto. Sólo
afecta degradación o completitud cuando su causa representa una pérdida real del
alcance previsto según la normativa Accepted. Los descartes permanecen
observables para Evaluation.

### 9.4 Fallas de Evaluation

Una falla de Evaluation puede impedir promover un cambio, pero no reescribe un
reporte ya producido ni altera Rules durante la ejecución. Debe distinguirse de
una falla del camino de producción.

## 10. Estrategia de pruebas

### 10.1 Unit

Verifican de forma aislada una sola responsabilidad o módulo, con dependencias
controladas. Las aserciones se realizan sobre unidades, relaciones, estados y
motivos, nunca mediante mera semejanza textual.

### 10.2 Integration

Verifican la composición completa desde un Local Context Contract válido hasta
un Inference Report conforme. Incluyen entradas inválidas, degradación,
abstención local, parcial y total, y fallas en cada frontera.

También demuestran que el motor no accede a fuentes ni modifica la entrada.

### 10.3 Reasoning

Verifican normas que atraviesan más de una responsabilidad lógica para demostrar
la conformidad de una transición o cadena epistemológica, sin requerir el
pipeline completo ni infraestructura externa. Incluyen atomicidad de Claims,
compatibilidad de Scope, verificabilidad y falsabilidad de Hypotheses,
propagación de limitaciones, contradicciones visibles y prohibición de saltos en
la cadena. Las aserciones se realizan sobre unidades, relaciones, estados y
motivos, nunca mediante mera semejanza textual.

### 10.4 Regression

Comparan ejecuciones repetidas bajo la misma entrada y Rules. Detectan cambios
en unidades, relaciones, orden, Confidence, Uncertainty, Abstention, cobertura y
causas de descarte.

Todo cambio deliberado debe explicar su impacto y volver a establecer una línea
base gobernada.

### 10.5 Golden Dataset

Golden Dataset es la instancia técnica, canónica y versionada del conjunto de
evaluación definido por Capability-002; no crea un segundo artefacto conceptual
ni duplica responsabilidades. Debe cubrir, como mínimo:

- Evidence suficiente, parcial, ausente, ambigua y contradictoria;
- Claims válidos, compuestos, incompatibles y sin respaldo;
- Hypotheses respaldadas, debilitadas, contradichas y no falsables;
- Findings válidos, limitados e inválidos;
- Abstention local, parcial y total;
- reportes completos, incompletos e inválidos;
- trazabilidad ramificada y referencias rotas;
- repetición determinista.

Cada caso conserva entrada, Rules vigentes, resultado conceptual esperado y
justificación normativa. El conjunto no otorga autoridad para cambiar las Rules
automáticamente.

## 11. Evidencia técnica requerida para cumplir la Definition of Done de Capability-002

Capability-002 §19 es la fuente normativa de su Definition of Done. TD-002 no la
reemplaza ni la resume exhaustivamente. Esta sección enumera solamente la
evidencia técnica que esta arquitectura debe producir para demostrar
cumplimiento; cualquier requisito vigente de Capability-002 continúa aplicando
aunque no esté repetido aquí.

La arquitectura debe producir evidencia técnica de:

- ejecución conforme del pipeline y arbitraje único de Validation;
- trazabilidad cerrada desde cada Finding hasta Evidence;
- determinismo bajo entradas, Rules y políticas equivalentes;
- tratamiento explícito de errores, degradación y Abstention;
- observabilidad suficiente para reconstruir etapas y resultados;
- cobertura de Unit, Integration, Reasoning, Regression y Golden Dataset;
- demos observables y repetibles para cada incremento.

## 12. Incrementos de entrega

Cada incremento introduce una sola capacidad nueva y termina con una demo
repetible. Ninguno autoriza anticipar el siguiente.

### Incremento 0 — Arquitectura vacía

**Capacidad:** establecer el límite de entrada y salida, la secuencia de etapas
y los controles transversales sin producir inferencias.

**Demo observable:** una entrada controlada recorre la admisión, muestra la
identidad de ejecución y el orden de etapas habilitadas, y termina de forma
declarada antes de crear Claims. La demo prueba que no existe acceso a fuentes.

### Incremento 1 — Evidence → Claim

**Capacidad:** admitir Evidence y derivar Claims atómicos mediante Rules
gobernadas, con trazabilidad, Confidence mínima, Uncertainty explícita cuando
corresponda y descartes explícitos.

**Demo observable:** una entrada mínima produce Claims válidos que referencian
Evidence y Rule, con Confidence fundamentada y Uncertainty explícita cuando
corresponde; un caso sin respaldo se descarta o produce Abstention. No se crean
Hypotheses.

### Incremento 2 — Claim → Hypothesis

**Capacidad:** relacionar Claims compatibles en Hypotheses verificables y
falsables, cada una con Confidence y Uncertainty propias. Incluye detectar
incompatibilidad entre Claims, impedir combinaciones inválidas y conservar el
motivo del rechazo o la Abstention correspondiente, sin incorporar todavía el
tratamiento transversal completo de Contradiction.

**Demo observable:** Claims controlados producen una Hypothesis con soporte,
condición de falsación, Scope, Confidence fundamentada y Uncertainty explícita
cuando corresponde; otro conjunto incompatible no se combina. No se crean
Findings.

### Incremento 3 — Hypothesis → Finding

**Capacidad:** formar Findings consumibles con Confidence y Uncertainty propias
a partir de Hypotheses válidas.

**Demo observable:** una Hypothesis válida produce un Finding con cadena
completa, Confidence fundamentada, Uncertainty explícita, límites y preguntas
abiertas; una Hypothesis inválida no se publica. El resultado no contiene
Recommendation ni Decision.

### Incremento 4 — Propagación avanzada, Contradiction y Abstention

**Capacidad:** incorporar representación explícita de Contradiction, preservar
todas las posiciones, propagar su impacto a Confidence, Uncertainty y Scope a
través de Hypotheses, Findings y reporte, y producir Abstention local, parcial y
total asociada.

**Demo observable:** el mismo conjunto controlado demuestra cómo Confidence y
Uncertainty se propagan entre Claims, Hypotheses y Findings, exhibe todas las
posiciones de una Contradiction y su impacto en el reporte, y muestra casos de
Abstention local, parcial y total.

### Incremento 5 — Inference Report

**Capacidad:** consolidar el razonamiento en el contrato conceptual de salida.

**Demo observable:** una ejecución produce un reporte completo y trazable; otra
produce un reporte incompleto con sus causas explícitas; una entrada
incompatible falla sin publicar un reporte parcial.

## 13. Decisiones tomadas

- Existe un único camino de producción, lineal y gobernado por las transiciones
  normativas.
- Validation y Traceability controlan cada frontera, no sólo el resultado final.
- Confidence y Uncertainty nacen y se propagan junto con las unidades afectadas.
- Rule Runtime ejecuta conocimiento gobernado, pero no posee autoridad sobre él.
- Report Builder consolida; no agrega razonamiento.
- Evaluation es independiente del camino de producción.
- Los errores sistémicos fallan de forma cerrada; la insuficiencia
  epistemológica produce Abstention; la pérdida acotada produce degradación.
- El determinismo se define sobre entrada, Rules, políticas y resultado
  conceptual identificables.
- Los límites internos son lógicos hasta que exista una razón demostrada para
  separarlos físicamente.

## 14. Decisiones explícitamente postergadas

Se postergan hasta contar con necesidad y evidencia:

- topología física y cantidad de unidades desplegables;
- entorno de ejecución y empaquetado;
- representación física del Inference Report;
- mecanismo de distribución y ciclo operativo de Rules;
- retención de entradas, resultados y registros;
- transporte entre productor, motor y consumidores;
- concurrencia, paralelismo y límites de capacidad;
- objetivos cuantitativos de tiempo, disponibilidad y costo;
- taxonomías concretas de Finding por producto;
- mecanismo de presentación y feedback de capabilities consumidoras.

## 15. Riesgos abiertos

| Riesgo                                | Impacto                                  | Tratamiento inicial                                   |
| ------------------------------------- | ---------------------------------------- | ----------------------------------------------------- |
| Rules ambiguas o incompatibles        | Resultados inconsistentes                | Gobierno externo, versionado y casos de frontera      |
| Variación no declarada                | Pérdida de reproducibilidad              | Fijar entrada, Rules, políticas y orden por ejecución |
| Trazabilidad excesivamente voluminosa | Reportes difíciles de consumir           | Medir tamaño sin eliminar vínculos normativos         |
| Abstention mal calibrada              | Exceso de silencio o inferencias débiles | Golden Dataset y métricas por alcance                 |
| Propagación incompleta de Uncertainty | Falsa confianza                          | Validación en cada transición y regresión             |
| Acoplamiento a un dominio consumidor  | Pérdida de neutralidad                   | Mantener vocabulario y presentación fuera del motor   |
| Observabilidad con contenido sensible | Ampliación de exposición                 | Minimización y referencias antes que contenido        |
| Evaluation no representativa          | Calidad aparente sin utilidad real       | Casos gobernados y revisión humana independiente      |

## 16. Preguntas abiertas para futuras capabilities

- ¿Qué subconjunto de Findings necesita cada consumidora sin alterar el contrato
  común?
- ¿Quién gobierna las Rules específicas de un dominio y cómo demuestra su
  autoridad?
- ¿Qué nuevas categorías de contexto justifican evolucionar el contrato de
  entrada?
- ¿Qué compatibilidad física necesitarán consumidores con ciclos de evolución
  diferentes?
- ¿Cómo se construyen Golden Datasets representativos de dominios distintos sin
  incorporar conocimiento de producto al motor?
- ¿Qué feedback de utilidad puede regresar a Evaluation sin convertirse en
  Evidence ni modificar Rules automáticamente?
- ¿Qué umbrales de completitud y latencia necesita cada producto antes de
  establecer objetivos compartidos?

## 17. Historial del documento

| Fecha      | Cambio                                     | Estado   |
| ---------- | ------------------------------------------ | -------- |
| 2026-08-04 | Propuesta inicial de TD-002                | Proposed |
| 2026-08-04 | Promoción formal del diseño técnico TD-002 | Accepted |
