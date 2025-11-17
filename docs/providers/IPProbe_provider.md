# IPProbe Zone Provider

`Dns.ZoneProvider.IPProbe.IPProbeZoneProvider` continuously probes configured endpoints and only advertises addresses that are currently healthy. It is the preferred choice when you want DNS round-robin coupled with basic liveness detection.

## Configuration

The provider is enabled when `server.zone.provider` points to `Dns.ZoneProvider.IPProbe.IPProbeZoneProvider`. All other settings live under `zoneprovider`:

```json
{
  "server": {
    "zone": {
      "name": ".example.com",
      "provider": "Dns.ZoneProvider.IPProbe.IPProbeZoneProvider"
    }
  },
  "zoneprovider": {
    "PollingIntervalSeconds": 15,
    "Hosts": [
      {
        "Name": "www",
        "Probe": "ping",
        "Timeout": 30,
        "AvailabilityMode": "all",
        "Ip": [
          "192.0.2.10",
          "192.0.2.11"
        ]
      },
      {
        "Name": "api",
        "Probe": "noop",
        "Timeout": 100,
        "AvailabilityMode": "first",
        "Ip": [
          "192.0.2.20",
          "192.0.2.21"
        ]
      }
    ]
  }
}
```

### Host settings

- `Name`: the left-most label served to clients. The provider appends the configured zone name, so `"Name": "www"` plus `"zone": ".example.com"` becomes `www.example.com`.
- `Probe`: strategy label. Built-in options are `ping` (ICMP echo), and `noop` (always healthy, helpful for lab testing). Unknown values fall back to `noop`.
- `Timeout`: milliseconds passed to the strategy implementation.
- `AvailabilityMode`:
  - `all` – advertise every healthy IP.
  - `first` – advertise only the first healthy IP (useful when you want to fail over to a single target).
- `Ip`: list of IPv4 or IPv6 addresses. Each entry is monitored independently but deduplicated if multiple hosts reference the same target.

`PollingIntervalSeconds` controls how long the provider sleeps between probe batches. Each batch records status, updates the rolling window, emits a new zone (if the provider is still running), and then waits out the remaining interval.

## Health Evaluation

- Every `Target` keeps a ring buffer of up to 10 recent `ProbeResult` entries.
- `Target.IsAvailable` returns true only if the last three results were successful. This smooths out occasional probe failures.
- When a probe function throws (e.g., ping exceptions) the provider treats the result as unavailable for the cycle.
- Hosts marked `AvailabilityMode.First` return the first healthy address in ascending order from the configuration list; otherwise all healthy addresses are used. SmartZoneResolver still applies its own round-robin logic to the resulting ZoneRecord.

## Behavior & Limitations

- Records are emitted as `A` records today; the provider accepts IPv6 addresses but the resolver currently serves only IPv4 responses.
- There is no persistent storage—restarts lose probe history, so it may take a few cycles before `IsAvailable` returns true.
- Probe strategies run in parallel (up to four at a time). Ensure your environment allows outbound ICMP if you rely on `ping`.

## Observability

The provider logs probe loop start/end plus any exception raised during probing or zone publication. Future instrumentation (see docs/product_requirements.md §4) will hang metrics off this loop.

## Tests & Assets

- `Dns/appsettings.json` ships with an example IPProbe configuration you can tweak for local smoke tests.
- `dnstest/Integration` wiring spins up `dns-cli` with probe data; add or update those assets whenever you change the provider surface.

Always run `dotnet test csharp-dns-server.sln` before submitting changes so the integration harness exercises your updates end-to-end.
