# Capability-002 — Inference Engine

| Campo                | Valor                                     |
| -------------------- | ----------------------------------------- |
| Identificador        | Capability-002                            |
| Nombre               | Inference Engine                          |
| Estado               | **Proposed**                              |
| Tipo                 | Capability transversal de razonamiento    |
| Owner propuesto      | Engineering Platform                      |
| Audiencia            | Product, Architecture, Engineering y Risk |
| Última actualización | 4 de agosto de 2026                       |

---

## 1. Propósito y autoridad

Este documento define la capability canónica **Inference Engine** de la
Engineering Intelligence Platform. Su propósito es transformar evidencia
estructurada en inferencias trazables, explícitas y evaluables.

El Inference Engine no crea conocimiento nuevo: hace explícitas inferencias que
pueden derivarse del conocimiento disponible y gobernado, sin excederlo.

La capability no toma decisiones. No aprueba, rechaza, ejecuta ni modifica. Su
valor consiste en producir unidades de razonamiento que una capability de
producto pueda consumir sin perder la relación con la evidencia original, la
incertidumbre ni las preguntas que permanecen abiertas.

Las fuentes normativas son, en este orden:

1. Product Vision v1.1, Accepted;
2. Architecture v1.0, Accepted;
3. ADR-001 a ADR-013 registrados por Architecture v1.0;
4. ADR-013 — Local Context Contract, Accepted;
5. Local Context Contract, versión 1, Accepted;
6. VS-001 — Architecture Review Intelligence, Accepted;
7. VS-001 — Technical Design, Accepted.

Ante una contradicción prevalece la fuente de mayor autoridad. Este documento no
modifica ninguna de ellas. Define la capability que comienza después del límite
establecido por ADR-013.

## 2. Contexto

El Context Retrieval Pipeline de VS-001 quedó finalizado como primera
capability. Su resultado oficial es `local-context.json`: un conjunto ordenado,
validado y reproducible de evidencia y metadata explícita.

Ese resultado resuelve recuperación, no razonamiento. Todavía no establece qué
afirmaciones pueden derivarse, cómo se relacionan entre sí, cuánto respaldo
tienen, qué incertidumbre conservan ni qué preguntas impiden una conclusión más
fuerte.

Architecture v1.0 exige separar contexto, razonamiento y acción. ADR-013 vuelve
esa separación concreta: el Inference Engine sólo recibe el contrato de contexto
local y permanece aislado de las fuentes y del proceso que lo produjo.

Capability-002 formaliza la segunda etapa sin absorber responsabilidades del
retrieval pipeline ni de los productos que presentarán o utilizarán sus
resultados.

## 3. Problema

La evidencia estructurada no se convierte por sí sola en inteligencia útil. Un
consumidor necesita reconocer afirmaciones respaldadas, relacionarlas, plantear
explicaciones verificables y expresar qué parte del razonamiento continúa
incierta.

Sin una capability explícita de inferencia aparecen riesgos materiales:

- productos distintos derivan conclusiones incompatibles de la misma evidencia;
- una afirmación pierde el vínculo con aquello que la sustenta;
- ausencia de información se reemplaza silenciosamente con una suposición;
- confianza se comunica como certeza o como un número sin fundamento;
- hechos observables y conclusiones derivadas se mezclan;
- una explicación plausible se presenta como decisión;
- cada producto reconstruye su propio modelo de razonamiento;
- no puede reproducirse ni evaluar por qué se produjo un resultado.

El problema a resolver es:

> Transformar evidencia estructurada en inferencias consumibles sin perder
> trazabilidad, reproducibilidad, incertidumbre ni control humano.

## 4. Outcome esperado

El outcome primario es:

> **Permitir que cualquier producto autorizado consuma inferencias cuya cadena
> de respaldo pueda reconstruirse completamente hasta la evidencia de entrada.**

Outcomes secundarios:

- separar consistentemente observación, afirmación, hipótesis y hallazgo;
- hacer visible cuándo el respaldo es fuerte, limitado o insuficiente;
- preservar preguntas abiertas en lugar de completar vacíos;
- producir resultados comparables para una misma entrada y reglas vigentes;
- permitir evaluación independiente de cada paso de razonamiento;
- mantener la decisión final fuera del motor.

El éxito no se define por cantidad, extensión o persuasión de las inferencias.
Se define por trazabilidad, consistencia, utilidad y manejo correcto de límites.

## 5. Alcance

Capability-002:

1. recibe un `local-context.json` válido;
2. reconoce Evidence dentro del contexto autorizado;
3. aplica Rules declarativas vigentes;
4. deriva Claims exclusivamente respaldados por Evidence;
5. relaciona Claims en Hypotheses verificables y falsables;
6. valida respaldo, coherencia, incertidumbre y trazabilidad;
7. produce Findings consumibles sin convertirlos en decisiones;
8. consolida el resultado conceptual en un `inference-report`.

Su alcance termina cuando el reporte de inferencia queda disponible para un
producto consumidor. La presentación, la decisión humana y cualquier acción
posterior pertenecen fuera de esta capability.

