# BIND Zone Provider

The `Dns.ZoneProvider.Bind.BindZoneProvider` watcher ingests a forward zone file written in standard BIND syntax, validates it aggressively, and publishes the resulting address records into `SmartZoneResolver`. This note captures the supported directives, configuration, validation rules, and troubleshooting steps so operators can confidently run static zone files alongside the existing CSV/IPProbe providers.

## Configuration

Add the provider to either the `Dns` or `dns-cli` host configuration. Only the zone name and the provider type change from the default template:

```json
{
  "server": {
    "zone": {
      "name": ".example.com",
      "provider": "Dns.ZoneProvider.Bind.BindZoneProvider"
    }
  },
  "zoneprovider": {
    "FileName": "C:/zones/example.com.zone"
  }
}
```

The provider watches the specified file (after expanding environment variables and resolving to an absolute path). Any file system notification resets a 10-second settlement timer; once the timer expires, the provider re-parses the zone. This protects against partial writes and ensures the resolver only sees complete zones.

## Supported Syntax & Records

- **Directives**: `$ORIGIN`, `$TTL` are honored; `$INCLUDE` currently returns a validation error so you know the file is unsupported.
- **Records**: SOA, NS, A, AAAA, CNAME, MX, and TXT. Additional RR types incur an `unsupported record type` error.
- **Fields**: Owner name, TTL, class, and type tokens are parsed in the same order BIND allows (owner optional when indented; TTL/Class optional before the type). TTLs accept numeric suffixes (`s`, `m`, `h`, `d`, `w`).
- **Comments & multi-line records**: Semicolons outside quoted strings begin a comment. Parentheses join multi-line records, including SOA definitions.

Only `A` and `AAAA` data become `ZoneRecord` entries today—the resolver still emits IPv4 answers exclusively, but caching the IPv6 data keeps us ready for future SmartZoneResolver updates.

## Validation Guarantees

Before replacing the active zone the provider enforces:

1. **Lexical/syntactic**: balanced parentheses, terminated quotes, escaped characters, and valid TTL literals.
2. **Directive integrity**: `$ORIGIN` cannot move records outside the configured zone; missing `$TTL` values cause per-record failures unless the record specifies its own TTL.
3. **SOA/NS requirements**: exactly one SOA at the apex and at least one NS record for the zone root.
4. **Record semantics**:
   - A/AAAA addresses must match their IP family; duplicates are suppressed.
   - MX preference is a valid `ushort`; target names are canonicalized.
   - CNAME exclusivity—once a name is a CNAME it cannot host other record types, and conflicting targets are rejected.
   - Owner names must stay within the configured zone.
5. **Zone completeness**: at least one address record must be produced; otherwise the zone is considered unusable.

If any validation fails the generated zone is discarded, the previous zone remains live, and an actionable error (with line number) is written to the console.

## Unsupported Features

The following are explicitly out of scope for this iteration, but the parser surfaces intentional errors so you know why a reload failed:

- `$INCLUDE`, `$GENERATE`, DNSSEC record types, and all RR classes besides `IN`.
- Cross-record dependency checks (e.g., verifying MX targets exist) beyond the per-record rules listed above.
- Serving SOA/NS/MX/CNAME/TXT answers—these records are validated but not yet surfaced in `DnsServer` responses.

## Troubleshooting

1. **Console errors**: the provider logs `BIND zone parse error (<file>:<line>): <message>`; fix the offending line and save the file to trigger a reload.
2. **No reload after saving**: ensure file events fire for the resolved path. For temporary editors that save via rename, keep the file in place so the watcher can see `Created`/`Changed` events.
3. **Zone not updating**: confirm the new zone actually produces at least one address record; otherwise the provider logs “did not produce any address records” and skips publication.

## Testing & Samples

Unit tests live under `dnstest/BindZoneProviderTests.cs`, driving sample zones stored in `dnstest/TestData/Bind/`. To add new regression cases, drop another `.zone` file in that directory and reference it from the tests. Running `dotnet test csharp-dns-server.sln` exercises these fixtures automatically.

For a ready-made example, `dnstest/TestData/Bind/simple.zone` demonstrates the accepted SOA, NS, A, AAAA, and CNAME records with mixed TTL declarations.
