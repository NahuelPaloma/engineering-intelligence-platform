# Contract Change Detection Contract

| Campo                | Valor                                      |
| -------------------- | ------------------------------------------ |
| Contrato             | `contract-change-detection`                |
| Resultado conceptual | `contract-change-report`                   |
| Versión conceptual   | 1                                          |
| Estado               | **Accepted**                               |
| Productor            | Capability-003 — Contract Change Detection |
| Consumidor inicial   | Capability-004                             |
| Audiencia            | Capabilities consumidoras                  |
| Última actualización | 5 de agosto de 2026                        |

---

## 1. Propósito

Este documento define el contrato conceptual oficial entre:

```text
Capability-003 — Contract Change Detection
                       ↓
              contract-change-report
                       ↓
                  Capability-004
```

El contrato establece qué significa un resultado de Contract Change Detection,
qué información garantiza, qué límites conserva y qué responsabilidades impone
al productor y a sus consumidores.

Su propósito es permitir que una capability posterior conozca de forma trazable:

- qué archivos modificados fueron considerados Contract Candidates;
- qué contratos pudieron detectarse;
- qué Contract Type pudo sostenerse;
- si la Evidence alcanzó para la detección;
- si existe preparación suficiente para una capability posterior;
- qué parte del alcance quedó sin cubrir;
- qué incertidumbres, contradicciones o abstenciones permanecen abiertas.

Este contrato no define implementación, clases, JSON, schema, serialización,
transporte, persistencia ni presentación.

## 2. Autoridad y relación con otros documentos

Este contrato deriva de:

1. Product Vision v1.1, Accepted;
2. Architecture v1.0 y sus ADR vigentes;
3. ADR-013 — Local Context Contract, Accepted;
4. Local Context Contract, versión conceptual 1, Accepted;
5. Capability-002 — Inference Engine, Accepted;
6. Inference Report Contract, versión conceptual 1, Accepted;
7. Inference Engine Reasoning Specification, Accepted;
8. Capability-003 — Contract Change Detection, Accepted.

Local Context Contract gobierna la Evidence que atraviesa Context Retrieval.
Inference Report Contract gobierna el resultado genérico de Capability-002. Este
documento gobierna únicamente el significado observable del resultado
especializado producido por Capability-003 para sus consumidores.

```text
Local Context Contract
          ↓
Modified File Evidence
          ↓
Capability-002 + conocimiento gobernado de Capability-003
          ↓
Inference Report
          ↓
Capability-003
          ↓
Contract Change Detection Contract
          ↓
Capability-004
```

Este flujo es conceptual. No decide si algunas responsabilidades comparten una
implementación ni agrega una nueva entrada física a Capability-002.

Contract Change Detection Reasoning Specification, Accepted, es la fuente
normativa del ciclo de vida y las reglas de razonamiento específicas de este
dominio. Este contrato no autoriza por sí mismo Rules ni implementación.

## 3. Productor único

El único productor de este contrato es:

> **Capability-003 — Contract Change Detection**

Ninguna capability consumidora, adaptador, renderer, herramienta de evaluación o
persona puede presentarse como productor del contrato.

El productor debe:

- usar únicamente Evidence autorizada y admitida;
- preservar identidad, Scope, Traceability, Confidence y Uncertainty;
- aplicar conocimiento declarativo gobernado;
- publicar sólo resultados contractualmente válidos;
- expresar cobertura, insuficiencia y abstención;
- mantener el resultado descriptivo;
- rechazar toda conclusión fuera de Capability-003.

Producir el contrato no concede autoridad sobre la fuente, la capability
consumidora ni una decisión humana.

## 4. Consumidores permitidos

El consumidor inicial declarado es:

> **Capability-004**

Este contrato no asume todavía el nombre, dominio, propósito, arquitectura ni
autoridad de Capability-004. Sólo establece que Capability-004 consumirá este
límite sin reconstruir Contract Change Detection.

También pueden consumirlo futuras capabilities autorizadas, por ejemplo para:

- Architecture Review;
- Incident Intelligence;
- Observability Intelligence;
- Security Intelligence;
- API Governance;
- otras experiencias que necesiten localizar contratos modificados.

La enumeración no incorpora esos dominios al contrato. Todos los consumidores
reciben las mismas garantías y prohibiciones.

Un consumidor puede seleccionar, correlacionar o presentar detecciones bajo su
propio contrato. No puede modificar retroactivamente el resultado original ni
atribuirle conclusiones que Capability-003 no produjo.

