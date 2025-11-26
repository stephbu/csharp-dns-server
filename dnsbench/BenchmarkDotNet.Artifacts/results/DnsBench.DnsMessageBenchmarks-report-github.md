```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD


```
| Method                                  | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------------------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| &#39;Parse: Simple query&#39;                   |   272.8 ns |  2.88 ns |  2.70 ns | 0.1669 |      - |    1048 B |
| &#39;Parse: AAAA query&#39;                     |   293.5 ns |  1.71 ns |  1.51 ns | 0.1783 |      - |    1120 B |
| &#39;Parse: Simple response (1 A)&#39;          |   292.1 ns |  2.62 ns |  2.32 ns | 0.1974 |      - |    1240 B |
| &#39;Parse: CNAME response (2 records)&#39;     |   616.7 ns |  3.85 ns |  3.41 ns | 0.4587 | 0.0010 |    2880 B |
| &#39;Parse: Large response (12 records)&#39;    | 2,738.8 ns | 14.46 ns | 12.82 ns | 2.1782 | 0.0229 |   13664 B |
| &#39;Write: Query (new MemoryStream)&#39;       |   144.6 ns |  0.77 ns |  0.64 ns | 0.1528 | 0.0002 |     960 B |
| &#39;Write: Response (new MemoryStream)&#39;    |   396.1 ns |  1.88 ns |  1.66 ns | 0.2217 |      - |    1392 B |
| &#39;Write: Query (reused MemoryStream)&#39;    |   123.6 ns |  0.56 ns |  0.52 ns | 0.0572 |      - |     360 B |
| &#39;Write: Response (reused MemoryStream)&#39; |   367.3 ns |  1.44 ns |  1.21 ns | 0.1259 |      - |     792 B |
| &#39;Round-trip: Parse + Write query&#39;       |   427.3 ns |  2.62 ns |  2.45 ns | 0.3376 | 0.0005 |    2120 B |
| &#39;Round-trip: Parse + Write response&#39;    | 1,190.5 ns |  3.10 ns |  2.59 ns | 0.7420 | 0.0019 |    4656 B |
