# Task List (Prioritized)

1. **Fix DNS compressed-name parsing regression** — Address issue #26 in `Dns/DnsProtocol`/`DnsMessage` and add regression tests to ensure compliant decoding/encoding under RFC 1035.
2. **Authoritative response verification suite** — Create integration tests running `dns-cli` with sample zones to validate AA/RA/SOA behavior and caching semantics (P0 reliability).
3. **Implement RFC 2308-compliant caching** — Extend `DnsServer`/`DnsCache` to honor positive/negative TTLs, flush stale entries, and cover with tests.
4. **Harden SmartZoneResolver concurrency** — Ensure zone reloads and address dispensers are thread-safe and resilient to null/empty provider updates.
5. **Health-probe simulation tests** — Build deterministic tests for `Dns/ZoneProvider/IPProbe` strategies to guarantee consistent handling of latency/timeouts.
6. **Migrate to Microsoft.Extensions.DependencyInjection** — Replace Ninject usage in `Dns/Program.cs` and related projects with built-in DI, updating configuration wiring accordingly.
7. **Upgrade solution to .NET 8** — Move all projects to `net8.0`, update dependencies, and validate builds/tests across Windows/Linux.
8. **Instrument DNS & HTTP surfaces (OpenTelemetry-ready)** — Add metrics/tracing hooks (without bundling collectors) so operators can export via OTLP.
9. **Secure HTTP admin surface** — Provide configuration for bindings/authz and document operational guidance to avoid exposing diagnostic endpoints unintentionally.
10. **Complete BIND zone provider** — Implement parsing logic for `Dns/ZoneProvider/Bind`, supporting `$ORIGIN`, `$TTL`, and core record types.
11. **Add dynamic configuration providers** — Introduce REST/service-backed zone/configuration sources with validation and hot reload pipelines.
12. **Implement parental/time-based/MAC policies** — Deliver requested zone behaviors (issues #3/#4/#9) leveraging the SmartZoneResolver framework.
13. **Extend health probes (HTTP/TCP)** — Add richer probe strategies with retries/weights within the IPProbe provider.
14. **Enhance HTTP operational UX** — Replace HTML dumps with JSON/metrics endpoints and basic dashboards aligned with the observability plan.
