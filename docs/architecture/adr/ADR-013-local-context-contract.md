# ADR-013 — Local Context Contract

- Status: Accepted
- Date: 2026-08-04
- Decision owners: Engineering Platform / Architecture Owner

## Contexto

VS-001 separa la recuperación de evidencia de su interpretación. Los primeros
incrementos construyeron un Context Retrieval Pipeline determinista que obtiene
evidencia autorizada, localiza y lee documentación, extrae metadata explícita,
la ordena y finalmente la consolida en `local-context.json`.

El siguiente bloque de trabajo incorporará un Inference Engine. Antes de
iniciarlo es necesario fijar un límite estable que conserve la separación entre
contexto e inteligencia definida por Product Vision v1.1 y Architecture v1.0.

## Problema

Si el Inference Engine dependiera directamente de GitHub, del checkout, de
README o de artefactos intermedios del pipeline:

- mezclaría recuperación e inferencia en una misma responsabilidad;
- quedaría acoplado a un proveedor, formato documental y flujo de revisión;
- necesitaría credenciales y acceso a fuentes que no requiere para inferir;
- podría recuperar evidencia diferente de la validada por el pipeline;
- dificultaría reproducir resultados sobre una entrada fija;
- obligaría a modificarlo cuando cambien las fuentes o las reglas de
  recuperación;
- ampliaría su superficie de seguridad y el riesgo de eludir controles de
  autorización, minimización y trazabilidad.

## Decisión

`local-context.json` es el único contrato oficial entre el **Context Retrieval
Pipeline** y el **Inference Engine**.

El Context Retrieval Pipeline produce el artefacto conforme al
[Local Context Contract](../../contracts/local-context-contract.md). El
Inference Engine consume únicamente ese artefacto y no accede a sus cinco
artefactos de origen.

El Inference Engine:

- no integra ni consulta GitHub;
- no accede a Pull Requests;
- no busca ni lee README;
- no interpreta Markdown como mecanismo de recuperación;
- no accede al repositorio ni al checkout;
- no conoce ni reproduce el proceso de recuperación;
- no completa contexto faltante consultando fuentes adicionales.

Los identificadores de repositorio y Pull Request presentes en el contrato se
tratan como procedencia y correlación de la entrada. No autorizan al consumidor
a consultar esos sistemas.

El límite es lógico. Esta decisión no exige nuevos proyectos, procesos,
deployables, servicios ni infraestructura.

## Consecuencias

### Positivas

- recuperación e inferencia pueden probarse y evolucionar de forma separada;
- una ejecución de inferencia puede reproducirse sobre bytes de entrada fijos;
- el Inference Engine opera sin credenciales ni acceso al repositorio;
- la autorización y minimización permanecen antes del límite de inferencia;
- cambiar un proveedor o context provider no altera al consumidor mientras el
  contrato permanezca compatible;
- la procedencia, el orden y la evidencia llegan explícitamente al consumidor;
- las fallas de recuperación se representan antes de ejecutar inferencia.

### Negativas

- el contrato debe gobernarse y mantenerse compatible;
- el Inference Engine no puede recuperar por sí mismo información faltante;
- nueva información útil requiere evolucionar primero el pipeline y el contrato;
- materializar el artefacto agrega una frontera de serialización y validación.

### Trade-offs

Se acepta menor flexibilidad del Inference Engine a cambio de aislamiento,
reproducibilidad, mínimo privilegio y capacidad de reemplazar fuentes. También
se acepta gobernar un contrato explícito en lugar de compartir estructuras
internas o acceso directo a herramientas.

## Alternativas descartadas

### Inference Engine leyendo GitHub

Se descarta porque acopla inferencia al proveedor del piloto, requiere
credenciales de la fuente y duplica autorización y recolección de evidencia.

### Inference Engine leyendo README

Se descarta porque acopla inferencia a un tipo documental y al layout del
repositorio, evita el conjunto validado de candidatos y amplía el acceso al
filesystem.

### Inference Engine reconstruyendo contexto

Se descarta porque duplicaría la recuperación, los resultados podrían divergir
de la evidencia registrada y se perdería la reproducción determinista.

### Inference Engine usando múltiples fuentes directamente

Se descarta porque cada fuente ampliaría los permisos, modos de falla y
conocimiento específico del consumidor. La coordinación de fuentes pertenece
antes del límite contractual.

### Inference Engine consumiendo los artefactos intermedios

Se descarta porque los archivos intermedios son detalles internos del pipeline.
Su evolución independiente no debe forzar cambios en el límite de inferencia.

## Impacto futuro

Futuros context providers podrán recuperar evidencia desde GitLab, Jira,
Confluence, ADR, runbooks, reportes de incidentes u otras fuentes autorizadas.
Podrán extender o reemplazar pasos de recuperación y artefactos intermedios sin
modificar el Inference Engine, siempre que el `local-context.json` resultante
permanezca compatible con el contrato oficial.

Incorporar esas fuentes no está autorizado por este ADR. Cada incorporación
seguirá requiriendo una necesidad de producto demostrada, sus propios controles
de acceso y la gobernanza incremental establecida por Architecture v1.0.

Un cambio incompatible del Local Context Contract requiere un nuevo ADR.
