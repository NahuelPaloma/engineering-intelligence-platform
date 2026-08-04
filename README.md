# Engineering Intelligence Platform

Engineering Intelligence Platform (EIP) is an extensible platform that
transforms engineering knowledge into actionable intelligence.

## Current Status

🚧 Early Design Phase

## Documentation

- [Product Vision](docs/vision/)
- [Architecture](docs/architecture/)

Implementation has not started.

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
└── readme-contents.json
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

See the [Increment 1 demo](docs/demos/VS-001/increment-01.md) and its
[example manifest](docs/demos/VS-001/examples/increment-01-manifest.json). The
next baby step is recorded in the
[Increment 2.1 demo](docs/demos/VS-001/increment-02-1.md), with an
[example readmes file](docs/demos/VS-001/examples/increment-02-1-readmes.json).
Content reading is demonstrated in
[Increment 2.2](docs/demos/VS-001/increment-02-2.md), with an
[example readme contents file](docs/demos/VS-001/examples/increment-02-2-readme-contents.json).