## 6. Límites de conocimiento

El Inference Engine es completamente agnóstico del dominio. No posee
conocimiento intrínseco sobre proveedores, flujos de cambio, formatos
documentales, fuentes, repositorios ni disciplinas profesionales.

En particular, no conoce:

- proveedores de control de versiones;
- Pull Requests ni Merge Requests;
- README ni formatos de marcado;
- repositorios, checkouts o estructuras de directorios;
- protocolos o plataformas de integración;
- ingeniería de software ni arquitectura;
- incidentes u observabilidad;
- seguridad como dominio de análisis.

Los nombres de campos heredados por `local-context.json` son parte del contrato
de procedencia. El motor puede conservarlos y citarlos, pero no les atribuye
semántica externa ni los utiliza para consultar su origen.

El motor razona sobre Evidence, Claims, Hypotheses, Rules, Confidence,
Uncertainty y Findings. La semántica específica de un producto pertenece al
producto consumidor o a conocimiento declarativo explícitamente gobernado, no a
suposiciones internas del motor.

## 7. Usuarios y consumidores

Capability-002 no es una experiencia de usuario final. Sus consumidores son
capabilities de producto autorizadas que necesitan razonamiento trazable sobre
contexto estructurado.

El consumidor:

- decide qué Findings son relevantes para su caso de uso;
- presenta los resultados en su propio lenguaje y experiencia;
- conserva las categorías y la trazabilidad recibidas;
- no convierte Confidence en certeza;
- mantiene a una persona como responsable de la decisión;
- puede descartar, cuestionar o solicitar verificación adicional.

El consumidor no delega al motor autoridad de aprobación, rechazo o ejecución.

## 8. Entrada oficial

La única entrada oficial de una ejecución es:

```text
local-context.json
```

El artefacto debe cumplir el Local Context Contract vigente. Capability-002 no
acepta como entradas alternativas archivos intermedios, texto libre, referencias
a fuentes ni acceso directo a sistemas externos.

Las Rules vigentes forman parte de la definición gobernada de la capability; no
constituyen una fuente de evidencia ni una entrada contextual adicional. Una
Rule puede determinar cómo razonar, pero nunca puede aportar un hecho que no
exista en `local-context.json`.

### 8.1 Responsabilidades sobre la entrada

El productor garantiza autorización, recuperación, orden, integridad y
compatibilidad contractual. El Inference Engine:

- valida que la entrada respete el contrato soportado;
- trata el contenido como datos no confiables;
- no lo modifica;
- no intenta reparar campos requeridos ausentes;
- no completa contenido nulo;
- no consulta el origen para enriquecerlo;
- conserva la identidad del pack durante toda la cadena de trazabilidad.

### 8.2 Determinismo de entrada

Los mismos bytes identifican el mismo contexto de ejecución. Cualquier variación
de Evidence exige una nueva entrada producida por Context Retrieval; no puede
ocultarse como una variación interna del razonamiento.

## 9. Modelo de dominio

El modelo define unidades epistemológicas, no estructuras de software. Cada
unidad tiene una responsabilidad distinta y no puede sustituir a otra.

### 9.1 Evidence

**Evidence** representa un hecho observable contenido en una fuente autorizada y
entregado mediante el contrato de entrada.

Evidence debe ser:

- **trazable:** permite identificar su ubicación dentro del contexto recibido;
- **inmutable:** no cambia durante una ejecución;
- **observable:** su contenido puede verificarse en la entrada;
- **autorizada:** proviene del productor que aplicó los controles de acceso;
- **identificable:** posee identidad inequívoca dentro de la ejecución;
- **reproducible:** puede recuperarse de los mismos bytes de entrada;
- **neutral:** conserva lo observado sin agregar juicio.

Evidence nunca contiene opiniones, recomendaciones ni conclusiones. Que un texto
de origen exprese una opinión es en sí un hecho observable —“la fuente afirma
X”—, pero no convierte X en un hecho verdadero.

Evidence no prueba automáticamente autoridad, vigencia, exhaustividad ni verdad
universal. Sólo prueba que una observación determinada está presente en la
entrada autorizada.

### 9.2 Claim

**Claim** representa una afirmación derivada exclusivamente de una o más
Evidence. Es la primera unidad de razonamiento.

Un Claim debe:

- expresar una sola afirmación evaluable;
- citar toda Evidence utilizada para derivarlo;
- distinguir observación directa de interpretación;
- ser más específico que la Evidence combinada, sin exceder su respaldo;
- poder confirmarse, refutarse o limitarse al revisar sus referencias;
- registrar la Rule que habilita su derivación;
- conservar su Confidence y Uncertainty propias.

Un Claim no puede existir sin Evidence. Una Rule no reemplaza esta obligación.
Si la entrada no respalda la afirmación, ésta no se produce como Claim.

Claims incompatibles pueden coexistir cuando la Evidence está en conflicto. El
motor no elige silenciosamente uno; conserva el conflicto como Uncertainty.

### 9.3 Hypothesis

