# Project Priorities

## P0 – Security & Maintenance
- ✅ **COMPLETED**: Runtime upgraded to .NET 8; Ninject replaced with `Microsoft.Extensions.DependencyInjection`.
- Monitor CVEs and enforce regular patch cadence; upgrade remaining 3.1.9 `Microsoft.Extensions.*` packages to 8.x.
- Implement observability guardrails (metrics, logging, tracing) to detect anomalies quickly.
- Plan authentication/authorization for admin surfaces (HTTP endpoints) and adopt secure defaults (TLS, restricted ports).
- Document operational runbooks and incident response procedures.

## P1 – Reliability & Protocol Accuracy
- Ensure the DNS server produces RFC-compliant responses (1034/1035, 2181, 2308, etc.) and handles compressed pointers, caching semantics, and upstream delegation correctly.
- Maintain deterministic behavior under load: thread safety in `DnsServer`, zone reload consistency, and fault tolerance for zone-provider errors.
- Expand automated tests (unit + integration) to catch regressions in parsing, message serialization, and health probes. Failing tests block merges.

## P2 – Feature Growth
- Deliver new zone providers (BIND, dynamic sources), parental-control/time-based policies, MAC-scoped records, and richer health-probe strategies.
- Enhance management surfaces (API/UI) and AI-assist documentation (AGENTS.md) to streamline contributions.
- Iterate on HTTP dashboards/metrics export per roadmap once P0/P1 goals are satisfied.

## Execution Plan Highlights
- **Dependency Injection**: ✅ **COMPLETED** \u2014 migrated from Ninject to `Microsoft.Extensions.DependencyInjection` across `Dns`, `dns-cli`, and supporting libraries.
- **Telemetry Direction**: instrument the DNS server, zone providers, and HTTP endpoints with OpenTelemetry-compatible metrics/traces. External operators are expected to supply collectors/exporters; the codebase will emit OTLP-compatible data but will not bundle collector infrastructure.
- **DNSSEC Roadmap**: implement in phases: (1) EDNS(0) support as prerequisite, (2) DNSSEC record parsing, (3) validating resolver, (4) future authoritative signing. Coordinate buffer work with performance tuning (#29).
- **Roadmap Sync**: keep `docs/product_requirements.md`, this priorities doc, and `AGENTS.md` in sync whenever workstreams change so contributors understand current focus.
