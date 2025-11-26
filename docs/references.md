# References
The DNS protocol is specified and built on a raft of IETF RFCs. These links serve as the canonical references for features implemented (or planned) in this repository.

## Core DNS RFCs
- **[RFC 1034](https://datatracker.ietf.org/doc/html/rfc1034)** — Domain names: concepts and facilities; foundational DNS architecture.
- **[RFC 1035](https://datatracker.ietf.org/doc/html/rfc1035)** — Domain names: implementation and specification; wire format, message structures, and resource records.

## Other key DNS RFCs
- **[RFC 1123](https://datatracker.ietf.org/doc/html/rfc1123)** — Requirements for Internet Hosts; DNS-related operational requirements.
- **[RFC 2181](https://datatracker.ietf.org/doc/html/rfc2181)** — Clarifications to the DNS specification; authoritative guidance for modern resolvers.
- **[RFC 2308](https://datatracker.ietf.org/doc/html/rfc2308)** — Negative Caching of DNS Queries; defines TTL behavior for NXDOMAIN responses (relevant to caching work).
- **[RFC 3596](https://datatracker.ietf.org/doc/html/rfc3596)** — DNS Extensions to Support IP Version 6; specifies AAAA records handled by this server.
- **[RFC 4033](https://datatracker.ietf.org/doc/html/rfc4033)** — DNS Security Introduction and Requirements; foundation for DNSSEC features.
- **[RFC 4034](https://datatracker.ietf.org/doc/html/rfc4034)** — Resource Records for the DNS Security Extensions; details DNSSEC record types.
- **[RFC 4035](https://datatracker.ietf.org/doc/html/rfc4035)** — Protocol Modifications for DNSSEC; describes resolver behavior for signed responses.

## Supporting Standards & Transports
- **[RFC 768](https://datatracker.ietf.org/doc/html/rfc768)** — User Datagram Protocol; the primary transport for DNS queries handled by `DnsServer`.
- **[RFC 9293](https://datatracker.ietf.org/doc/html/rfc9293)** — Transmission Control Protocol; DNS fallbacks to TCP must conform when implementing large responses or zone transfers.
- **[RFC 6891](https://datatracker.ietf.org/doc/html/rfc6891)** — Extension Mechanisms for DNS (EDNS(0)); governs modern DNS extensions and message size negotiation.
- **[RFC 7766](https://datatracker.ietf.org/doc/html/rfc7766)** — DNS Transport over TCP: Requirements; clarifies persistent TCP usage for DNS.
- **[RFC 8484](https://datatracker.ietf.org/doc/html/rfc8484)** — DNS Queries over HTTPS (DoH); specifies DNS resolution over HTTPS for privacy and firewall traversal.
- **[RFC 9110](https://datatracker.ietf.org/doc/html/rfc9110)** — HTTP Semantics; referenced by the embedded HTTP server providing health and diagnostics.