## 5. Definición del resultado contractual

Un **Contract Change Report** es el resultado autocontenido, trazable y acotado
de una ejecución de Capability-003 sobre Modified File Evidence identificable.

Representa:

- la identidad de la ejecución y su alcance;
- la disponibilidad de Modified File Evidence;
- los Contract Candidates evaluados;
- los Contract Detections válidos;
- Contract Types respaldados o no determinados;
- Detection Sufficiency;
- Analysis Readiness;
- Confidence y Uncertainty;
- Traceability hasta Evidence;
- Coverage;
- Abstentions y Contradictions;
- el estado de completitud del resultado.

El reporte conserva conocimiento derivado. No modifica Evidence, no interpreta
contenido funcional y no constituye una decisión.

## 6. Garantías

Todo resultado válido garantiza que:

1. corresponde a una única entrada y ejecución identificables;
2. toda Evidence proviene del límite contractual autorizado;
3. la disponibilidad de `modified_files` conserva la diferencia entre
   `available` y `not_provided`;
4. todo Contract Candidate referencia Modified File Evidence existente;
5. todo Contract Detection referencia un Candidate válido;
6. todo Contract Type asignado posee Classification Basis trazable;
7. toda derivación identifica conocimiento declarativo gobernado;
8. Confidence está fundamentada y no representa certeza;
9. Uncertainty material permanece explícita;
10. Scope no excede la revisión, archivo y Evidence observados;
11. Detection Sufficiency y Analysis Readiness permanecen separadas;
12. Coverage permite conocer qué Evidence fue evaluada;
13. Unknown y Ambiguous no fueron reemplazados por una clasificación probable;
14. Abstention explica la condición incumplida;
15. Contradiction conserva todas las posiciones respaldadas;
16. el resultado no contiene análisis funcional ni breaking changes;
17. el estado de completitud puede interpretarse sin consultar la fuente;
18. ninguna unidad expresa Recommendation, aprobación, rechazo ni Decision.

Estas garantías describen integridad contractual y epistemológica. No garantizan
que el archivo siga existiendo fuera de la entrada ni que su contenido sea
válido.

## 7. Lo que el contrato nunca garantiza

El contrato nunca garantiza:

- existencia universal o actual del archivo fuera de la Evidence recibida;
- contenido disponible;
- sintaxis válida;
- conformidad con OpenAPI, AsyncAPI, GraphQL u otro formato;
- interpretación de endpoints, operations, messages, schemas o fields;
- que la semántica del contrato haya cambiado;
- que un cambio sea compatible o incompatible;
- existencia de un breaking change;
- ausencia de breaking changes;
- severidad, criticidad, prioridad, impacto o riesgo;
- exhaustividad sobre contratos no presentes en la entrada;
- consumidores, providers o dependencias;
- que Analysis Readiness implique un análisis exitoso;
- que un Detected Contract requiera una acción;
- que una persona deba aceptar la clasificación.

Capability-003 no interpreta OpenAPI, AsyncAPI ni GraphQL. Tampoco interpreta
Avro, Protobuf, JSON Schema, Terraform, Kubernetes CRD, GitHub Actions, Docker
Compose u otros formatos. Sólo puede clasificar un Contract Type cuando Evidence
y conocimiento gobernado autorizan esa afirmación descriptiva.

## 8. Límites

### 8.1 Detección, no análisis

El contrato responde únicamente:

- ¿existe Modified File Evidence para un candidato?
- ¿qué Contract Type puede sostenerse?
- ¿la Evidence alcanza para la detección?
- ¿existe preparación declarada para una capability posterior?

No responde qué cambió dentro del contrato.

### 8.2 Clasificación, no validación del formato

Asignar un Contract Type no prueba que el archivo cumpla ese formato. Un path
clasificado como OpenAPI no fue parseado ni validado por Capability-003.

### 8.3 Sufficiency, no interpretación

Detection Sufficiency declara si la Evidence alcanza para la detección. Analysis
Readiness declara si las condiciones conocidas para entregar el candidato al
siguiente límite están satisfechas. Ninguna analiza contenido.

### 8.4 Hallazgo, no decisión

Un Contract Detection es conocimiento descriptivo. El consumidor conserva la
responsabilidad sobre cualquier análisis, recomendación o decisión posterior.

### 8.5 Procedencia, no acceso

Las referencias a repositorio, revisión, archivo o provider son procedencia. No
autorizan consultar fuentes ni ampliar permisos.

## 9. Unidades conceptuales