**Hypothesis** relaciona uno o más Claims para proponer una explicación,
relación o consecuencia posible.

Una Hypothesis debe ser:

- **explícita:** declara exactamente qué relación propone;
- **verificable:** identifica qué observación permitiría evaluarla;
- **falsable:** explica qué Evidence podría contradecirla;
- **trazable:** referencia todos los Claims que la sostienen o limitan;
- **provisional:** nunca se presenta como hecho confirmado;
- **acotada:** no generaliza más allá del contexto disponible.

Una Hypothesis puede quedar respaldada, debilitada, contradicha o sin evidencia
suficiente. Ninguno de esos estados equivale a una decisión.

La ausencia de Claims suficientes impide construir una Hypothesis. No habilita
una explicación por defecto.

### 9.4 Finding

**Finding** es el resultado consumible por una capability de producto. Organiza
el razonamiento validado sin emitir un veredicto.

Todo Finding debe contener conceptualmente:

- una categoría explícita;
- una formulación clara y acotada;
- Confidence;
- referencias a toda Evidence utilizada;
- las Hypotheses asociadas;
- Uncertainty explícita;
- preguntas abiertas;
- límites de aplicabilidad;
- trazabilidad completa hasta la entrada.

La categoría describe la naturaleza del resultado, no su importancia ni la
acción que debe tomarse. Las categorías concretas formarán parte del futuro
contrato de `inference-report`; este documento no las convierte todavía en un
schema.

Un Finding nunca contiene aprobación, rechazo ni decisión final. Tampoco ordena
una acción. Puede hacer visible una pregunta o una consecuencia posible para que
otro producto la presente a una persona.

#### Finding y Recommendation

Finding y Recommendation son conceptos diferentes. Un Finding expresa
conocimiento derivado: qué puede sostenerse, con qué respaldo y bajo qué
incertidumbre. Una Recommendation propone una posible acción frente a un
Finding.

Capability-002 produce Findings y nunca Recommendations. Determinar si un
Finding justifica una posible acción requiere propósito, contexto de decisión y
autoridad que el Inference Engine no posee. Esa responsabilidad pertenece a las
capabilities consumidoras y permanece sujeta a decisión humana.

### 9.5 Rule

**Rule** representa conocimiento declarativo que establece una relación
permitida entre Evidence, Claims, Hypotheses y Findings. No representa código,
un procedimiento técnico ni una fuente de verdad.

Toda Rule debe explicar:

- **cuándo aplica:** precondiciones observables y alcance;
- **qué Evidence requiere:** cantidad, tipo lógico y calidad mínima de respaldo;
- **qué Claims puede generar:** forma y límite de las afirmaciones permitidas;
- **qué relaciones admite:** condiciones para construir Hypotheses;
- **qué restricciones posee:** exclusiones, conflictos y condiciones de
  abstención;
- **qué incertidumbre introduce:** limitaciones inevitables de la regla;
- **cómo puede evaluarse:** ejemplos positivos, negativos y fronteras;
- **qué versión está vigente:** identidad necesaria para reproducibilidad.

Una Rule no puede:

- introducir Evidence ausente;
- declarar verdadera una Hypothesis;
- ocultar una contradicción;
- elevar Confidence sin respaldo;
- decidir qué debe hacer una persona;
- modificar el contexto recibido.

Capability-002 consume Rules gobernadas externamente. La definición,
administración, aprobación, modificación y retiro de Rules no pertenecen al
Inference Engine. El motor nunca crea, modifica ni aprueba una Rule; sólo aplica
la versión vigente que una autoridad externa le proporciona bajo condiciones
gobernadas.

Las Rules son versionadas y auditables. Su mera aprobación no las convierte en
correctas: deben demostrar consistencia y utilidad mediante evaluación. Esta
frontera evita que ejecutar razonamiento otorgue también autoridad para cambiar
las reglas que lo controlan.

### 9.6 Confidence

**Confidence** expresa el grado de respaldo que la Evidence disponible aporta a
una inferencia. No expresa probabilidad matemática, verdad objetiva, prioridad
ni severidad.

Confidence es un concepto de dominio de primera clase y un Value Object
conceptual. No es una etiqueta aislada ni una entidad con identidad
independiente: su valor queda definido por la combinación inseparable de:

- **nivel:** grado cualitativo de respaldo;
- **fundamento:** explicación de por qué corresponde ese nivel;
- **limitaciones:** condiciones que impiden una afirmación más fuerte;
- **dimensiones relevantes:** aspectos del respaldo considerados por separado.

Dos Confidence sólo son equivalentes cuando esos componentes expresan el mismo
grado de respaldo bajo los mismos límites. Nombrar únicamente el nivel no
constituye una Confidence completa.

Confidence se comunica mediante niveles cualitativos ordenados:

- **fuerte:** respaldo directo, suficiente y consistente dentro del alcance;
- **moderado:** respaldo relevante, pero parcial o con límites materiales;
- **débil:** respaldo indirecto, escaso, ambiguo o conflictivo;
- **insuficiente:** no existe base para sostener la inferencia.

