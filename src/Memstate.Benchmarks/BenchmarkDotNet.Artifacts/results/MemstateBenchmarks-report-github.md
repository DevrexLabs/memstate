``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 2 (10.0.15063)
Processor=Intel Core i7-4710HQ CPU 2.50GHz (Haswell), ProcessorCount=8
Frequency=2435770 Hz, Resolution=410.5478 ns, Timer=TSC
.NET Core SDK=2.0.2
  [Host] : .NET Core 1.1.2 (Framework 4.6.25211.01), 64bit RyuJIT DEBUG


```
 |           Method |                   StorageProviderTypes | Mean | Error | P90 | P95 | Kurtosis | Op/s |
 |----------------- |--------------------------------------- |-----:|------:|----:|----:|---------:|-----:|
 | CommandRoundtrip | Memstate.Postgresql.PostgresqlProvider |   NA |    NA |  NA |  NA |       NA |   NA |

Benchmarks with issues:
  MemstateBenchmarks.CommandRoundtrip: DefaultJob [StorageProviderTypes=Memstate.Postgresql.PostgresqlProvider]