El contrato contiene o referencia conceptualmente:

- Execution Identity;
- Modified File Evidence Availability;
- Contract Candidate;
- Contract Detection;
- Contract Type;
- Classification Basis;
- Detection Sufficiency;
- Analysis Readiness;
- Scope;
- Confidence;
- Uncertainty;
- Traceability;
- Coverage;
- Abstention;
- Contradiction;
- Contract Change Report Status.

Estas unidades expresan significado, no campos físicos. El futuro diseño de
representación deberá demostrar que conserva todas las obligaciones sin asumir
que esta lista define un schema.

## 10. Contract Candidate

Un **Contract Candidate** representa un archivo modificado para el cual existe
Evidence suficiente para evaluar si puede ser un Engineering Contract.

Todo Candidate válido garantiza:

- identidad inequívoca dentro de la ejecución;
- referencia a exactamente una Modified File Evidence primaria;
- path, Change Status y provenance preservados;
- Scope explícito;
- señal observable de candidatura;
- Rule o conocimiento gobernado que autoriza considerarlo;
- Confidence y Uncertainty propias;
- estado de evaluación explícito.

Un Candidate no garantiza que el archivo sea un contrato. Tampoco transporta
contenido ni sustituye un Contract Detection.

### 10.1 Estados válidos de Candidate

Un Candidate puede estar conceptualmente:

- **identified:** existe Evidence válida para evaluarlo;
- **classified:** existe un Contract Type respaldado;
- **unknown:** no existe respaldo suficiente para asignar tipo;
- **ambiguous:** más de un tipo incompatible conserva respaldo;
- **outside_coverage:** el tipo o señal no pertenece a la cobertura declarada;
- **abstained:** una condición impidió completar su detección.

Estos estados no son severidades ni resultados de compatibilidad.

### 10.2 Candidate inválido

Un Candidate es inválido cuando:

- no referencia Modified File Evidence;
- referencia Evidence de otra ejecución;
- altera path, Change Status o provenance;
- carece de Scope;
- su identidad es ambigua;
- inventa contenido;
- afirma un tipo sin Classification Basis;
- incorpora breaking change, severidad o Recommendation.

Un Candidate inválido no puede publicarse ni convertirse en Detection.

## 11. Contract Detection

Un **Contract Detection** es un resultado consumible que determina, dentro de un
Scope explícito, que un Candidate representa un Engineering Contract de un tipo
respaldado o conserva de forma explícita una clasificación no resuelta.

Debe conservar:

- Candidate de origen;
- estado de clasificación;
- Contract Type o ausencia explícita;
- Classification Basis;
- Detection Sufficiency;
- Analysis Readiness;
- Confidence;
- Uncertainty;
- Traceability;
- preguntas abiertas;
- límites de aplicabilidad.

### 11.1 Estados válidos de Detection

Una Detection puede estar:

- **detected:** existe un Contract Type respaldado;
- **unknown:** existe Candidate, pero no tipo respaldado;
- **ambiguous:** existen clasificaciones incompatibles sin precedencia;
- **outside_coverage:** el Candidate no puede evaluarse bajo la cobertura
  vigente;
- **abstained:** no puede producirse una clasificación válida.

`unknown`, `ambiguous`, `outside_coverage` y `abstained` son resultados
explícitos, no errores escondidos ni tipos de contrato.

### 11.2 Detection inválida

Una Detection es inválida cuando:

- no posee Candidate válido;
- su Type no está respaldado;
- omite Uncertainty material;
- oculta alternativas incompatibles;
- amplía Scope;
- rompe Traceability;
- presenta Change Status como cambio semántico;
- declara compatibilidad, severidad o breaking change;
- contiene una Recommendation o Decision.

## 12. Contract Type

**Contract Type** es una categoría tecnológica descriptiva. Puede representar,
entre otros tipos gobernados:

- OpenAPI;
- AsyncAPI;
- GraphQL;
- Avro;
- Protobuf;
- JSON Schema;
- Terraform;
- Kubernetes CRD;
- GitHub Actions;
- Docker Compose.

Enumerar un tipo no equivale a soportar su parsing ni su interpretación. El
contrato sólo garantiza que la categoría puede expresarse cuando Classification
Basis y Rules vigentes la respaldan.

Un Contract Type:

- no representa dominio de negocio;
- no contiene versión inferida del formato;
- no implica validez sintáctica;
- no implica compatibilidad;
- no implica criticidad;
- no habilita una acción;
- no puede asignarse por preferencia.