Cada nivel debe acompañarse con su fundamento. Los porcentajes no son
obligatorios y no deben usarse sin calibración y significado demostrables.

El grado de respaldo considera, como dimensiones separadas:

- relación directa entre Evidence y afirmación;
- cobertura de Evidence necesaria;
- consistencia o contradicción entre observaciones;
- calidad de la cadena de trazabilidad;
- límites declarados por las Rules aplicadas.

Confidence nunca elimina Uncertainty. Una inferencia puede tener respaldo fuerte
y conservar una limitación importante de alcance.

### 9.7 Uncertainty

**Uncertainty** representa aquello que limita, debilita o impide una inferencia.
Es información obligatoria del razonamiento, no una advertencia decorativa.

Puede originarse en:

- Evidence ausente;
- cobertura parcial;
- observaciones ambiguas;
- Evidence contradictoria;
- relación indirecta entre Evidence y Claim;
- límites de aplicabilidad de una Rule;
- imposibilidad de verificar o falsar una Hypothesis con el contexto disponible;
- campos nulos o errores declarados por el contrato de entrada.

Toda Uncertainty debe indicar:

- qué se desconoce o está en conflicto;
- qué parte del razonamiento afecta;
- cómo limita Confidence o alcance;
- qué Evidence sería necesaria para reducirla, cuando pueda expresarse sin
  inventar una fuente.

La ausencia nunca se reemplaza con una suposición. Si falta respaldo, el motor
reduce el alcance, formula una pregunta abierta o se abstiene.

### 9.8 Traceability

**Traceability** es la capacidad de reconstruir cada resultado mediante una
cadena íntegra de referencias:

```text
Finding
  ↓
Hypothesis
  ↓
Claim
  ↓
Evidence
```

La cadena puede ramificarse: un Finding puede relacionar varias Hypotheses, una
Hypothesis varios Claims y un Claim varias Evidence. Ningún salto puede omitir
el nivel que justifica la derivación.

Cada vínculo debe responder:

- qué unidad origina el vínculo;
- qué unidad lo respalda;
- qué Rule permitió la relación;
- qué límites o contradicciones se conservaron.

Una referencia rota invalida la unidad dependiente. La validación no inventa ni
repara vínculos; rechaza o degrada el resultado.

## 10. Invariantes del razonamiento

Los siguientes invariantes aplican a toda ejecución:

1. No existe Claim sin Evidence.
2. No existe Hypothesis sin Claims.
3. No existe Finding sin trazabilidad completa.
4. Una Rule nunca cuenta como Evidence.
5. Confidence nunca sustituye el fundamento.
6. Uncertainty nunca se omite cuando afecta el resultado.
7. Contradicción no equivale a error ni se resuelve sin precedencia observable.
8. Ausencia de Evidence no prueba inexistencia.
9. Repetición de una afirmación no aumenta por sí sola su respaldo.
10. Correlación no se presenta como causalidad.
11. Una inferencia no se presenta como observación.
12. Ningún resultado constituye una decisión.
13. La entrada permanece inmutable.
14. Un resultado inválido no se completa con contenido plausible.
15. Toda abstención explica qué condición no pudo satisfacerse.

## 11. Pipeline conceptual

El flujo lógico es:

```text
Evidence
    ↓
Evidence Processing
    ↓
Claims
    ↓
Reasoning
    ↓
Hypotheses
    ↓
Validation
    ↓
Findings
    ↓
Inference Report
```

### 11.1 Evidence

Se reconoce la evidencia disponible en `local-context.json` sin alterar su
contenido ni semántica de procedencia.

### 11.2 Evidence Processing

Se establece qué Evidence puede participar en una inferencia, qué limitaciones
declara y qué relaciones explícitas ya contiene. Esta etapa no produce
conclusiones ni recupera información adicional.

### 11.3 Claims

Se formulan afirmaciones atómicas cuyo respaldo puede señalarse directamente en
Evidence. Una afirmación sin respaldo queda excluida.

### 11.4 Reasoning

Se aplican Rules vigentes para relacionar Claims dentro de límites explícitos.
Esta etapa no agrega hechos ni autoridad.

### 11.5 Hypotheses

Se expresan relaciones provisionales, verificables y falsables. Cada una
conserva Claims favorables, contradictorios y faltantes.

### 11.6 Validation

Se comprueban integridad de trazabilidad, restricciones de Rules, categorías,
Confidence, Uncertainty, contradicciones y condiciones de abstención. Validar
estructura no equivale a demostrar verdad.

### 11.7 Findings

Se construyen unidades consumibles únicamente a partir de razonamiento validado.
Un Finding conserva preguntas y límites; no los resuelve artificialmente.

### 11.8 Inference Report

Se agrupan Findings, trazabilidad, incertidumbre y estado de la ejecución en el
contrato conceptual `inference-report`.

### 11.9 Evaluación posterior

