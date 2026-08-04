# VS-001 — Architecture Review Intelligence — Technical Design

| Campo                | Valor                                     |
| -------------------- | ----------------------------------------- |
| Estado               | **Accepted**                              |
| Tipo                 | Diseño técnico del piloto                 |
| Vertical slice       | VS-001 — Architecture Review Intelligence |
| Owner propuesto      | Engineering Platform                      |
| Última actualización | 3 de agosto de 2026                       |

## 1. Propósito y autoridad

Este documento propone la implementación técnica mínima del piloto de VS-001.
Define cómo transformar la referencia de un cambio en un **Architecture Review
Context Pack** verificable, sin implementar todavía la solución ni extender el
alcance del vertical slice.

Las fuentes normativas son, en este orden:

1. [Product Vision v1.1](../vision/engineering-intelligence-platform-product-vision-v1.1.md),
   Accepted.
2. [Architecture v1.0](../architecture/engineering-intelligence-platform-architecture-v1.0.md),
   Accepted.
3. [VS-001 — Architecture Review Intelligence](./VS-001-Architecture-Review-Intelligence.md),
   Accepted.
4. [Detailed Architecture Reference](../architecture/reference/engineering-intelligence-platform-architecture-v1.0-detailed-reference.md),
   Supporting Reference.

También se observaron los índices de visión, arquitectura y ADR, el README, el
workflow de contribución, la plantilla de pull request y el workflow de calidad
existentes. Ante una contradicción prevalecen los documentos Accepted.

Este diseño usa un único nombre para la salida completa: **Architecture Review
Context Pack**. El pack contiene tanto la representación estructurada como su
vista humana navegable y satisface la salida exigida funcionalmente por VS-001.

## 2. Alcance y restricciones

El piloto procesa, por solicitud explícita, un Merge Request de un único
repositorio autorizado a través del **Git Provider del piloto**. Lee el cambio y
contenido versionado, recupera contexto, genera un pack local, recoge feedback y
permite evaluar el outcome.

No publica comentarios, aprueba, rechaza, fusiona ni modifica el Merge Request.
Tampoco crea Knowledge Platform, Agent Platform, Event Backbone, Action Gateway,
plataforma MCP, microservicios, arquitectura distribuida, base de datos,
integraciones múltiples ni abstracción de proveedores.

La solución propuesta es una sola aplicación de línea de comandos, modular y sin
servicio propio desplegado. El flujo habitual de revisión sigue disponible si la
aplicación falla o se deshabilita.

**Justificación VS-001:** VS001-D03, D04, D05 y D10; RF-08, RF-09 y RF-15;
secciones 15 y 18.1 de VS-001; Architecture v1.0, principios 9–10 y Fase 0.

## 3. Git Provider del piloto

El diseño utiliza el concepto **Git Provider del piloto**. Para el piloto actual
se utilizará **GitHub**, y su artefacto de cambio será un Pull Request.

No se elige GitHub como proveedor definitivo de EIP. Se elige porque el propio
repositorio ya contiene convenciones y automatización de GitHub
(`.github/PULL_REQUEST_TEMPLATE.md` y `.github/workflows/quality.yml`), por lo
que permite demostrar el slice sin incorporar otra plataforma. En este
documento, Pull Request es la instancia de GitHub del concepto Merge Request.

| Alternativa necesaria            | Ventaja para el piloto                                                                | Costo                                                                | Decisión    |
| -------------------------------- | ------------------------------------------------------------------------------------- | -------------------------------------------------------------------- | ----------- |
| GitHub Pull Requests             | Flujo ya evidenciado en el repositorio; API y enlaces de archivo/revisión disponibles | Acoplamiento explícito del piloto                                    | **Elegida** |
| GitLab Merge Requests            | Cumple funcionalmente, pero no existe evidencia local de uso                          | Segunda integración o cambio del flujo piloto                        | Postergada  |
| Carga manual de diff y metadatos | Evita una API                                                                         | Pierde identidad, permisos, conversaciones y navegación reproducible | Rechazada   |

El código usa nombres de dominio como `ChangeRef` y `ReviewContext`, pero no
crea una interfaz de proveedores. El adaptador del piloto es concretamente
GitHub y sólo se extraerá un contrato multi-proveedor si un nuevo vertical slice
aceptado lo demuestra. GitLab queda abierto como posible elección de un piloto
posterior, incluido uno en PPAY, pero no se implementan ambos proveedores
simultáneamente.

**Justificación VS-001:** entradas 9.1–9.2; fuente 11.1; RF-01, RF-08, RF-11;
DoD 18.1. La elección de una sola fuente cierra PD-02 únicamente para el piloto.

## 4. Forma de ejecución y experiencia mínima

El reviewer ejecuta, desde un checkout local autorizado del repositorio:

```text
vs001 review <merge-request-url> [--focus "pregunta"]
vs001 feedback <pack-id>
```

La salida se guarda en un directorio local ignorado por Git y se abre como
Markdown. Los enlaces apuntan al Pull Request, al diff y a archivos fijados al
SHA analizado. La CLI muestra progreso, degradaciones y la ubicación del pack.

Esta experiencia es deliberadamente manual para un grupo piloto: evita un bot,
webhook, aplicación web, cola, hosting y publicación automática. Sigue
conviviendo con el flujo de revisión porque parte de la URL del Pull Request y
devuelve navegación hacia GitHub. Antes del piloto, una prueba con reviewers
debe confirmar que esta interacción satisface “dentro de un flujo real”; si no,
el siguiente incremento puede adjuntar el artefacto mediante GitHub Actions, sin
cambiar el núcleo.

**Justificación VS-001:** secciones 6, UC-01, UC-05 y UC-07; RF-11, RF-12 y
RF-15; no interferencia; DoD 18.1.

## 5. Flujo extremo a extremo

