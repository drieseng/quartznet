``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.17763.914 (1809/October2018Update/Redstone5)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), X64 RyuJIT
  DefaultJob : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), X64 RyuJIT


```
|   Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
| CloneNew | 215.0 ns | 3.07 ns | 2.72 ns | 0.0780 |     - |     - |     328 B |
| CloneOld | 188.0 ns | 3.89 ns | 3.82 ns | 0.0780 |     - |     - |     328 B |
