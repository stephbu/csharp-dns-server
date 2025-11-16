# Task 1 Plan – Fix DNS Compressed-Name Parsing Regression

## Goal
Resolve issue #26 (“Error in the compressed string pointer parsing”) by repairing the DNS message parser and adding regression coverage so malformed compressed names can’t slip through again.

## Scope
- Code: `Dns/DnsProtocol.cs`, `Dns/DnsMessage.cs`, and any helper classes that read domain-name labels.
- Tests: `dnstest/DnsProtocolTest.cs` (unit), plus optional integration validation if needed.

## Steps
1. **Reproduce the bug**  
   - Capture the failing packet(s) from issue #26 or craft equivalents.  
   - Add a failing test in `dnstest/DnsProtocolTest.cs` that exposes the incorrect compressed-pointer behavior.
2. **Inspect parser logic**  
   - Review `DnsProtocol.ReadString` and downstream usage in `DnsMessage.TryParse`.  
   - Verify pointer offset handling, pointer loops, and name termination per RFC 1035 §4.1.4.
3. **Implement fix**  
   - Adjust parsing logic to correctly handle offsets, prevent infinite loops, and ensure the buffer cursor is restored after following compression pointers.  
   - Consider additional validation (max label length, recursion limits).
4. **Extend regression tests**  
   - Add positive/negative cases covering compressed names at different positions (questions, answers, authority, additional).  
   - Include edge cases (nested pointers, zero-length labels).
5. **Optional integration test**  
   - Use `dns-cli` with a crafted response to ensure end-to-end decoding works.
6. **Documentation / notes**  
   - Update `docs/task_list.md` checkbox when complete.  
   - Reference this fix in release notes or issues if needed.

## Acceptance Criteria
- All new tests pass and demonstrate correct compressed-name parsing.  
- No regressions in existing protocol tests.  
- The issue #26 reproduction no longer fails.