```mermaid
sequenceDiagram
  actor R as Reviewer
  participant CLI as Aplicación VS-001
  participant GP as Git Provider del piloto (GitHub)
  participant Git as Checkout local
  participant IE as Inference Engine autorizado
  participant FS as Archivos locales del piloto

  R->>CLI: review(PR URL, foco opcional)
  CLI->>GP: PR, permisos efectivos, head/base, archivos y conversación
  GP-->>CLI: metadatos y diff autorizados
  CLI->>Git: verificar repo y obtener contenido en head/base SHA
  Git-->>CLI: archivos, docs, ADR, ownership, config, tests e historia acotada
  CLI->>CLI: seleccionar, etiquetar y presupuestar evidencia
  CLI->>IE: instrucciones fijas + evidencia minimizada no confiable
  IE-->>CLI: análisis candidato estructurado con source IDs
  CLI->>CLI: validar esquema, categorías y citas; degradar o abstenerse
  CLI->>FS: manifest.json + context-pack.md + audit.json + feedback.jsonl
  CLI-->>R: pack navegable y advertencias
  R->>CLI: feedback del pack
  CLI->>FS: señal de feedback separada
```

Pasos y condiciones:

1. Parsear una URL del Git Provider permitida y rechazar hosts o repositorios
   fuera de la allowlist.
2. Usar la credencial del reviewer para leer el Merge Request. Verificar acceso
   antes de recuperar contenido y no usar una credencial de mayor alcance.
3. Fijar `base_sha` y `head_sha`; toda lectura y enlace se construye sobre esos
   SHAs. Si cambian durante el análisis, terminar el pack con estado `stale`.
4. Recuperar metadatos y diff con límites explícitos. Registrar omisiones por
   truncamiento, binarios, tamaño o permisos.
5. Verificar que el checkout local corresponde al mismo `owner/repository` y que
   contiene ambos SHAs; usar Git local para contenido e historia acotada.
6. Derivar términos y rutas del cambio y ejecutar la búsqueda determinista de
   contexto descrita en la sección 9.
7. Construir un catálogo de evidencias inmutable para la ejecución. Cada
   fragmento recibe un `source_id` antes de cualquier inferencia.
8. Enviar al Inference Engine sólo los fragmentos seleccionados, con
   instrucciones fijas y un esquema de salida cerrado. Tratar todo contenido de
   repositorio, PR y usuario como datos no confiables.
9. Validar fuera del Inference Engine que todo hecho tiene citas existentes, que
   toda inferencia cita hechos y que categorías y campos respetan el esquema.
   Mover afirmaciones inválidas a ausencias o descartarlas; nunca inventar una
   cita.
10. Renderizar el Architecture Review Context Pack y archivos de auditoría
    local. Volver a consultar el `head_sha` y advertir obsolescencia si cambió.
11. Recoger feedback voluntario y asociado al pack, separado de la evidencia.

El flujo es síncrono y cancelable. No hay eventos, workers ni reintentos de una
ejecución completa.

**Justificación VS-001:** UC-01 a UC-08; RF-01 a RF-15; observaciones de
infraestructura 1–9; Architecture v1.0, secciones 9, 13 y 15.

## 6. Componentes mínimos y límites

Todos los componentes son módulos internos del mismo proceso y deployable.

| Componente                           | Responsabilidad                                                                                                    | Límite explícito                                                                              | Justificación directa                                        |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| CLI de revisión                      | Validar entrada, capturar foco, coordinar una ejecución, presentar estado y recoger feedback                       | No contiene recuperación ni razonamiento; no modifica el Git Provider                         | UC-01, UC-05, UC-07; RF-12, RF-15; DoD 18.1                  |
| Adaptador del Git Provider read-only | Leer identidad efectiva, MR, commits, archivos, diff, reviews y comentarios permitidos; crear enlaces por revisión | Sólo GitHub en el piloto actual; sin write scopes, webhooks, comentarios ni interfaz genérica | entradas 9.1–9.2; fuente 11.1; RF-01, RF-08, RF-11           |
| Lector Git local                     | Leer snapshots base/head, buscar contexto y ejecutar historia acotada                                              | No indexa globalmente, no clona por sí solo, no cambia el working tree                        | 10.1–10.4; RF-02, RF-05; VS001-D03                           |
| Ensamblador de contexto              | Seleccionar evidencia mínima, asignar IDs, conservar procedencia, detectar faltantes y aplicar presupuesto         | No recomienda ni persiste conocimiento compartido                                             | 10.5–10.6; RF-02, RF-05, RF-06; UC-06                        |
| Analizador de review                 | Producir salida estructurada candidata con hechos, inferencias, riesgos y preguntas                                | No decide ni actúa; no accede directamente a fuentes; puede abstenerse                        | UC-02 a UC-04 y UC-08; RF-04, RF-09, RF-13                   |
| Validador y renderer                 | Validar esquema y citas, detectar categorías inválidas, renderizar el pack navegable                               | No corrige afirmaciones inventando evidencia                                                  | RF-03, RF-04, RF-11, RF-14; métricas de groundedness y citas |
| Registro local del piloto            | Escribir auditoría minimizada, métricas de ejecución y feedback explícito                                          | No es Knowledge ni fuente de verdad; sin contenido completo por defecto                       | RF-14; métricas 16.2–16.5; DoD 18.4–18.5                     |
| Arnés de evaluación                  | Ejecutar casos versionados y calcular métricas antes/durante el piloto                                             | No promociona feedback a conocimiento ni mide individuos                                      | sección 17; VS001-D08 y D09; DoD 18.4                        |

No se propone un “Engineering Context Server” desplegado. En Fase 0, su
responsabilidad mínima es el ensamblador de contexto dentro del slice, tal como
permite Architecture v1.0 §10.1.

## 7. Información exacta recuperada

### 7.1 Del Merge Request

Se recupera únicamente:

- URL canónica, owner, repositorio y número;
- título, cuerpo, autor, estado, draft y timestamps;
- `base.ref`, `base.sha`, `head.ref`, `head.sha` y merge base calculada;
- lista de commits con SHA, título, autoría declarada y timestamp;
- archivos cambiados con estado, ruta anterior/nueva, líneas agregadas y
  eliminadas;
