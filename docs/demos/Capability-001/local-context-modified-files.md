# Capability-001 — Local Context Modified File Evidence

## Objetivo

Demostrar que Context Retrieval transporta la evidencia autorizada de
`manifest.changed_files` hasta `local-context.json` sin releer GitHub, acceder
al checkout ni clasificar los archivos.

## Decisión contractual

`modified_files` es una adición opcional compatible de Local Context Contract
versión conceptual 1. El productor actualizado siempre emite la colección; el
consumidor actualizado acepta contextos v1 anteriores donde no existe y los
conserva como Evidence `not_provided`. Una colección presente y vacía significa
Evidence `available` sin elementos.

`pack_id` continúa derivándose del repositorio, Pull Request, base SHA y head
SHA. `execution_id` incorpora la identidad canónica del contexto admitido, el
contrato soportado y el Rule Set. Por eso distingue campo ausente, presente
vacío y presente con elementos, aunque ninguna Rule actual consuma esta nueva
evidencia.

## Entrada

Un `manifest.json` válido que contenga, por ejemplo:

```json
{
  "changed_files": [
    {
      "path": "contracts/payments.yaml",
      "status": "modified"
    }
  ]
}
```

Los demás campos del manifest se omiten aquí solamente para enfocar la demo.

## Resultado esperado

`local-context.json` contiene una colección como la del
[ejemplo completo](examples/local-context-modified-files.json):

```json
{
  "modified_files": [
    {
      "path": "contracts/payments.yaml",
      "change_status": "modified",
      "provenance": {
        "provider": "github",
        "repository": "example/widgets",
        "pull_request": 123,
        "source": "manifest.changed_files"
      }
    }
  ]
}
```

## Demo ejecutable

```bash
dotnet test \
  --configuration Release \
  --filter 'FullyQualifiedName~LocalContextBuilderTests.TransportsModifiedFilesWithNormalizedStatusProvenanceAndStableOrder'
```

## Garantías demostradas

- se conservan path, estado demostrado, orden estable y procedencia mínima;
- `removed` de GitHub se normaliza como `deleted`;
- duplicados exactos se colapsan por primera aparición;
- paths inválidos, estados no soportados y duplicados contradictorios fallan de
  forma cerrada;
- no se transportan diff, patch, contenido ni metadata interpretada;
- los cinco artefactos de entrada permanecen intactos;
- Capability-002 valida y conserva la colección sin acceder al filesystem;
- las Rules existentes no generan Evidence, Claims, Hypotheses ni Findings
  adicionales.
- formato y orden de propiedades JSON equivalentes producen la misma identidad;
- el orden contractual de las colecciones se preserva y participa de la
  identidad.

## Limitaciones

- sólo se admiten `added`, `modified`, `removed` y `renamed` desde el manifest;
- no se conserva `previous_path` de un rename porque no pertenece al contrato
  mínimo autorizado;
- no se detectan contratos ni breaking changes;
- la ausencia de `modified_files` en un contexto v1 anterior no permite inferir
  si el Pull Request realmente carecía de archivos modificados.

## Decisión de continuar

La frontera transporta la evidencia mínima requerida. Contract Change Detection
continúa fuera de alcance hasta que se autorice e implemente explícitamente una
Rule posterior.
