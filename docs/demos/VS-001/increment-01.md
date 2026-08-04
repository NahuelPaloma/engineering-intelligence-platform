# VS-001 — Increment 01 — Evidence Collection

## Objetivo

Demostrar que una URL de GitHub Pull Request puede convertirse en evidencia
reproducible identificada por sus revisiones base y head.

## Comando ejecutado

```bash
export EIP_GITHUB_REPOSITORIES="dotnet/runtime"
dotnet run --project src/Eip.Cli/Eip.Cli.csproj \
  --configuration Release \
  --no-build \
  -- review https://github.com/dotnet/runtime/pull/100000
```

La ejecución se realizó con `dotnet/runtime` en la allowlist y sin token contra
un repositorio público. Un repositorio privado requiere `GITHUB_TOKEN` o
`GH_TOKEN` con acceso read-only efectivo del usuario. Una allowlist ausente,
vacía o sin coincidencia detiene el comando antes de consultar GitHub.

## Resultado esperado

```text
output/<pack-id>/manifest.json
```

El JSON debe contener únicamente:

- `repository`;
- `owner`;
- `pull_request`;
- `title`;
- `author`;
- `base_sha`;
- `head_sha`;
- `commits`;
- `changed_files`;
- `generated_at`.

## Salida observada

```text
output/366414987c360cd41bc01ea6832892eb53c4e8952e1f5e503db0780090268b57/manifest.json
```

Resumen verificado de la salida:

```json
{
  "repository": "dotnet/runtime",
  "pull_request": 100000,
  "base_sha": "589beb0b2af3edd1b272f8ac21ca5e3fb142bdd0",
  "head_sha": "7a4a308fa7351ff447cffc1f465af4db98d0e411",
  "title": "Trim unused interfaces",
  "author": "MichalStrehovsky",
  "commit_count": 3,
  "changed_file_count": 21
}
```

El resumen anterior es evidencia de la demo, no el schema del manifest. El
[manifest de ejemplo](examples/increment-01-manifest.json) muestra el contrato
completo con datos ilustrativos pequeños.

## Validación automatizada

```text
Build: 0 warnings, 0 errors
Tests: 15 passed
Format: no changes required
```

Las pruebas cubren URL válida, URLs rechazadas y el pipeline completo con
respuestas HTTP controladas. También verifican la allowlist, comparación
case-insensitive, comportamiento fail-closed y cero llamadas HTTP para un
repositorio rechazado. El test end-to-end verifica que no aparecen campos
superiores adicionales.

## Limitaciones

- sólo acepta URLs `https://github.com/<owner>/<repo>/pull/<number>`;
- GitHub es el único Git Provider implementado;
- la allowlist se configura mediante `EIP_GITHUB_REPOSITORIES`, con valores
  `owner/repository` separados por comas;
- sin token sólo funciona para repositorios públicos y queda sujeto al rate
  limit anónimo de GitHub;
- no recolecta conversaciones, reviews, checks ni patches;
- no genera contexto, Markdown de producto, inferencias, preguntas, métricas ni
  feedback;
- `generated_at` cambia entre ejecuciones; el pack ID permanece estable mientras
  repository, Pull Request, base SHA y head SHA no cambien.

## Decisión de continuar

**Continuar al review del incremento.** La demo confirma que el pipeline mínimo
puede recuperar un Pull Request real, paginar commits y archivos, fijar
base/head SHA y escribir el manifest acordado. La continuación al Incremento 2
requiere una decisión explícita después de revisar esta evidencia.