- patch por archivo cuando la API lo entregue, o diff calculado localmente entre
  los SHAs;
- reviewers solicitados y reviews con estado y timestamp;
- comentarios generales y threads inline con autor, timestamp, ruta, posición o
  línea y estado resoluble cuando esté disponible;
- enlaces explícitos contenidos en título, cuerpo o conversación hacia archivos,
  ADR y documentación del mismo repositorio.

No se recuperan perfiles ampliados, actividad ajena al PR, métricas personales,
issues externos, checks de CI completos ni secretos. Los checks sólo podrían
incorporarse en un incremento posterior si un asunto arquitectónico de VS-001
demuestra su necesidad.

### 7.2 Del repositorio

Para `base_sha` y `head_sha`, según corresponda:

- hunks y contenido completo de archivos cambiados que entren en los límites;
- archivos renombrados/eliminados desde base y agregados/modificados desde head;
- manifiestos y lockfiles modificados, y la entrada relevante de dependencias;
- contratos, esquemas, interfaces y configuración modificados;
- tests modificados y tests vecinos encontrados por convención de nombre/ruta;
- README y documentación en el directorio modificado y sus ancestros;
- `CODEOWNERS` y archivos equivalentes explícitamente configurados;
- `CONTRIBUTING.md`, guías y estándares versionados aplicables;
- ADR y documentos de arquitectura localizados por la estrategia de la sección
  9, conservando título, estado, fecha y reemplazos declarados;
- definiciones y usos textuales de símbolos o claves modificados, limitados al
  repositorio y presupuesto;
- hasta un límite configurable de commits/blame sólo para fragmentos donde la
  historia pueda explicar una decisión material.

No se recorre ni envía por defecto el repositorio completo. Binarios, archivos
generados, vendored, secretos detectables y rutas denegadas se excluyen y quedan
registrados como cobertura no analizada.

**Justificación VS-001:** secciones 9.2, 10.1–10.4 y 11.2; RF-02, RF-05 y RF-07.

## 8. Modelo del Architecture Review Context Pack y trazabilidad

Cada ejecución produce un directorio:

```text
<output>/<pack-id>/
├── manifest.json
├── context-pack.md
├── audit.json
└── feedback.jsonl
```

`manifest.json` es el contrato interno versionado del pack:

```json
{
  "schema_version": "1",
  "pack_id": "sha256:...",
  "status": "complete|partial|insufficient|stale|failed",
  "purpose": "architecture-review",
  "change": {
    "repository": "owner/name",
    "merge_request": 123,
    "base_sha": "...",
    "head_sha": "...",
    "observed_head_sha_after_analysis": "...",
    "analyzed_at": "RFC3339"
  },
  "coverage": {
    "included": ["changed-files", "repository-docs"],
    "omitted": [{ "area": "history", "reason": "budget" }]
  },
  "sources": [
    {
      "source_id": "SRC-001",
      "kind": "merge_request|code|test|config|documentation|adr|ownership|history",
      "repository": "owner/name",
      "revision": "git-sha",
      "path": "docs/adr/0001-example.md",
      "line_start": 10,
      "line_end": 18,
      "url": "https://github.com/owner/name/blob/git-sha/...#L10-L18",
      "observed_updated_at": "RFC3339|null",
      "declared_status": "Accepted|null",
      "content_digest": "sha256:..."
    }
  ],
  "claims": [
    {
      "claim_id": "CLM-001",
      "category": "fact|inference|risk|question|absence|contradiction",
      "text": "...",
      "source_ids": ["SRC-001"],
      "supports_claim_ids": [],
      "relevance": "...",
      "uncertainty": "...",
      "consequence": "...",
      "suggested_verification": "..."
    }
  ],
  "generation": {
    "application_version": "...",
    "ruleset_version": "...",
    "inference_engine": "exact-engine-version",
    "instruction_version": "..."
  }
}
```

Las líneas son opcionales sólo cuando el artefacto no admite esa granularidad.
Para el cuerpo o comentario de un PR, la ubicación es URL + identificador de
comentario. El digest permite comprobar que el fragmento citado coincide con lo
analizado sin copiarlo al audit log.

Reglas de trazabilidad:

- un `fact` requiere al menos un `source_id`;
- una `inference` requiere fuentes y/o `supports_claim_ids` que apunten a
  hechos;
- un `risk` requiere evidencia o debe declarar exactamente qué falta;
- una `contradiction` requiere dos o más fuentes incompatibles;
- una `absence` registra búsqueda y alcance, no prueba inexistencia universal;
- cada fuente usa SHA, no una rama mutable;
- `context-pack.md` incluye enlaces por claim y una sección de evidencia
  deduplicada;
- `audit.json` registra IDs, versiones, tiempos, decisiones de exclusión y
  errores, pero no duplica prompts ni contenido sensible por defecto.

**Justificación VS-001:** UC-05 y UC-06; procedencia 11.4; salida 12.1–12.2;
RF-01, RF-03, RF-05, RF-06, RF-10, RF-11 y RF-14.

## 9. Localización mínima de documentación y ADR

La recuperación es determinista primero y asistida por inferencia después. No se
crea índice vectorial ni base de datos.

### 9.1 Descubrimiento

1. Partir de rutas, nombres de archivo, extensiones, símbolos y claves visibles
   en el diff.
2. Leer documentos de directorios modificados y ancestros: `README*`,
   `CONTRIBUTING*`, `ARCHITECTURE*`, `docs/**` y rutas configuradas para el
   repositorio piloto.
3. Descubrir ADR mediante patrones explícitos: `docs/adr/**`, `adr/**`,
   `decisions/**`, nombre `ADR-*` y enlaces encontrados. No se presupone que
   todos existan ni que sean vigentes.
4. Buscar coincidencias exactas de rutas, módulos, símbolos, contratos y
   términos distintivos del cambio con `git grep` en el snapshot fijado.
5. Seguir sólo enlaces relativos internos y referencias explícitas que
   permanezcan en el repositorio y dentro de una profundidad y cantidad máximas.
