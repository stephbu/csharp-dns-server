# Task T24 – Stabilize UDP Listener Shutdown & Endpoint Capture

## Goal
Ensure `UdpListener.Start` terminates cleanly after `Stop()` is invoked and that each received packet preserves its source endpoint so responses never get misrouted.

## Plan

1. **Assess Current Behavior**
   - Inspect `Dns/UdpListener.cs` to map out how `Start`, `Stop`, and the receive loop interact (socket lifetime, cancellation tokens, exception handling).
   - Trace how the listener hands off messages to `Dns/DnsServer.cs` (or other consumers) to see whether remote endpoints are currently cached/shared.

2. **Design Improvements**
   - Introduce deterministic shutdown semantics (e.g., cancellation token source + awaited receive loop task) so `Stop` closes sockets exactly once and `Start` returns promptly.
   - Ensure each `ReceiveFromAsync` (or equivalent) call allocates/captures an `IPEndPoint` per packet rather than mutating shared instances.
   - Propagate the captured endpoint through the processing pipeline so replies target the correct remote peer even with concurrent sends.
   - Handle race conditions: guard shared state, tolerate socket disposal exceptions, and prevent `Start` re-entry mishaps.

3. **Implementation Steps**
   - Update `UdpListener` members (fields, constructor) to store cancellation/disposal state.
   - Refactor the receive loop to honor cancellation and to package `(byte[] buffer, IPEndPoint remote)` results.
   - Modify consumers (likely `DnsServer`) to accept and reuse per-packet endpoints when forming responses.
   - Add logging where necessary to aid future diagnostics without spamming hot paths.

4. **Testing & Validation**
   - Unit tests: add focused tests around new helper methods or endpoint forwarding logic.
   - Integration tests (e.g., expand `dnstest/DnsCliAuthoritativeBehaviorTests`) to simulate multiple senders, verify responses go to the correct addresses, and ensure `Stop` fully releases the port.
   - Run `dotnet test csharp-dns-server.sln` locally; capture any flakes/regressions.
   - Concurrency confidence: stress scenarios (multiple clients + repeated `Start`/`Stop`) can be scripted, but race bugs remain timing-dependent, so pair tests with code review and optional manual stress loops to boost assurance.

5. **Documentation & Tracking**
   - Update inline comments to explain shutdown behavior.
   - If necessary, note operational changes (e.g., new logging) in README/docs.
   - Mark T24 complete in `docs/task_list.md` once fixes and tests land, recording any follow-up issues discovered during testing.
