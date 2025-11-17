# CSV/AP Zone Provider

The historical CSV/AP provider (`Dns.ZoneProvider.AP.APZoneProvider`) is the simplest way to preload static IPv4 answers. It watches a CSV file, groups rows by machine function, and emits one `ZoneRecord` per function with every configured address so SmartZoneResolver can round-robin them.

## Configuration

Point the DNS host (or `dns-cli`) at the provider and supply a CSV path via `zoneprovider.FileName`:

```json
{
  "server": {
    "zone": {
      "name": ".example.com",
      "provider": "Dns.ZoneProvider.AP.APZoneProvider"
    }
  },
  "zoneprovider": {
    "FileName": "C:/zones/machineinfo.csv"
  }
}
```

`FileWatcherZoneProvider` handles the reload mechanics: any file change restarts a 10-second settlement timer and the CSV is re-parsed after the timer expires. If parsing succeeds a brand-new zone replaces the previous one atomically.

## CSV Schema

The provider only reads three columns—`MachineFunction`, `StaticIP`, and `MachineName`. All other columns in the CSV are ignored. The parser expects a header declaration in the first non-comment line (mirroring both `Dns/Data/machineinfo.csv` and `dnstest/TestData/Zones/integration_machineinfo.csv`):

```
#Fields:MachineName,MachineFunction,StaticIP
myhost01,www,192.0.2.10
myhost02,www,192.0.2.11
api01,api,192.0.2.20
```

- The hostname served to DNS clients is `<MachineFunction><ZoneName>`, so with the example above and `ZoneName=".example.com"` the provider emits `www.example.com` and `api.example.com` records.
- Duplicate `MachineFunction` values are grouped and all IPv4 addresses are returned to SmartZoneResolver, enabling round-robin responses.
- The parser ignores blank lines and comment lines beginning with `#` or `;`.

## Behavior & Limitations

- Records are always `A`/`IN` entries; IPv6 is not supported.
- No TTL metadata exists in the CSV, so the DNS server continues using its default per-answer TTL (10 seconds today).
- The provider trusts the CSV contents—malformed IP addresses throw at parse time and block publication, logging the exception to the console.

## Samples & Tests

- `Dns/Data/machineinfo.csv` – legacy data used for local experiments.
- `dnstest/TestData/Zones/integration_machineinfo.csv` – trimmed-down fixture consumed by the integration tests. Update this file (and the tests that reference it) if you change the CSV schema.

Run `dotnet test csharp-dns-server.sln` after editing either the provider or its CSV assets to ensure the integration suite still passes.