### 12.1 Tipo desconocido

Cuando no existe respaldo suficiente, el resultado es `unknown`; “unknown” no es
un Contract Type.

### 12.2 Tipo ambiguo

Cuando dos o más tipos incompatibles poseen respaldo, se conservan como
alternativas en conflicto; “ambiguous” no es un Contract Type.

### 12.3 Tipo fuera de cobertura

Un tipo no gobernado por la versión vigente se declara `outside_coverage`. No se
clasifica como “no contrato”.

## 13. Classification Basis

**Classification Basis** identifica la Evidence y las condiciones declarativas
que sostienen una candidatura o Contract Type.

Debe permitir conocer:

- qué observación fue utilizada;
- qué relación autorizó la derivación;
- qué versión estaba vigente;
- qué Scope limita la clasificación;
- qué alternativas fueron consideradas;
- qué limitaciones impiden una Confidence mayor.

Classification Basis no contiene contenido inventado ni una explicación
retrospectiva no verificable. Si falta, la clasificación es inválida.

## 14. Detection Sufficiency

**Detection Sufficiency** expresa si la Evidence disponible alcanza para
producir una detección válida dentro del alcance de Capability-003.

### 14.1 Estados válidos

- **sufficient:** la Evidence satisface todas las condiciones de detección;
- **partial:** algunas condiciones se satisfacen, pero otra limita el resultado;
- **insufficient:** producir una detección excedería el respaldo;
- **not_provided:** la Evidence requerida no fue transportada.

### 14.2 Semántica

`sufficient` no significa que el contrato sea válido ni analizable.

`partial` exige identificar qué pudo sostenerse y qué condición falta.

`insufficient` exige abstención para toda afirmación que exceda el respaldo.

`not_provided` no equivale a una colección disponible con cero elementos.

Detection Sufficiency nunca se expresa como porcentaje obligatorio ni se deduce
de la cantidad de archivos.

## 15. Analysis Readiness

**Analysis Readiness** expresa si la Evidence declarada satisface las
precondiciones conocidas para entregar una Detection a una capability posterior.

### 15.1 Estados válidos

- **ready:** todas las precondiciones declaradas están satisfechas;
- **not_ready:** al menos una precondición necesaria no está satisfecha;
- **unknown:** el contrato no permite determinar readiness;
- **not_applicable:** no existe Detection apta para análisis.

### 15.2 Límites

`ready` no garantiza que Capability-004 pueda analizar con éxito. No garantiza
contenido válido, compatibilidad ni ausencia de degradación posterior.

`not_ready` debe conservar la Evidence faltante o condición incumplida cuando
pueda identificarse sin inventar una fuente.

`unknown` es preferible a asumir readiness.

Capability-003 no prescribe cómo recuperar lo faltante ni qué análisis debe
realizar Capability-004.

## 16. Scope

**Scope** delimita dónde una unidad contractual es válida. Debe conservar como
mínimo los límites conceptuales de:

- ejecución;
- entrada contractual;
- revisión o cambio correlacionado;
- repositorio de procedencia;
- path de Modified File Evidence;
- Rule o taxonomía vigente;
- cobertura declarada.

Scope no concede acceso a esos recursos.

Dos unidades sólo pueden combinarse cuando sus Scope son compatibles según la
Reasoning Specification. Compartir extensión o Contract Type no demuestra
compatibilidad de Scope.

Cambiar Scope cambia la identidad conceptual de Candidate o Detection.

## 17. Confidence

Confidence conserva el significado normativo de Capability-002: grado
cualitativo de respaldo, nunca certeza, probabilidad, severidad ni prioridad.

Toda Confidence debe incluir:

- nivel;
- fundamento;
- limitaciones;
- dimensiones relevantes.

La Confidence de Candidate, clasificación, Detection Sufficiency y Analysis
Readiness debe evaluarse por separado cuando corresponda. Una clasificación con
Confidence fuerte no eleva automáticamente Analysis Readiness.

Una señal de path aislada no autoriza Confidence mayor que la demostrada por sus
casos de evaluación. Repetir convenciones equivalentes no aumenta respaldo.

Confidence insuficiente exige abstención o un estado no clasificado; nunca una
categoría probable.

## 18. Uncertainty

Uncertainty expresa todo límite material que debilita o impide una detección.

Puede originarse en:

- `modified_files` no proporcionado;
- Evidence parcial;
- convención de path ambigua;
- extensión compartida por varios Contract Types;
- Contract Type fuera de cobertura;
- ausencia de contenido cuando sería necesario para confirmar tipo;
- Classification Basis indirecta;
- Rules incompatibles;
- Scope no combinable;
- incapacidad de determinar Analysis Readiness.

Debe indicar:

- qué se desconoce o está en conflicto;
- qué Candidate o Detection afecta;
- cómo limita Confidence, Sufficiency, Readiness o Scope;
- qué Evidence reduciría el límite, cuando pueda nombrarse;
- qué pregunta permanece abierta.

Uncertainty no es una advertencia genérica y no desaparece por tener una
Detection válida.

## 19. Traceability

Toda unidad publicada debe permitir reconstruir:

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

La representación futura puede evitar duplicar unidades ya presentes en un
Inference Report, pero nunca puede ocultar ni romper esta cadena.

Traceability debe ser:

- completa;
- inequívoca;
- cerrada sobre la entrada;
- navegable;
- estable;
- explicable;
- auditable.

Cada transición identifica la Rule o conocimiento gobernado que la autorizó. Una
referencia rota invalida la unidad dependiente. El consumidor no puede repararla
consultando la fuente.

## 20. Coverage

Coverage expresa qué parte de Modified File Evidence fue considerada por la
ejecución.

Debe permitir distinguir:

- Evidence total disponible;
- Evidence evaluada;
- Candidates identificados;
- Detections válidas;
- Candidates Unknown o Ambiguous;
- Evidence fuera de cobertura;
- Evidence no procesada y su causa;
- alcance afectado por Abstention.

### 20.1 Cobertura completa

Coverage es completa cuando toda Modified File Evidence disponible fue evaluada
bajo las Rules vigentes y cada resultado quedó representado como Detection,
Unknown, Ambiguous, outside_coverage o Abstention.

Cobertura completa puede contener cero Candidates o cero Detections.

### 20.2 Cobertura parcial

Coverage es parcial cuando una limitación conocida impide evaluar parte de la
Evidence, pero todas las unidades publicadas continúan siendo válidas.

### 20.3 Cobertura desconocida

Cuando `modified_files` es `not_provided`, Coverage es desconocida para
Capability-003. No puede representarse como cero.

Coverage no significa exhaustividad sobre archivos o contratos fuera de la
entrada.

## 21. Abstention

Abstention representa que Capability-003 no puede producir una unidad válida sin
exceder la Evidence o violar el contrato.

Puede ser:

- **local:** afecta un Candidate o clasificación;
- **partial:** afecta parte de Coverage;
- **total:** impide producir Detections válidas para la ejecución.

Toda Abstention debe conservar:

- identidad;
- unidad que no pudo producirse;
- condición incumplida;
- Evidence disponible;
- Evidence faltante, cuando pueda identificarse;
- Scope afectado y Scope restante;
- Confidence y Uncertainty relacionadas;
- pregunta abierta.

`modified_files: not_provided` exige Abstention total para Capability-003.

Abstention no es una Detection negativa. “No puede detectarse” no significa “no
existe contrato”.

## 22. Contradiction

Contradiction corresponde a este contrato cuando Evidence o clasificaciones
respaldadas sostienen posiciones materialmente incompatibles dentro del mismo
Scope.

Ejemplos conceptuales:

- dos Contract Types incompatibles respaldados para el mismo Candidate;
- dos Rules vigentes que asignan tipos distintos sin precedencia;
- una condición que declara Coverage completa mientras existe Evidence no
  procesada;
- estados de Sufficiency incompatibles sobre la misma Evidence y Rule.

El contrato debe:

- conservar todas las posiciones respaldadas;
- identificar Evidence y Rules de cada posición;
- mostrar qué Candidates, Detections y estados resultan afectados;
- reducir Confidence cuando corresponda;
- expresar Uncertainty;
- abstenerse de elegir precedencia sin respaldo.

Contradiction no corresponde cuando sólo falta Evidence. Ausencia e
incompatibilidad son límites diferentes.

Una Contradiction no invalida automáticamente todo el reporte. Invalida o limita
las unidades que dependen de resolverla; resultados independientes permanecen
válidos.

## 23. Estados del reporte

### 23.1 Complete

Un reporte es **complete** cuando:

- Modified File Evidence fue proporcionada;
- toda Evidence disponible fue evaluada;
- Coverage es completa;
- todas las unidades publicadas cumplen sus invariantes;
- Traceability está íntegra;
- Confidence y Uncertainty están expresadas;
- Unknown, Ambiguous, outside_coverage y Abstentions están declarados;
- ninguna degradación impidió cubrir el alcance previsto.