La evaluación del razonamiento es necesaria, pero no es una etapa productora
entre Findings e Inference Report. Ocurre después y fuera del pipeline de una
ejecución: toma reportes y casos de referencia para determinar trazabilidad,
consistencia, utilidad, cobertura y manejo de incertidumbre.

Mantenerla fuera del flujo evita que una ejecución se evalúe y se apruebe a sí
misma. Evaluation no modifica el reporte evaluado, no altera Rules, no incorpora
feedback automáticamente y no produce aprendizaje. Sus resultados pertenecen a
la gobernanza y al lifecycle de la capability.

## 12. Salida oficial

La única salida conceptual es:

```text
inference-report
```

El reporte debe permitir a un consumidor:

- identificar la entrada a la que corresponde;
- enumerar Findings sin perder su orden declarado;
- recorrer la cadena completa hasta Evidence;
- distinguir Confidence de Uncertainty;
- conocer abstenciones, conflictos y cobertura;
- identificar las Rules vigentes que participaron;
- reproducir las condiciones conceptuales de la ejecución.

Este documento no define un esquema JSON, formato de presentación ni renderer.
El contrato estructural y su versionado serán objeto de una decisión posterior.
Hasta entonces, `inference-report` es un contrato conceptual y no autoriza una
implementación.

## 13. Principios

### 13.1 Evidence First

Toda inferencia comienza en Evidence autorizada y termina con referencias a esa
Evidence. Fluidez, conveniencia o plausibilidad nunca sustituyen respaldo.

### 13.2 Traceability by Design

La trazabilidad forma parte del modelo de dominio desde el primer Claim. No se
agrega al final como explicación retrospectiva.

### 13.3 Explicit Uncertainty

Lo desconocido, ambiguo, contradictorio o incompleto se representa de forma
explícita. Una salida más corta o una abstención son preferibles a rellenar un
vacío.

### 13.4 Domain Agnostic

El motor opera sobre unidades epistemológicas estables. Los dominios, fuentes y
experiencias cambian sin redefinir el núcleo conceptual de inferencia.

### 13.5 Human Decision

La capability informa a productos y personas. La autoridad para decidir
permanece fuera del motor.

### 13.6 Reproducibility

Debe poder reconstruirse qué entrada, Rules y relaciones dieron origen a cada
resultado. Una variación debe ser atribuible y visible.

### 13.7 Deterministic Inputs

La inferencia opera sobre una entrada completa e inmutable. No mezcla consultas
tardías ni evidencia recuperada durante el razonamiento.

### 13.8 Explainability

Toda salida material explica su respaldo, sus límites y el camino de derivación
en términos comprensibles para el consumidor.

### 13.9 Abstention over Fabrication

Cuando una Rule no puede satisfacerse o la trazabilidad es incompleta, la
capability se abstiene. No produce un resultado plausible para completar una
sección.

### 13.10 Separation of Concerns

Recuperar, inferir, presentar, decidir y actuar son responsabilidades distintas.
Capability-002 sólo infiere.

La misma separación aplica a las Rules: una autoridad externa las gobierna y el
motor sólo las consume. Aplicar una Rule no concede autoridad para crearla,
modificarla ni aprobarla.

## 14. Anti-Goals

Capability-002 explícitamente no:

- aprueba ni rechaza cambios;
- revisa código;
- interpreta proveedores o flujos de control de versiones;
- busca, abre ni modifica repositorios;
- interpreta formatos documentales;
- conoce arquitectura, incidentes, observabilidad o seguridad como dominios;
- recupera contexto adicional;
- genera código;
- modifica Evidence;
- resuelve contradicciones sin respaldo explícito;
- convierte una Hypothesis en decisión;
- produce Recommendations;
- produce veredictos de cumplimiento;
- genera una vista de presentación;
- publica comentarios ni mensajes;
- ejecuta acciones;
- reemplaza expertos ni responsabilidad humana;
- aprende automáticamente de feedback;
- crea una fuente de verdad paralela;
- define una plataforma técnica general.

## 15. Manejo de ausencia, conflicto y degradación

### 15.1 Entrada inválida

Si `local-context.json` no cumple su contrato, no se produce un reporte parcial.
La ejecución falla de forma explícita antes del razonamiento.

### 15.2 Evidence insuficiente

Si existe contexto válido pero no alcanza para una inferencia, la salida válida
es una abstención trazable. Debe indicar el límite y la pregunta abierta, sin
proponer una respuesta.

### 15.3 Evidence contradictoria

Las observaciones incompatibles se conservan. Los Claims resultantes pueden
mostrar posiciones en conflicto, pero ninguna obtiene precedencia si ésta no
forma parte de la Evidence.

### 15.4 Documento no legible

Un error declarado en la entrada es Evidence de una limitación de recuperación,
no del contenido ausente. No se reconstruye ni se supone ese contenido.

### 15.5 Trazabilidad incompleta

Un Claim, Hypothesis o Finding con referencias faltantes queda inválido. Puede
degradarse a una abstención si la limitación misma está respaldada; nunca se
publica como inferencia completa.

### 15.6 Rule no aplicable

