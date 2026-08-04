# VS-001 — Architecture Review Intelligence

**Estado:** Accepted  
**Tipo:** Especificación funcional de vertical slice  
**Producto:** Engineering Intelligence Platform (EIP)  
**Owner:** Engineering Platform  
**Audiencia:** CTO, líderes de Ingeniería, arquitectos, Tech Leads, Engineering Managers, reviewers y equipos de plataforma  
**Última actualización:** 3 de agosto de 2026

---

## ¿Por qué VS-001 primero?

Las revisiones de cambios son uno de los puntos donde la falta de contexto de ingeniería se vuelve más visible, frecuente y costosa. Un reviewer debe comprender en poco tiempo qué cambia, por qué cambia, qué decisiones previas condicionan la solución, qué partes del sistema podrían verse afectadas y qué evidencia respalda una observación. Esa información suele existir, pero está distribuida entre el cambio, el repositorio, su documentación, las decisiones de arquitectura y el historial disponible.

VS-001 se elige como primer vertical slice porque reúne condiciones favorables para validar la tesis central de EIP:

- ocurre dentro de un flujo de ingeniería real y recurrente;
- tiene un usuario, una necesidad y un momento de uso concretos;
- permite medir utilidad, calidad y ahorro de tiempo;
- puede operar exclusivamente en modo de lectura;
- utiliza evidencia disponible en una fuente Git autorizada;
- permite que una persona verifique cada afirmación antes de actuar;
- limita el riesgo porque no aprueba, modifica ni ejecuta cambios;
- obliga a resolver el problema esencial de EIP: recuperar contexto mínimo, relevante, vigente y citable para una decisión específica.

Elegir este slice no implica que la revisión de arquitectura sea el único ni el principal destino de EIP. Es el primer entorno controlado en el que puede demostrarse que relacionar evidencia dispersa mejora una decisión de ingeniería. El aprendizaje buscado no es sólo si se puede producir un resumen, sino si el contexto entregado ayuda a un reviewer a detectar riesgos, formular mejores preguntas y justificar sus conclusiones sin aumentar innecesariamente la carga de revisión.

VS-001 precede a cualquier inversión en capacidades compartidas o infraestructura general. Su aceptación depende del outcome observado para reviewers y autores, no de la cantidad de tecnología construida.

---

## 1. Propósito del documento

Este documento define la especificación funcional canónica de **VS-001 — Architecture Review Intelligence**.

Establece el problema, los usuarios, los casos de uso, las entradas, las salidas, la información que debe recuperarse, las fuentes admitidas, los límites, las métricas y la Definition of Done del primer vertical slice de EIP.

El documento describe **qué valor debe entregar el slice y bajo qué condiciones se considera válido**. No prescribe su implementación ni define una arquitectura técnica.

---

## 2. Resumen ejecutivo

VS-001 asiste a una persona que revisa un cambio de software proporcionando un brief de arquitectura basado en evidencia del repositorio autorizado.

El brief permite comprender rápidamente:

- el propósito y alcance aparente del cambio;
- los componentes, contratos y dependencias potencialmente afectados;
- las decisiones, restricciones y estándares relevantes;
- los riesgos arquitectónicos que merecen atención;
- las preguntas que el reviewer debería resolver antes de aprobar;
- la evidencia que respalda cada hecho o inferencia;
- los vacíos, contradicciones e incertidumbres del contexto disponible.

El slice es asistivo y de sólo lectura. No reemplaza al reviewer, no emite una aprobación, no modifica el cambio y no presenta inferencias como hechos. La salida debe poder verificarse contra las fuentes originales.

---

## 3. Problema

### 3.1 Situación actual

Durante una revisión, comprender el impacto arquitectónico de un cambio requiere reconstruir contexto que rara vez está contenido por completo en su descripción o diff. El reviewer alterna entre archivos, documentación, decisiones, configuraciones e historial; busca nombres y conceptos; identifica relaciones; y trata de determinar qué información sigue vigente.

Este trabajo presenta varias dificultades:

- el contexto relevante está fragmentado;
- la descripción del cambio puede ser incompleta o asumir conocimiento tácito;
- el diff muestra qué se modificó, pero no necesariamente por qué ni qué compromisos previos aplican;
- la documentación y las decisiones pueden estar alejadas del código afectado;
- los nombres técnicos no siempre coinciden entre código y documentación;
- la relevancia de una fuente depende del cambio concreto;
- el reviewer puede desconocer antecedentes que el autor considera obvios;
- bajo presión de tiempo, la búsqueda se reduce y aumenta la dependencia de memoria o intuición;
- una observación correcta puede ser difícil de justificar si no conserva la evidencia que la originó.

### 3.2 Consecuencias

La reconstrucción manual de contexto produce uno o más de estos resultados:

- revisiones lentas o bloqueadas por preguntas básicas;
- riesgos arquitectónicos detectados tarde;
- repetición de debates ya resueltos;
- incumplimiento involuntario de decisiones o estándares vigentes;
- exceso de dependencia en reviewers con conocimiento histórico;
- diferencias de calidad entre equipos y personas;
- aprobaciones basadas en contexto parcial;
- comentarios difíciles de verificar o explicar;
- mayor carga cognitiva tanto para reviewers como para autores.

