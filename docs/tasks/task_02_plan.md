# Task 2 Plan – Authoritative Response Verification Suite

## Goal
Build a deterministic integration test suite that executes the shipping `dns-cli` host against sample zones so we can assert the DNS protocol surface (AA/RA flags, SOA authority section, caching-related TTL fields) behaves as expected end-to-end.

## Scope
- **Code/Test targets**: `dns-cli` (process runner), `Dns/Program.cs`, `Dns/DnsServer.cs`, `Dns/SmartZoneResolver.cs`, and new xUnit integration fixtures that live under `dnstest`.
- **Assets**: reproducible sample zone definitions/configuration files that live in-repo (likely under `dnstest/TestData/`).
- **Out of scope**: modifying server behavior or introducing RFC 2308 caching logic (that is T03). T02 only codifies the current semantics via integration coverage.

## Steps
1. **Define behavior checklist**  
   - Re-read RFC 1034/1035 + existing implementation to document what “correct” looks like for AA, RA, NXDOMAIN, SOA authority counts, TTL/minimum TTL, and round-robin ordering.  
   - Capture these expectations in the test plan so every assertion has a justification (e.g., `AA=1` for in-zone answers, `RA=0` because recursion is not provided, SOA present for NXDOMAIN with `MinimumTTL` acting as the negative-cache TTL).

2. **Create deterministic zone + config assets**  
   - Place a CSV/AP-zone file with a handful of hosts (single-address, multi-address for rotation, and an empty gap for NXDOMAIN) under `dnstest/TestData/Zones`.  
   - Add an integration `appsettings` template that points the zone provider at this CSV and exposes tokens for DNS/HTTP ports so tests can substitute an available port at runtime.  
   - Keep assets self-contained so the suite never depends on developer-specific paths or live IP probes.

3. **Spin up `dns-cli` from tests**  
   - Build a reusable `DnsCliHostFixture` that:  
     - Chooses free UDP/TCP ports (using `Socket`/`TcpListener`) to avoid conflicts with system services.  
     - Writes the tokenized config (step 2) to a temp file with the resolved ports and zone path.  
     - Launches `dotnet <path>/dns-cli.dll <tempConfig>` with redirected stdout/stderr and a cancellation token; wait until the server is ready by probing the HTTP `/dump/dnsresolver` endpoint or by polling the UDP port with a health query.  
     - Implements `IDisposable` to cancel/kill the process after the test collection completes and to surface logs when startup fails.

4. **Author request helper**  
   - Within the test project, create a `DnsQueryClient` utility that uses `DnsMessage`/`DnsProtocol` to craft queries (A + NXDOMAIN) and parse responses.  
   - Support toggling RD flag, capturing round-trips, verifying TTLs, and exposing raw `DnsMessage` for assertions.  
   - Consider adding simple retry/timeout handling so the integration tests are resilient to transient startup delays.

5. **AA/RA/SOA assertions**  
   - Add tests that query an in-zone A record and assert: `QR=1`, `AA=1`, `RA=0`, `RCode=NOERROR`, and that the answer payload matches the CSV data (including TTL=10 and round-robin order).  
   - Add a test that sets the RD flag on the query to confirm the server still responds with `RA=0` for authoritative answers (baseline recursion semantics).  
   - Add an NXDOMAIN test that validates the authority section contains a single SOA record populated with the resolver’s current serial and minimum TTL, proving negative answers include the caching hints mandated by RFC 2308.

6. **Caching-semantic coverage**  
   - Positive caching: issue the same query multiple times and assert TTL remains at 10 seconds (current behavior) and that responses stay authoritative; this guards future TTL changes.  
   - Negative caching: query a nonexistent record twice and ensure the SOA `MinimumTTL` mirrors the configured 300 seconds value each time.  
   - Lay groundwork for future RFC 2308 work by encapsulating “wait for TTL expiry” helpers (even if currently skipped) so T03 can plug in actual caching checks without rewriting the harness.

7. **Document and wire CI**  
   - Update `docs/task_list.md`/`AGENTS.md` with instructions on running the new integration suite (e.g., `dotnet test` now launches `dns-cli`, required ports, how to tweak sample config).  
   - Ensure the suite is part of `dotnet test csharp-dns-server.sln` locally and add notes on troubleshooting (port collisions, residual processes).

## Acceptance Criteria
- Integration tests spin up `dns-cli` automatically and tear it down reliably across Windows/Linux.  
- Tests assert AA, RA, SOA (authority section), and TTL/minimum TTL semantics for both successful and NXDOMAIN responses.  
- Repeated queries verify current caching-related TTL behavior so future caching work has a safety net.  
- Running `dotnet test csharp-dns-server.sln` executes the suite without manual steps, and documentation reflects the new coverage.
