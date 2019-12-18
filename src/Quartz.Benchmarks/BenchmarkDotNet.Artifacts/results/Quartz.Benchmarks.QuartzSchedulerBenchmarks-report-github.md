``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.17763.914 (1809/October2018Update/Redstone5)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), X64 RyuJIT
  DefaultJob : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), X64 RyuJIT


```
|      Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------ |---------:|--------:|--------:|-------:|------:|------:|----------:|
|       Empty | 455.8 ns | 8.63 ns | 8.08 ns | 0.0663 |     - |     - |     280 B |
| OneInternal | 512.2 ns | 7.61 ns | 7.12 ns | 0.0849 |     - |     - |     360 B |