6. Consultar historia únicamente si el contexto presente deja una pregunta
   material y el commit/blame puede responderla.

### 9.2 Ranking y presupuesto

Se puntúa de manera explicable, sin embeddings, por:

1. referencia o enlace explícito desde el PR/diff;
2. misma ruta o símbolo modificado;
3. aplicabilidad declarada al módulo/componente;
4. autoridad, estado y reemplazo declarados;
5. proximidad de directorio;
6. coincidencia léxica distintiva;
7. señal observable de vigencia.

La aplicación aplica límites configurables de archivos, bytes, fragmentos,
historia, tokens y tiempo. Deduplica contenido por digest. El pack enumera qué
categorías quedaron fuera por presupuesto.

### 9.3 Vigencia y conflictos

Se preservan metadatos explícitos como `Status`, `Date`, `Supersedes` y enlaces
de reemplazo. No se deduce vigencia sólo por fecha o cercanía al código. Dos
afirmaciones incompatibles sin precedencia explícita producen una
`contradiction`; no se elige una silenciosamente.

**Justificación VS-001:** UC-03 y UC-06; secciones 10.2–10.6 y 11.3; RF-02,
RF-05, RF-06 y RF-13; métricas de relevancia y contradicciones ocultas.

## 10. Hechos, inferencias e información faltante

La separación es parte del modelo, no sólo del formato visual.

| Categoría        | Regla de producción                                               | Presentación                                       |
| ---------------- | ----------------------------------------------------------------- | -------------------------------------------------- |
| Hecho            | Observable literalmente en evidencia fijada y citada              | “Hecho” + cita                                     |
| Inferencia       | Relación o interpretación derivada de hechos citados              | “Inferencia” + fundamento + incertidumbre          |
| Riesgo potencial | Consecuencia posible, nunca defecto confirmado por defecto        | “Riesgo” + evidencia + consecuencia + verificación |
| Pregunta         | Información que una persona debe resolver                         | Pregunta específica + motivo                       |
| Ausencia         | Búsqueda acotada sin evidencia suficiente, inaccesible o truncada | Qué se buscó, dónde, límite y efecto               |
| Contradicción    | Fuentes relevantes incompatibles sin precedencia resolutiva       | Ambas fuentes + efecto sobre confianza             |

El Inference Engine no asigna porcentajes de confianza. La confianza se expresa
con dimensiones observables: autoridad, vigencia, cobertura, recuperación e
incertidumbre de inferencia. El validator rechaza hechos sin cita y evita que
verbos de aprobación o cumplimiento aparezcan como veredictos propios.

**Justificación VS-001:** salida 12.2; RF-03 a RF-07 y RF-09; métricas de
clasificación, afirmaciones no sustentadas y falsos veredictos.

## 11. Generación del Architecture Review Context Pack

El analizador recibe un objeto estructurado, no archivos libres, compuesto por
metadatos del cambio, hunks, evidencias etiquetadas y límites de cobertura. Sus
instrucciones son versionadas dentro del código y exigen salida JSON conforme a
un esquema cerrado.

El renderer genera `context-pack.md` de forma determinista desde el manifest
validado:

1. identificación del análisis y advertencia de obsolescencia;
2. resumen: propósito declarado, alcance observable y divergencias;
3. contexto arquitectónico relevante y por qué aplica;
4. implicancias y riesgos priorizados;
5. preguntas concretas para la revisión;
6. vacíos, contradicciones y cobertura;
7. evidencia navegable;
8. aviso visible de que el reviewer conserva la decisión.

Si falla la inferencia, puede emitirse un pack parcial con identificación,
alcance, evidencia recuperada y faltantes, sin riesgos generados. Si la
evidencia es insuficiente, se emite `insufficient`, que es un resultado válido.

**Justificación VS-001:** salida 12.1–12.4; UC-01 a UC-08; RF-04, RF-07, RF-09,
RF-11 y RF-13; DoD 18.2.

## 12. Feedback y métricas del piloto

### 12.1 Feedback por ejecución

`vs001 feedback <pack-id>` solicita únicamente:

- utilidad global: útil, parcialmente útil o no útil;
- si aportó evidencia nueva relevante: sí/no;
- claims útiles, irrelevantes, incorrectos o con clasificación incorrecta;
- citas que no respaldan el claim;
- asuntos relevantes omitidos;
- utilidad de cada pregunta seleccionada;
- confianza para verificar/descartar y razón opcional;
- comentario libre opcional.

El feedback se guarda como señal con `pack_id`, versión de formulario y
timestamp. No modifica reglas, prompts, fuentes ni futuros resultados
automáticamente.

### 12.2 Medición del outcome

Antes del piloto, owner y reviewers fijan muestra, baseline y umbrales. Para
evitar vigilancia individual:

- el reviewer inicia y detiene manualmente “tiempo hasta contexto suficiente”;
- se informa la finalidad y se permite no responder campos cualitativos;
- se reportan medianas y agregados del piloto, nunca rankings por persona;
- la correspondencia identidad–feedback no se conserva salvo consentimiento y
  necesidad aprobada;
- se comparan casos habituales y asistidos razonablemente equivalentes.

El arnés calcula:

- precisión de citas y groundedness fáctico;
- relevancia y recall de asuntos conocidos;
- clasificación epistemológica y contradicciones ocultas;
- afirmaciones no sustentadas y falsos veredictos;
- utilidad de preguntas;
- latencia, completitud, degradaciones y packs obsoletos;
- uso recurrente voluntario y evidencia nueva aportada;
- incidentes de acceso, cuyo umbral aceptable es cero para el piloto.

La configuración de baseline, muestra y umbrales se versiona antes de ejecutar
la evaluación; los resultados no pueden reescribirla.

**Justificación VS-001:** secciones 16 y 17 completas; RF-14; DoD 18.4–18.5;
VS001-D08 y D09.

## 13. Errores y degradación

