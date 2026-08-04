# Inference Report Contract

| Campo                | Valor                             |
| -------------------- | --------------------------------- |
| Contrato             | `inference-report`                |
| Versión conceptual   | 1                                 |
| Estado               | **Accepted**                      |
| Productor            | Capability-002 — Inference Engine |
| Audiencia            | Capabilities consumidoras         |
| Última actualización | 4 de agosto de 2026               |

---

## 1. Propósito

Este documento define el contrato conceptual estable entre:

```text
Capability-002 — Inference Engine
                 ↓
        Capabilities consumidoras
```

El contrato establece qué significa un `inference-report`, qué garantías ofrece,
qué límites conserva y qué responsabilidades impone a productor y consumidores.

No define implementación, clases, estructura física, serialización, transporte
ni presentación. Tampoco autoriza la construcción del Inference Engine.

## 2. Autoridad y relación con otros contratos

Este contrato deriva de:

1. Product Vision v1.1;
2. Architecture v1.0;
3. ADR-013 — Local Context Contract;
4. Local Context Contract;
5. Capability-002 — Inference Engine.

ADR-013 y Local Context Contract gobiernan el límite de entrada de
Capability-002. Este documento gobierna únicamente su límite de salida.

La Inference Engine Reasoning Specification es la fuente normativa única de las
Laws, los Invariants y el comportamiento del razonamiento. Este contrato resume
las garantías observables en su límite sin redefinir esa normativa.

```text
Context Retrieval
       ↓
local-context.json
       ↓
Inference Engine
       ↓
inference-report
       ↓
Capability consumidora
```

Los dos contratos no son intercambiables. `local-context.json` contiene
evidencia recuperada; `inference-report` contiene inferencias derivadas y su
trazabilidad.

## 3. Productor

El único productor del contrato es:

> **Capability-002 — Inference Engine**

Ninguna capability consumidora puede presentarse como productora de un
`inference-report`. Puede seleccionar, contextualizar o presentar Findings para
su producto, pero esa transformación queda fuera de este contrato y no altera el
reporte original.

El productor no obtiene autoridad sobre la evidencia, el dominio del consumidor
ni las decisiones humanas por producir el reporte.

## 4. Consumidores

El contrato puede ser consumido por capabilities como:

- Architecture Review;
- Incident Intelligence;
- Observability Intelligence;
- Security Intelligence;
- futuras capabilities autorizadas.

La enumeración identifica consumidores posibles, no dominios incorporados al
contrato. `inference-report` no conoce sus conceptos, vocabulario, decisiones,
interfaces ni experiencias.

Todo consumidor recibe el mismo límite conceptual. Una especialización de
producto ocurre después del contrato y no cambia el significado de las unidades
recibidas.

## 5. Definición de Inference Report

Un **Inference Report** es el resultado autocontenido y trazable de una
ejecución de razonamiento sobre una entrada `local-context.json` identificable.

Representa:

- qué Findings pudieron sostenerse;
- qué Hypotheses relacionan esos Findings con Claims;
- qué Claims fueron derivados de Evidence;
- qué Evidence respalda cada derivación;
- qué Rules gobernadas participaron;
- qué Confidence acompaña cada inferencia y por qué;
- qué Uncertainty limita el resultado;
- qué contradicciones permanecen abiertas;
- qué ausencia de Evidence impidió inferencias más fuertes;
- qué preguntas continúan sin respuesta;
- cuál fue la cobertura y el estado conceptual de la ejecución.

El reporte conserva conocimiento derivado. No crea conocimiento nuevo, no
modifica la evidencia y no sustituye la decisión de una persona.

## 6. Unidad y alcance del reporte

Cada reporte corresponde a una única entrada contractual identificable y a un
único conjunto gobernado de condiciones de razonamiento.

El reporte debe permitir distinguir:

- la identidad del contexto utilizado;
- la versión conceptual del contrato;
- las Rules vigentes aplicadas;
- los resultados producidos;
- las abstenciones y degradaciones;
- las limitaciones que afectan cobertura o Confidence.

No pueden combinarse silenciosamente Findings derivados de entradas diferentes.
Una agregación entre reportes pertenece a otra responsabilidad y debe conservar
la identidad de cada reporte original.

### 6.1 Scope

Scope representa la frontera conceptual dentro de la cual una unidad del reporte
es válida. Proviene de los límites explícitos conservados por Evidence, Rules y
unidades de soporte; limita qué puede afirmar cada Claim, Hypothesis y Finding.

