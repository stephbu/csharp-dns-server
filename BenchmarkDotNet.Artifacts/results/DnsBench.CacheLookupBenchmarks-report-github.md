```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD
  Job-RDUOPE : .NET 8.0.4 (8.0.424.16909), Arm64 RyuJIT AdvSIMD

IterationCount=5  WarmupCount=2  

```
| Method                                | Mean      | Error    | StdDev   | Gen0   | Allocated |
|-------------------------------------- |----------:|---------:|---------:|-------:|----------:|
| &#39;Zone Lookup: Dictionary&lt;string&gt;&#39;     | 102.18 ns | 1.732 ns | 0.268 ns | 0.0255 |     160 B |
| &#39;Zone Lookup: Dictionary&lt;struct&gt;&#39;     |  44.19 ns | 0.750 ns | 0.116 ns |      - |         - |
| &#39;Zone Lookup: ConcurrentDict&lt;string&gt;&#39; | 102.22 ns | 1.065 ns | 0.276 ns | 0.0255 |     160 B |
| &#39;Zone Lookup: ConcurrentDict&lt;struct&gt;&#39; |  41.82 ns | 0.905 ns | 0.235 ns |      - |         - |
| &#39;Zone Lookup: FrozenDict&lt;struct&gt;&#39;     |  44.88 ns | 5.114 ns | 1.328 ns |      - |         - |
