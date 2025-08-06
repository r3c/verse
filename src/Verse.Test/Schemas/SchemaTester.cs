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

        SchemaHelper<TNative>.AssertRoundTripWithLinker(Format, schema, new NestedArray
        {
            Children =
            [
                new NestedArray
                {
                    Children = [],
                    Value = "a"
                },
                new NestedArray
                {
                    Children =
                    [
                        new NestedArray
                        {
                            Children = [],
                            Value = "b"
                        },
                        new NestedArray
                        {
                            Children = [],
                            Value = "c"
                        }
                    ],
                    Value = "d"
                },
                new NestedArray
                {
                    Children = [],
                    Value = "e"
                }
            ],
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
            .IsValue(Format.To.Integer32S);

        schema.EncoderDescriptor
            .IsObject()
            .HasField("virtual")
            .IsObject()
            .HasField("value", source => source)
            .IsValue(Format.From.Integer32S);

        SchemaHelper<TNative>.AssertRoundTripWithCustom(schema.CreateDecoder(), schema.CreateEncoder(), 17);
    }

    [Test]
    [Ignore("Enum types are not handled by linker yet.")]
    public void RoundTripObjectMixedFieldTypes()
    {
        var schema = CreateSchema<MixedContainer>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(Format, schema, new MixedContainer
        {
            Floats = [1.1f, 2.2f, 3.3f],
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

        SchemaHelper<TNative>.AssertRoundTripWithLinker(Format, schema, new Container<double?> { Value = value });
    }

    [Test]
    [TestCase("Hello")]
    [TestCase(25.5)]
    public void RoundTripValueNative<T>(T instance)
    {
        var schema = CreateSchema<T>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(Format, schema, instance);
    }

    [Test]
    public void RoundTripValueNested()
    {
        var schema = CreateSchema<NestedValue>();

        SchemaHelper<TNative>.AssertRoundTripWithLinker(Format, schema, new NestedValue
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

        SchemaHelper<TNative>.AssertRoundTripWithLinker(Format, schema, value);
    }

    protected abstract ISchema<TNative, TEntity> CreateSchema<TEntity>();

    protected abstract IFormat<TNative> Format { get; }

    private struct Container<T>
    {
        public T Value;
    }

    private class MixedContainer
    {
        public float[] Floats = [];
        public short Integer;
        public SomeEnum Option;
        public Dictionary<string, string> Pairs = new();
        public string Text = string.Empty;
    }

    private class NestedArray
    {
        public NestedArray[] Children = [];
        public string Value = string.Empty;
    }

    private class NestedValue
    {
        public NestedValue? Child;
        public int Value;
    }

    private enum SomeEnum
    {
        A,
        B,
        C
    }
}