### 3.3 Problema a resolver

> Un reviewer necesita comprender el contexto arquitectónico relevante de un cambio sin localizar, reconciliar y relacionar manualmente toda la evidencia distribuida en el repositorio.

VS-001 debe reducir ese esfuerzo entregando un brief verificable y específico para el cambio, sin sustituir el criterio humano.

### 3.4 Hipótesis de producto

Si a un reviewer se le presenta, dentro de su flujo de revisión, un conjunto breve de contexto arquitectónico relevante, con riesgos, preguntas y citas verificables, entonces podrá alcanzar una comprensión útil del cambio más rápido y realizar una revisión mejor fundamentada que con el flujo actual.

---

## 4. Outcome esperado

El outcome primario es:

> **Reducir el tiempo y el esfuerzo necesarios para realizar una revisión arquitectónica informada, manteniendo o mejorando la capacidad de detectar asuntos relevantes.**

Outcomes secundarios:

- aumentar la proporción de observaciones respaldadas por evidencia;
- hacer visibles decisiones y restricciones que de otro modo se omitirían;
- identificar explícitamente cuándo la evidencia no alcanza para una conclusión;
- reducir preguntas de descubrimiento que podrían responderse con información ya disponible;
- facilitar una conversación más precisa entre autor y reviewer;
- generar aprendizaje medible sobre qué contexto resulta útil en una revisión real.

El éxito no se define por producir texto convincente ni por maximizar la cantidad de información recuperada. Se define por mejorar la tarea del reviewer con contexto correcto, relevante, trazable y manejable.

---

## 5. Usuario objetivo

### 5.1 Usuario primario

El usuario primario es la persona responsable de revisar las implicancias arquitectónicas de un cambio de software. Según la organización, puede ser:

- arquitecto o referente de arquitectura;
- Tech Lead;
- Staff o Principal Engineer;
- senior engineer con responsabilidad de revisión;
- owner técnico de un servicio, componente o dominio;
- integrante de un grupo de revisión de arquitectura.

El usuario conoce prácticas de ingeniería y puede evaluar evidencia técnica, pero no necesariamente posee todo el conocimiento histórico o local del área modificada.

### 5.2 Usuario secundario

El autor del cambio puede usar el brief antes o durante la revisión para:

- comprobar si documentó adecuadamente el propósito y el impacto;
- anticipar preguntas del reviewer;
- localizar decisiones o restricciones relevantes;
- reconocer vacíos de evidencia;
- mejorar la explicación del cambio.

El slice se optimiza para la decisión del reviewer. La utilidad para el autor es complementaria y no debe alterar ese foco.

### 5.3 Necesidades del usuario

El reviewer necesita:

- orientarse rápidamente sin leer material indiscriminado;
- separar hechos observables de interpretaciones;
- entender el alcance declarado y el alcance aparente;
- conocer decisiones y restricciones aplicables;
- identificar potenciales impactos y riesgos;
- formular preguntas proporcionadas a la evidencia;
- abrir la fuente original de cada afirmación relevante;
- reconocer información faltante, contradictoria o posiblemente obsoleta;
- conservar la decisión final bajo su control.

---

## 6. Momento y contexto de uso

VS-001 se utiliza cuando existe un cambio identificable y disponible para revisión en una fuente Git autorizada. El momento principal es antes de que el reviewer complete su evaluación.

Puede consultarse:

- al iniciar la revisión, para obtener orientación;
- durante la revisión, para profundizar sobre un aspecto del cambio;
- después de leer el diff, para contrastar una interpretación;
- por el autor, antes de solicitar o responder una revisión.

La experiencia debe convivir con el flujo habitual de revisión. No debe exigir que el usuario migre documentación, replique fuentes ni abandone el lugar donde toma la decisión.

---

## 7. Alcance funcional

VS-001 recibe la referencia de un cambio y el propósito de revisión, recupera evidencia autorizada relacionada y entrega un brief de arquitectura citable.

El slice debe:

1. identificar el cambio y su alcance observable;
2. interpretar su descripción sin asumir que es completa o correcta;
3. localizar artefactos del repositorio relacionados con lo modificado;
4. recuperar decisiones, restricciones, estándares y documentación relevantes;
5. relacionar esa evidencia con el cambio concreto;
6. presentar hechos, inferencias y preguntas como categorías distintas;
7. señalar riesgos potenciales sin convertirlos en veredictos;
8. citar la procedencia de las afirmaciones materiales;
9. declarar faltantes, contradicciones y límites del análisis;
10. mantener al reviewer como responsable de la conclusión.

---

## 8. Casos de uso

### UC-01 — Obtener orientación inicial

**Actor:** reviewer.  
**Disparador:** comienza la revisión de un cambio.  
**Necesidad:** comprender rápidamente de qué trata y dónde concentrar la atención.  
**Resultado esperado:** un resumen breve del propósito, alcance observable, áreas afectadas, decisiones relevantes y principales preguntas abiertas.

El resultado debe ayudar a priorizar la lectura; no reemplazarla.

### UC-02 — Identificar implicancias arquitectónicas

