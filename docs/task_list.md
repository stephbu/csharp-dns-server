# Task List (Prioritized)

1. [ ] **T01 – Fix DNS compressed-name parsing regression** — Address issue #26 in `Dns/DnsProtocol`/`DnsMessage` and add regression tests to ensure compliant decoding/encoding under RFC 1035.
2. [ ] **T02 – Authoritative response verification suite** — Create integration tests running `dns-cli` with sample zones to validate AA/RA/SOA behavior and caching semantics (P0 reliability).
3. [ ] **T03 – Implement RFC 2308-compliant caching** — Extend `DnsServer`/`DnsCache` to honor positive/negative TTLs, flush stale entries, and cover with tests.
4. [ ] **T04 – Harden SmartZoneResolver concurrency** — Ensure zone reloads and address dispensers are thread-safe and resilient to null/empty provider updates.
5. [ ] **T05 – Health-probe simulation tests** — Build deterministic tests for `Dns/ZoneProvider/IPProbe` strategies to guarantee consistent handling of latency/timeouts.
6. [ ] **T06 – Migrate to Microsoft.Extensions.DependencyInjection** — Replace Ninject usage in `Dns/Program.cs` and related projects with built-in DI, updating configuration wiring accordingly.
7. [ ] **T07 – Upgrade solution to .NET 8** — Move all projects to `net8.0`, update dependencies, and validate builds/tests across Windows/Linux.
8. [ ] **T08 – Instrument DNS & HTTP surfaces (OpenTelemetry-ready)** — Add metrics/tracing hooks (without bundling collectors) so operators can export via OTLP.
9. [ ] **T09 – Fix CA2241 format warning** — Update the logging call in `Dns/DnsServer.cs` (line 250) to use the correct string-format arguments so builds are warning-free.
10. [ ] **T10 – Secure HTTP admin surface** — Provide configuration for bindings/authz and document operational guidance to avoid exposing diagnostic endpoints unintentionally.
11. [ ] **T11 – Complete BIND zone provider** — Implement parsing logic for `Dns/ZoneProvider/Bind`, supporting `$ORIGIN`, `$TTL`, and core record types.
12. [ ] **T12 – Add dynamic configuration providers** — Introduce REST/service-backed zone/configuration sources with validation and hot reload pipelines.
13. [ ] **T13 – Implement parental/time-based/MAC policies** — Deliver requested zone behaviors (issues #3/#4/#9) leveraging the SmartZoneResolver framework.
14. [ ] **T14 – Extend health probes (HTTP/TCP)** — Add richer probe strategies with retries/weights within the IPProbe provider.
15. [ ] **T15 – Enhance HTTP operational UX** — Replace HTML dumps with JSON/metrics endpoints and basic dashboards aligned with the observability plan.