Dos Scope son compatibles cuando coinciden o poseen una intersección explícita
dentro de la cual la relación conserva validez. Son incompatibles cuando
difieren en una condición esencial o sólo pueden combinarse ocultando una
limitación material. Todo reporte debe preservar Scope y no ampliar su alcance.

## 7. Información garantizada

Todo reporte válido garantiza que:

1. corresponde a una entrada `local-context.json` identificable;
2. la entrada no fue modificada por el proceso de inferencia;
3. todo Finding conserva su categoría conceptual y sus límites;
4. todo Finding referencia sus Hypotheses asociadas;
5. toda Hypothesis referencia los Claims que la sostienen, contradicen o
   limitan;
6. todo Claim referencia Evidence presente en la entrada;
7. toda relación derivada identifica las Rules que la permitieron;
8. Confidence incluye nivel, fundamento, limitaciones y dimensiones relevantes;
9. Uncertainty es explícita cuando afecta respaldo o alcance;
10. ausencias y contradicciones no fueron reemplazadas por suposiciones;
11. toda abstención explica qué condición no pudo satisfacerse;
12. el estado de completitud puede interpretarse sin consultar una fuente
    externa;
13. una referencia rota invalida la unidad dependiente;
14. ningún resultado constituye una decisión o acción.

Estas garantías describen integridad epistemológica y contractual. No garantizan
que una inferencia sea verdadera fuera del alcance de la Evidence disponible.

## 8. Información nunca garantizada

Un Inference Report nunca garantiza:

- verdad absoluta;
- exhaustividad del conocimiento disponible en el mundo;
- ausencia de Evidence fuera del contexto recibido;
- vigencia o autoridad superior a la declarada por la entrada;
- causalidad a partir de correlación;
- que toda Hypothesis sea correcta;
- que todo asunto relevante haya sido identificado;
- que Confidence equivalga a probabilidad matemática;
- que un Finding requiera una acción;
- que una persona deba aceptar el resultado;
- cumplimiento, aprobación, rechazo ni aptitud para un propósito específico;
- compatibilidad semántica con un dominio que el contrato no conoce.

Una capability consumidora no puede atribuir al reporte una garantía más fuerte
que las establecidas aquí.

## 9. Findings

Un **Finding** es la unidad consumible principal del reporte. Expresa
conocimiento derivado dentro de un alcance explícito y nunca una acción
sugerida.

Cada Finding debe conservar conceptualmente:

- categoría;
- formulación acotada;
- Confidence;
- Uncertainty;
- Hypotheses asociadas;
- Evidence utilizada;
- preguntas abiertas;
- límites de aplicabilidad;
- cadena de trazabilidad completa.

Un Finding sin trazabilidad íntegra no es un Finding válido. Puede convertirse
en una abstención explicada, pero no publicarse como inferencia respaldada.

El orden declarado de Findings es parte del reporte y debe preservarse. Este
contrato no asigna semántica universal de prioridad, severidad ni acción a ese
orden.

## 10. Hypotheses

Una **Hypothesis** expresa una relación provisional, verificable y falsable
entre uno o más Claims.

El reporte debe permitir conocer:

- qué relación propone;
- qué Claims la respaldan;
- qué Claims la contradicen o limitan;
- qué condición permitiría verificarla;
- qué Evidence podría falsarla;
- qué Uncertainty impide una afirmación más fuerte;
- con qué Findings está asociada.

Una Hypothesis nunca se presenta como observación ni decisión. Su presencia en
un reporte no prueba que sea verdadera.

## 11. Claims

Un **Claim** es una afirmación atómica derivada exclusivamente de Evidence.

El reporte debe permitir identificar:

- la afirmación exacta;
- la Evidence que la respalda;
- la Rule que habilitó su derivación;
- su Confidence;
- su Uncertainty;
- las Hypotheses que la utilizan.

No existe Claim válido sin Evidence. Repetir una afirmación, encontrarla
plausible o incluirla en una Rule no constituye respaldo.

Claims incompatibles pueden coexistir. El contrato conserva su conflicto y no
elige precedencia sin Evidence que la establezca.

## 12. Evidence

**Evidence** es el ancla observable e inmutable de toda cadena de inferencia.

El reporte referencia Evidence presente en la entrada; no la reescribe, corrige,
resume ni reemplaza. La referencia debe permitir reconstruir inequívocamente qué
parte del contexto fue utilizada.