Un reporte complete puede contener cero Detections. También puede contener
Uncertainty o Contradictions preservadas si no rompen integridad ni Coverage.

### 23.2 Incomplete

Un reporte es **incomplete pero válido** cuando:

- existe una limitación conocida sobre parte del alcance;
- las unidades publicadas conservan integridad;
- Coverage parcial está declarada;
- la causa y el Scope afectado son explícitos;
- no se presenta como completa ninguna evaluación omitida.

### 23.3 Reporte válido con Abstention total

Abstention no es un estado de reporte separado. Un reporte complete o incomplete
puede contener Abstention total cuando no puede producir Detections válidas sin
violar el contrato, pero explica de forma trazable la condición que lo impide.

La ausencia de Detections no convierte el reporte en invalid. La completitud se
determina por Coverage e integridad, no por cantidad de resultados positivos.

### 23.4 Invalid

Un resultado es **invalid** cuando viola una garantía esencial, por ejemplo:

- no identifica entrada o ejecución;
- mezcla ejecuciones;
- contiene referencias rotas;
- publica Contract Type sin Evidence;
- oculta Uncertainty material;
- presenta ausencia como colección vacía;
- declara Coverage falsa;
- incorpora análisis funcional, breaking change o severidad;
- incluye Recommendation o Decision;
- atribuye al productor autoridad que no posee.

Un resultado invalid no puede publicarse como Contract Change Report.

## 24. Criterios de completitud

La completitud se evalúa respecto de entrada, Rules, taxonomía y Scope
declarados. No significa exhaustividad universal.

Para ser complete, el reporte debe permitir responder:

1. ¿Fue proporcionada Modified File Evidence?
2. ¿Qué Evidence fue elegible?
3. ¿Qué Evidence fue evaluada?
4. ¿Qué Candidates nacieron?
5. ¿Qué Contract Types pudieron sostenerse?
6. ¿Qué casos quedaron Unknown, Ambiguous o outside_coverage?
7. ¿Qué Detection Sufficiency posee cada resultado?
8. ¿Qué Analysis Readiness posee cada resultado?
9. ¿Qué Uncertainty, Contradictions y Abstentions permanecen?
10. ¿Puede recorrerse cada Detection hasta Evidence?

No se exige producir al menos una Detection.

## 25. Criterios de invalidez

El reporte o una unidad dependiente es inválido cuando:

- falta una identidad requerida;
- una referencia apunta fuera de la entrada;
- Candidate no posee Evidence;
- Detection no posee Candidate;
- Contract Type no posee Classification Basis;
- Sufficiency o Readiness contradice Evidence observable;
- Scope fue ampliado;
- Confidence carece de fundamento;
- Uncertainty material fue omitida;
- Contradiction fue ocultada o resuelta sin respaldo;
- Coverage no puede reconciliarse con las unidades;
- una Abstention se expresa como negación;
- una inferencia se presenta como Evidence;
- se afirma semántica interna de un contrato;
- se afirma breaking change, compatibilidad, severidad o acción;
- el resultado contiene datos de ejecuciones incompatibles.

Una unidad inválida no se degrada silenciosamente a válida. Debe descartarse o
producir Abstention cuando la limitación misma esté respaldada.

## 26. Contenido excluido

El contrato nunca contiene:

- contenido funcional interpretado;
- diferencias semánticas;
- breaking changes;
- compatibilidad backward o forward;
- severidad, prioridad, criticidad, riesgo o impacto;
- consumidores o dependencias inferidos;
- Recommendations;
- Decisions;
- aprobaciones o rechazos;
- acciones propuestas o ejecutadas;
- comentarios humanos;
- instrucciones de presentación;
- Markdown o UI;
- código generado;
- cambios sobre Evidence;
- feedback convertido en verdad;
- credenciales o autorización para consultar fuentes.

Las preguntas abiertas y faltantes explícitos sí pertenecen al contrato cuando
explican Sufficiency, Readiness, Uncertainty o Abstention.

## 27. Responsabilidades del productor

Capability-003 debe:

- aceptar únicamente Evidence admitida por límites vigentes;
- preservar disponibilidad, identidad y orden contractual;
- no acceder a fuentes para completar contexto;
- producir Candidates y Detections sólo con respaldo;
- aplicar taxonomía y Rules gobernadas;
- distinguir Unknown, Ambiguous y outside_coverage;
- mantener Detection Sufficiency separada de Analysis Readiness;
- preservar Scope, Confidence, Uncertainty y Traceability;
- declarar Coverage real;
- conservar Contradictions;
- producir Abstention cuando corresponda;
- validar integridad antes de publicar;
- mantener el contrato neutral respecto del consumidor;
- no interpretar formatos ni contenido;
- no producir breaking changes, severidad, Recommendations ni Decisions.

El productor responde por la integridad de detección, no por análisis o
decisiones posteriores.

## 28. Responsabilidades del consumidor

Capability-004 y todo consumidor futuro deben:

- verificar que soportan la versión conceptual recibida;
- preservar identidad, estado, Scope y Coverage;
- conservar Candidate, Type, Sufficiency y Readiness sin reinterpretarlos;
- mantener Traceability hasta Evidence;
- no presentar Confidence como certeza;
- no ocultar Uncertainty, Contradictions ni Abstentions;
- no presentar Unknown como “no contrato”;
- no presentar `not_provided` como cero archivos;
- no presentar `ready` como análisis exitoso;
- no atribuir breaking changes o severidad a Capability-003;
- distinguir todo análisis propio del resultado contractual;
- no consultar fuentes usando provenance como autoridad implícita;
- conservar responsabilidad humana sobre decisiones.

Un consumidor puede analizar contratos bajo un contrato posterior. Ese análisis
no modifica Contract Change Report ni se convierte retroactivamente en Evidence
de su ejecución original.

## 29. Compatibilidad

La compatibilidad se evalúa por significado observable, no por representación
física.

### 29.1 Cambios backward compatible

Un cambio puede ser compatible cuando:

- aclara una definición sin alterar garantías;
- agrega información conceptual opcional que un consumidor puede ignorar sin
  perder integridad;
- agrega un Contract Type sin redefinir tipos existentes;
- agrega una Uncertainty o pregunta opcional preservando estados;
- amplía ejemplos o Evaluation;
- fortalece validación de resultados que ya eran inválidos;
- agrega una relación opcional sin romper Traceability mínima.

Un cambio aditivo no es automáticamente compatible. Si altera completitud,
autoridad, Scope, Sufficiency, Readiness o interpretación de estados, es
incompatible.

### 29.2 Productor anterior y consumidor nuevo

Un consumidor nuevo debe admitir únicamente versiones que pueda interpretar. No
puede inventar unidades ausentes de un productor anterior.

### 29.3 Productor nuevo y consumidor anterior

Un productor nuevo debe preservar todas las garantías de la versión soportada.
La información opcional nueva debe ser ignorable sin cambiar el significado de
las unidades existentes.

### 29.4 Coexistencia

Una evolución incompatible debe permitir coexistencia o migración explícita. No
puede reemplazar silenciosamente una versión utilizada por Capability-004.

## 30. Cambios incompatibles

Son incompatibles, entre otros:

- cambiar productor único;
- eliminar a Capability-004 como consumidor declarado sin transición;
- redefinir Candidate, Detection o Contract Type;
- convertir Unknown en ausencia de contrato;
- convertir Ambiguous en selección automática;
- hacer equivalentes Detection Sufficiency y Analysis Readiness;
- presentar `not_provided` como colección vacía;
- cambiar el significado de complete, incomplete o invalid;
- permitir Contract Type sin Classification Basis;
- romper la cadena hasta Modified File Evidence;
- eliminar Scope, Confidence, Uncertainty o Coverage;
- ocultar Contradictions;
- permitir parsing o interpretación funcional dentro del contrato;
- incorporar breaking changes, compatibilidad o severidad;
- permitir Recommendations, Decisions, aprobación o rechazo;
- trasladar al consumidor validaciones que corresponden al productor;
- permitir acceso a fuentes como responsabilidad del consumidor;
- mezclar resultados de entradas sin identidad independiente.

Todo cambio incompatible requiere una nueva versión conceptual, estrategia de
migración y ADR.

## 31. Versionado

La versión conceptual inicial Accepted es **1**.

La versión identifica el significado normativo del contrato, no un campo ni un
formato físico. Este documento no decide cómo se representa o transporta la
versión.

Una versión debe permitir identificar:

- significado de las unidades;
- estados válidos;
- garantías y límites;
- reglas de completitud e invalidez;
- obligaciones de productor y consumidor;
- compatibilidad con versiones anteriores.

Una versión Accepted permanece estable durante su lifecycle. Una versión
incompatible posterior debe coexistir durante una transición o incluir una
migración explícita.