**Actor:** reviewer.  
**Disparador:** necesita determinar si el cambio afecta límites, responsabilidades, dependencias o contratos.  
**Necesidad:** relacionar modificaciones concretas con la arquitectura documentada.  
**Resultado esperado:** una lista priorizada de implicancias potenciales, cada una explicada y respaldada por evidencia o marcada explícitamente como inferencia.

### UC-03 — Verificar alineación con decisiones y restricciones

**Actor:** reviewer o autor.  
**Disparador:** el cambio parece tocar una materia regulada por una decisión, convención o estándar.  
**Necesidad:** conocer qué disposiciones vigentes podrían aplicar.  
**Resultado esperado:** decisiones y restricciones relevantes, su relación con el cambio y citas a sus fuentes. Si su vigencia no puede determinarse, debe decirse.

### UC-04 — Detectar riesgos que merecen revisión humana

**Actor:** reviewer.  
**Disparador:** evalúa las consecuencias del cambio.  
**Necesidad:** evitar omisiones causadas por contexto fragmentado.  
**Resultado esperado:** riesgos potenciales priorizados por relevancia, con razonamiento breve, evidencia disponible y una pregunta o verificación sugerida.

Un riesgo señalado no equivale a un defecto confirmado.

### UC-05 — Investigar una afirmación

**Actor:** reviewer.  
**Disparador:** desea verificar un hecho, inferencia o riesgo incluido en el brief.  
**Necesidad:** acceder a la evidencia original sin repetir la búsqueda.  
**Resultado esperado:** referencia precisa al artefacto, versión y ubicación relevantes, con contexto suficiente para entender por qué fue citado.

### UC-06 — Reconocer vacíos y contradicciones

**Actor:** reviewer o autor.  
**Disparador:** las fuentes no permiten establecer una conclusión confiable.  
**Necesidad:** evitar que la ausencia o inconsistencia se oculte detrás de una respuesta plausible.  
**Resultado esperado:** declaración explícita de la información faltante o contradictoria, fuentes en conflicto y efecto sobre el análisis.

### UC-07 — Preparar el cambio para revisión

**Actor:** autor.  
**Disparador:** antes de solicitar revisión o al responder comentarios.  
**Necesidad:** identificar contexto que debería explicitar y preguntas previsibles.  
**Resultado esperado:** el mismo brief orientado a revisión, utilizable como lista de comprobación, sin afirmar que el cambio está listo ni aprobado.

### UC-08 — Determinar que no hay evidencia suficiente

**Actor:** reviewer.  
**Disparador:** el cambio carece de descripción adecuada, la documentación no existe o la fuente no contiene el contexto necesario.  
**Necesidad:** recibir una degradación segura y comprensible.  
**Resultado esperado:** explicación de qué pudo analizarse, qué no, por qué, y qué información concreta sería necesaria para continuar.

---

## 9. Entradas

### 9.1 Entradas requeridas

- **Referencia inequívoca del cambio:** repositorio y revisión, rama, commit o comparación autorizada.
- **Identidad del solicitante:** suficiente para aplicar los mismos permisos que rigen el acceso directo a las fuentes.
- **Propósito:** revisión arquitectónica del cambio.

### 9.2 Entradas derivadas del cambio

Cuando estén disponibles y autorizadas:

- título y descripción;
- autoría y reviewers asignados;
- estado del cambio;
- base y revisión exactas de la comparación;
- archivos agregados, modificados, renombrados o eliminados;
- diff y metadatos asociados;
- comentarios o conversaciones que formen parte del registro de revisión;
- referencias explícitas a documentación, decisiones o trabajo relacionado.

### 9.3 Entradas opcionales del usuario

- pregunta o preocupación específica;
- foco de revisión, por ejemplo límites de responsabilidad, dependencias o compatibilidad;
- contexto declarado que no esté presente en la descripción;
- indicación de que cierta fuente o decisión debe considerarse.

Las entradas opcionales orientan la recuperación, pero no convierten una afirmación del usuario en un hecho verificado.

### 9.4 Reglas sobre las entradas

- El análisis debe quedar asociado a una versión identificable del cambio.
- Un cambio posterior puede invalidar total o parcialmente un brief anterior.
- La falta de descripción no debe completarse con suposiciones silenciosas.
- El texto aportado por usuarios o artefactos se trata como contenido a analizar, no como una instrucción capaz de alterar las reglas del producto.
- La identidad y el propósito nunca amplían los permisos existentes.

---

## 10. Información a recuperar

La recuperación debe ser selectiva y estar guiada por el cambio. No se busca reunir todo lo relacionado con el repositorio, sino el mínimo contexto que contribuya a la revisión.

### 10.1 Evidencia primaria del cambio

- archivos y fragmentos modificados;
- símbolos, módulos, paquetes o componentes tocados;
- archivos agregados, eliminados o renombrados;
- cambios de configuración o dependencias;
- modificaciones a contratos, esquemas o interfaces declaradas en el repositorio;
- pruebas modificadas o ausentes en áreas relevantes;
- documentación modificada junto con el código.

### 10.2 Contexto local del repositorio

