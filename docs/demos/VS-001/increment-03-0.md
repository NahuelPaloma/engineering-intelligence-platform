# VS-001 — Increment 3.0 — Local Context Consolidation

## Objetivo

Demostrar que la CLI puede consolidar los cinco artefactos del Context Retrieval
Pipeline en un único `local-context.json`, sin releer el repositorio ni agregar
interpretación o información nueva.

## Entradas

- `manifest.json`;
- `readmes.json`;
- `readme-contents.json`;
- `readme-metadata.json`;
- `readme-ranking.json`.

## Comando del usuario

Desde el checkout correspondiente al Pull Request:

```bash
export EIP_GITHUB_REPOSITORIES="org/repo"
dotnet run --project src/Eip.Cli -- review https://github.com/org/repo/pull/123
```

## Resultado esperado

```text
output/<pack-id>/
├── manifest.json
├── readmes.json
├── readme-contents.json
├── readme-metadata.json
├── readme-ranking.json
└── local-context.json
```

Los cinco artefactos de entrada permanecen byte por byte sin cambios.

## Demo ejecutable

La demo controlada crea las cinco entradas, ejecuta el mismo consolidador que
invoca `vs001 review` y verifica valores, contenido, evidence y orden:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~LocalContextBuilderTests.ConsolidatesExistingValuesInRankingOrder'
```

Salida observada:

```text
Passed: 1, Failed: 0, Skipped: 0
```

El contrato resultante se muestra en el
[local-context.json de ejemplo](examples/increment-03-0-local-context.json).

## Reglas demostradas

- toda la información proviene exclusivamente de los cinco artefactos;
- `pack_id` usa la misma identidad estable del Incremento 1;
- el orden de documentos coincide con `readme-ranking.json`;
- score, reason, metadata, content, status y evidence se preservan;
- un error de lectura previo se conserva sin impedir otros documentos;
- paths faltantes, extra o duplicados se rechazan;
- JSON inválido se rechaza;
- ninguna salida parcial se publica si la validación falla;
- ejecuciones repetidas producen exactamente los mismos bytes;
- todos los archivos de entrada permanecen byte por byte intactos.

## Limitaciones

- el artefacto contiene únicamente contexto README recuperado por los
  incrementos anteriores;
- los datos del Pull Request expuestos en el nivel superior se limitan a
  repositorio, número y pack ID según el contrato de este incremento;
- los documentos no legibles tienen `content: null` y conservan el error
  genérico producido en el Incremento 2.2;
- no se evalúan vigencia, autoridad, calidad ni relevancia semántica;
- no existe IA, Inference Engine, resumen, clasificación, recomendación, riesgo,
  ADR, contrato ni dependencia.

## Decisión de continuar

**Listo para review del incremento.** `local-context.json` es un único artefacto
consumible que cierra el Context Retrieval Pipeline actual. Cualquier trabajo de
Inference Engine requiere una decisión explícita posterior.
