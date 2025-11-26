```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD


```
| Method                                          | Mean        | Error     | StdDev    | Gen0   | Allocated |
|------------------------------------------------ |------------:|----------:|----------:|-------:|----------:|
| &#39;ReadString: Simple (legacy StringBuilder)&#39;     |  60.2319 ns | 0.4076 ns | 0.3813 ns | 0.0471 |     296 B |
| &#39;ReadString: Simple (Phase 3 Span)&#39;             |  34.3911 ns | 0.1426 ns | 0.1264 ns | 0.0076 |      48 B |
| &#39;ReadString: Medium (legacy StringBuilder)&#39;     | 164.0997 ns | 0.9025 ns | 0.8442 ns | 0.1261 |     792 B |
| &#39;ReadString: Medium (Phase 3 Span)&#39;             |  49.9105 ns | 0.2611 ns | 0.2443 ns | 0.0166 |     104 B |
| &#39;ReadString: Compressed (legacy StringBuilder)&#39; |  81.3111 ns | 0.4062 ns | 0.3800 ns | 0.0739 |     464 B |
| &#39;ReadString: Compressed (Phase 3 Span)&#39;         |  51.5990 ns | 0.1687 ns | 0.1409 ns | 0.0344 |     216 B |
| ReadUshort                                      |   0.8055 ns | 0.0058 ns | 0.0054 ns |      - |         - |
| &#39;ReadUshort (BigEndian Span)&#39;                   |   0.0000 ns | 0.0000 ns | 0.0000 ns |      - |         - |
| ReadUint                                        |   0.8114 ns | 0.0016 ns | 0.0014 ns |      - |         - |
| &#39;ReadUint (BigEndian Span)&#39;                     |   0.0371 ns | 0.0010 ns | 0.0008 ns |      - |         - |
