# VS-001 — Increment 2.1 — README Discovery

## Objetivo

Demostrar que, a partir de los archivos modificados registrados en
`manifest.json`, la CLI puede localizar referencias a README potencialmente
relevantes sin leer su contenido.

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
└── readmes.json
```

`manifest.json` permanece sin cambios. `readmes.json` contiene exclusivamente
`candidate_readmes`, con `path` y `reason` para cada candidato.

## Demo ejecutable

La demo controlada crea un checkout temporal con README en el directorio
modificado, un ancestro, la raíz y un directorio no relacionado. Luego ejecuta
el mismo localizador que invoca `vs001 review`:

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~ReadmeLocatorTests.WritesCandidatesInRequiredPriorityWithoutUnrelatedDirectories'
```

Salida observada:

```text
Passed: 1, Failed: 0, Skipped: 0
```

El archivo producido coincide con el
[readmes.json de ejemplo](examples/increment-02-1-readmes.json):

```json
{
  "candidate_readmes": [
    {
      "path": "src/payments/readme.md",
      "reason": "changed file directory"
    },
    {
      "path": "src/Readme.md",
      "reason": "nearest ancestor of changed files"
    },
    {
      "path": "README.md",
      "reason": "repository root"
    }
  ]
}
```

La prueba también verifica que el README de un directorio no relacionado no se
incluye y que los bytes de `manifest.json` no cambian.

## Reglas demostradas

- sólo se consideran `README.md`, `Readme.md` y `readme.md`;
- primero se agregan README del directorio modificado;
- después, el primer ancestro con README para cada directorio modificado;
- finalmente, el README raíz;
- las rutas se deduplican;
- no se recorre ningún subárbol no relacionado;
- no se abre ni serializa contenido de README;
- una ruta modificada que intenta salir del checkout es rechazada.

## Validación automatizada

```text
Build: 0 warnings, 0 errors
Tests: 18 passed
```

## Limitaciones

- la CLI debe ejecutarse desde el checkout correspondiente al repositorio del
  Pull Request;
- no se valida todavía la correspondencia entre el remoto local y el Pull
  Request;
- no se determina si un README está vigente o es importante;
- no se lee contenido, no se buscan ADR y no se localiza otra documentación;
- no existe ranking fuera de las tres prioridades explícitas.

## Decisión de continuar

**Listo para review del incremento.** El usuario obtiene un artefacto nuevo y
demostrable (`readmes.json`) sin ampliar el alcance a lectura o interpretación.
La continuación a Incremento 2.2 requiere una decisión explícita después de
revisar esta evidencia.
