using System;
using System.Collections.Generic;
using NUnit.Framework;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local

namespace Verse.Test.Schemas;

public abstract class SchemaTester<TNative>
{
    [Test]
    public void RoundTripArrayNested()
    {
        var schema = CreateSchema<NestedArray>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(schema, new NestedArray
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
    public void RoundTripObjectFlattenField()
    {
        var schema = CreateSchema<int>();

        schema.DecoderDescriptor
            .IsObject(() => 0)
            .HasField("virtual")
            .IsObject(() => 0)
            .HasField<int>("value", (_, source) => source)
            .IsValue(schema.NativeTo.Integer32S);

        schema.EncoderDescriptor
            .IsObject()
            .HasField("virtual")
            .IsObject()
            .HasField("value", source => source)
            .IsValue(schema.NativeFrom.Integer32S);

        SchemaHelper<TNative>.AssertRoundTripWithCustom(schema.CreateDecoder(), schema.CreateEncoder(), 17);
    }

    [Test]
    [Ignore("Enum types are not handled by linker yet.")]
    public void RoundTripObjectMixedFieldTypes()
    {
        var schema = CreateSchema<MixedContainer>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(schema, new MixedContainer
        {
            Floats = new[] { 1.1f, 2.2f, 3.3f },
            Integer = 17,
            Option = SomeEnum.B,
            Pairs = new Dictionary<string, string>
            {
                { "a", "aaa" },
                { "b", "bbb" }
            },
            Text = "Hello, World!"
        });
    }

    [Test]
    [TestCase(null)]
    [TestCase(42d)]
    public void RoundTripObjectNullableField(double? value)
    {
        var schema = CreateSchema<Container<double?>>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(schema, new Container<double?> { Value = value });
    }

    [Test]
    [TestCase("Hello")]
    [TestCase(25.5)]
    public void RoundTripValueNative<T>(T instance)
    {
        var schema = CreateSchema<T>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(schema, instance);
    }

    [Test]
    public void RoundTripValueNested()
    {
        var schema = CreateSchema<NestedValue>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(schema, new NestedValue
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
    [TestCase(null)]
    [TestCase(42d)]
    public void RoundTripValueNullable(double? value)
    {
        var schema = CreateSchema<double?>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(schema, value);
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