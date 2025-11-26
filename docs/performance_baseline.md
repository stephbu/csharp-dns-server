# DNS Server Performance Baseline

**Date**: November 2025  
**Runtime**: .NET 8.0.4, Arm64 RyuJIT AdvSIMD  
**Platform**: macOS, Apple M1 Max  

## Overview

This document captures the baseline performance metrics for the DNS server before optimization work begins. These benchmarks establish the current memory allocation patterns and timing characteristics that will be improved in Issue #29 (Performance Tuning).

## Benchmark Results

### DnsProtocol Parsing Benchmarks

| Method | Mean | Error | StdDev | Gen0 | Allocated |
|--------|------|-------|--------|------|-----------|
| ReadString: Simple (www.msn.com) | 91.8 ns | 0.30 ns | 0.27 ns | 0.0166 | 104 B |
| ReadString: Medium (7 labels) | 161.4 ns | 0.65 ns | 0.61 ns | 0.0267 | 168 B |
| ReadString: Compressed pointer | 53.7 ns | 0.33 ns | 0.30 ns | 0.0089 | 56 B |
| ReadUshort | 0.76 ns | 0.002 ns | 0.002 ns | - | - |
| ReadUint | 0.76 ns | 0.001 ns | 0.001 ns | - | - |

**Key Observations:**
- `ReadString` allocates 104-168 bytes per call due to `StringBuilder` usage
- Compression pointer handling adds minimal overhead (~54 ns)
- Primitive reads (`ReadUshort`/`ReadUint`) are highly efficient

### DnsMessage Parsing & Serialization Benchmarks

| Method | Mean | Error | StdDev | Gen0 | Allocated |
|--------|------|-------|--------|------|-----------|
| Parse: Simple query | 273.4 ns | 1.31 ns | 1.16 ns | 0.0830 | 520 B |
| Parse: AAAA query | 323.5 ns | 1.23 ns | 1.15 ns | 0.1011 | 632 B |
| Parse: Simple response (1 A) | 395.0 ns | 2.56 ns | 2.14 ns | 0.1168 | 736 B |
| Parse: CNAME response (2 records) | 579.3 ns | 2.64 ns | 2.47 ns | 0.1736 | 1096 B |
| Parse: Large response (12 records) | 1,576.7 ns | 6.88 ns | 6.10 ns | 0.4635 | 2920 B |
| Write: Query (new MemoryStream) | 121.3 ns | 0.92 ns | 0.77 ns | 0.1452 | 912 B |
| Write: Response (new MemoryStream) | 191.9 ns | 0.71 ns | 0.63 ns | 0.2143 | 1344 B |
| Write: Query (reused MemoryStream) | 48.9 ns | 0.17 ns | 0.14 ns | 0.0038 | 24 B |
| Write: Response (reused MemoryStream) | 90.2 ns | 0.40 ns | 0.38 ns | 0.0038 | 24 B |
| Round-trip: Parse + Write query | 374.2 ns | 3.23 ns | 3.02 ns | 0.2232 | 1400 B |
| Round-trip: Parse + Write response | 744.5 ns | 3.61 ns | 3.38 ns | 0.3138 | 1976 B |

**Key Observations:**
- Parsing allocates significant memory per message (520 B - 2920 B)
- **Reusing MemoryStream reduces allocations by ~97%** (912 B → 24 B)
- Large responses with many records scale linearly in allocations

### Request Processing Benchmarks

| Method | Mean | Error | StdDev | Gen0 | Gen1 | Allocated |
|--------|------|-------|--------|------|------|-----------|
| GetKeyName: string.Format (current) | 104.2 ns | 0.47 ns | 0.42 ns | 0.0331 | - | 208 B |
| GetKeyName: interpolation | 48.6 ns | 0.15 ns | 0.14 ns | 0.0128 | - | 80 B |
| Full cycle: Parse → Build → Serialize | 501.8 ns | 4.62 ns | 4.32 ns | 0.3424 | - | 2152 B |
| Buffer copy: new byte[] + BlockCopy | 8.3 ns | 0.03 ns | 0.03 ns | 0.0102 | - | 64 B |
| MemoryStream: new per request | 147.0 ns | 1.71 ns | 1.42 ns | 0.1528 | 0.0002 | 960 B |
| SocketAsyncEventArgs: new per send | 326.1 ns | 5.21 ns | 4.88 ns | 0.0367 | 0.0181 | 232 B |
| RequestMap: Add with RWLock | 58.7 ns | 0.23 ns | 0.20 ns | 0.0076 | - | 48 B |
| RequestMap: TryGet with RWLock | 82.9 ns | 0.19 ns | 0.17 ns | 0.0076 | - | 48 B |

**Key Observations:**
- **String interpolation is 2x faster and allocates 62% less** than `string.Format`
- Full request cycle allocates **2152 bytes per request** - primary optimization target
- `SocketAsyncEventArgs` creation is expensive (326 ns, 232 B) - pooling candidate
- `MemoryStream` allocation per request wastes 960 B - pooling candidate

## Identified Optimization Opportunities

### High Impact (Phase 2)

| Component | Current Allocation | Potential Savings |
|-----------|-------------------|-------------------|
| MemoryStream per request | 960 B | ~95% with pooling |
| SocketAsyncEventArgs per send | 232 B | ~95% with pooling |
| Buffer copy per receive | 64 B | ~100% with MemoryPool |

### Medium Impact (Phase 3 - Span/Memory)

| Component | Current Allocation | Notes |
|-----------|-------------------|-------|
| DnsProtocol.ReadString | 104-168 B | StringBuilder → Span |
| DnsMessage.Parse | 520-2920 B | Reduce internal allocations |

### Quick Wins (Phase 4)

| Component | Current | Optimized | Savings |
|-----------|---------|-----------|---------|
| GetKeyName (string.Format → interpolation) | 208 B | 80 B | 62% |
| Request key (string → struct) | 80 B | ~0 B | 100% |

## Running Benchmarks

```bash
cd dnsbench
dotnet run -c Release -- --filter "*"

# Run specific benchmark class
dotnet run -c Release -- --filter "DnsProtocol*"
dotnet run -c Release -- --filter "DnsMessage*"
dotnet run -c Release -- --filter "RequestProcessing*"
```

## Success Criteria for Issue #29

Based on these baselines, the following targets are set:

1. **Full request cycle**: Reduce from 2152 B to < 500 B per request (75% reduction)
2. **MemoryStream allocations**: Eliminate per-request allocations via pooling
3. **Buffer copies**: Eliminate via `MemoryPool<byte>` 
4. **No throughput regression**: Maintain or improve latency numbers

## Related Issues

- #29 - Performance Tuning (this baseline supports)
- #32 - EDNS(0) Support (coordinate buffer sizing to 4096 bytes)