| Caso                                                      | Comportamiento seguro                                                                        | Estado del pack            |
| --------------------------------------------------------- | -------------------------------------------------------------------------------------------- | -------------------------- |
| URL inválida, host o repo no permitido                    | Rechazar antes de acceso; no generar análisis                                                | `failed`                   |
| Sin autenticación o acceso al PR                          | Informar acceso insuficiente sin confirmar contenido restringido                             | `failed`                   |
| Permiso incierto para un artefacto                        | Excluirlo y registrar limitación genérica                                                    | `partial`                  |
| PR inexistente o SHAs no resolubles                       | Detener con causa accionable                                                                 | `failed`                   |
| Checkout no corresponde o está incompleto                 | No mezclar repositorios; pedir fetch/checkout correcto                                       | `failed`                   |
| PR cambia durante el análisis                             | Conservar SHAs analizados y advertir que no es actual                                        | `stale`                    |
| Diff/API truncado, binario o cambio demasiado grande      | Analizar sólo cobertura explícita; no generalizar                                            | `partial` o `insufficient` |
| No se encuentran docs o ADR                               | Declarar búsqueda y ausencia; no inventar decisiones                                         | `partial` o `insufficient` |
| Documentos contradictorios o vigencia incierta            | Mostrar conflicto o incertidumbre                                                            | `partial`                  |
| Rate limit, timeout o GitHub indisponible                 | Reintento corto sólo de lecturas idempotentes; luego detener o usar evidencia ya fijada      | `partial` o `failed`       |
| Servicio de inferencia indisponible                       | Entregar evidencia recuperada y cobertura sin análisis inventado                             | `partial`                  |
| Salida del Inference Engine inválida o citas inexistentes | Un reintento de reparación con el mismo contexto; luego eliminar claims inválidos            | `partial` o `insufficient` |
| Prompt injection en contenido                             | Mantener contenido como datos; ignorar instrucciones embebidas; registrar caso de evaluación | Según cobertura            |
| Posible secreto en evidencia                              | Excluir antes de inferencia y marcar área no analizada                                       | `partial`                  |
| Límite de costo, tokens o tiempo                          | Cancelar inferencia restante y explicar el límite                                            | `partial`                  |
| Escritura local falla                                     | No afirmar que existe pack auditable; limpiar sólo temporales propios                        | `failed`                   |

Una falla nunca altera ni bloquea GitHub. La CLI retorna códigos de salida
distintos para completo, degradado y fallido.

**Justificación VS-001:** UC-06 y UC-08; RF-06, RF-07, RF-10 y RF-15; fail-safe
y no interferencia; métricas operativas; DoD 18.3 y 18.5.

## 14. Seguridad y acceso mínimos

1. **Identidad efectiva:** token del propio usuario piloto, no token compartido
   ni credencial del Inference Engine.
2. **Mínimo privilegio:** fine-grained token o GitHub App user token con lectura
   de metadata, contents y pull requests sólo del repositorio piloto. La CLI
   verifica scopes/configuración al inicio.
3. **Allowlist:** host y `owner/repository` configurados; no se siguen enlaces a
   otros repositorios o dominios.
4. **Secretos:** credencial sólo por keychain/credential helper o variable de
   entorno; nunca argumentos, archivos del pack, prompts o logs.
5. **Autorización doble:** GitHub autoriza los artefactos de PR; el checkout
   local sólo se usa después de comprobar identidad del remoto y SHA. La
   posesión local no autoriza ampliar el propósito.
6. **Minimización:** sólo evidencia rankeada llega al proveedor; binarios,
   secretos detectables, rutas denegadas y contenido fuera de presupuesto se
   excluyen.
7. **Inference Engine autorizado:** antes del piloto, Security/Source Owner
   aprueban tecnología, residencia, retención y uso para entrenamiento para la
   clasificación de datos del repositorio. Sin aprobación, el modo con
   inferencia no opera.
8. **Prompt injection:** instrucciones fijas, datos delimitados, ninguna tool
   call del Inference Engine, esquema cerrado, validación externa y cero
   capacidades de escritura.
9. **Archivos locales:** directorio con permisos del usuario, ignorado por Git,
   retención corta configurada y borrado documentado. El audit log usa hashes e
   IDs en lugar de duplicar contenido.
10. **Auditoría minimizada:** registra identidad seudonimizada si es necesaria,
    propósito, SHAs, IDs de fuente, versiones, exclusiones, latencia, costo y
    errores. No registra tokens ni contenido completo por defecto.
11. **Kill switch:** variable/configuración local que desactiva inferencia o
    todo el slice; desinstalar la CLI no afecta el review.
12. **Pruebas adversariales:** permisos, exfiltración, enlaces hostiles, path
    traversal, prompt injection y secretos forman parte del gate del piloto.

**Justificación VS-001:** reglas de entrada 9.4; acceso 11.5; RF-07 a RF-09 y
RF-14–15; requisitos de seguridad; métricas 16.5; DoD 18.3.

## 15. Ejecución local y dependencias externas

| Responsabilidad                                | Ubicación                                  | Motivo                                                                                                |
| ---------------------------------------------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| CLI, coordinación y validación                 | Local                                      | Un deployable, control del reviewer y desactivación simple                                            |
| Lectura Git, búsqueda, ranking inicial         | Local                                      | Fuente versionada disponible sin copiarla a otra plataforma                                           |
| Ensamblado, render, audit y feedback           | Local                                      | No requiere servicio ni base de datos                                                                 |
| Merge Request, permisos, reviews y comentarios | Git Provider del piloto (GitHub)           | Fuente autoritativa del artefacto de revisión                                                         |
| Inferencia estructurada                        | Inference Engine autorizado                | Necesaria para relacionar evidencia y formular implicancias/preguntas; limitada a contexto minimizado |
| Agregación del piloto                          | Job local/offline sobre archivos aprobados | Suficiente para la muestra inicial; no requiere backend                                               |

No se aloja ningún servicio EIP en el piloto. El acceso de red se limita al Git
Provider del piloto y al Inference Engine aprobado. Si la política prohíbe
enviar el código al engine seleccionado, el piloto queda bloqueado hasta elegir
uno local autorizado o cambiar la clasificación; no se incorporan ambos como
abstracción anticipada.