El reporte puede conservar la procedencia necesaria para la trazabilidad, pero
esa procedencia no autoriza al consumidor a consultar fuentes ni a ampliar el
contexto.

Una Rule, Claim, Hypothesis, Finding, comentario o evaluación nunca puede actuar
como Evidence de la misma ejecución.

## 13. Traceability

La trazabilidad mínima obligatoria es:

```text
Finding
  ↓
Hypothesis
  ↓
Claim
  ↓
Evidence
```

La cadena puede ramificarse y contener relaciones de apoyo, contradicción o
limitación. Cada vínculo conserva la Rule que permitió derivarlo y la
Uncertainty que lo afecta.

La trazabilidad debe ser:

- **completa:** ningún nivel obligatorio está ausente;
- **navegable:** el consumidor puede recorrer ambos sentidos de una relación;
- **inequívoca:** cada referencia identifica una sola unidad;
- **cerrada:** todas las referencias terminan en Evidence de la entrada;
- **inmutable:** el reporte no cambia retroactivamente sus vínculos;
- **explicable:** puede describirse por qué existe cada relación.

Una cadena rota no se repara mediante inferencia. Invalida el resultado
dependiente y debe expresarse como falla o degradación.

## 14. Uncertainty

Uncertainty es una parte obligatoria del reporte cuando algo limita, debilita o
impide una inferencia.

Debe expresar:

- qué se desconoce, es ambiguo o está en conflicto;
- qué Claims, Hypotheses o Findings afecta;
- cómo limita Confidence o alcance;
- qué Evidence sería necesaria para reducirla, cuando pueda señalarse sin
  inventar una fuente;
- qué preguntas permanecen abiertas.

Uncertainty no es una cláusula general de exención. Debe ser específica para el
razonamiento que limita. Tampoco puede ocultarse porque un Finding tenga
Confidence fuerte.

## 15. Ausencia de Evidence

La ausencia se expresa como una limitación observada dentro del alcance de la
entrada. Nunca prueba inexistencia universal.

El reporte debe distinguir entre:

- Evidence explícitamente ausente;
- Evidence no disponible por una degradación declarada;
- Evidence presente pero insuficiente;
- Evidence que no satisface una Rule aplicable;
- cobertura que no permite determinar si la Evidence existe.

Cuando una ausencia impide derivar un Claim o evaluar una Hypothesis, el reporte
conserva la abstención y la pregunta abierta correspondiente. No completa el
vacío con una afirmación probable.

## 16. Contradicciones

Una contradicción existe cuando Evidence o Claims sostienen afirmaciones
materialmente incompatibles dentro del mismo alcance.

El reporte debe:

- conservar todas las posiciones respaldadas;
- identificar la Evidence de cada posición;
- mostrar qué Hypotheses y Findings resultan afectados;
- reducir Confidence cuando corresponda;
- expresar la Uncertainty resultante;
- abstenerse de elegir precedencia si ésta no está respaldada.

Contradicción no equivale a error del reporte. Ocultarla o resolverla sin
respaldo sí viola el contrato.

Toda contradicción genera Uncertainty para las unidades afectadas. No toda
Uncertainty proviene de una contradicción: también puede originarse en ausencia,
ambigüedad, cobertura parcial o límites de Scope.

## 17. Confidence

Confidence representa el grado cualitativo de respaldo de una inferencia. Es un
concepto compuesto por:

- nivel;
- fundamento;
- limitaciones;
- dimensiones relevantes del respaldo.

El reporte nunca comunica sólo un nivel aislado. Confidence no representa
probabilidad matemática, verdad, prioridad, impacto ni severidad.

Una Confidence fuerte no elimina Uncertainty ni amplía el alcance de la
Evidence. Una Confidence insuficiente obliga a abstenerse de publicar la unidad
como inferencia respaldada.

### 17.1 Abstention

Abstention forma parte del Inference Report como una unidad conceptual distinta
de Finding. Posee identidad propia dentro del reporte y representa que una
condición impidió producir una unidad válida.

Puede ser:

- **local:** impide un Claim, Hypothesis o Finding específico;
- **parcial:** impide cubrir una parte de Scope;
- **total:** impide producir Findings válidos para el reporte.

Toda Abstention debe ser trazable y explicar la condición incumplida, la
Evidence disponible, la Evidence faltante cuando pueda identificarse, la
Uncertainty o Contradiction involucrada y el Scope que permanece válido.

Abstention nunca se representa como Finding ni como inferencia negativa. No
producir una unidad válida no equivale a afirmar que su proposición es falsa.

