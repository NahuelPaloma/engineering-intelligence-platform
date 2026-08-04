# VS-001 — Increment 2.4 — Deterministic README Ranking

## Objetivo

Demostrar que la CLI puede ordenar los README descubiertos mediante reglas de
ubicación explícitas y reproducibles, sin leer el repositorio ni interpretar
metadata o contenido.

## Entradas autorizadas

- `readme-metadata.json` aporta los paths y la metadata extraída;
- `readmes.json` aporta la relación de descubrimiento necesaria.

Ninguna otra fuente participa del ranking.

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
└── readme-ranking.json
```

Los cuatro artefactos anteriores permanecen sin cambios.

## Demo ejecutable

La demo controlada proporciona un README en el directorio modificado, dos
ancestros y la raíz. Después ejecuta el mismo generador que invoca
`vs001 review`:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ReadmeRankingGeneratorTests.ReducesScoreForEachHigherAncestorLevel'
```

Salida observada:

```text
Passed: 1, Failed: 0, Skipped: 0
```

El resultado coincide con el
[readme-ranking.json de ejemplo](examples/increment-02-4-readme-ranking.json):

```json
{
  "documents": [
    {
      "path": "src/payments/processing/README.md",
      "score": 100,
      "reason": "same_directory"
    },
    {
      "path": "src/payments/README.md",
      "score": 80,
      "reason": "nearest_ancestor"
    },
    {
      "path": "src/README.md",
      "score": 60,
      "reason": "nearest_ancestor"
    },
    {
      "path": "README.md",
      "score": 10,
      "reason": "repository_root"
    }
  ]
}
```

## Reglas demostradas

- `changed file directory` produce score 100 y `same_directory`;
- el ancestro más cercano produce score 80 y `nearest_ancestor`;
- cada nivel superior en la misma cadena resta 20;
- ningún score puede ser negativo;
- la raíz siempre produce score 10 y `repository_root`;
- la salida se ordena por score descendente y path ordinal ascendente;
- los estados `extracted`, `missing_name`, `missing_purpose` e `insufficient` no
  modifican el score;
- paths duplicados o conjuntos inconsistentes se rechazan;
- la ejecución repetida produce los mismos bytes;
- `readme-metadata.json` y `readmes.json` permanecen intactos.

## Validación automatizada

```text
Build: 0 warnings, 0 errors
Tests: 55 passed
```

## Limitaciones

- el ranking sólo conoce las relaciones ya presentes en `readmes.json`;
- un ancestro superior sólo puede recibir reducción adicional si su cadena está
  representada explícitamente en las entradas;
- no se usa `name`, `purpose` ni `evidence` para calcular el score;
- no se vuelve a leer el repositorio ni contenido README;
- no existe similitud semántica, IA, embeddings ni Inference Engine;
- no se generan resúmenes, recomendaciones, ADR, riesgos ni Context Pack.

## Decisión de continuar

**Listo para review del incremento.** El usuario obtiene un artefacto nuevo y
demostrable (`readme-ranking.json`) producido únicamente por reglas de
ubicación. La continuación a Incremento 2.5 requiere una decisión separada.