## 16. Decisiones técnicas indispensables

### TD-01 — Aplicación modular única; runtime pendiente

**Decisión:** una sola CLI modular incorporada al repositorio. El lenguaje y el
runtime se seleccionan durante el Incremento 0.

La comparación se limita a las alternativas que el equipo pueda sostener y usa
estos criterios: simplicidad total, velocidad de implementación, ecosistema de
librerías, integración con Git y el Git Provider, validación de contratos,
facilidad de pruebas y experiencia real del equipo. La presencia actual de
Node.js 22 en el workflow documental es una señal, no una decisión de runtime
para EIP. .NET, Node.js u otra alternativa sólo se elegirán con esa evaluación.

**Justificación:** RF-01, RF-03, RF-04, RF-14; estrategia modular-first;
workflow actual del repositorio.

### TD-02 — CLI local, síncrona y bajo demanda

**Decisión:** sin daemon, webhook, bot, cola ni scheduler.

**Justificación:** momento de uso de la sección 6, RF-15, no interferencia y
Architecture v1.0 ADR-010/011.

### TD-03 — Git Provider del piloto concreto y read-only

**Decisión:** para el piloto actual, API oficial de GitHub para Pull Requests y
Git local para snapshots. Un cliente HTTP pequeño o SDK oficial puede usarse; no
se crea una interfaz genérica ni se decide el proveedor definitivo de EIP.

**Justificación:** VS001-D03/D04, RF-08 y sección 3 de este documento.

### TD-04 — Sin base de datos ni índice semántico

**Decisión:** artefactos JSON/Markdown locales, búsqueda léxica y estructural
por ejecución.

**Justificación:** RF-02 puede satisfacerse con la única fuente Git; feedback y
audit del piloto son acotados. Architecture v1.0 sólo permite Knowledge cuando
la recuperación directa deja de alcanzar.

### TD-05 — Un único Inference Engine autorizado

**Decisión:** seleccionar durante el Incremento 0 un solo Inference Engine con
salida estructurada y política compatible con los datos. Puede ser un servicio
comercial, un modelo interno o ejecución local. La configuración registra el
engine y versión exactos; no existe gateway ni fallback multi-engine.

**Comparación mínima:** reglas puramente deterministas conservan citas pero no
cubren suficientemente UC-02/UC-04 ni preguntas contextualizadas; un Inference
Engine aporta esa inferencia. La selección final se hace con el dataset offline,
seguridad, latencia y costo, no por preferencia. Hasta esa evaluación, el nombre
del engine queda como decisión del Incremento 0, no como plataforma pendiente.

**Justificación:** UC-02, UC-04; RF-03, RF-04, RF-13; métricas 16.3 y 16.4.

### TD-06 — JSON Schema interno y Markdown como vista

**Decisión:** el Inference Engine produce JSON cerrado; la aplicación valida y
renderiza Markdown de forma determinista.

**Justificación:** salida 12.1–12.3; RF-03, RF-04, RF-11 y RF-14.

### TD-07 — Artefactos fijados por SHA

**Decisión:** toda fuente y todo pack se vincula con `base_sha/head_sha`, digest
y versión de generador.

**Justificación:** RF-01, RF-05, RF-10 y RF-14.

### TD-08 — Límites configurados y abstención

**Decisión:** presupuestos explícitos y estados de degradación forman parte del
contrato, no son sólo observabilidad.

**Justificación:** criterios 10.6; salida 12.4; RF-07; proporcionalidad y
oportunidad.

## 17. Decisiones que no se toman en este diseño

Este documento deliberadamente **no decide**:

- lenguaje ni runtime;
- proveedor o tecnología definitiva del Inference Engine;
- proveedor Git definitivo de EIP;
- CI de ejecución del producto;
- hosting, despliegue remoto ni topología;
- caché ni persistencia;
- UI definitiva.

También se postergan hasta que evidencia concreta las justifique:

- GitLab, otros hosts Git y una interfaz multi-proveedor;
- GitHub App organizacional, bot, checks, comentarios o UI web;
- despliegue remoto, contenedores, Kubernetes y separación en servicios;
- webhook, procesamiento asíncrono, colas y Event Backbone;
- Knowledge Platform, grafo, catálogo, embeddings, vector store o base de datos;
- MCP general y registro de conectores;
- Agent Platform, routing, tool broker y model gateway;
- Action Gateway y cualquier permiso de escritura;
- caché compartida, sesiones compartidas y memoria organizacional;
- múltiples modelos, fallback automático y optimización multi-proveedor;
- soporte de fuentes externas: issues, observabilidad, catálogo, chat o tickets;
- SLO definitivos y escalado, hasta obtener baseline del piloto;
- retención organizacional de packs, hasta acordar política y necesidad;
- aprendizaje automático desde feedback;
- análisis de CI/checks, binarios, repositorios múltiples o submódulos;
- publicar el Architecture Review Context Pack automáticamente dentro del Git
  Provider.

La señal para reconsiderar una decisión debe vincularse a un fallo medido del
piloto: recuperación insuficiente, fricción de experiencia, latencia, repetición
entre slices, necesidad de colaboración o control no cubierto. La mera
posibilidad futura no es evidencia.

## 18. Estructura mínima de código propuesta

No se crea esta estructura al aprobar el documento; orienta incrementos futuros.

```text
src/vs001/
├── cli.ts
├── review-orchestrator.ts
├── github.ts
├── git.ts
├── context-builder.ts
├── analyze.ts
├── validate.ts
├── render.ts
├── feedback.ts
├── types.ts
└── prompts/
    └── architecture-review-v1.ts
test/vs001/
├── fixtures/
├── unit/
├── integration/
├── contract/
├── adversarial/
└── evaluation/
config/vs001/
├── pilot.example.json
├── context-pack.schema.json
└── evaluation.schema.json
```

Las extensiones mostradas son ilustrativas hasta seleccionar el runtime en el
Incremento 0; los nombres de responsabilidad se conservan en la tecnología
elegida. `review-orchestrator.ts` es el único orquestador. `github.ts` es
concreto y read-only. `context-builder.ts` implementa sólo la recuperación del
slice. `analyze.ts` tiene un único cliente de Inference Engine.

