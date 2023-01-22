using System;
using System.Collections.Generic;
using NUnit.Framework;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local

namespace Verse.Test.Schemas;

public abstract class SchemaTester<TNative>
{
    [Test]
    public void RoundTripFieldFlatten()
    {
        var schema = CreateSchema<int>();

        schema.DecoderDescriptor
            .IsObject(() => 0)
            .HasField("virtual")
            .IsObject(() => 0)
            .HasField("value", (ref int target, int source) => target = source)
            .IsValue(schema.DecoderAdapter.ToInteger32S);

        schema.EncoderDescriptor
            .HasField("virtual")
            .HasField("value", source => source)
            .HasValue(schema.EncoderAdapter.FromInteger32S);

        SchemaHelper<TNative>.AssertRoundTrip(schema.CreateDecoder(), schema.CreateEncoder(), 17);
    }

    [Test]
    public void RoundTripNestedArray()
    {
        var schema = CreateSchema<NestedArray>();
        var decoder = Linker.CreateDecoder(schema);
        var encoder = Linker.CreateEncoder(schema);

        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, new NestedArray
        {
            Children = new[]
            {
                new NestedArray
                {
                    Children = Array.Empty<NestedArray>(),
                    Value = "a"
                },
                new NestedArray
                {
                    Children = new[]
                    {
                        new NestedArray
                        {
                            Children = Array.Empty<NestedArray>(),
                            Value = "b"
                        },
                        new NestedArray
                        {
                            Children = Array.Empty<NestedArray>(),
                            Value = "c"
                        }
                    },
                    Value = "d"
                },
                new NestedArray
                {
                    Children = Array.Empty<NestedArray>(),
                    Value = "e"
                }
            },
            Value = "f"
        });
    }

    [Test]
    [Ignore("Enum types are not handled by linker yet.")]
    public void RoundTripMixedTypes()
    {
        var schema = CreateSchema<MixedContainer>();
        var decoder = Linker.CreateDecoder(schema);
        var encoder = Linker.CreateEncoder(schema);

        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, new MixedContainer
        {
            Floats = new[] {1.1f, 2.2f, 3.3f},
            Integer = 17,
            Option = SomeEnum.B,
            Pairs = new Dictionary<string, string>
            {
                {"a", "aaa"},
                {"b", "bbb"}
            },
            Text = "Hello, World!"
        });
    }

    [Test]
    public void RoundTripNestedValue()
    {
        var schema = CreateSchema<NestedValue>();
        var decoder = Linker.CreateDecoder(schema);
        var encoder = Linker.CreateEncoder(schema);

        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, new NestedValue
        {
            Child = new NestedValue
            {
                Child = new NestedValue
                {
                    Child = null,
                    Value = 64
                },
                Value = 42
            },
            Value = 17
        });
    }

    [Test]
    [TestCase("Hello")]
    [TestCase(25.5)]
    public void RoundTripValueNative<T>(T instance)
    {
        var schema = CreateSchema<T>();
        var decoder = Linker.CreateDecoder(schema);
        var encoder = Linker.CreateEncoder(schema);

        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, instance);
    }

    [Test]
    public void RoundTripValueNullableField()
    {
        var schema = CreateSchema<Container<double?>>();
        var decoder = Linker.CreateDecoder(schema);
        var encoder = Linker.CreateEncoder(schema);

        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, new Container<double?>());
        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, new Container<double?> {Value = 42});
    }

    [Test]
    public void RoundTripValueNullableValue()
    {
        var schema = CreateSchema<double?>();
        var decoder = Linker.CreateDecoder(schema);
        var encoder = Linker.CreateEncoder(schema);

        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, null);
        SchemaHelper<TNative>.AssertRoundTrip(decoder, encoder, 42);
    }

    protected abstract ISchema<TNative, TEntity> CreateSchema<TEntity>();

    private struct Container<T>
    {
        public T Value;
    }

    private class MixedContainer
    {
        public float[] Floats;
        public short Integer;
        public SomeEnum Option;
        public Dictionary<string, string> Pairs;
        public string Text;
    }

    private class NestedArray
    {
        public NestedArray[] Children;
        public string Value;
    }

    private class NestedValue
    {
        public NestedValue Child;
        public int Value;
    }

    private enum SomeEnum
    {
        A,
        B,
        C
    }
}