La falta de precondiciones impide aplicar la Rule. No se relajan sus requisitos
para producir más Findings.

## 16. Reproducibilidad y consistencia

Una ejecución reproducible permite identificar:

- la identidad exacta de `local-context.json`;
- la versión contractual soportada;
- las Rules vigentes;
- las relaciones de trazabilidad producidas;
- las abstenciones y degradaciones;
- cualquier variación material entre resultados.

Reproducibilidad no significa que dos textos deban ser visualmente idénticos si
el futuro contrato admite variación explícita. Significa que la misma entrada y
las mismas condiciones gobernadas no pueden producir contradicciones materiales
sin declararlas y atribuirlas.

Consistencia tampoco exige repetir una inferencia incorrecta. Una corrección de
Rules crea condiciones distintas, debe versionarse y debe poder compararse con
la versión anterior.

## 17. Métricas

Las métricas evalúan la calidad del razonamiento y su utilidad. No califican una
tecnología subyacente ni el desempeño individual de personas.

### 17.1 Trazabilidad

- **cobertura de trazabilidad:** proporción de Findings que permiten recorrer
  todos los vínculos hasta Evidence;
- **integridad de referencias:** proporción de referencias que apuntan a
  unidades existentes y compatibles;
- **Claims sin respaldo:** cantidad y proporción de Claims sin Evidence válida;
- **saltos epistemológicos:** relaciones que omiten un nivel requerido.

El objetivo normativo para Claims sin respaldo y referencias rotas es cero.

### 17.2 Reproducibilidad

- proporción de ejecuciones repetidas sin contradicciones materiales;
- proporción de diferencias atribuibles a una entrada o Rule versionada;
- capacidad de reconstruir las condiciones de cada Finding;
- tasa de resultados cuya procedencia de entrada no puede identificarse.

### 17.3 Utilidad

- proporción de Findings considerados comprensibles y pertinentes por
  consumidores autorizados;
- proporción de Findings que ayudan a formular una verificación o pregunta útil;
- tasa de Findings descartados por ser vagos, redundantes o no accionables para
  comprensión;
- reducción del esfuerzo necesario para verificar el fundamento de una
  inferencia.

Utilidad no convierte al Finding en decisión ni autoriza optimizar por
aceptación acrítica del usuario.

### 17.4 Consistencia

- tasa de inferencias materialmente incompatibles ante Evidence equivalente;
- aplicación uniforme de Rules bajo las mismas precondiciones;
- consistencia de clasificación entre Claim, Hypothesis y Finding;
- contradicciones ocultas o resueltas sin respaldo.

### 17.5 Incertidumbre explícita

- proporción de Findings limitados que declaran Uncertainty;
- tasa de ausencias reemplazadas incorrectamente por suposiciones;
- proporción de conflictos visibles para el consumidor;
- calidad de las preguntas abiertas para describir Evidence faltante;
- tasa de Confidence expresada sin fundamento.

### 17.6 Cobertura

- proporción de Evidence elegible considerada por al menos una Rule aplicable;
- proporción de Claims relevantes representados en Findings;
- categorías de razonamiento omitidas por falta de Evidence;
- tasa de abstención correcta en casos insuficientes;
- tasa de inferencias relevantes omitidas en un conjunto de evaluación.

Cobertura no significa exhaustividad. Aumentarla no justifica debilitar
trazabilidad, precisión ni condiciones de abstención.

## 18. Estrategia de evaluación

La capability debe evaluarse con casos versionados cuya Evidence, Claims
admisibles, Hypotheses posibles, incertidumbres y límites hayan sido
establecidos antes de ejecutar la evaluación.

El conjunto debe incluir:

- Evidence suficiente, parcial y ausente;
- observaciones consistentes y contradictorias;
- Claims directos e inferencias que exceden el respaldo;
- Hypotheses verificables y no falsables;
- Rules aplicables, no aplicables y mutuamente restrictivas;
- Findings útiles, redundantes y demasiado generales;
- referencias rotas;
- contenido diseñado para intentar alterar las reglas de la capability;
- ejecuciones repetidas sobre la misma entrada;
- contextos de dominios diferentes para comprobar agnosticismo.

Evaluadores calificados revisan trazabilidad, clasificación, incertidumbre,
consistencia y utilidad. Los umbrales cuantitativos se fijan antes de declarar
la capability Accepted y no se ajustan retroactivamente para producir
aprobación.

Esta Evaluation es una actividad de gobierno posterior a la producción del
`inference-report`. No forma parte de la cadena epistemológica y sus resultados
no regresan automáticamente como Evidence ni Rules.

## 19. Definition of Done

Capability-002 se considera terminada y candidata a estado **Accepted** sólo
cuando cumple todas las condiciones siguientes.

### 19.1 Propósito y límites

- problema, outcome, owner y consumidores están aceptados;
- `local-context.json` permanece como única entrada oficial;
- `inference-report` posee un contrato estructural canónico y versionado;
- recuperación, presentación, decisión y acción permanecen fuera del alcance;
- el motor demuestra agnosticismo mediante contextos de más de un dominio;
- existe un mecanismo de desactivación que no invalida Context Retrieval.

