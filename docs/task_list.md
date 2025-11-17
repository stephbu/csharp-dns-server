# Task List (Prioritized)

1. [x] **T01 – Fix DNS compressed-name parsing regression** — Address issue #26 in `Dns/DnsProtocol`/`DnsMessage` and add regression tests to ensure compliant decoding/encoding under RFC 1035.
2. [x] **T02 – Authoritative response verification suite** — Added `dnstest` integration coverage that boots `dns-cli` with deterministic zone/config assets to assert AA/RA/SOA flags, TTL stability, and NXDOMAIN authority responses (run via `dotnet test csharp-dns-server.sln`).
3. [ ] **T03 – Implement RFC 2308-compliant caching** — Extend `DnsServer`/`DnsCache` to honor positive/negative TTLs, flush stale entries, and cover with tests (issue #15).
4. [ ] **T04 – Harden SmartZoneResolver concurrency** — Ensure zone reloads and address dispensers are thread-safe and resilient to null/empty provider updates.
5. [ ] **T05 – Health-probe simulation tests** — Build deterministic tests for `Dns/ZoneProvider/IPProbe` strategies to guarantee consistent handling of latency/timeouts.
6. [ ] **T06 – Migrate to Microsoft.Extensions.DependencyInjection** — Replace Ninject usage in `Dns/Program.cs` and related projects with built-in DI, updating configuration wiring accordingly.
7. [x] **T07 – Upgrade solution to .NET 8** — Move all projects to `net8.0`, update dependencies, and validate builds/tests across Windows/Linux.
8. [ ] **T08 – Instrument DNS & HTTP surfaces (OpenTelemetry-ready)** — Add metrics/tracing hooks (without bundling collectors) so operators can export via OTLP (issue #16).
9. [x] **T09 – Fix CA2241 format warning** — Update the logging call in `Dns/DnsServer.cs` (line 250) to use the correct string-format arguments so builds are warning-free.
10. [ ] **T10 – Secure HTTP admin surface** — Provide configuration for bindings/authz and document operational guidance to avoid exposing diagnostic endpoints unintentionally.
11. [ ] **T11 – Complete BIND zone provider** — Implement parsing logic for `Dns/ZoneProvider/Bind`, supporting `$ORIGIN`, `$TTL`, and core record types (addresses “Static Zone declaration file” issue #1).
12. [ ] **T12 – Add dynamic configuration providers** — Introduce REST/service-backed configuration sources with validation and hot reload pipelines (issues #7/#8/#19).
13. [ ] **T13 – Implement parental/time-based/MAC policies** — Deliver requested zone behaviors (issues #3/#4/#9) leveraging the SmartZoneResolver framework.
14. [ ] **T14 – Extend health probes (HTTP/TCP)** — Add richer probe strategies with retries/weights within the IPProbe provider.
15. [ ] **T15 – Enhance HTTP operational UX** — Replace HTML dumps with JSON/metrics endpoints and improvements requested in issue #25.
16. [ ] **T16 – Trace logging tools** — Implement structured trace logging and tooling per issue #10.
17. [ ] **T17 – Implement BitPacker.Write** — Complete the BitPacker.Write implementation and accompanying tests (issue #11).
18. [ ] **T18 – Windows/NT service packaging** — Add installers/scripts so the server can run as a Windows service (issue #5).
19. [ ] **T19 – DNSSEC support** — Add foundational DNSSEC record handling and validation paths (issue #2).
20. [ ] **T20 – Documented static zone workflow** — Provide a simple static zone declaration option (issue #1) for setups that don’t rely on the BIND parser.
21. [ ] **T21 – Fix AppVeyor build configuration** — Repair `appveyor.yml` so CI restores/builds/tests the .NET solution using the current SDK/runtime matrix.
22. [x] **T22 – Add GitHub Actions CI pipeline** — Introduce a workflow under `.github/workflows/` that restores, builds, and tests the solution on Windows/Linux runners aligned with PR gating guidance.
23. [x] **T23 – Correct IPv4 RDATA endianness (Critical)** — Fix `ANameRData.Parse` so addresses parsed from wire format are not byte-swapped before being forwarded to clients; add regression tests.
24. [ ] **T24 – Stabilize UDP listener shutdown & endpoint capture (High)** — Ensure `UdpListener.Start` exits cleanly after `Stop()` and capture the remote endpoint per receive so responses aren’t misrouted.
25. [ ] **T25 – Support larger UDP payloads (Medium)** — Increase `UdpListener` buffer sizing and/or detect truncated packets so EDNS-sized responses don’t silently corrupt parsing.
26. [ ] **T26 – Allow full 8-bit DNS labels (Medium)** — Relax `DnsProtocol.ReadString` ASCII enforcement in line with RFC 2181 so internationalized/underscored names don’t throw.
27. [x] **T27 – Refresh VS Code launch/tasks configs** — Update `.vscode/launch.json` and `tasks.json` to mirror the current build/test/debug workflow so contributors get accurate defaults.