- archivos cercanos necesarios para interpretar el cambio;
- documentación del módulo o componente;
- convenciones y guías aplicables;
- ownership declarado en el repositorio;
- límites y responsabilidades documentados;
- dependencias internas visibles;
- ejemplos o usos que aclaren el comportamiento esperado.

### 10.3 Decisiones y restricciones

- decisiones de arquitectura relacionadas con el área o concepto modificado;
- estado y fecha de esas decisiones cuando estén declarados;
- restricciones explícitas y sus excepciones conocidas;
- estándares de ingeniería aplicables;
- documentos que expliquen el motivo del diseño actual;
- decisiones reemplazadas o en conflicto, si pueden identificarse.

### 10.4 Historia relevante

Sólo cuando aporte evidencia material para comprender el cambio:

- cambios previos sobre el mismo artefacto;
- motivo documentado de una línea o sección;
- revisiones anteriores directamente relacionadas;
- evolución de una decisión o restricción.

La historia no debe recuperarse por defecto si sólo añade volumen.

### 10.5 Relaciones a establecer

El slice debe intentar relacionar:

- archivo modificado con componente o responsabilidad;
- cambio con documentación relevante;
- cambio con decisión o restricción aplicable;
- contrato modificado con consumidores o usos visibles;
- dependencia alterada con el área que la utiliza;
- riesgo potencial con evidencia concreta;
- afirmaciones contradictorias entre fuentes.

Una relación inferida debe etiquetarse como tal y expresar el fundamento disponible.

### 10.6 Criterios de selección

La información se prioriza según:

- relación directa con elementos modificados;
- capacidad de alterar la evaluación del reviewer;
- autoridad de la fuente;
- vigencia observable;
- especificidad para el cambio;
- proximidad al artefacto afectado;
- utilidad para verificar una afirmación o resolver una pregunta.

Más resultados no implican mejor contexto. La salida debe evitar material tangencial, duplicado o meramente coincidente por palabras.

---

## 11. Fuentes

### 11.1 Fuente autorizada para VS-001

La fuente inicial es un repositorio Git autorizado y los artefactos de revisión asociados al cambio. El repositorio conserva la autoridad sobre su contenido; VS-001 no lo reemplaza.

### 11.2 Tipos de artefactos admitidos

Dentro de la fuente autorizada pueden considerarse:

- código;
- archivos de configuración;
- manifiestos y declaraciones de dependencias;
- pruebas;
- documentación versionada;
- diagramas y especificaciones textuales almacenadas en el repositorio;
- registros de decisiones de arquitectura;
- guías, estándares y convenciones;
- archivos de ownership;
- historial de commits;
- metadatos, descripción y conversación del cambio.

### 11.3 Autoridad y precedencia

VS-001 no debe inventar una jerarquía universal entre fuentes. Cuando el repositorio declare autoridad, estado, vigencia o reemplazo, esa información debe conservarse. Cuando dos artefactos se contradigan y no exista una precedencia explícita, la contradicción debe mostrarse.

La proximidad al código no convierte automáticamente un artefacto en más autoritativo, ni una documentación formal garantiza que esté vigente. La salida debe comunicar las señales observables y evitar resolver silenciosamente los conflictos.

### 11.4 Procedencia mínima

Cada evidencia citada debe conservar, en la medida disponible:

- repositorio;
- artefacto y ubicación precisa;
- versión del contenido analizado;
- tipo de artefacto;
- fecha o señal de actualización relevante;
- relación con la afirmación que respalda.

### 11.5 Acceso y sensibilidad

Sólo puede recuperarse información que el solicitante esté autorizado a consultar. La salida no debe revelar, confirmar ni resumir contenido fuera de esos permisos. La falta de acceso debe tratarse como una limitación, sin exponer la existencia o naturaleza de material restringido más allá de lo permitido.

---

## 12. Salidas

### 12.1 Salida principal: Architecture Review Brief

La salida principal es un brief compacto, navegable y verificable compuesto por las siguientes secciones.

#### A. Identificación del análisis

- cambio analizado;
- versión o revisión exacta;
- fecha del análisis;
- alcance considerado;
- advertencia si el cambio avanzó desde el análisis.

#### B. Resumen del cambio

- propósito declarado;
- propósito y alcance observables;
- áreas principales afectadas;
- divergencias relevantes entre lo declarado y lo observable.

#### C. Contexto arquitectónico relevante

- componentes y responsabilidades relacionadas;
- decisiones, restricciones y estándares aplicables;
- dependencias o contratos que merecen atención;
- explicación breve de por qué cada elemento es relevante.

#### D. Implicancias y riesgos potenciales

Lista priorizada de asuntos que requieren criterio humano. Cada elemento debe incluir:

- enunciado claro;
- clasificación como hecho o inferencia;
- evidencia a favor;
- incertidumbre o evidencia faltante;
- posible consecuencia;
- pregunta o verificación sugerida.

#### E. Preguntas para la revisión

Preguntas concretas, no genéricas, derivadas del cambio y de la evidencia. Deben ayudar a decidir o solicitar información, no simular una decisión.

#### F. Vacíos y contradicciones

- información necesaria que no se encontró;
- fuentes que discrepan;
- contenido cuya vigencia es incierta;
- áreas que no pudieron analizarse;
- efecto de cada limitación sobre la confianza del brief.

