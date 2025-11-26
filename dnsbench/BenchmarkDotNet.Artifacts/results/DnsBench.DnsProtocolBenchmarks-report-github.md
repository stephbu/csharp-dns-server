```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD


```
| Method                             | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------- |------------:|----------:|----------:|-------:|----------:|
| &#39;ReadString: Simple (www.msn.com)&#39; |  60.6599 ns | 0.3570 ns | 0.3164 ns | 0.0471 |     296 B |
| &#39;ReadString: Medium (7 labels)&#39;    | 165.7388 ns | 0.6849 ns | 0.5719 ns | 0.1261 |     792 B |
| &#39;ReadString: Compressed pointer&#39;   |  80.2575 ns | 0.4812 ns | 0.4018 ns | 0.0739 |     464 B |
| ReadUshort                         |   0.8081 ns | 0.0049 ns | 0.0038 ns |      - |         - |
| ReadUint                           |   0.8254 ns | 0.0271 ns | 0.0266 ns |      - |         - |
