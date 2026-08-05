# Engineering Intelligence Platform

Engineering Intelligence Platform (EIP) is an extensible platform that
transforms engineering knowledge into actionable intelligence.

## Current Status

VS-001 now integrates the completed Context Retrieval Pipeline with
Capability-002. Architecture Review Intelligence consumes the official Inference
Report and produces a deterministic Architecture Review Context Pack.

## Documentation

- [Product Vision](docs/vision/)
- [Architecture](docs/architecture/)
- [Local Context Contract](docs/contracts/local-context-contract.md)

## Development

VS-001 uses .NET 9 for its pilot implementation. This selects the runtime for
the pilot, not for EIP as a whole.

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet format --verify-no-changes --no-restore
```

The solution contains one executable (`Eip.Cli`) and one test project
(`Eip.Tests`). Logical boundaries will begin as namespaces and folders inside
the executable.

### VS-001 Increment 1 — Evidence Collection

Collect reproducible evidence from a GitHub Pull Request:

```bash
export EIP_GITHUB_REPOSITORIES="org/repo"
export GITHUB_TOKEN="<fine-grained-read-only-token>" # Optional for public repositories
dotnet run --project src/Eip.Cli -- review https://github.com/org/repo/pull/123
```

`EIP_GITHUB_REPOSITORIES` is required and accepts a comma-separated allowlist of
`owner/repository` values. Repository matching is case-insensitive. Missing,
empty or non-matching configuration rejects the request before GitHub is
contacted.

The command prints the generated file path:

```text
output/<pack-id>/
├── manifest.json
├── readmes.json
├── readme-contents.json
├── readme-metadata.json
├── readme-ranking.json
├── local-context.json
├── inference-execution.json
├── inference-report.json
└── context-pack.md
```

The pack ID is derived from repository, Pull Request number, base SHA and head
SHA. Re-running the command for the same revision updates the same manifest. The
increment only collects Pull Request metadata, commits and changed files; it
does not build context or produce analysis.

Run the command from the corresponding repository checkout. `readmes.json`
contains only candidate paths for `README.md`, `Readme.md` or `readme.md` found
in changed-file directories, their nearest ancestors and the repository root.
That discovery step does not read file contents.

`readme-contents.json` reads only the candidates listed in `readmes.json`, keeps
their order and preserves their complete text. An unreadable candidate records a
generic per-document error without stopping the remaining reads.

`readme-metadata.json` deterministically extracts a name from the first explicit
H1 and a purpose from the first nearby descriptive paragraph. Missing evidence
produces `null`; no external source or inference engine is used.

`readme-ranking.json` combines paths from `readme-metadata.json` with discovery
relations from `readmes.json`. It applies fixed location scores and orders ties
by ordinal path without reading repository files or README content.

`local-context.json` consolidates the five preceding artifacts into one
deterministic input for the future Inference Engine. It is the official contract
between Context Retrieval and inference, as established by
[ADR-013](docs/architecture/adr/ADR-013-local-context-contract.md). It preserves
changed-file Evidence, ranking order, content, metadata, status and evidence
without rereading or interpreting the repository. `modified_files` contains only
validated paths, demonstrated change status and minimal Pull Request provenance;
it does not contain diffs, content or contract classification. Inconsistent
inputs fail without producing partial context.

The Input Boundary distinguishes an older context where `modified_files` was not
provided from a present empty collection. `pack_id` continues to identify the
Pull Request revision; `execution_id` additionally binds the canonical admitted
context, supported contract and Rule Set.

Capability-002 is implemented end to end. The Inference Engine now transforms a
valid `local-context.json` into a deterministic, contractually validated
`inference-report.json`. Inference Report Builder remains functionally pure and
Validation alone classifies and authorizes publication. The separate
`inference-execution.json` technical record references the published report.
Neither artifact contains Recommendations or Decisions.

Capability-003 Increment 1 admits Modified File Evidence through the shared
Input Boundary without reading repository files. A missing `modified_files`
field produces unknown Coverage and a total Abstention; a present empty array
produces complete Coverage; a populated collection remains partially uncovered
with cause `no_candidate_rules_registered`. Contract Candidates and Contract
Types are not implemented.

VS-001 consumes `inference-report.json` as its only functional input after
Context Retrieval. It renders validated Findings into `context-pack.md` and does
not reread README content, traverse the repository, create reasoning units or
recalculate Confidence. `inference-execution.json` remains an internal technical
record and is never used to construct the Architecture Review Context Pack.

See the [Increment 1 demo](docs/demos/VS-001/increment-01.md) and its
[example manifest](docs/demos/VS-001/examples/increment-01-manifest.json). The
next baby step is recorded in the
[Increment 2.1 demo](docs/demos/VS-001/increment-02-1.md), with an
[example readmes file](docs/demos/VS-001/examples/increment-02-1-readmes.json).
Content reading is demonstrated in
[Increment 2.2](docs/demos/VS-001/increment-02-2.md), with an
[example readme contents file](docs/demos/VS-001/examples/increment-02-2-readme-contents.json).
Deterministic metadata extraction is demonstrated in
[Increment 2.3](docs/demos/VS-001/increment-02-3.md), with an
[example metadata file](docs/demos/VS-001/examples/increment-02-3-readme-metadata.json).
Deterministic location ranking is demonstrated in
[Increment 2.4](docs/demos/VS-001/increment-02-4.md), with an
[example ranking file](docs/demos/VS-001/examples/increment-02-4-readme-ranking.json).
The completed retrieval pipeline is demonstrated in
[Increment 3.0](docs/demos/VS-001/increment-03-0.md), with an
[example local context](docs/demos/VS-001/examples/increment-03-0-local-context.json).
The compatible Local Context Contract evolution is demonstrated in
[Capability-001 — modified file Evidence](docs/demos/Capability-001/local-context-modified-files.md),
with an
[updated example local context](docs/demos/Capability-001/examples/local-context-modified-files.json).
The empty Inference Engine pipeline is demonstrated in
[Capability-002 Increment 0](docs/demos/Capability-002/increment-00.md), with an
[example execution record](docs/demos/Capability-002/examples/increment-00-inference-execution.json).
The first Evidence-to-Claim transition is demonstrated in
[Capability-002 Increment 1](docs/demos/Capability-002/increment-01.md), with an
[example claim execution](docs/demos/Capability-002/examples/increment-01-inference-execution.json).
The first unary Hypothesis is demonstrated in
[Capability-002 Increment 2](docs/demos/Capability-002/increment-02.md), with an
[example hypothesis execution](docs/demos/Capability-002/examples/increment-02-inference-execution.json).
The first consumable Finding is demonstrated in
[Capability-002 Increment 3](docs/demos/Capability-002/increment-03.md), with an
[example finding execution](docs/demos/Capability-002/examples/increment-03-inference-execution.json).
Transversal reasoning controls are demonstrated in
[Capability-002 Increment 4](docs/demos/Capability-002/increment-04.md), with an
[example controlled execution](docs/demos/Capability-002/examples/increment-04-inference-execution.json).
The completed Inference Engine is demonstrated in
[Capability-002 Increment 5](docs/demos/Capability-002/increment-05.md), with an
[example official report](docs/demos/Capability-002/examples/increment-05-inference-report.json).
Execution-profile coexistence is demonstrated in
[Capability-003 Increment 0](docs/demos/Capability-003/increment-00.md).
Modified File Evidence admission and Coverage are demonstrated in
[Capability-003 Increment 1](docs/demos/Capability-003/increment-01.md). The
first complete VS-001 vertical flow is demonstrated in
[VS-001 and Capability-002 integration](docs/demos/VS-001/integration-capability-002.md),
with an
[example Architecture Review Context Pack](docs/demos/VS-001/examples/integration-context-pack.md).
