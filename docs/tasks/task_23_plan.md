# Task 23 Plan – Correct IPv4 RDATA Endianness

## Goal
Fix `ANameRData.Parse` so IPv4 addresses extracted from DNS responses retain the network-order byte layout, preventing byte-swapped answers from being relayed to clients, and ensure the regression never returns.

## Scope
- **Code**: `Dns/RData.cs` (specifically `ANameRData.Parse` and any related serialization helpers), plus any ancillary utilities that assume host-endian IPv4 storage.
- **Tests**: Extend `dnstest` with unit coverage that parses raw DNS response buffers containing known A records and asserts the resulting `IPAddress` matches the on-wire address. Add an integration-style test that exercises the forwarder path in `DnsServer` to confirm responses are emitted correctly.

## Steps
1. **Reproduce the issue**  
   - Craft a byte array that represents a DNS answer with an A record (e.g., `127.0.0.1`) and parse it through the current `ANameRData.Parse` to observe the reversed address (`1.0.0.127`).  
   - Add a failing unit test in `dnstest` capturing this scenario to guard against regressions.

2. **Inspect serialization assumptions**  
   - Review everywhere `IPAddress` instances are read/written (e.g., `ResourceRecord.WriteToStream`, `SmartZoneResolver` address handling) to understand whether we rely on `IPAddress.GetAddressBytes()` (network order) or host-endian integers.  
   - Confirm the bug is isolated to `ANameRData.Parse` rather than a broader serialization mismatch.

3. **Implement the fix**  
   - Update `ANameRData.Parse` to either call `DnsProtocol.ReadUint` and `SwapEndian()` before constructing `IPAddress`, or, preferably, slice the original four bytes (`byte[] address = new byte[4]; Buffer.BlockCopy(...)`) and pass them to `new IPAddress(byte[])`.  
   - Ensure the change handles both IPv4 and potential future IPv6 extensions gracefully (guarding against `DataLength != 4`).

4. **Add regression tests**  
   - Unit test: feed a minimal DNS message containing a single A record into `ResourceList.LoadFrom` and assert the resulting `Address` property equals the source IP.  
   - End-to-end test: simulate `DnsServer` receiving an upstream response with a known A record and verify the serialized bytes sent to the original client preserve the correct order.

5. **Documentation and task tracking**  
   - Update `docs/task_list.md` to mark T23 complete once merged, and reference the new tests in `docs/task_list.md` or release notes if needed.  
   - Note any follow-on cleanup (e.g., IPv6 handling) discovered during the fix.

## Acceptance Criteria
- Parsing an IPv4 RDATA blob yields an `IPAddress` matching the on-wire order.  
- Forwarded responses from `DnsServer` contain correct byte ordering, validated by tests.  
- New unit/integration tests fail prior to the fix and pass afterward.  
- No regressions in existing DNS parsing tests.***
