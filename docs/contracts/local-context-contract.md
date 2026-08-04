# Local Context Contract

- Contract: `local-context.json`
- Contract version: 1
- Status: Accepted
- Governing decision:
  [ADR-013 — Local Context Contract](../architecture/adr/ADR-013-local-context-contract.md)

## Purpose

`local-context.json` carries the retrieved, validated and ordered local context
for one VS-001 execution across the boundary between Context Retrieval and
inference. It contains existing evidence and metadata; it does not contain
inferences, recommendations or an Architecture Review.

## Producer

The sole producer is the **Context Retrieval Pipeline**. It must publish the
artifact only after its inputs have been validated as a coherent set.

Intermediate retrieval artifacts are not part of this contract.

## Consumers

The **Inference Engine** is the consumer this contract exists to isolate: this
document defines and protects the boundary between it and Context Retrieval. The
Inference Engine must use this artifact as its only source of retrieved context
and must not use its source identifiers to access Git providers, repositories or
documentation directly.

Any other reader of this artifact (for example, offline evaluation or audit
tooling) is bound by the same restriction and does not change the two-party
boundary this contract governs: the Context Retrieval Pipeline as sole producer,
and the Inference Engine as the consumer whose isolation from GitHub, README and
the internal retrieval process this contract guarantees.

## Contract shape

The top-level object contains:

| Field          | Type           | Requirement | Meaning                                     |
| -------------- | -------------- | ----------- | ------------------------------------------- |
| `repository`   | string         | Required    | Source repository identifier and provenance |
| `pull_request` | positive int   | Required    | Source change identifier and correlation    |
| `pack_id`      | string         | Required    | Stable identifier for the source revision   |
| `documents`    | document array | Required    | Context ordered by retrieval ranking        |

Each document contains:

| Field      | Type                 | Requirement | Meaning                                    |
| ---------- | -------------------- | ----------- | ------------------------------------------ |
| `path`     | string               | Required    | Source-relative document path              |
| `score`    | non-negative integer | Required    | Deterministic retrieval score              |
| `reason`   | string               | Required    | Deterministic reason for the score         |
| `name`     | string or null       | Required    | Explicitly extracted name                  |
| `purpose`  | string or null       | Required    | Explicitly extracted purpose               |
| `content`  | string or null       | Required    | Exact retrieved content when readable      |
| `error`    | string               | Conditional | Generic retrieval error when not readable  |
| `status`   | string               | Required    | Metadata extraction status                 |
| `evidence` | evidence array       | Required    | Source lines supporting extracted metadata |

Allowed `reason` values are:

- `same_directory`;
- `nearest_ancestor`;
- `repository_root`.

Allowed `status` values are:

- `extracted`;
- `missing_name`;
- `missing_purpose`;
- `insufficient`.

Each evidence item contains `field`, `text`, `source_line_start` and
`source_line_end`. `field` is `name` or `purpose`; line numbers are one-based
and identify the explicit source evidence.

For a readable document, `content` is a string and `error` is absent. For an
unreadable document, `content` is `null` and `error` contains the generic error
already produced by retrieval.

The order of `documents` is significant and must be preserved by consumers.

## Compatibility rules

A compatible producer:

- preserves the meaning and type of every existing field;
- preserves required fields and the ordering semantics of `documents`;
- preserves the meaning of existing enum values;
- does not replace explicit absence with invented values;
- does not add inferred information to the retrieval contract;
- produces deterministic bytes for identical inputs under the same contract
  version.

Consumers must reject malformed artifacts or artifacts that do not conform to a
contract version they support. Consumers may ignore new optional fields
introduced compatibly, but must not infer missing required fields.

The following changes are incompatible:

- removing or renaming a required field;
- changing a field type or meaning;
- making an optional field required;
- changing document ordering semantics;
- removing or redefining an existing enum value;
- changing the identity semantics of `pack_id`;
- adding source access as a consumer responsibility.

## Versioning

The current contract is version 1. Version 1 is identified by this canonical
document; the JSON artifact does not contain a version field. Adding such a
field would be a separate contract change and is not authorized here.

Compatible clarifications and optional additions may update this document while
retaining version 1. Incompatible changes require a new contract version,
migration guidance and an Architecture Decision Record.

**Los cambios incompatibles requieren un ADR.**

## Stability

Version 1 is stable for the transition from the completed VS-001 Context
Retrieval Pipeline to the Inference Engine. Intermediate filenames, internal
modules and retrieval steps are not stable interfaces.

Stability does not freeze future context categories or providers. GitLab, Jira,
Confluence, ADRs, runbooks, incident reports and other sources may contribute
context in the future if they preserve this boundary and follow the
compatibility rules above.
