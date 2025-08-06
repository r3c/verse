using System;
using BenchmarkDotNet.Attributes;

namespace Verse.Benchmark.Compare;

[MemoryDiagnoser]
public class JsonCompareNestedArray : JsonCompare<JsonCompareNestedArray.MyNestedArray>
{
    protected override MyNestedArray Instance => new()
    {
        Children =
        [
            new MyNestedArray
            {
                Children = null,
                Value = "a"
            },
            new MyNestedArray
            {
                Children =
                [
                    new MyNestedArray
                    {
                        Children = null,
                        Value = "b"
                    },
                    new MyNestedArray
                    {
                        Children = null,
                        Value = "c"
                    }
                ],
                Value = "d"
            },
            new MyNestedArray
            {
                Children = [],
                Value = "e"
            }
        ],
        Value = "f"
    };

    public class MyNestedArray : IEquatable<MyNestedArray>
    {
        public MyNestedArray[] Children;

        public string Value;

        public bool Equals(MyNestedArray other)
        {
            if (other is null || Children.Length != other.Children.Length)
                return false;

            for (var i = 0; i < Children.Length; ++i)
            {
                if (!Equals(Children[i], other.Children[i]))
                    return false;
            }

            return Value == other.Value;
        }
    }
}