#### G. Evidencia

Referencias navegables y suficientemente precisas para verificar las afirmaciones materiales en la fuente original.

### 12.2 Categorías epistemológicas

La salida debe distinguir visual y semánticamente:

- **Hecho:** afirmación observada directamente en una fuente citada.
- **Inferencia:** interpretación razonada a partir de uno o más hechos.
- **Riesgo potencial:** consecuencia posible que requiere evaluación humana.
- **Pregunta:** asunto que el reviewer o el autor debe resolver.
- **Ausencia:** información buscada pero no encontrada o no verificable.
- **Contradicción:** fuentes relevantes que sostienen afirmaciones incompatibles.

Una recomendación, si aparece, debe presentarse como orientación para el reviewer y no como una orden ni una decisión tomada.

### 12.3 Características de calidad de la salida

El brief debe ser:

- específico para el cambio;
- conciso en relación con el volumen de evidencia;
- priorizado por relevancia;
- verificable;
- comprensible para un reviewer que no conoce todo el historial;
- explícito sobre incertidumbre;
- estable en sus afirmaciones fácticas ante la misma versión de las fuentes;
- seguro ante evidencia insuficiente;
- útil sin exigir confianza ciega.

### 12.4 Resultado permitido ante información insuficiente

Una salida válida puede concluir que no existe evidencia suficiente para producir determinadas secciones. Debe conservar lo que sí pudo establecerse, explicar las limitaciones y evitar rellenar vacíos con contenido genérico o plausible.

---

## 13. Requisitos funcionales

### RF-01 — Identificación reproducible

El brief debe indicar exactamente qué versión del cambio y de las fuentes fue analizada.

### RF-02 — Recuperación contextual

Debe recuperar evidencia relacionada con el cambio a partir de los artefactos autorizados, priorizando relevancia sobre cantidad.

### RF-03 — Trazabilidad

Toda afirmación material presentada como hecho debe citar evidencia verificable. Las inferencias deben citar los hechos que las sustentan.

### RF-04 — Separación de categorías

Debe diferenciar hechos, inferencias, riesgos, preguntas, ausencias y contradicciones.

### RF-05 — Vigencia

Debe preservar las señales disponibles sobre estado, versión, fecha y reemplazo de los artefactos. No debe presentar como vigente aquello cuya vigencia no puede establecerse.

### RF-06 — Contradicciones

Debe exponer contradicciones relevantes cuando no exista evidencia suficiente para resolverlas.

### RF-07 — Degradación segura

Ante entradas incompletas, permisos insuficientes, fuentes no disponibles o evidencia débil, debe reducir el alcance de sus conclusiones y explicar la causa.

### RF-08 — Permisos

Debe respetar el acceso efectivo del solicitante durante toda la recuperación y en toda la salida.

### RF-09 — Control humano

El producto debe dejar explícito que el reviewer conserva la responsabilidad de evaluar, comentar y aprobar o rechazar el cambio.

### RF-10 — Actualización

Si el cambio analizado se modifica, el brief anterior debe identificarse como correspondiente a una revisión previa y no presentarse como actual.

### RF-11 — Navegación a evidencia

El usuario debe poder pasar de una afirmación a la ubicación de la evidencia que la respalda.

### RF-12 — Foco configurable

El usuario puede aportar una pregunta o foco de revisión sin perder el resumen general necesario para interpretar la respuesta.

### RF-13 — Explicación de relevancia

El brief debe explicar por qué una decisión, restricción, dependencia o fragmento recuperado resulta pertinente al cambio.

### RF-14 — Auditabilidad funcional

Debe poder reconstruirse qué cambio, propósito y evidencia dieron lugar a un brief, sujeto a las políticas aplicables.

### RF-15 — Desactivación

El slice debe poder dejar de ofrecerse sin impedir que el flujo de revisión continúe por sus medios habituales.

---

## 14. Requisitos de calidad y seguridad

- **Groundedness:** las afirmaciones fácticas se apoyan en evidencia citada.
- **Relevancia:** el contenido incluido contribuye a comprender o evaluar el cambio.
- **Cobertura útil:** se consideran las categorías de contexto importantes sin exigir exhaustividad imposible.
- **Claridad:** el usuario distingue evidencia, interpretación y desconocimiento.
- **Consistencia:** la misma evidencia no produce clasificaciones contradictorias sin explicación.
- **Privacidad y acceso:** no se amplían permisos ni se filtra contenido restringido.
- **Fail-safe:** la indisponibilidad o ambigüedad reduce confianza y alcance; nunca habilita una conclusión más fuerte.
- **No interferencia:** una falla del slice no bloquea el mecanismo habitual de revisión.
- **Oportunidad:** el brief llega dentro de un tiempo compatible con una revisión interactiva.
- **Proporcionalidad:** el volumen del resultado guarda relación con el tamaño y riesgo aparente del cambio.
- **Accesibilidad:** citas, categorías y advertencias pueden comprenderse sin depender exclusivamente de color o elementos visuales.

---

## 15. Fuera de alcance

VS-001 no incluye:

- aprobar, rechazar, fusionar o modificar cambios;
- publicar comentarios o realizar acciones en nombre del usuario;
- corregir código o generar una implementación alternativa;
- sustituir una revisión de arquitectura o de código;
- emitir un veredicto vinculante sobre cumplimiento;
- garantizar que todos los riesgos fueron detectados;
- analizar fuentes externas al repositorio Git inicial y sus artefactos de revisión;
- incorporar observabilidad, incidentes, sistemas de trabajo, mensajería o catálogos externos;
- migrar, duplicar o declarar una nueva fuente de verdad;
- resolver contradicciones sin evidencia de precedencia;
- evaluar desempeño individual de autores o reviewers;
- establecer vigilancia o rankings de productividad;
- automatizar decisiones de gobierno;
- diseñar plataformas o capacidades técnicas generales.

Tampoco forma parte de esta especificación el diseño de interfaces técnicas, mecanismos de integración, persistencia, ejecución, coordinación, procesamiento asíncrono, almacenamiento temporal ni plataformas de conocimiento. Esas decisiones sólo podrán abordarse por separado, a partir de necesidades demostradas y sin alterar el contrato funcional aceptado aquí.

---

## 16. Métricas

Las métricas deben evaluar el outcome y los riesgos del producto. No deben utilizarse para calificar individualmente a ingenieros.

### 16.1 Métrica primaria

**Tiempo hasta contexto suficiente para revisar:** tiempo desde que el reviewer inicia la tarea hasta que declara poseer contexto suficiente para realizar una evaluación informada.

Se compara contra una baseline del flujo sin VS-001 mediante una evaluación controlada o un piloto acordado. La medición debe preservar privacidad y evitar interpretar velocidad como calidad individual.

### 16.2 Métricas de resultado

- reducción mediana del tiempo hasta contexto suficiente;
- porcentaje de reviewers que consideran el brief útil para orientar la revisión;
- porcentaje de revisiones en las que el brief aporta al menos una evidencia relevante que el reviewer no había localizado inicialmente;
- reducción de preguntas de descubrimiento respondibles por fuentes existentes;
- proporción de asuntos relevantes identificados con y sin asistencia en un conjunto de evaluación;
- tasa de uso recurrente voluntario durante el piloto.

### 16.3 Métricas de calidad

- **precisión de citas:** proporción de citas que realmente respaldan la afirmación asociada;
- **groundedness fáctico:** proporción de hechos materiales respaldados por citas válidas;
- **relevancia:** proporción de elementos evaluados como pertinentes al cambio por reviewers expertos;
- **recall de asuntos conocidos:** proporción de asuntos relevantes previamente establecidos que aparecen en el brief;
- **calidad de clasificación:** proporción de afirmaciones correctamente separadas entre hecho, inferencia, riesgo, pregunta, ausencia y contradicción;
- **tasa de contradicciones ocultas:** contradicciones conocidas que el brief resolvió o ignoró incorrectamente;
- **tasa de afirmaciones no sustentadas:** afirmaciones materiales sin evidencia suficiente;
- **utilidad de preguntas:** proporción de preguntas consideradas específicas y accionables por el reviewer.

### 16.4 Métricas operativas percibidas por el usuario

- tiempo de entrega del brief;
- tasa de análisis completados;
- tasa de degradaciones explicadas correctamente;
- tasa de briefs obsoletos mostrados sin advertencia;
- disponibilidad del flujo habitual cuando el slice no está disponible.

### 16.5 Métricas de seguridad y confianza

- incidentes de acceso o exposición no autorizada;
- porcentaje de hechos con procedencia reproducible;
- porcentaje de salidas que declaran correctamente evidencia insuficiente en casos diseñados para ello;
- tasa de falsos veredictos: lenguaje que presenta una inferencia o riesgo como conclusión confirmada;
- porcentaje de usuarios que comprenden que la decisión continúa siendo humana.

### 16.6 Criterios de aceptación cuantitativa

Antes del piloto deben acordarse baseline, muestra y umbrales numéricos para:

- reducción del tiempo hasta contexto suficiente;
- precisión de citas;
- groundedness fáctico;
- relevancia;
- recall de asuntos conocidos;
- afirmaciones no sustentadas;
- tiempo de entrega;
- incidentes de acceso.

Este documento no inventa valores sin evidencia. La aceptación funcional exige que esos umbrales sean fijados antes de evaluar el piloto y que no se modifiquen retroactivamente para declarar éxito.

---

## 17. Estrategia de evaluación

### 17.1 Evaluación previa al piloto

Se utilizará un conjunto representativo de cambios históricos o preparados, con evidencia y asuntos relevantes establecidos por reviewers calificados. Debe incluir:

- cambios pequeños y grandes dentro del alcance aceptado;
- documentación completa e incompleta;
- decisiones vigentes, reemplazadas y contradictorias;
- cambios sin implicancias arquitectónicas relevantes;
- casos con evidencia insuficiente;
- contenido al que el usuario no tiene acceso;
- modificaciones posteriores que vuelvan obsoleto un brief;
- descripciones engañosas o instrucciones incrustadas en el contenido.

La evaluación mide exactitud, relevancia, trazabilidad, clasificación y degradación segura.

### 17.2 Piloto con usuarios

El piloto debe comparar la tarea habitual con la tarea asistida en condiciones razonablemente equivalentes. Debe recoger:

- outcome medible;
- evaluación de utilidad por el reviewer;
- evidencia que fue útil, redundante o faltante;
- falsos positivos y omisiones;
- efecto sobre la carga cognitiva;
- razones para confiar, verificar o descartar una sugerencia.

### 17.3 Criterio de interpretación

Una demo convincente no valida VS-001. La decisión de continuar debe apoyarse en resultados repetibles, feedback cualitativo y ausencia de fallas críticas de seguridad o trazabilidad.

---

## 18. Definition of Done

VS-001 se considera terminado y validado únicamente cuando se cumplen todas las condiciones siguientes.

### 18.1 Producto y alcance

- problema, usuario, owner, outcome e hipótesis están documentados y aceptados;
- entradas, salidas, fuentes y límites son explícitos;
- el slice se encuentra disponible dentro de un flujo real de revisión;
- opera de extremo a extremo para una fuente Git autorizada;
- funciona exclusivamente en modo de lectura;
- el flujo de revisión continúa si el slice se desactiva o falla.

### 18.2 Calidad del contexto

- identifica de forma reproducible la versión analizada;
- recupera evidencia relevante del cambio, su contexto y decisiones aplicables;
- cada hecho material conserva una cita verificable;
- toda inferencia material expone los hechos que la sustentan;
- distingue hechos, inferencias, riesgos, preguntas, ausencias y contradicciones;
- informa vigencia incierta, fuentes en conflicto y contexto faltante;
- evita presentar contenido tangencial como contexto útil;
- alcanza los umbrales acordados de precisión, groundedness, relevancia y recall.

### 18.3 Seguridad y control humano

- respeta identidad, permisos y sensibilidad en todos los casos evaluados;
- no revela información fuera del acceso del solicitante;
- no ejecuta acciones ni altera el cambio;
- no comunica aprobación, rechazo o cumplimiento como decisión propia;
- degrada de forma segura ante evidencia o acceso insuficiente;
- puede auditarse qué entradas y evidencia originaron una salida;
- puede deshabilitarse de forma controlada;
- no registra incidentes críticos de acceso durante la evaluación y el piloto.

### 18.4 Evaluación y outcome

- existe una baseline acordada antes del piloto;
- existen evaluaciones representativas previas al uso real;
- los umbrales cuantitativos fueron fijados antes de medir el resultado;
- el piloto demuestra una mejora significativa en el outcome primario sin degradar la detección de asuntos relevantes;
- reviewers reales confirman que la salida es comprensible, verificable y útil;
- falsos positivos, omisiones y degradaciones quedan registrados y evaluados;
- el resultado no depende de uno o dos casos seleccionados favorablemente;
- existe una decisión explícita de aceptar, iterar o retirar el slice basada en evidencia.

### 18.5 Operación responsable

- el tiempo de entrega es compatible con el flujo de revisión;
- las fallas son visibles y explicables para el usuario;
- un brief obsoleto no se presenta como correspondiente a la versión actual;
- existen señales para conocer uso, calidad y degradaciones sin medir productividad individual;
- el owner puede responder por el comportamiento, los límites y la evaluación del slice.

Completar una implementación no satisface por sí solo esta Definition of Done.

---

## 19. Observaciones sobre infraestructura, sin diseñarla

Esta sección registra necesidades que la implementación deberá satisfacer. No define componentes, tecnologías ni una arquitectura de solución.

1. **El acceso debe conservar la identidad y los permisos del solicitante.** Cualquier solución futura deberá demostrar que recuperar y presentar contexto no amplía el acceso disponible en la fuente.

2. **La procedencia es parte del producto.** La evidencia necesita conservar suficiente información para que una persona pueda verificar una afirmación contra la versión original analizada.

3. **El cambio es mutable.** La solución deberá distinguir revisiones y evitar que una salida anterior parezca vigente después de una modificación.

4. **La recuperación debe ser acotada.** El producto requiere contexto mínimo y relevante, por lo que cualquier solución deberá controlar volumen, tiempo y costo sin ocultar limitaciones.

5. **La falla debe ser segura y no bloqueante.** Si una fuente no está disponible o el análisis no puede completarse, el flujo habitual de revisión debe continuar y el usuario debe conocer la degradación.

6. **La auditabilidad debe ser proporcional.** Debe ser posible reconstruir las condiciones funcionales de una salida respetando, al mismo tiempo, las políticas aplicables de seguridad, privacidad y retención.

7. **Las evaluaciones forman parte de la operabilidad.** La solución deberá permitir comprobar calidad y seguridad de manera repetible antes y durante el piloto.

8. **La desactivación es un requisito.** El owner debe poder retirar el slice si los resultados, costos o riesgos dejan de ser aceptables.

9. **No se justifica infraestructura general por anticipado.** Las necesidades compartidas sólo podrán extraerse cuando la evidencia de uso y repetición lo amerite.

Estas observaciones son restricciones del problema. No autorizan ni anticipan decisiones sobre interfaces técnicas, integración, persistencia, ejecución, coordinación, procesamiento o almacenamiento.

---

## 20. Suposiciones, dependencias y preguntas abiertas

### 20.1 Suposiciones aceptadas

