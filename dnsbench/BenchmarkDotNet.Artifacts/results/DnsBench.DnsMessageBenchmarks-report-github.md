```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD


```
| Method                                  | Mean       | Error    | StdDev   | Median     | Gen0   | Gen1   | Allocated |
|---------------------------------------- |-----------:|---------:|---------:|-----------:|-------:|-------:|----------:|
| &#39;Parse: Simple query&#39;                   |   107.3 ns |  0.64 ns |  0.54 ns |   107.4 ns | 0.0675 |      - |     424 B |
| &#39;Parse: AAAA query&#39;                     |   112.0 ns |  0.65 ns |  0.54 ns |   112.1 ns | 0.0688 |      - |     432 B |
| &#39;Parse: Simple response (1 A)&#39;          |   172.5 ns |  0.50 ns |  0.45 ns |   172.5 ns | 0.1261 |      - |     792 B |
| &#39;Parse: CNAME response (2 records)&#39;     |   333.3 ns |  4.49 ns |  4.20 ns |   332.1 ns | 0.2193 | 0.0005 |    1376 B |
| &#39;Parse: Large response (12 records)&#39;    | 1,412.8 ns | 27.71 ns | 51.36 ns | 1,396.3 ns | 0.9022 | 0.0095 |    5664 B |
| &#39;Write: Query (new MemoryStream)&#39;       |   147.4 ns |  1.47 ns |  1.15 ns |   147.2 ns | 0.1528 | 0.0002 |     960 B |
| &#39;Write: Response (new MemoryStream)&#39;    |   400.4 ns |  5.25 ns |  4.65 ns |   398.6 ns | 0.2217 |      - |    1392 B |
| &#39;Write: Query (reused MemoryStream)&#39;    |   126.6 ns |  2.51 ns |  3.84 ns |   124.6 ns | 0.0572 |      - |     360 B |
| &#39;Write: Response (reused MemoryStream)&#39; |   368.4 ns |  3.39 ns |  2.83 ns |   368.0 ns | 0.1259 |      - |     792 B |
| &#39;Round-trip: Parse + Write query&#39;       |   319.9 ns |  2.61 ns |  2.18 ns |   319.4 ns | 0.2384 | 0.0005 |    1496 B |
| &#39;Round-trip: Parse + Write response&#39;    |   912.5 ns |  5.30 ns |  4.69 ns |   911.8 ns | 0.5016 | 0.0019 |    3152 B |