### 19.2 Modelo de razonamiento

- Evidence, Claim, Hypothesis, Finding, Rule, Confidence, Uncertainty y
  Traceability poseen definiciones no ambiguas;
- todos los invariantes de la sección 10 pueden verificarse;
- cada Finding reconstruye su cadena completa hasta Evidence;
- las Rules están identificadas, versionadas y evaluadas por su autoridad
  externa;
- el motor demuestra que no crea, modifica ni aprueba Rules;
- Confidence siempre incluye fundamento y no se presenta como certeza;
- Uncertainty y preguntas abiertas se conservan cuando corresponde;
- contradicciones y ausencias nunca se resuelven mediante suposición;
- abstención es un resultado soportado y evaluado.

### 19.3 Calidad

- no existen Claims publicados sin Evidence válida;
- no existen referencias rotas en Findings aceptados;
- se alcanzan umbrales preacordados de trazabilidad, reproducibilidad, utilidad,
  consistencia, incertidumbre explícita y cobertura;
- casos adversariales no logran alterar Rules ni introducir Evidence;
- resultados materialmente distintos pueden atribuirse a cambios gobernados;
- los errores y degradaciones son explícitos y evaluables.

### 19.4 Control humano y seguridad

- ningún Finding expresa aprobación, rechazo o decisión final;
- la capability no ejecuta acciones ni genera código;
- no accede a fuentes externas ni amplía los permisos de la entrada;
- contenido no confiable no puede modificar sus reglas de operación;
- los resultados conservan la clasificación y protección aplicable al contexto;
- la retención y exposición del reporte están gobernadas antes de un piloto;
- una persona conserva responsabilidad sobre toda decisión posterior.

### 19.5 Evaluación y outcome

- existe baseline y conjunto de evaluación versionado;
- los umbrales fueron aprobados antes de medir;
- evaluadores autorizados confirman que los Findings son comprensibles,
  verificables y útiles;
- se registran omisiones, conflictos, degradaciones y abstenciones;
- la evaluación incluye dominios distintos y casos insuficientes;
- existe una decisión explícita de aceptar, iterar o retirar basada en
  evidencia;
- el outcome demostrado supera la mera producción de un reporte.

Completar una implementación no satisface por sí solo esta Definition of Done.

## 20. Riesgos y trade-offs

| Riesgo o trade-off                   | Consecuencia                      | Respuesta requerida                                |
| ------------------------------------ | --------------------------------- | -------------------------------------------------- |
| Inferencia plausible sin respaldo    | Confianza indebida                | Claims sólo con Evidence y abstención obligatoria  |
| Cadena demasiado compleja            | Difícil verificación              | Unidades atómicas y trazabilidad navegable         |
| Rules demasiado generales            | Findings vagos o incorrectos      | Alcance explícito y evaluación de fronteras        |
| Rules demasiado específicas          | Baja reutilización                | Generalizar sólo tras evidencia entre dominios     |
| Confidence interpretada como certeza | Decisión delegada incorrectamente | Nivel cualitativo con fundamento y Uncertainty     |
| Exceso de abstención                 | Baja utilidad                     | Medir cobertura sin debilitar requisitos           |
| Baja abstención                      | Suposiciones ocultas              | Casos insuficientes y métrica de fabricación       |
| Contradicciones ocultas              | Falsa coherencia                  | Preservar Claims incompatibles y su impacto        |
| Salida excesiva                      | Sobrecarga del consumidor         | Findings acotados, deduplicados y pertinentes      |
| Acoplamiento al primer dominio       | Reutilización aparente            | Evaluar con dominios distintos                     |
| Contrato de salida prematuro         | Rigidez sin evidencia             | Mantener `inference-report` conceptual por ahora   |
| Reproducibilidad mal entendida       | Ocultar variación material        | Exigir atribución, no identidad textual artificial |

El trade-off rector es aceptar menos inferencias a cambio de que cada resultado
sea trazable, limitado y verificable.

## 21. Gobernanza y lifecycle

Capability-002 sigue el lifecycle definido por Architecture v1.0:

```text
proposed → experiment → limited → accepted → deprecated → retired
```

El paso desde **Proposed** requiere un contrato de salida propuesto, un owner,
Rules iniciales y un conjunto de evaluación. El paso a **Limited** requiere
umbrales previos, controles de datos y evidencia de degradación segura. El paso
a **Accepted** requiere demostrar el outcome y cumplir la Definition of Done.

Requieren revisión arquitectónica explícita:

- agregar una entrada distinta de `local-context.json`;
- permitir acceso directo a una fuente;
- otorgar autoridad de decisión o acción;
- cambiar las categorías epistemológicas fundamentales;
- introducir aprendizaje que modifique Rules sin gobierno;
- realizar un cambio incompatible al futuro contrato de `inference-report`;
- convertir la capability en dependencia crítica de un flujo.

Feedback sobre Findings es una señal de evaluación. No modifica automáticamente
Evidence, Rules ni conocimiento aceptado.

