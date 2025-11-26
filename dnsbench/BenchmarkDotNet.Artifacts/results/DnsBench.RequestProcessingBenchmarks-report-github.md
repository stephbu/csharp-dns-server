```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD
  Job-JTHIQB : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD

IterationCount=10  WarmupCount=3  

```
| Method                                   | Mean       | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|----------------------------------------- |-----------:|----------:|----------:|-------:|-------:|----------:|
| &#39;Buffer copy: new byte[] + BlockCopy&#39;    |   8.187 ns | 0.0289 ns | 0.0151 ns | 0.0102 |      - |      64 B |
| &#39;MemoryStream: new per request&#39;          | 147.317 ns | 1.2329 ns | 0.7337 ns | 0.1528 | 0.0002 |     960 B |
| &#39;MemoryStream: pooled (Phase 2)&#39;         | 167.541 ns | 0.7151 ns | 0.4730 ns | 0.0572 |      - |     360 B |
| &#39;SocketAsyncEventArgs: new per send&#39;     | 257.613 ns | 9.1866 ns | 5.4668 ns | 0.0367 | 0.0181 |     232 B |
| &#39;SocketAsyncEventArgs: pooled (Phase 2)&#39; |  69.538 ns | 0.9825 ns | 0.5846 ns |      - |      - |         - |
| &#39;Buffer: MemoryPool rental (Phase 2)&#39;    | 171.898 ns | 1.9070 ns | 1.2613 ns | 0.6604 |      - |    4144 B |
