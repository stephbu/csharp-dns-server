# AGENTS GUIDE

> Scope: assistants may edit **code, tests, and documentation** in this repository. Infrastructure/deployment assets remain off-limits unless explicitly approved.

## 1. Mission
- Maintain and extend the `csharp-dns-server` so it becomes a production-ready DNS service with rich testing, observability, and zone-provider capabilities.
- Follow the roadmap in `docs/product_requirements.md`, respect the priority tiers in `docs/priorities.md` (P0 reliability/protocol accuracy, P1 security & maintenance, P2 features), and focus on open GitHub issues aligned with those tiers.
- Reference the prioritized backlog in `docs/task_list.md` when picking up work to stay aligned with near-term goals.

## 2. Repository Orientation
| Project | Purpose | Notes |
|---------|---------|-------|
| `Dns/` | Core library implementing DNS protocol, server loop, zone providers, HTTP status surface. | Entry point: `Dns/Program.cs`. Zone providers under `Dns/ZoneProvider`. |
| `dns-cli/` | Console host that runs the DNS server for local testing. | Mirrors `Dns/appsettings.json`. |
| `dnstest/` | xUnit suite covering protocol/utility components. | Expand here before adding new test assemblies. |
| `docs/` | Specs, PRD and future design docs. | `docs/product_requirements.md` drives priorities. |

Key classes & files:
- `Dns/Program.cs`: wiring for DI/config/servers via `Microsoft.Extensions.DependencyInjection`.
- `Dns/DnsServer.cs`: UDP DNS loop and upstream forwarding.
- `Dns/SmartZoneResolver.cs`: in-memory zone cache & round-robin dispenser.
- `Dns/HttpServer.cs`: embedded status/diagnostics surface.
- `Dns/ZoneProvider/**`: implementations (CSV, IP probes, BIND placeholder).

## 3. Getting Started
```bash
# restore & build
dotnet build csharp-dns-server.sln

# run tests
dotnet test csharp-dns-server.sln

# run server (localhost)
cd dns-cli
dotnet run -- ./appsettings.json
```
Gotchas:
- UDP port 53 may be taken by Docker/ICS on Windows; change listener port in `appsettings.json`.
- `Dns/appsettings.json` is copied to output; edit with care when adding samples.
- Zone providers may depend on local files (CSV) or ping-able IPs—mock or isolate tests accordingly.

## 4. Coding Standards
- C# 8 / .NET 3.1 currently, migrating to .NET 8 (see PRD). Prefer idiomatic C# and existing project style.
- Keep ASCII unless file already uses Unicode.
- Windows (/r/n) line delimiters
- Prefer spaces not tabs
- Add comments only where logic is non-obvious.
- ```dotnet format``` all code before submission
- MIT license headers already present — preserve them.

### Endianness
- **DNS uses network byte order (big-endian)** for all multi-byte values per RFC 1035.
- The codebase supports **both big-endian and little-endian host systems** via the `SwapEndian()` extension methods in `Extensions.cs`.
- `SwapEndian()` checks `BitConverter.IsLittleEndian` and only swaps bytes when necessary.
- Use `.SwapEndian()` when reading/writing multi-byte DNS fields (QueryID, counts, TTL, Type, Class, etc.).
- Semantic aliases `NetworkToHost()` and `HostToNetwork()` are available for clarity.
- **Test coverage**: `dnstest/EndianTests.cs` validates correct byte order on any platform.

## 5. Allowed / Disallowed Work
- ✅ Modify C# source, tests, sample configs, docs within `docs/` and root (`AGENTS.md`, README).
- ✅ Add new tests or scripts that live in-repo (delete temporary tooling before submitting).
- 🚫 Do **not** edit deployment/infrastructure assets (Dockerfiles, systemd service files, external config stores) unless explicitly authorized by a maintainer.
- 🚫 No secret management or external network calls without approval.

## 6. Workflow
1. **Plan**: understand issue context (link to PRD sections). If multiple files touched, outline steps before coding.
2. **Implement**: keep changes scoped; ensure zone providers/tests stay deterministic.
3. **Validate**: run `dotnet build` and relevant `dotnet test` subsets. Document skipped tests or environment assumptions.
4. **Document**: update `docs/` where appropriate.  Update README when adding features, config switches and any other project-wide relevant information.
5. **Submit Pull Request**: run `dotnet format`.  Follow the contribution workflow in README (squash commits, include rationale).

## 7. Testing Expectations
- Minimum: `dotnet test csharp-dns-server.sln`.
- The dns-cli integration harness (`dnstest/Integration` + `DnsCliAuthoritativeBehaviorTests`) runs automatically with `dotnet test`, spins up `dns-cli` using the sample assets in `dnstest/TestData`, and needs free TCP/UDP ports; keep configs deterministic when extending it.
- For networking changes, add/extend unit tests in `dnstest` or new integration fixtures.
- Capture repro cases for fixed bugs (#26 compressed pointers, #11 BitPacker write) and ensure tests fail before fixes.

## 8. Observability & Diagnostics
- Prefer structured logging (use `Console.WriteLine` only as placeholder).
- When adding metrics or tracing, integrate with future Prometheus/OTel plan (see PRD §4).

## 9. Communication & Review
- DO STOP and ask questions if there is missing, ambiguous, or inconsistent information.
- Document assumptions and remaining risks in PR descriptions.
- If blocked by environmental constraints (e.g., network access), leave instructions for a maintainer.
- Keep PRs focused; split unrelated fixes.

## 10. Safety & Guardrails
- Treat `appsettings.json` samples as templates—do not embed secrets.
- Respect the agent scope limit: no infrastructure edits.
- When unsure, create an issue/comment instead of guessing.

Thank you for helping build a reliable C# DNS server!
