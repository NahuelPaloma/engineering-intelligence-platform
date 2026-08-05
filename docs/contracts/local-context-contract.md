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

| Field            | Type                | Requirement | Meaning                                                        |
| ---------------- | ------------------- | ----------- | -------------------------------------------------------------- |
| `repository`     | string              | Required    | Source repository identifier and provenance                    |
| `pull_request`   | positive int        | Required    | Source change identifier and correlation                       |
| `pack_id`        | string              | Required    | Stable identifier for the source revision                      |
| `modified_files` | modified file array | Optional    | Authorized changed-file evidence when supplied by the producer |
| `documents`      | document array      | Required    | Context ordered by retrieval ranking                           |

Each modified file contains:

| Field           | Type       | Requirement | Meaning                                       |
| --------------- | ---------- | ----------- | --------------------------------------------- |
| `path`          | string     | Required    | Valid repository-relative changed-file path   |
| `change_status` | string     | Required    | Demonstrated change kind                      |
| `provenance`    | provenance | Required    | Minimal reference to the authorized PR source |

Allowed `change_status` values are:

- `added`;
- `modified`;
- `deleted`;
- `renamed`.

The GitHub producer maps the source status `removed` to `deleted`. It preserves
`added`, `modified` and `renamed`. Any other source status is unsupported and
must fail closed rather than be copied or inferred into this contract.

`path` is validated lexically. It must be non-empty, repository-relative, use
forward-slash separators and contain neither control characters nor empty, `.`
or `..` segments. The producer and consumer do not resolve symlinks or use this
value to access the filesystem at the inference boundary.

Each `provenance` contains only `provider`, `repository`, `pull_request` and
`source`. `repository` and `pull_request` must match the top-level identity;
`source` is `manifest.changed_files`. These identifiers demonstrate origin and
correlation but never authorize further source access.

The order of `modified_files` is the stable first-occurrence order from
`manifest.changed_files`. Exact duplicates are collapsed deterministically;
conflicting entries for one path are invalid. An empty array explicitly means
that the producer observed zero supported changed files.

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

`modified_files` is an additive optional field in version 1. Updated producers
always emit it, including as an empty array. Updated consumers accept older
version 1 artifacts where it is absent, but preserve that absence as
`not_provided` rather than treating it as an empty Evidence collection. A
present empty array is `available` Evidence with zero elements. Older consumers
remain compatible because they may ignore the optional field. Presence of the
field does not authorize current reasoning Rules to create Claims from it.

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

The `modified_files` addition retains version 1 because it is optional, does not
alter existing fields or ordering semantics and is ignorable by older consumers.
The artifact still has no physical `contract_version` field; `local-context-v1`
remains the supported conceptual contract identifier.

`pack_id` continues to identify repository, Pull Request, base SHA and head SHA;
the new collection does not change it. `execution_id` identifies the admitted
context and therefore includes a deterministic canonical identity of the known
version 1 fields, the supported conceptual contract identifier and the Rule set
identifier. It does not use timestamps, paths to the artifact, machine state or
other external values.

Canonicalization uses fixed property ordering for known contract objects,
normalizes irrelevant JSON formatting and object property order, and preserves
the contractual order of `modified_files`, `documents` and nested Evidence. It
includes an explicit `available` or `not_provided` presence marker before
hashing, so absent, present-empty and present-populated collections have
distinct identities. Changes to `path`, `change_status` or any admitted
provenance field also change the canonical identity. Unknown optional fields
ignored by a version 1 consumer do not enter that consumer's admitted identity.

Artifacts with and without `modified_files` can retain the same `pack_id`
because they describe the same source revision, but they produce distinct
`execution_id` values because their admitted contextual coverage differs. No
current Rule consumes the new collection, so the functional Findings and
downstream product behavior remain unchanged.

**Los cambios incompatibles requieren un ADR.**

## Stability

Version 1 is stable for the transition from the completed VS-001 Context
Retrieval Pipeline to the Inference Engine. Intermediate filenames, internal
modules and retrieval steps are not stable interfaces.

Stability does not freeze future context categories or providers. GitLab, Jira,
Confluence, ADRs, runbooks, incident reports and other sources may contribute
context in the future if they preserve this boundary and follow the
compatibility rules above.

## Document history

| Date       | Change                                                                       | Result     |
| ---------- | ---------------------------------------------------------------------------- | ---------- |
| 2026-08-04 | Initial canonical contract governed by ADR-013                               | `Accepted` |
| 2026-08-04 | Added optional `modified_files` Evidence without changing contract v1        | `Accepted` |
| 2026-08-04 | Preserved absence semantics and bound execution identity to admitted context | `Accepted` |
