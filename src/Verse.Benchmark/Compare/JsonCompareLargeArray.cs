using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Verse.Benchmark.Compare;

public class JsonCompareLargeArray : JsonCompare<IReadOnlyList<int>>
{
    [Params(1, 100, 10000, 1000000)]
    public int Length { get; set; }

    protected override IReadOnlyList<int> Instance => Enumerable.Range(0, Length)
        .Select(i => (int)((ulong)i * 65563UL % int.MaxValue))
        .ToList();
}