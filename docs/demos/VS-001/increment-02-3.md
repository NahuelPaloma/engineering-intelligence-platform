# VS-001 — Increment 2.3 — Deterministic README Metadata

## Objetivo

Demostrar que la CLI puede extraer `name` y `purpose` exclusivamente desde
evidencia explícita ya disponible en `readme-contents.json`, sin volver a leer
el repositorio ni utilizar IA.

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
└── readme-metadata.json
```

Los tres artefactos anteriores permanecen byte por byte sin cambios. La nueva
salida conserva el orden de `readme-contents.json`.

## Demo ejecutable

La demo controlada proporciona un H1 y un párrafo descriptivo con líneas
conocidas, ejecuta el mismo extractor que invoca `vs001 review` y verifica los
valores, el estado y la evidencia:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ReadmeMetadataExtractorTests.ExtractsH1AndFollowingDescriptiveParagraphWithEvidence'
```

Salida observada:

```text
Passed: 1, Failed: 0, Skipped: 0
```

El resultado coincide con el
[readme-metadata.json de ejemplo](examples/increment-02-3-readme-metadata.json):

```json
{
  "documents": [
    {
      "path": "src/example/README.md",
      "name": "Example Service",
      "purpose": "Provides example processing capabilities.",
      "status": "extracted",
      "evidence": [
        {
          "field": "name",
          "text": "Example Service",
          "source_line_start": 1,
          "source_line_end": 1
        },
        {
          "field": "purpose",
          "text": "Provides example processing capabilities.",
          "source_line_start": 3,
          "source_line_end": 3
        }
      ]
    }
  ]
}
```

## Reglas demostradas

- el primer H1 ATX no vacío es el nombre;
- sin H1 ATX, un H1 Setext o una línea `Title:` explícita puede aportar nombre;
- badges anteriores al título se ignoran;
- el propósito es el primer párrafo descriptivo cercano al título;
- sin título, el primer párrafo inicial puede aportar propósito y produce estado
  `missing_name`;
- no se cruza otro heading para buscar propósito;
- párrafos multilínea se unen con un espacio y conservan el rango de líneas;
- contenido Unicode se conserva;
- documentos con error previo quedan `insufficient`;
- una segunda ejecución produce exactamente los mismos bytes;
- el orden de documentos no cambia;
- entradas malformadas se rechazan;
- `manifest.json`, `readmes.json` y `readme-contents.json` no se modifican.

## Estados

| Estado            | Condición                                     |
| ----------------- | --------------------------------------------- |
| `extracted`       | Nombre y propósito tienen evidencia explícita |
| `missing_name`    | Sólo el propósito tiene evidencia explícita   |
| `missing_purpose` | Sólo el nombre tiene evidencia explícita      |
| `insufficient`    | Ninguno tiene evidencia suficiente            |

## Validación automatizada

```text
Build: 0 warnings, 0 errors
Tests: 40 passed
```

## Limitaciones

- se reconocen únicamente H1 ATX con hash y espacio, H1 Setext con `=` y
  `Title:`;
- el propósito debe aparecer antes de otro heading o estructura Markdown;
- las líneas de un párrafo se unen con un espacio, sin interpretación semántica;
- no se elimina formato Markdown dentro del texto extraído;
- no se evalúan vigencia, autoridad, relevancia ni calidad del documento;
- no hay resumen, clasificación, riesgos, recomendaciones ni Context Pack;
- no se usa IA, Inference Engine ni embeddings.

## Decisión de continuar

**Listo para review del incremento.** El usuario obtiene un artefacto nuevo y
demostrable (`readme-metadata.json`) con valores y evidencia explícita. La
continuación a Incremento 2.4 requiere una decisión separada.