Artefactos de ejecución van fuera de `src`, en un directorio local ignorado. El
dataset real del piloto sólo se versiona si fue sanitizado y autorizado; de lo
contrario se conserva fuera del repositorio con manifest versionado.

## 19. Estrategia de pruebas

### 19.1 Unitarias deterministas

- parseo y allowlist de PR URL;
- fijación y comparación de SHAs;
- selección, ranking, deduplicación y presupuestos;
- parseo de estado/fecha/reemplazo de ADR;
- creación y validación de source IDs, claims y digests;
- reglas epistemológicas y renderer Markdown;
- redacción de secretos y códigos de degradación.

### 19.2 Contratos

- respuestas grabadas y sanitizadas de GitHub para estados, renombres,
  truncamiento, reviews y errores;
- Architecture Review Context Pack contra JSON Schema;
- salida estructurada del único Inference Engine contra el esquema;
- enlaces GitHub fijados a SHA y líneas válidas.

### 19.3 Integración local

Repositorios fixture temporales con commits base/head, docs, ADR vigentes,
reemplazados y contradictorios, ownership, tests, contratos, historia y archivos
no analizables. Se prueba el flujo completo sin red con dobles del Git Provider
y del Inference Engine.

### 19.4 End-to-end controlado

Un repositorio sandbox de GitHub autorizado y un Inference Engine aprobado
validan permisos reales, mutabilidad, rate limits, latencia y navegación. Las
pruebas no publican ni modifican el PR.

### 19.5 Evaluación de calidad

Dataset versionado con cambios pequeños/grandes, sin implicancia, documentación
completa/incompleta, decisiones reemplazadas/contradictorias, acceso denegado,
pack obsoleto y descripción engañosa. Reviewers calificados establecen asuntos y
evidencia esperados antes de medir precisión, recall, relevancia, groundedness,
clasificación y utilidad.

### 19.6 Seguridad y resiliencia

- token sin scopes, repositorio no permitido y material restringido;
- prompt injection en PR, código, comentario y ADR;
- enlaces externos, path traversal, symlinks y remoto Git inconsistente;
- secretos en diff, archivos grandes, binarios y contenido generado;
- timeout, rate limit, respuesta parcial, respuesta de inferencia inválida y
  cambio de head SHA;
- verificación de que no ocurre ninguna llamada de escritura a GitHub.

Las regresiones deterministas corren en el workflow existente. Las evaluaciones
con red e inferencia son manuales o un job separado con aprobación y secretos;
no deben hacer que el review habitual dependa de VS-001.

**Justificación VS-001:** estrategia 17.1–17.3; todas las métricas 16.3–16.5;
DoD 18.2–18.5.

## 20. Definition of Done técnica del piloto

La implementación está técnicamente lista para evaluar —no automáticamente
Accepted como outcome— cuando:

- procesa end-to-end un PR del repositorio piloto mediante una sola CLI;
- sólo realiza operaciones de lectura y existe una prueba que falla ante
  cualquier intento de mutación;
- fija base/head SHA, detecta cambios posteriores y nunca presenta un pack
  obsoleto como actual;
- recupera las categorías exactas de la sección 7 dentro de presupuestos y
  declara cada omisión;
- cada hecho material tiene cita válida y cada inferencia expone sustento;
- hechos, inferencias, riesgos, preguntas, ausencias y contradicciones son
  distintos en schema y Markdown;
- las citas navegan a repositorio, SHA, artefacto y ubicación precisa;
- la localización de docs/ADR cubre enlaces, proximidad, términos, estado y
  reemplazo sin índice persistente;
- genera `manifest.json`, `context-pack.md` y `audit.json` válidos y
  reproducibles para las mismas fuentes, salvo campos explícitamente temporales
  o variación de inferencia registrada;
- permisos insuficientes, falta de evidencia, truncamiento, servicios caídos y
  output inválido producen los estados seguros definidos;
- secretos no aparecen en argumentos, logs, audit, feedback ni prompts;
- el flujo habitual continúa si se desactiva, cancela o falla la CLI;
- existen pruebas unitarias, de contrato, integración, E2E, adversariales y de
  evaluación con los casos de la sección 19;
- baseline, muestra, umbrales y política de datos están aprobados y versionados
  antes del piloto;
- feedback queda separado de evidencia y no modifica el sistema automáticamente;
- métricas se pueden calcular de forma agregada sin ranking individual;
- el owner puede desactivar inferencia o retirar la herramienta;
- el Inference Engine está aprobado para los datos y su versión queda
  registrada;
- el piloto alcanza los umbrales cuantitativos acordados y demuestra el outcome
  de VS-001 sin incidentes de acceso.

Los últimos dos puntos distinguen “lista para pilotear” de “VS-001 validado”. Un
software completo sin outcome no satisface la DoD Accepted de VS-001.

## 21. Plan de implementación incremental

Cada incremento termina con una demostración independiente y no presupone el
siguiente.

### Incremento 0 — Contrato de evaluación y seguridad

- seleccionar repositorio/equipo, data owner y Capability Owner;
- seleccionar runtime con los criterios de TD-01;
- clasificar datos y aprobar acceso e Inference Engine;
- acordar baseline, muestra y umbrales sin valores retroactivos;
- crear fixtures sanitizados y manifest del dataset.

**Demo:** configuración aprobada y un caso evaluable manualmente. **Gate:** no
se procesan datos reales sin estas decisiones. Justifica VS001-D08 y DoD
18.3–18.4.

### Incremento 1 — Identificación y evidencia primaria

- CLI, URL/allowlist, autenticación del usuario y cliente GitHub read-only;
- fijación base/head, archivos, diff y manifest de fuentes;
- enlaces por SHA y detección de obsolescencia.

**Demo:** Merge Request URL → Architecture Review Context Pack mínimo, todavía
sin inferencias, con identificación y evidencia primaria citable. Cubre RF-01,
RF-08, RF-10–11.