## 18. Completitud

### 18.1 Reporte completo

Un reporte está **completo** cuando:

- procesó toda la Evidence elegible declarada por su entrada y Rules vigentes;
- todas las unidades producidas cumplen sus invariantes;
- todas las cadenas de trazabilidad están íntegras;
- Confidence y Uncertainty están expresadas donde corresponden;
- ausencias, contradicciones y abstenciones están declaradas;
- ninguna degradación impidió cumplir el alcance previsto de la ejecución.

Completo significa contractualmente íntegro respecto de su entrada y alcance. No
significa exhaustivo, verdadero, suficiente para decidir ni libre de
incertidumbre. Un reporte completo puede contener cero Findings si la abstención
está plenamente justificada.

### 18.2 Reporte incompleto

Un reporte está **incompleto pero válido** cuando una limitación conocida impide
cubrir parte del alcance, pero todo resultado publicado conserva integridad y
trazabilidad.

Debe explicar:

- qué parte no pudo procesarse o validarse;
- por qué ocurrió;
- qué Findings pueden estar afectados;
- qué garantías continúan vigentes;
- qué no puede concluirse.

La incompletitud nunca habilita relajar las invariantes de los Findings que sí
se publican.

### 18.3 Reporte inválido

Un resultado es **inválido** cuando incumple una garantía estructural o
epistemológica esencial, por ejemplo:

- no puede identificar su entrada;
- contiene referencias rotas;
- publica Claims sin Evidence;
- oculta Uncertainty material;
- mezcla ejecuciones sin conservar identidad;
- incluye una categoría expresamente prohibida por este contrato.

Un resultado inválido no puede entregarse como Inference Report completo ni
incompleto. Debe rechazarse de forma segura.

## 19. Contenido excluido

El contrato nunca contiene:

- Recommendations;
- decisiones;
- aprobaciones;
- rechazos;
- acciones propuestas o ejecutadas;
- comentarios humanos;
- Markdown;
- UI ni instrucciones de presentación;
- texto específico para una capability o producto;
- contenido generado para persuadir a una audiencia;
- modificaciones de Evidence;
- feedback incorporado como verdad o Rule.

Las preguntas abiertas forman parte de Findings y Uncertainty; no son
Recommendations ni comentarios humanos.

## 20. Responsabilidades del productor

Capability-002 debe:

- aceptar únicamente un `local-context.json` compatible;
- conservar la identidad e inmutabilidad de la entrada;
- producir sólo unidades permitidas por su modelo de dominio;
- aplicar exclusivamente Rules vigentes y gobernadas externamente;
- garantizar la cadena Finding → Hypothesis → Claim → Evidence;
- incluir fundamento y límites de Confidence;
- expresar Uncertainty, ausencias, contradicciones y abstenciones;
- distinguir reporte completo, incompleto e inválido;
- rechazar resultados que violen invariantes;
- mantener el reporte agnóstico del dominio consumidor;
- permitir reproducir qué condiciones conceptuales originaron el resultado;
- no agregar Recommendations, decisiones ni contenido de presentación.

El productor responde por la integridad del razonamiento, no por decisiones
tomadas posteriormente por un consumidor.

## 21. Responsabilidades del consumidor

Toda capability consumidora debe:

- verificar que soporta la versión contractual recibida;
- preservar identidad, categorías, trazabilidad, Confidence y Uncertainty;
- respetar el estado de completitud;
- no presentar un reporte incompleto como completo;
- no convertir Claims o Hypotheses en Evidence;
- no presentar Confidence como certeza o probabilidad;
- no ocultar contradicciones, ausencias ni abstenciones;
- no atribuir Recommendations o decisiones al Inference Engine;
- no usar referencias de Evidence para consultar fuentes por autoridad
  implícita;
- distinguir cualquier texto o acción propios del contenido contractual;
- conservar a una persona como responsable de la decisión final.

Un consumidor puede producir una Recommendation bajo su propio contrato y
autoridad. Esa Recommendation no forma parte de `inference-report` y debe
permanecer distinguible del Finding que la motivó.

## 22. Compatibilidad

La compatibilidad se evalúa por significado observable para productor y
consumidores, no por una representación física.

### 22.1 Cambios backward compatible

Un cambio es compatible cuando:

- aclara una definición sin alterar su significado;
- agrega información conceptual opcional que consumidores existentes pueden
  ignorar sin perder garantías;
