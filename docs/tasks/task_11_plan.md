# Task T11 – Complete BIND Zone Provider

## Goal
Deliver a production-ready `Dns/ZoneProvider/Bind` plugin that can read BIND-style forward-zone files, emit the records SmartZoneResolver expects, and fail fast with actionable diagnostics when zone files are invalid.

## Feasibility
Feasible with current architecture: the zone provider abstraction already allows pluggable data sources, SmartZoneResolver caches zone sets with TTL/round-robin behavior, and docs/product_requirements.md explicitly calls for a BIND provider with `$ORIGIN`/`$TTL` handling and change detection. Work primarily involves parser/validator implementation plus deterministic tests and assets under `dnstest`.

## Plan

1. **Understand Inputs & Expectations**
   - Review `Dns/ZoneProvider` interfaces (how providers publish `ZoneRecord` collections, reload cadence, logging hooks).
   - Capture requirements from `docs/product_requirements.md` §2.2 and open issue #1 (“Static Zone declaration file”) to ensure priority record types (NS/A/AAAA/CNAME/MX/TXT) are in scope.  Supporting SOA is a non-goal.
   - Inventory how SmartZoneResolver currently consumes CSV/IPProbe output so the BIND provider returns consistent object models (zones keyed by fqdn + record set).

2. **Design the Parser**
   - Implement a streaming tokenizer that handles whitespace, comments (`;`), quoted strings, escaped characters, and parentheses for multi-line records.
   - Support directives: `$ORIGIN`, `$TTL`, `$INCLUDE` (optional; stub/not-supported errors are acceptable if documented), ensuring defaults cascade per RFC 1035/2308.
   - Parse owner name, TTL, class (`IN`), type, and RDATA for SOA/NS/A/AAAA/CNAME/MX/TXT to start; emit “unsupported RR type” diagnostics for others to keep validation strict.  While SOA may be included in the file.  It is a NON-GOAL to support SOA in the resolver.
   - Treat zone-file reloads as atomic: stage parsed data in-memory before publishing to SmartZoneResolver so partial failures do not corrupt active zones.

3. **Implement Comprehensive Validation**
   - **Lexical/Syntactic**: detect malformed directives, unterminated quotes/parentheses, numeric bounds (TTL fits `uint`, MX preference range, IPv4/IPv6 shape).
   - **Semantic**: enforce one SOA per zone file, at least one NS, A/AAAA data matches family, CNAME exclusivity, duplicate record suppression, and TTL min/max constraints.
   - **Cross-record**: verify references (e.g., MX targets exist), ensure `$ORIGIN` changes do not leak records outside the zone, and optionally ensure serial monotonicity when reloading.
   - Surface rich error messages with line/column indicators; fail fast before updating SmartZoneResolver if any validation errors exist.

4. **Wire Provider Into the System**
   - Create `Dns/ZoneProvider/Bind/BindZoneProvider.cs` (or similar) implementing the existing provider interface with configuration (path, reload interval, optional file watcher).
   - Integrate configuration binding through `appsettings.json`/DI so dns-cli can enable the provider (mirroring CSV provider wiring).
   - Record metrics/logging for reload success/failure counts to align with observability goals.

5. **Testing & Assets**
   - Unit tests under `dnstest` covering: directive handling, per-record parsing (SOA/NS/A/AAAA/CNAME/MX/TXT), validation failures (duplicate SOA, invalid TTLs, bad MX target, etc.), and error messaging.
   - Integration tests leveraging existing `DnsCliAuthoritativeBehaviorTests`: drop sample `.zone` files under `dnstest/TestData/Bind` and boot dns-cli with the new provider to assert full query/response flows, TTL application, and failure modes (invalid file should prevent startup or log errors without modifying live zones).
   - Add regression fixtures for edge cases: multi-line SOA records, records inheriting owner names, default TTL changes, comments interleaved with data.

6. **Documentation & Follow-up**
   - Document supported directives/types, configuration knobs, validation guarantees, and troubleshooting tips in `docs/task_list.md` (mark complete later) and README/docs as appropriate.
   - Note any unsupported BIND features (e.g., `$GENERATE`, DNSSEC records) plus follow-up issues if needed.
   - After implementation, run `dotnet format`, `dotnet build`, and `dotnet test csharp-dns-server.sln` to validate before submitting.