### Incremento 2 — Contexto local mínimo

- lector Git de snapshots;
- docs, ADR, ownership, config, tests, símbolos y ranking determinista;
- presupuesto, deduplicación, ausencias, vigencia y contradicciones.

**Demo:** el Architecture Review Context Pack incorpora contexto priorizado y
explica por qué cada evidencia aplica. Cubre RF-02, RF-05–07 y RF-13.

### Incremento 3 — Architecture Review Context Pack estructurado

- un Inference Engine autorizado, instrucciones fijas y schema cerrado;
- categorías epistemológicas, validador de citas y renderer;
- abstención y fallback a evidencia sin inferencia.

**Demo:** Architecture Review Context Pack para fixtures con y sin evidencia.
Cubre UC-01 a UC-08 y RF-03–04, RF-07, RF-09.

### Incremento 4 — Operabilidad y seguridad

- audit minimizado, timeouts, límites, códigos de estado y kill switch;
- pruebas de permisos, prompt injection, secretos y cero escrituras;
- feedback local.

**Demo:** fallas inyectadas degradan visiblemente y GitHub permanece sin
cambios. Cubre RF-14–15 y DoD 18.3/18.5.

### Incremento 5 — Evaluación offline

- ejecutar dataset representativo;
- calcular métricas y corregir sólo fallos observados dentro del mismo alcance;
- congelar versión candidata del generador.

**Demo:** reporte reproducible contra umbrales preacordados. Cubre sección 17.1
y DoD 18.2/18.4.

### Incremento 6 — Piloto limitado

- habilitar grupo voluntario y medir flujo habitual versus asistido;
- recoger feedback, omisiones, falsos positivos, latencia y uso recurrente;
- monitorear incidentes y retirar mediante kill switch si corresponde.

**Demo:** informe agregado y decisión explícita de aceptar, iterar o retirar.
Cubre secciones 16, 17.2–17.3, 18 y 22.

## 22. Riesgos y trade-offs

| Riesgo o trade-off                             | Impacto                         | Mitigación y señal de revisión                                                                               |
| ---------------------------------------------- | ------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| CLI agrega un cambio de contexto               | Puede reducir adopción          | Medir fricción; integrar más en GitHub sólo si el piloto muestra que limita el outcome                       |
| GitHub específico genera acoplamiento          | Refactor si aparece otra fuente | Aceptarlo para reducir alcance; extraer contrato sólo ante segundo caso aceptado                             |
| Checkout local puede diferir del remoto        | Evidencia incorrecta            | Verificar remoto y SHAs; fallar ante mismatch                                                                |
| Búsqueda léxica pierde relaciones semánticas   | Menor recall                    | Medir asuntos omitidos; sólo considerar índice/embeddings si el déficit es material                          |
| Inferencia externa expone contexto             | Riesgo de privacidad            | Aprobación por clasificación, minimización, redacción y retención contractual; modo bloqueado sin aprobación |
| Inference Engine produce claims plausibles     | Decisiones mal fundamentadas    | Schema, citas, validación externa, abstención y evaluación adversarial                                       |
| Validar citas no valida verdad                 | Falsa confianza                 | Medir precisión de respaldo, autoridad, vigencia y contradicciones por separado                              |
| Sin base de datos limita colaboración/historia | Agregación manual               | Adecuado para muestra pequeña; persistencia sólo si el piloto la necesita                                    |
| Sin caché aumenta latencia/costo               | Pack más lento                  | Presupuestos y medición; caché sólo tras baseline que demuestre necesidad                                    |
| Pack local puede contener datos sensibles      | Exposición en workstation       | Permisos, directorio ignorado, minimización, retención y borrado                                             |
| Merge Request grande excede presupuesto        | Cobertura parcial               | Estado `partial`, desglose de omisiones y foco configurable; no simular exhaustividad                        |
| Docs obsoletos o contradictorios               | Recomendación incorrecta        | Conservar estado/fecha/reemplazo y mostrar conflicto sin precedencia inventada                               |
| Feedback sesgado o contaminado                 | Optimización incorrecta         | Tratarlo como señal, revisión agregada y sin aprendizaje automático                                          |
| Medición de tiempo se vuelve vigilancia        | Daño cultural y datos inválidos | Inicio/fin voluntario, agregación, sin rankings ni evaluación individual                                     |
| Un buen demo se confunde con éxito             | Inversión prematura             | Dataset diverso, baseline, umbrales previos y decisión aceptar/iterar/retirar                                |
| Dependencia accidental del review              | Falla bloquea trabajo           | Ejecución opt-in, sin status requerido y kill switch                                                         |

El trade-off central es aceptar una experiencia manual, búsqueda por ejecución y
acoplamiento a GitHub para aprender con el menor sistema posible. Cualquier
complejidad adicional debe responder a una métrica o caso de VS-001 que esta
solución no logre satisfacer.

## 23. Decisiones previas al inicio del piloto

Este diseño puede pasar a implementación incremental, pero el piloto con datos
reales requiere cerrar y registrar:

1. repositorio, equipo y reviewers participantes;
2. owner operativo de la capability y source owner;
3. clasificación de datos, rutas excluidas y retención local;
4. mecanismo exacto de token de usuario y scopes;
5. Inference Engine único aprobado mediante evaluación offline;
6. límites iniciales de tamaño, tiempo, tokens y costo;
7. baseline, muestra y umbrales cuantitativos;
8. ubicación autorizada de resultados agregados del piloto.

Estas decisiones parametrizan el slice; no autorizan una plataforma compartida
ni una ampliación de fuentes o acciones.

## 24. Registro de revisión

| Fecha      | Cambio                                                                                                        | Estado                                       |
| ---------- | ------------------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| 2026-08-03 | Primera propuesta del diseño técnico mínimo                                                                   | Proposed                                     |
| 2026-08-03 | TDR: Git Provider conceptual, Inference Engine, runtime pendiente, nomenclatura única y decisiones no tomadas | **Accepted with minor changes incorporated** |