- fortalece validaciones sin convertir un reporte antes válido en inválido;
- incorpora una nueva relación opcional preservando la cadena mínima;
- amplía ejemplos o guía de evaluación sin cambiar responsabilidades;
- mejora explicabilidad sin modificar categorías epistemológicas.

Un cambio aditivo no es automáticamente compatible. Si cambia cómo un consumidor
debe interpretar completitud, trazabilidad o autoridad, es incompatible.

### 22.2 Breaking changes

Son incompatibles, entre otros:

- eliminar o redefinir Evidence, Claim, Hypothesis, Finding, Confidence,
  Uncertainty o Traceability;
- omitir un nivel de la cadena mínima;
- permitir Claims sin Evidence;
- cambiar el significado de completo, incompleto o inválido;
- convertir ausencias en afirmaciones o contradicciones en precedencia;
- permitir Recommendations, decisiones, aprobaciones o rechazos;
- incorporar semántica de un dominio consumidor;
- permitir múltiples entradas sin identidad independiente;
- trasladar al consumidor la validación que corresponde al productor;
- debilitar las obligaciones de Confidence o Uncertainty;
- permitir que Rules actúen como Evidence;
- otorgar acceso a fuentes como responsabilidad del consumidor.

## 23. Versionado

La versión conceptual actual es **1 Accepted**. Identifica el significado de
este documento, no un campo, formato o mecanismo de serialización.

La versión Accepted mantiene compatibilidad durante su lifecycle. Una versión
posterior incompatible debe coexistir durante una transición explícita o incluir
una estrategia de migración para productores y consumidores.

Este documento no define cómo se transporta ni representa el identificador de
versión.

## 24. Estabilidad

Son límites conceptuales estables:

- productor único;
- agnosticismo del dominio;
- entrada identificable;
- Findings como unidad consumible;
- cadena Finding → Hypothesis → Claim → Evidence;
- Confidence fundamentada;
- Uncertainty explícita;
- ausencia y contradicción visibles;
- separación entre Finding y Recommendation;
- decisión humana fuera del contrato.

No son interfaces estables:

- nombres de clases o módulos;
- algoritmos o estrategias internas;
- representación física;
- orden o nombre de campos;
- serialización;
- transporte;
- almacenamiento;
- presentación de producto.

## 25. Gobernanza

El Capability Owner responde por la conformidad del productor. Architecture
Owner responde por la coherencia del límite. Las capabilities consumidoras
responden por no ampliar ni tergiversar sus garantías.

Toda propuesta de evolución debe indicar:

- necesidad demostrada;
- impacto sobre productor y consumidores;
- efecto sobre trazabilidad y control humano;
- compatibilidad con versiones vigentes;
- estrategia de evaluación y, si corresponde, migración.

**Cualquier cambio incompatible requiere un ADR.**

Una nueva necesidad de producto no modifica automáticamente este contrato. Debe
resolverse primero en la capability consumidora y elevarse al contrato sólo si
representa una necesidad transversal demostrada.

## 26. Decisiones postergadas

Este contrato no define todavía:

- `inference-report.json`;
- schema;
- campos JSON;
- formatos binarios o textuales;
- serialización;
- transporte;
- persistencia;
- presentación;
- categorías concretas de Finding;
- mecanismo físico de versionado;
- estrategia de inferencia;
- mecanismo de evaluación o feedback.

Estas decisiones requieren evidencia del primer consumidor y no deben
anticiparse desde el contrato conceptual.

## 27. Criterio de aceptación

La aceptación formal del contrato se sustenta en que:

- Capability-002 y al menos un consumidor acuerdan sus términos;
- completo, incompleto e inválido pueden distinguirse sin ambigüedad;
- todos los conceptos poseen criterios de conformidad evaluables;
- productor y consumidor demuestran responsabilidades separadas;
- Findings preservan trazabilidad integral en casos suficientes, contradictorios
  e incompletos;
- no existen Recommendations, decisiones ni contenido específico de producto;
- las reglas de compatibilidad y evolución son aplicables;
- Architecture Owner acepta el límite;
- toda decisión física continúa fuera del contrato o se gobierna separadamente.

Una implementación o un ejemplo convincente no bastan para aceptar el contrato.

## 28. Historial

| Fecha      | Cambio                                            | Estado   |
| ---------- | ------------------------------------------------- | -------- |
| 2026-08-04 | Propuesta inicial de Inference Report Contract v1 | Proposed |
| 2026-08-04 | Promoción formal de la versión conceptual 1       | Accepted |
