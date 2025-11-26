# csharp-dns-server – Product Requirements

## 1. Overview
- **Purpose**: capture the feature, testing, and operational requirements needed to evolve the C# DNS server into a production-ready, multi-platform service and seed long-term maintenance/AI-assisted development.
- **Current State**: unified `.NET 8` solution (`Dns`, `dns-cli`, `dnstest`) providing an authoritative UDP DNS server with pluggable zone providers (CSV file, IP-health probes) and a minimal HTML status endpoint. The .NET 8 migration is complete. No production deployments exist yet.
- **Primary Goals**
  - Ship a reliable DNS service with extensible zone data sources, configurable health probes, and first-class observability.
  - Establish a comprehensive automated testing strategy.
  - Plan the .NET runtime upgrade path (targeting .NET 8 LTS) across Windows/Linux targets.
  - Enable AI-accelerated development via clear contributor guidelines (`AGENTS.md`).

## 2. Functional Requirements
### 2.1 DNS Resolution & Protocol Support
- Maintain authoritative responses for configured zones with round-robin rotation and delegation to upstream DNS when needed.
- **Cross-Platform Endianness**: support both big-endian and little-endian host systems. DNS wire format uses network byte order (big-endian) per RFC 1035; all multi-byte field reads/writes must handle endian conversion correctly using `BinaryPrimitives` or equivalent safe APIs.
- Expand record coverage beyond A/PTR:
  - Support AAAA, CNAME, and MX records emitted by zone providers.
  - Provide an extension point for future record types (SRV, TXT).
- Implement RFC-compliant caching with configurable TTL respect (`Issue #15`).
- Add DNSSEC support in phases: (1) EDNS(0) prerequisite, (2) DNSSEC record parsing (RRSIG, DNSKEY, DS, NSEC), (3) validating resolver for upstream responses, and (4) future authoritative zone signing (`Issue #2`). Coordinate buffer/memory changes with performance tuning (`Issue #29`).
- Fix compressed-pointer parsing defects and add regression tests (`Issue #26`).

### 2.2 Zone Providers & Configuration
- **BIND Zone Provider**: implement forward-zone parsing, $ORIGIN/$TTL handling, and change detection to replace the current `NotImplementedException`.
- **Dynamic Configuration**:
  - Support multiple configuration providers (file watcher, REST, database) with a standard schema (`Issues #19, #9, #8`).
  - Enable hot reload with validation, rollback, and observable serial increments.
- **Parental Control/Time-Based Rules**: ingest blocklists or schedules from configuration or services (`Issues #3, #9`).
- **MAC Scoped Records**: extend zone syntax to emit responses based on client identity when available (`Issue #4`).
- **Health-Probe Enhancements**:
  - IPProbe provider must support HTTP/TCP/Synthetic probes, retries, and weighted routing.
  - Record probe latency/availability for monitoring.

## 3. Testing & Quality Requirements
- **Unit Tests**: expand beyond bit packing and protocol parsing to cover zone provider logic, DNS caching, HTTP handlers, and SmartZoneResolver behaviors.
- **Integration Tests**:
  - Spin up the DNS server with test zone providers and assert full query/response flows.
  - Simulate upstream delegation and caching behavior.
  - Exercise IPProbe pathways with mocked probes/timeouts.
- **Regression Suites**: include fixtures for the compressed-pointer bug (#26) and BitPacker.Write implementation (#11).
- **Performance/Load**: define baseline throughput/latency targets and create repeatable load tests.
- **CI Gates**: `dotnet build` + `dotnet test` required on every PR, with optional fuzz testing for DNS message parsing.

## 4. Observability & Monitoring
- Replace HTML dumps with structured JSON and/or metrics endpoints (Prometheus/OpenTelemetry).
- Track DNS/HTTP request counts, latencies, cache hit/miss rates, probe health, and zone reload stats (`Issue #16`).
- Implement structured logging with trace correlation (`Issue #10`).
- Provide `/health` endpoints for liveness/readiness plus synthetic probes for upstream dependencies.
- Document alerting thresholds and dashboards for initial production rollout.

## 5. Deployment & Operations
- **Targets**: support Windows and Linux deployments (console, Windows Service per `Issue #5`, and container/systemd scenarios).
- **Configuration Management**: document secrets handling, validation pipelines, and rollback procedures.
- **Networking**: handle UDP port conflicts gracefully (e.g., Docker ICS note from README) and expose configurable listener ports.
- **Scalability**: specify requirements for running multiple instances (state sharing, consistent hashing, or health-probe coordination).
- **Security**: define TLS requirements for HTTP endpoints, access control for admin APIs, and logging of configuration changes.

## 6. .NET Maintenance & Upgrades
- **Target Runtime**: ✅ **COMPLETED** — `Dns`, `dns-cli`, and `dnstest` now target `.NET 8` (LTS) for multi-platform support.
- **Dependency Status**: Ninject removed; using `Microsoft.Extensions.DependencyInjection`. Some `Microsoft.Extensions.*` packages remain at 3.1.9 and should be upgraded to 8.x versions.
- **Completed Migration Steps**:
  - Updated SDK/TFM to `net8.0` across all projects.
  - Resolved API changes and verified builds on Windows/macOS/Linux.
- **Ongoing Maintenance**: establish a quarterly review for SDK patches, dependency updates, and security advisories; codify required validation steps (unit, integration, smoke tests). Consider upgrading remaining 3.1.9 packages to .NET 8 compatible versions.

## 7. AI Agent Enablement
- Deliver an `AGENTS.md` modeled after [OpenAI’s reference](https://raw.githubusercontent.com/openai/agents.md/refs/heads/main/AGENTS.md) with:
  - Repository layout, key projects, and entry points.
  - Build/test commands, sample run instructions, and common gotchas.
  - Coding standards, review policies, and MIT license reminders.
  - Scope limitations: agents may modify code/tests only (no infrastructure or deployment assets).
  - Validation checklist before submitting PRs (run tests, lint, documentation updates).
- Provide guidance for prioritizing issues (testing, monitoring, zone providers) so automated contributors align with roadmap.

## 8. Roadmap Seeds (from open issues)
- `#1 Static Zone declaration file`
- `#2 DNS-Sec support`
- `#3 Time-based DNS resolver`
- `#4 MAC-address scoped zones`
- `#5 Install/run as NT Service`
- `#7/#8/#9/#19` configuration providers & dynamic updates
- `#10` trace logging tools
- `#11` BitPacker.Write
- `#15` DNS caching
- `#16` Metrics support
- `#25` HTTP server improvements
- `#26` Compressed string pointer parsing bug

## 9. Success Criteria
- All code runs on .NET 8 across Windows/Linux, with published deployment artifacts.
- Automated tests cover DNS parsing, caching, zone providers, and health probes; CI enforced.
- Metrics/logging are exported in structured formats and consumed by dashboards.
- At least one additional zone provider (BIND or equivalent) and enhanced health probes are production-ready.
- `AGENTS.md` is published and successfully guides AI/automation contributions within the defined scope.
