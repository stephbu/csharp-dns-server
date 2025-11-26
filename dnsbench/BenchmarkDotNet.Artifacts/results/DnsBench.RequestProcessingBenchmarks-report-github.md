```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD


```
| Method                                           | Mean       | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------------------------------------------------- |-----------:|----------:|----------:|-------:|-------:|----------:|
| &#39;GetKeyName: string.Format (current)&#39;            | 104.237 ns | 0.4740 ns | 0.4202 ns | 0.0331 |      - |     208 B |
| &#39;GetKeyName: interpolation&#39;                      |  48.580 ns | 0.1547 ns | 0.1371 ns | 0.0128 |      - |      80 B |
| &#39;Full cycle: Parse → Build response → Serialize&#39; | 501.776 ns | 4.6161 ns | 4.3179 ns | 0.3424 |      - |    2152 B |
| &#39;Buffer copy: new byte[] + BlockCopy&#39;            |   8.330 ns | 0.0335 ns | 0.0280 ns | 0.0102 |      - |      64 B |
| &#39;MemoryStream: new per request&#39;                  | 146.990 ns | 1.7060 ns | 1.4246 ns | 0.1528 | 0.0002 |     960 B |
| &#39;SocketAsyncEventArgs: new per send&#39;             | 326.122 ns | 5.2144 ns | 4.8775 ns | 0.0367 | 0.0181 |     232 B |
| &#39;RequestMap: Add with RWLock&#39;                    |  58.691 ns | 0.2279 ns | 0.2020 ns | 0.0076 |      - |      48 B |
| &#39;RequestMap: TryGet with RWLock&#39;                 |  82.904 ns | 0.1868 ns | 0.1656 ns | 0.0076 |      - |      48 B |