## 32. Gobernanza

Capability Owner de Capability-003 responde por conformidad del productor y
taxonomía de Contract Types. Architecture Owner responde por coherencia del
límite. Capability-004 y consumidores futuros responden por no ampliar sus
garantías.

Toda evolución debe documentar:

- necesidad demostrada;
- impacto sobre productor y consumidores;
- efecto sobre Traceability, Sufficiency, Readiness y Coverage;
- cambio de autoridad o acceso;
- compatibilidad;
- estrategia de Evaluation;
- migración, cuando corresponda;
- riesgo de confundir detección con análisis.

Las Rules no modifican automáticamente este contrato. Una necesidad específica
de un Contract Type debe resolverse primero mediante conocimiento gobernado y
elevarse al contrato sólo si cambia una garantía transversal.

**Todo cambio incompatible requiere ADR.**

## 33. Evolución

La evolución esperada sigue esta secuencia:

```text
Capability-003 Accepted
          ↓
Contract Change Detection Contract Accepted
          ↓
Reasoning Specification Accepted
          ↓
TD-004 Accepted
          ↓
implementación incremental
          ↓
evaluación y promoción gobernada
```

Este contrato no autoriza por sí mismo ninguna etapa posterior.

Agregar contenido contractual, base/head, diff u otra Evidence exige primero
evolucionar el límite de entrada correspondiente. Agregar breaking change,
severidad o Recommendation exige una capability y contrato distintos.

Capability-004 se diseñará después de estabilizar este límite. Su futura
definición no puede modificar retroactivamente las garantías de Capability-003.

## 34. Estabilidad

Son estables en la versión conceptual 1:

- productor único;
- Capability-004 como consumidor inicial;
- detección separada de análisis;
- Candidate y Detection como unidades distintas;
- Contract Type descriptivo;
- Detection Sufficiency separada de Analysis Readiness;
- Scope no expansivo;
- Confidence fundamentada;
- Uncertainty explícita;
- Traceability hasta Modified File Evidence;
- Coverage real;
- Unknown y Ambiguous preservados;
- Contradiction visible;
- Abstention legítima;
- ausencia de breaking changes, severidad, Recommendations y Decisions.

No son interfaces estables:

- nombres de clases o módulos;
- nombres o posiciones de campos;
- JSON o schema;
- serialización;
- transporte;
- persistencia;
- APIs;
- herramientas;
- runtime;
- almacenamiento;
- presentación.

## 35. Criterio de aceptación

El contrato puede promoverse a **Accepted** cuando:

- Capability-003 está Accepted o su promoción forma parte de la misma revisión
  gobernada;
- Capability Owner y Architecture Owner aceptan productor, consumidor y límites;
- Capability-004 puede diseñarse sin acceder a fuentes ni reconstruir
  detecciones;
- Candidate, Detection, Type, Sufficiency y Readiness no son ambiguos;
- complete, incomplete e invalid pueden distinguirse, y una Abstention total no
  se confunde con invalidez;
- Unknown, Ambiguous y outside_coverage poseen semántica verificable;
- Traceability y Coverage pueden reconstruirse;
- Contradiction y Abstention conservan la normativa de Capability-002;
- no existe análisis funcional, breaking change, severidad ni Recommendation;
- compatibilidad, versionado y gobernanza son aplicables;
- las decisiones físicas permanecen postergadas.

Una implementación, schema o demo no basta para aceptar el contrato.

## 36. Decisiones postergadas

Este contrato no define:

- `contract-change-report.json`;
- JSON Schema ni otro schema;
- campos físicos;
- serialización;
- transporte;
- persistencia;
- API;
- clases o interfaces;
- Rules concretas;
- taxonomía física;
- representación de IDs;
- orden físico de unidades;
- tecnología de implementación;
- parsing de formatos;
- recuperación de contenido;
- comparación de versiones;
- breaking change detection;
- severidad, riesgo o Recommendations;
- dominio, propósito o nombre de Capability-004;
- integración con VS-001;
- hosting, cache, base de datos o infraestructura compartida.

Estas decisiones requieren documentos posteriores y no quedan autorizadas por su
mención.

## 37. Historial

| Fecha      | Cambio                                                  | Estado   |
| ---------- | ------------------------------------------------------- | -------- |
| 2026-08-04 | Propuesta inicial de Contract Change Detection Contract | Proposed |
| 2026-08-05 | Promoción formal coordinada                             | Accepted |