## 22. Decisiones postergadas

Este documento no decide:

- el esquema estructural de `inference-report`;
- sus reglas de compatibilidad y versionado;
- la tecnología que realizará el razonamiento;
- el runtime o lenguaje de una implementación;
- el mecanismo de configuración y distribución de Rules;
- persistencia, caché, hosting o despliegue;
- interfaz de usuario o formato de presentación;
- thresholds cuantitativos sin baseline;
- retención definitiva de reportes;
- mecanismos de feedback;
- especializaciones por dominio.

También quedan postergados dos conceptos candidatos:

- **Observation:** no forma parte del modelo principal mientras Evidence siga
  representando directamente un hecho observable. Se reconsiderará sólo si la
  evolución del contrato exige distinguir el artefacto probatorio de una
  observación atómica realizada sobre él;
- **Inference Strategy:** queda fuera del alcance actual. Nombrar cómo se
  aplican Rules puede resultar útil cuando existan estrategias alternativas
  demostradas, pero introducir el concepto ahora anticiparía variabilidad no
  observada y podría confundirse con diseño de ejecución.

Cada decisión futura debe responder a una necesidad demostrada de esta
capability y no autoriza infraestructura compartida por anticipado.

## 23. Suposiciones y preguntas abiertas

### 23.1 Suposiciones

- Context Retrieval entrega entradas válidas, autorizadas e inmutables;
- la Evidence disponible contiene señal suficiente para evaluar el primer caso;
- los consumidores pueden conservar la trazabilidad recibida;
- evaluadores humanos pueden establecer casos y límites esperados;
- el agnosticismo puede comprobarse sin ampliar fuentes en el piloto.

### 23.2 Preguntas abiertas

- ¿Qué categorías mínimas necesita `inference-report` sin acoplarlo a un
  dominio?
- ¿Cómo se identifican de forma estable Evidence y unidades derivadas?
- ¿Qué metadata de ejecución es indispensable para reproducibilidad?
- ¿Qué conjunto mínimo de Rules demuestra valor sin anticipar una plataforma?
- ¿Qué umbrales distinguen una inferencia útil de una explicación plausible?
- ¿Cómo se evalúa utilidad entre dominios sin perder especificidad del producto?
- ¿Qué política de retención corresponde a Findings y sus cadenas de Evidence?
- ¿Una futura Evidence representará siempre el hecho observable o será necesario
  distinguirla de una Observation sin duplicar responsabilidades?
- ¿Qué evidencia de variabilidad justificaría incorporar Inference Strategy al
  lenguaje de EIP?

Estas preguntas deben resolverse incrementalmente. No bloquean el estado
Proposed y no autorizan implementación.

## 24. Criterio de aceptación y evolución

El estado **Proposed** significa que este documento define una hipótesis formal
de capability pendiente de evaluación y aceptación. No significa que exista una
implementación ni que el razonamiento haya demostrado calidad.

La evolución debe mantener cuatro fronteras:

1. Context Retrieval produce evidencia; no infiere.
2. Inference Engine infiere; no decide.
3. El producto consumidor contextualiza y presenta; no altera la evidencia.
4. La persona evalúa y decide.

Una necesidad de dominio no debe incorporarse automáticamente al motor. Primero
debe demostrarse que corresponde a una Rule declarativa o a la capability de
producto consumidora.

## 25. Registro de decisiones de Capability-002

| ID         | Decisión                                                     | Estado   |
| ---------- | ------------------------------------------------------------ | -------- |
| CAP002-D01 | `local-context.json` es la única entrada oficial             | Proposed |
| CAP002-D02 | `inference-report` es la única salida conceptual             | Proposed |
| CAP002-D03 | Evidence, Claim, Hypothesis y Finding son unidades distintas | Proposed |
| CAP002-D04 | Toda inferencia conserva trazabilidad completa               | Proposed |
| CAP002-D05 | Confidence es cualitativa y fundamentada, no certeza         | Proposed |
| CAP002-D06 | Uncertainty es obligatoria cuando limita el razonamiento     | Proposed |
| CAP002-D07 | Rules son declarativas, versionadas y nunca Evidence         | Proposed |
| CAP002-D08 | El motor es agnóstico del dominio y de las fuentes           | Proposed |
| CAP002-D09 | La decisión permanece humana y fuera de la capability        | Proposed |
| CAP002-D10 | El contrato estructural de salida queda postergado           | Proposed |
| CAP002-D11 | El motor consume Rules gobernadas externamente               | Proposed |
| CAP002-D12 | Findings y Recommendations permanecen separados              | Proposed |
| CAP002-D13 | Evaluation ocurre fuera del pipeline productor               | Proposed |

## 26. Historial del documento

| Fecha      | Cambio                                                 | Estado   |
| ---------- | ------------------------------------------------------ | -------- |
| 2026-08-04 | Propuesta inicial de Capability-002 — Inference Engine | Proposed |
| 2026-08-04 | Revisión profunda del modelo de dominio                | Proposed |
