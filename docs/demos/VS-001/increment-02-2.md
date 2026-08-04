# VS-001 — Increment 2.2 — README Content Reading

## Objetivo

Demostrar que la CLI puede leer el contenido completo de los documentos
enumerados en `readmes.json`, sin buscar, interpretar, resumir ni clasificar el
texto.

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
└── readme-contents.json
```

Los dos artefactos anteriores permanecen sin cambios. La nueva salida contiene
una entrada por candidato y conserva el orden de `readmes.json`.

## Demo ejecutable

La demo controlada crea documentos con espacios, Unicode y finales de línea
distintos, además de un README no listado. Después ejecuta el mismo lector que
invoca `vs001 review`:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ReadmeContentReaderTests.ReadsOnlyListedDocumentsInOriginalOrderWithoutChangingContent'
```

Salida observada:

```text
Passed: 1, Failed: 0, Skipped: 0
```

El resultado tiene el contrato mostrado en el
[readme-contents.json de ejemplo](examples/increment-02-2-readme-contents.json):

```json
{
  "documents": [
    {
      "path": "src/payments/README.md",
      "content": "# Payments\n\nPayment component documentation.\n"
    },
    {
      "path": "README.md",
      "content": "# Example Repository\n\nRepository documentation.\n"
    }
  ]
}
```

## Error por documento

Si un candidato no puede leerse, conserva su posición y registra un error
genérico. Los documentos siguientes continúan procesándose:

```json
{
  "path": "missing/README.md",
  "error": "The document could not be read."
}
```

El error no incluye rutas absolutas, excepciones ni información del sistema.

## Reglas demostradas

- sólo se leen paths presentes en `readmes.json`;
- no se busca ningún archivo nuevo;
- el orden de entrada se conserva;
- el contenido se conserva completo, incluidos espacios y finales de línea;
- un documento no listado no aparece en la salida;
- un fallo afecta sólo a su documento;
- paths fuera del checkout y symlinks finales no se leen;
- `manifest.json` y `readmes.json` no se modifican.

## Validación automatizada

```text
Build: 0 warnings, 0 errors
Tests: 27 passed
```

## Limitaciones

- el contenido se acepta únicamente como texto UTF-8 válido;
- no existe límite de tamaño en este baby step;
- no se resuelve vigencia, autoridad ni relevancia;
- no se extrae propósito;
- no se interpreta, resume, clasifica ni renderiza Markdown;
- no se buscan ADR ni otros documentos;
- no se usa IA ni embeddings.

## Decisión de continuar

**Listo para review del incremento.** El usuario obtiene un artefacto nuevo y
demostrable (`readme-contents.json`) que contiene únicamente los documentos ya
descubiertos. La continuación a Incremento 2.3 requiere una decisión explícita.