- existe al menos un repositorio Git autorizable para el piloto;
- el repositorio contiene una porción suficiente de la evidencia necesaria para evaluar la hipótesis;
- puede identificarse una versión exacta del cambio;
- los usuarios piloto realizan revisiones arquitectónicas reales y pueden evaluar la utilidad;
- los owners de las fuentes conservan autoridad sobre su contenido;
- un slice de sólo lectura puede demostrar valor sin automatizar decisiones.

### 20.2 Dependencias funcionales

- selección del repositorio y equipo piloto;
- disponibilidad de reviewers y cambios representativos;
- definición previa de baseline y umbrales;
- clasificación de los datos incluidos;
- autorización para acceder a los artefactos del piloto;
- designación del owner responsable del comportamiento y la evaluación.

### 20.3 Preguntas abiertas que no bloquean esta especificación

- ¿Qué equipo y repositorio ofrecen la mejor combinación de dolor frecuente, evidencia disponible y riesgo controlable?
- ¿Qué tipos de asuntos arquitectónicos se usarán como conjunto de referencia inicial?
- ¿Cuál es el tiempo actual hasta contexto suficiente y cómo se medirá sin vigilancia individual?
- ¿Qué umbrales cuantitativos representan una mejora valiosa y segura?
- ¿Qué volumen de salida resulta útil antes de convertirse en sobrecarga?
- ¿Qué limitación observada justificaría ampliar fuentes o extraer una capacidad reutilizable en el futuro?

Estas respuestas deben documentarse antes del piloto cuando afecten su evaluación, sin ampliar el alcance funcional de VS-001.

---

## 21. Riesgos del producto y mitigaciones funcionales

| Riesgo | Consecuencia | Mitigación funcional |
| --- | --- | --- |
| Evidencia irrelevante | Sobrecarga y pérdida de confianza | Priorizar, explicar relevancia y medir utilidad |
| Afirmaciones sin sustento | Decisiones mal fundamentadas | Citas obligatorias y separación de inferencias |
| Documentación obsoleta | Aplicación de restricciones incorrectas | Mostrar señales de vigencia e incertidumbre |
| Contradicciones ocultas | Falsa certeza | Exponer fuentes en conflicto |
| Omisión de un riesgo | Exceso de confianza en el brief | Declarar límites y mantener revisión humana |
| Demasiados falsos positivos | Fatiga y abandono | Priorización y evaluación con casos reales |
| Acceso indebido | Exposición de información | Respetar permisos efectivos y fallar de forma segura |
| Brief obsoleto | Evaluación de una versión incorrecta | Identificar revisión y advertir cambios posteriores |
| Automatización percibida | Delegación inapropiada de responsabilidad | Lenguaje asistivo y decisión explícitamente humana |
| Métricas mal utilizadas | Vigilancia o incentivos dañinos | Medición agregada del producto, no de personas |
| Demo no representativa | Falsa validación | Evaluación diversa y piloto con outcome predefinido |
| Expansión prematura | Pérdida de foco y costo anticipado | Mantener fuentes y acciones fuera de alcance |

---

## 22. Criterio de aceptación y evolución

El estado **Accepted** significa que esta especificación funcional es la referencia aprobada para construir y evaluar VS-001. No significa que el outcome ya haya sido demostrado ni que el slice esté listo para adopción general.

Después del piloto sólo corresponden tres decisiones:

- **Aceptar el outcome:** continuar y definir el siguiente incremento a partir de evidencia;
- **Iterar:** mantener el alcance y corregir limitaciones medibles;
- **Retirar:** detener el slice si no aporta valor suficiente o si sus riesgos superan el beneficio.

Una ampliación de fuentes, usuarios, acciones o decisiones constituye nuevo alcance y requiere una especificación posterior. Toda necesidad técnica descubierta debe documentarse como evidencia, no convertirse automáticamente en una plataforma general.

---

## 23. Registro de decisiones funcionales

| ID | Decisión | Estado |
| --- | --- | --- |
| VS001-D01 | El primer usuario es el reviewer de implicancias arquitectónicas | Accepted |
| VS001-D02 | El outcome primario es reducir tiempo y esfuerzo hasta contexto suficiente sin degradar calidad | Accepted |
| VS001-D03 | La fuente inicial se limita a Git y artefactos de revisión asociados | Accepted |
| VS001-D04 | La operación es exclusivamente de lectura | Accepted |
| VS001-D05 | La salida principal es un Architecture Review Brief citable | Accepted |
| VS001-D06 | Hechos, inferencias, riesgos, preguntas, ausencias y contradicciones se presentan por separado | Accepted |
| VS001-D07 | El reviewer conserva la decisión y la responsabilidad | Accepted |
| VS001-D08 | Los umbrales cuantitativos se fijan antes del piloto con baseline real | Accepted |
| VS001-D09 | Una demo o implementación completa no equivale a Definition of Done | Accepted |
| VS001-D10 | La infraestructura general queda fuera de esta especificación | Accepted |

---

## 24. Historial del documento

| Fecha | Cambio | Estado |
| --- | --- | --- |
| 2026-08-03 | Creación desde cero de la especificación funcional canónica de VS-001 | Accepted |

