# Project Priorities

## P0 – Security & Maintenance
- Upgrade runtime/dependencies (target .NET 8), monitor CVEs, and enforce regular patch cadence.
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
