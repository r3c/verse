using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using Verse.Schemas.Json;

namespace Verse.Test.Schemas;

[TestFixture]
public class JsonSchemaTester : SchemaTester<JsonValue>
{
    [Test]
    [TestCase(false, "{\"parent\" : [ { \"child\" : 123 } ]}", new[] { 123 })]
    [TestCase(true, "{\"parent\" : [ { \"child\" : 123 } ]}", new[] { 123 })]
    [TestCase(false, "{\"parent\" : [ { \"child\" : 123 }, { \"child\" : 124 } ]}", new[] { 123, 124 })]
    [TestCase(true, "{\"parent\" : [ { \"child\" : 123 }, { \"child\" : 124 } ]}", new[] { 123, 124 })]
    [TestCase(false, "{\"parent\" : { \"child\" : 123 } }", new int[0])]
    [TestCase(true, "{\"parent\" : { \"child\" : 123 } }", new[] { 123 })]
    public void DecodeObjectAsArray(bool scalarAsArray, string json, int[] expected)
    {
        var schema = Schema.CreateJson<int[]>(new JsonConfiguration { ReadScalarAsOneElementArray = scalarAsArray });

        schema.DecoderDescriptor
            .IsObject(Array.Empty<int>)
            .HasField<int[]>("parent", (_, value) => value)
            .IsArray<int>(e => e.ToArray())
            .IsObject(() => 0)
            .HasField<int>("child", (_, value) => value)
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, json, expected);
    }

    [Test]
    [TestCase("{\"a\": 1, \"b\": 2}", 1, 2)]
    [TestCase("{\"a\": 42, \"b\": 17}", 42, 17)]
    [TestCase("{\"a\": 42}", 42, 0)]
    [TestCase("{\"b\": 17}", 0, 17)]
    [TestCase("{}", 0, 0)]
    public void DecodeObjectUsingConverter(string json, int expected1, int expected2)
    {
        var schema = Schema.CreateJson<(int, int)>();
        var obj = schema.DecoderDescriptor
            .IsObject(() => new int[2], intermediate => (intermediate[0], intermediate[1]));

        obj
            .HasField("a", SetterHelper.Mutation((int[] entity, int value) => entity[0] = value))
            .IsValue(schema.NativeTo.Integer32S);

        obj
            .HasField("b", SetterHelper.Mutation((int[] entity, int value) => entity[1] = value))
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, json, (expected1, expected2));
    }

    [Test]
    [TestCase("{\"value1\" : { \"value2\" : 123 } }", "123")]
    [TestCase("{\"value1\" : { \"value2\" : null } }", "defaultFromValue1")]
    [TestCase("{\"value1\" : { } }", "defaultFromValue1")]
    [TestCase("{\"value1\" : null }", "default")]
    [TestCase("{\"value2\" : 123 }", "default")]
    [TestCase("{}", "default")]
    public void DecodeNullField(string json, string expected)
    {
        var schema = Schema.CreateJson<string>();

        schema.DecoderDescriptor
            .IsObject(() => "default")
            .HasField<string>("value1", (_, v) => v)
            .IsObject(() => "defaultFromValue1")
            .HasField<string>("value2", (_, v) => v)
            .IsValue(value => value.Type switch
            {
                JsonType.Boolean => value.Boolean.ToString(),
                JsonType.Number => value.Number.ToString(CultureInfo.InvariantCulture),
                JsonType.String => value.String,
                JsonType.Undefined => "null",
                _ => throw new ArgumentOutOfRangeException(nameof(value.Type), value.Type, "invalid type")
            });

        var parser = schema.CreateDecoder();

        var decoderStream = parser.Open(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        Assert.IsTrue(decoderStream.TryDecode(out var result));
        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase("{\"value1\" : { \"value2\" : 123 } }", "123")]
    [TestCase("{\"value1\" : { \"value2\" : null } }", "defaultFromValue1")]
    [TestCase("{\"value1\" : { } }", "defaultFromValue1")]
    [TestCase("{\"value1\" : null }", "default")]
    [TestCase("{\"value2\" : 123 }", "default")]
    [TestCase("{}", "default")]
    public void DecodeNullFieldWithValueAssign(string json, string expected)
    {
        var schema = Schema.CreateJson<string>();

        schema.DecoderDescriptor
            .IsObject(() => "default")
            .HasField<string>("value1", (_, v) => v)
            .IsObject(() => "defaultFromValue1")
            .HasField<string>("value2", (_, v) => v)
            .IsValue(value => value.Type switch
            {
                JsonType.Boolean => value.Boolean.ToString(),
                JsonType.Number => value.Number.ToString(CultureInfo.InvariantCulture),
                JsonType.String => value.String,
                JsonType.Undefined => "null",
                _ => throw new ArgumentOutOfRangeException(nameof(value.Type), value.Type, "invalid type")
            });

        var parser = schema.CreateDecoder();

        var decoderStream = parser.Open(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        Assert.IsTrue(decoderStream.TryDecode(out var result));
        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase("{}", false)]
    [TestCase("{\"field\": null}", false)]
    [TestCase("{\"field\": 1}", true)]
    public void DecodeArrayIncompatibleInObject(string json, bool expectValue)
    {
        var schema = Schema.CreateJson<Container<int[]>>();

        schema.DecoderDescriptor
            .IsObject(() => new Container<int[]>())
            .HasField("field", SetterHelper.Mutation((Container<int[]> parent, int[] field) => parent.Value = field))
            .IsArray<int>(e => e.ToArray())
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, json,
            new Container<int[]> { Value = expectValue ? Array.Empty<int>() : null });
    }

    [Test]
    [TestCase("null", null)]
    [TestCase("1", new int[0])]
    public void DecodeArrayIncompatibleInRoot(string json, int[] expected)
    {
        var schema = Schema.CreateJson<int[]>();

        schema.DecoderDescriptor
            .IsArray<int>(e => e.ToArray())
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, json, expected);
    }

    [Test]
    [TestCase(false, "{\"key1\": 27.5, \"key2\": 19}", new double[0])]
    [TestCase(true, "{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
    public void DecodeValueAsArray(bool acceptAsArray, string json, double[] expected)
    {
        var schema = Schema.CreateJson<double[]>(new JsonConfiguration { ReadObjectValuesAsArray = acceptAsArray });

        schema.DecoderDescriptor
            .IsArray<double>(e => e.ToArray())
            .IsValue(schema.NativeTo.Float64);

        AssertDecodeAndEqual(schema, json, expected);
    }

    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[-42.1]", new[] { -42.1 })]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void DecodeArrayOfIntegers(string json, double[] expected)
    {
        var schema = Schema.CreateJson<double[]>();

        schema.DecoderDescriptor
            .IsArray<double>(e => e.ToArray())
            .IsValue(schema.NativeTo.Float64);

        AssertDecodeAndEqual(schema, json, expected);
    }

    [Test]
    [TestCase("~", 1)]
    [TestCase("\"Unfinished", 13)]
    [TestCase("[1.1.1]", 5)]
    [TestCase("[0 0]", 4)]
    [TestCase("{0}", 2)]
    [TestCase("{\"\" 0}", 5)]
    [TestCase("fail", 3)]
    public void DecodeFailsWithInvalidStream(string json, int expected)
    {
        var schema = Schema.CreateJson<string>();

        schema.DecoderDescriptor.IsValue(schema.NativeTo.String);

        var decoder = schema.CreateDecoder();
        var position = -1;

        decoder.Error += (p, m) => position = p;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        using (var decoderStream = decoder.Open(stream))
            Assert.IsFalse(decoderStream.TryDecode(out _));

        Assert.AreEqual(expected, position);
    }

    [Test]
    public void DecodeFieldFailsWhenNameDoesNotMatch()
    {
        var schema = Schema.CreateJson<int>();

        schema.DecoderDescriptor
            .IsObject(() => 0)
            .HasField<int>("match", (_, value) => value)
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, "{\"match1\": 17}", 0);
        AssertDecodeAndEqual(schema, "{\"matc\": 17}", 0);
    }

    [Test]
    public void DecodeFieldFailsWhenScopeDoesNotMatch()
    {
        var schema = Schema.CreateJson<int>();

        schema.DecoderDescriptor
            .IsObject(() => 0)
            .HasField<int>("match", (_, v) => v)
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, "{\"unknown\": {\"match\": 17}}", 0);
    }

    [Test]
    public void DecodeFieldFromSameEntity()
    {
        var schema = Schema.CreateJson<int[]>();
        var root = schema.DecoderDescriptor.IsObject(() => new[] { 0, 0 });

        root
            .HasField("virtual0", SetterHelper.Mutation((int[] target, int source) => target[0] = source))
            .IsValue(schema.NativeTo.Integer32S);

        root
            .HasField("virtual1", SetterHelper.Mutation((int[] target, int source) => target[1] = source))
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, "{\"virtual0\": 42, \"virtual1\": 17}", new[] { 42, 17 });
    }

    [Test]
    [TestCase("b", "{\"a\": 50, \"b\": 43, \"c\": [1, 5, 9]}", 43)]
    [TestCase("b", "{\"a\": {\"x\": 1, \"y\": 2}, \"b\": \"OK\", \"c\": 21.6}", "OK")]
    [TestCase("glittering", "{\"glittering\": \"prizes\"}", "prizes")]
    [TestCase("", "{\"\": 5}", 5)]
    [TestCase("1", "[\"hey!\", \"take me!\", \"not me!\"]", "take me!")]
    public void DecodeFieldWithinIgnoredContents<T>(string name, string json, T expected)
    {
        var schema = Schema.CreateJson<T>();
        var converter = SchemaHelper<JsonValue>.GetDecoderConverter<T>(schema.NativeTo);

        schema.DecoderDescriptor
            .IsObject(() => default)
            .HasField<T>(name, (_, v) => v)
            .IsValue(converter);

        AssertDecodeAndEqual(schema, json, expected);
    }

    [Test]
    public void DecodeRecursiveSchema()
    {
        var schema = Schema.CreateJson<RecursiveEntity>();
        var root = schema.DecoderDescriptor.IsObject(() => new RecursiveEntity());

        root
            .HasField("f", SetterHelper.Mutation((RecursiveEntity r, RecursiveEntity v) => r.Field = v), schema.DecoderDescriptor);

        root
            .HasField("v", SetterHelper.Mutation((RecursiveEntity r, int v) => r.Value = v))
            .IsValue(schema.NativeTo.Integer32S);

        var decoder = schema.CreateDecoder();

        using var stream = new MemoryStream("{\"f\": {\"f\": {\"v\": 42}, \"v\": 17}, \"v\": 3}"u8.ToArray());
        using var decoderStream = decoder.Open(stream);

        Assert.IsTrue(decoderStream.TryDecode(out var value));

        Assert.AreEqual(42, value.Field.Field.Value);
        Assert.AreEqual(17, value.Field.Value);
        Assert.AreEqual(3, value.Value);
    }

    [Test]
    [TestCase(false, "19", 0)]
    [TestCase(true, "19", 19)]
    [TestCase(true, "\"test\"", "test")]
    public void DecodeObjectAsArray<T>(bool acceptObjectAsArray, string json, T expected)
    {
        var schema = Schema.CreateJson<T>(new JsonConfiguration { ReadScalarAsOneElementArray = acceptObjectAsArray });
        var converter = SchemaHelper<JsonValue>.GetDecoderConverter<T>(schema.NativeTo);

        schema.DecoderDescriptor
            .IsArray<T>(elements => elements.FirstOrDefault())
            .IsValue(converter);

        AssertDecodeAndEqual(schema, json, expected);
    }

    [Test]
    [TestCase("\"test\"")]
    [TestCase("53")]
    public void DecodeValueFailsWhenTypeDoesNotMatch(string json)
    {
        var schema = Schema.CreateJson<int>();

        schema.DecoderDescriptor
            .IsArray<string>(elements => elements.Count())
            .IsValue(schema.NativeTo.String);

        AssertDecodeAndEqual(schema, json, 0);
    }

    [Test]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public void DecodeValueFromMultipleInput(int count)
    {
        var schema = Schema.CreateJson<int>();

        schema.DecoderDescriptor.IsValue(schema.NativeTo.Integer32S);

        var json = string.Join(" ", Enumerable.Range(0, count).Select(i => i.ToString()));

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        using var decoderStream = schema.CreateDecoder().Open(stream);

        for (var i = 0; i < count; ++i)
        {
            Assert.That(decoderStream.TryDecode(out var value), Is.True);
            Assert.That(value, Is.EqualTo(i));
        }
    }

    [Test]
    [TestCase("false", false)]
    [TestCase("true", true)]
    [TestCase("27", 27.0)]
    [TestCase("-15.5", -15.5)]
    [TestCase("-.5645", -.5645)]
    [TestCase("1.2e3", 1200.0)]
    [TestCase(".5e-2", 0.005)]
    [TestCase("2.945", 2.945)]
    [TestCase("1.1111111111111111111111", 1.1111111111111111111111)]
    [TestCase("8976435454354345437845468735", 8976435454354345437845468735d)]
    [TestCase("9007199254740992", 9007199254740992)] // 2^53
    [TestCase("-9007199254740992", -9007199254740992)] // -2^53
    [TestCase("\"\"", "")]
    [TestCase("\"Hello, World!\"", "Hello, World!")]
    [TestCase("\"\\u00FF \\u0066 \\uB3A8\"", "\xFF f \uB3A8")]
    [TestCase("\"\\\"\"", "\"")]
    [TestCase("\"\\\\\"", "\\")]
    [TestCase("9223372036854775807", 9223372036854775807.0)]
    public void DecodeValueFromNativeConstant<T>(string json, T expected)
    {
        var schema = Schema.CreateJson<T>();
        var converter = SchemaHelper<JsonValue>.GetDecoderConverter<T>(schema.NativeTo);

        schema.DecoderDescriptor.IsValue(converter);

        AssertDecodeAndEqual(schema, json, expected);
    }

    [TestCase(0d)]
    [TestCase(1d)]
    [TestCase(10d)]
    [TestCase(100d)]
    [TestCase(0.1d)]
    [TestCase(45d)]
    [TestCase(.45d)]
    [TestCase(-45d)]
    [TestCase(-.45d)]
    [TestCase(12.56496d)]
    [TestCase(double.NaN)]
    [TestCase(double.MaxValue)]
    [TestCase(double.MinValue)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.Epsilon)]
    [TestCase(4.94065645841247E-323)]
    public void JsonValueFromNumberTest(double value)
    {
        var jsonValue = JsonValue.FromNumber(value);
        Assert.AreEqual(value, jsonValue.Number);
    }

    [Test]
    public void DecodeValueFromNativeDecimal()
    {
        var schema = Schema.CreateJson<decimal>();

        schema.DecoderDescriptor.IsValue(schema.NativeTo.Decimal);

        AssertDecodeAndEqual(schema, "1e-28", 1e-28m);
    }

    [Test]
    public void DecodeValueWithCustomConstructor()
    {
        const int fromConstructor = 17;

        var schema = Schema.CreateJson<Tuple<int, int, int>>();
        var descriptor = schema.DecoderDescriptor;

        var tuple = descriptor
            .IsObject(() => Tuple.Create(0, 0, fromConstructor))
            .HasField<Tuple<int, int>>("tuple", (target, field) => Tuple.Create(field.Item1, field.Item2, target.Item3));

        var tupleObject = tuple
            .IsObject(() => Tuple.Create(0, 0));

        tupleObject
            .HasField<int>("a", (target, value) => Tuple.Create(value, target.Item2))
            .IsValue(schema.NativeTo.Integer32S);

        tupleObject
            .HasField<int>("b", (target, value) => Tuple.Create(target.Item1, value))
            .IsValue(schema.NativeTo.Integer32S);

        AssertDecodeAndEqual(schema, "{\"tuple\": {\"a\": 5, \"b\": 7}}",
            Tuple.Create(5, 7, fromConstructor));
    }

    [Test]
    [TestCase("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
    [TestCase("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
    public void DecodeValueWithCustomDecoder(string json, string expected)
    {
        var schema = Schema.CreateJson<Guid>();

        schema.DecoderDescriptor
            .IsValue(source => Guid.TryParse(source.String, out var target) ? target : Guid.Empty);

        AssertDecodeAndEqual(schema, json, Guid.Parse(expected));
    }

    [Test]
    [TestCase(new int[0], "[]")]
    [TestCase(new[] { 21 }, "[21]")]
    [TestCase(new[] { 54, 90, -3, 34, 0, 49 }, "[54,90,-3,34,0,49]")]
    public void EncodeArrayOfIntegers(int[] value, string expected)
    {
        var schema = Schema.CreateJson<int[]>();

        schema.EncoderDescriptor.IsArray(entity => entity).IsValue(schema.NativeFrom.Integer32S);

        AssertEncodeAndEqual(schema, value, expected);
    }

    [Test]
    [TestCase(false, false, false, false, "{\"array\":[1,2],\"object\":{\"f\":3},\"value\":4}")]
    [TestCase(false, true, false, false, "{\"array\":null,\"object\":{\"f\":3},\"value\":4}")]
    [TestCase(false, false, true, false, "{\"array\":[1,2],\"object\":null,\"value\":4}")]
    [TestCase(false, false, false, true, "{\"array\":[1,2],\"object\":{\"f\":3},\"value\":null}")]
    [TestCase(true, false, false, false, "{\"array\":[1,2],\"object\":{\"f\":3},\"value\":4}")]
    [TestCase(true, true, false, false, "{\"object\":{\"f\":3},\"value\":4}")]
    [TestCase(true, false, true, false, "{\"array\":[1,2],\"value\":4}")]
    [TestCase(true, false, false, true, "{\"array\":[1,2],\"object\":{\"f\":3}}")]
    public void EncodeEntityOmitNull(bool omitNull, bool nullArray, bool nullObject, bool nullValue, string expected)
    {
        var schema = Schema.CreateJson<string>(new JsonConfiguration { OmitNull = omitNull });
        var descriptor = schema.EncoderDescriptor;
        var root = descriptor.IsObject();

        root
            .HasField("array", s => s)
            .IsArray(_ => nullArray ? null : new[] { 1, 2 })
            .IsValue(schema.NativeFrom.Integer32S);

        root
            .HasField("object", s => nullObject ? null : s)
            .IsObject()
            .HasField("f", s => s)
            .IsValue(_ => JsonValue.FromNumber(3));

        root
            .HasField("value", s => s)
            .IsValue(_ => nullValue ? JsonValue.Undefined : JsonValue.FromNumber(4));

        AssertEncodeAndEqual(schema, "test", expected);
    }

    [Test]
    [TestCase("glittering", "prizes", "{\"glittering\":\"prizes\"}")]
    [TestCase("", "pwic", "{\"\":\"pwic\"}")]
    public void EncodeField<T>(string name, T value, string expected)
    {
        var schema = Schema.CreateJson<T>();
        var converter = SchemaHelper<JsonValue>.GetEncoderConverter<T>(schema.NativeFrom);

        schema.EncoderDescriptor.IsObject().HasField(name, v => v).IsValue(converter);

        AssertEncodeAndEqual(schema, value, expected);
    }

    [Test]
    public void EncodeFieldFromSameEntity()
    {
        var schema = Schema.CreateJson<int[]>();
        var descriptor = schema.EncoderDescriptor;
        var root = descriptor.IsObject();

        root.HasField("virtual0", source => source[0]).IsValue(schema.NativeFrom.Integer32S);
        root.HasField("virtual1", source => source[1]).IsValue(schema.NativeFrom.Integer32S);

        AssertEncodeAndEqual(schema, new[] { 42, 17 }, "{\"virtual0\":42,\"virtual1\":17}");
    }

    [Test]
    [TestCase(false, "{\"value\":null}")]
    [TestCase(true, "{}")]
    public void EncodeFieldWithOmitNull(bool omitNull, string expected)
    {
        var schema = Schema.CreateJson<string>(new JsonConfiguration { OmitNull = omitNull });

        schema.EncoderDescriptor
            .IsObject()
            .HasField("value", v => v)
            .IsValue(_ => JsonValue.Undefined);

        AssertEncodeAndEqual(schema, "dummy", expected);
    }

    [Test]
    [TestCase(1, 2, "{\"a\":1,\"b\":2}")]
    [TestCase(42, 17, "{\"a\":42,\"b\":17}")]
    [TestCase(42, null, "{\"a\":42}")]
    [TestCase(null, 17, "{\"b\":17}")]
    public void EncodeObjectUsingConverter(int? value1, int? value2, string expected)
    {
        var schema = Schema.CreateJson<(int?, int?)>(new JsonConfiguration { OmitNull = true });
        var obj = schema.EncoderDescriptor
            .IsObject(intermediate => new[] { intermediate.Item1, intermediate.Item2 });

        obj
            .HasField("a", entity => entity[0])
            .IsValue(v => v.HasValue ? schema.NativeFrom.Integer32S(v.Value) : JsonValue.Undefined);

        obj
            .HasField("b", entity => entity[1])
            .IsValue(v => v.HasValue ? schema.NativeFrom.Integer32S(v.Value) : JsonValue.Undefined);

        AssertEncodeAndEqual(schema, (value1, value2), expected);
    }

    [Test]
    public void EncodeRecursiveDescriptorDefinedAfterUse()
    {
        var schema = Schema.CreateJson<RecursiveEntity>();
        var descriptor = schema.EncoderDescriptor;
        var root = descriptor.IsObject();

        root.HasField("field", o => o.Field, descriptor);
        root.HasField("value", o => o.Value).IsValue(schema.NativeFrom.Integer32S);

        AssertEncodeAndEqual(schema,
            new RecursiveEntity { Field = new RecursiveEntity { Value = 1 }, Value = 2 },
            "{\"field\":{\"field\":null,\"value\":1},\"value\":2}");
    }

    [Test]
    [TestCase("90f59097-d06a-4796-b8d5-87eb6af7ed8b", "\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"")]
    [TestCase("c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025", "\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"")]
    public void EncodeValueWithCustomEncoder(string guid, string expected)
    {
        var schema = Schema.CreateJson<Guid>();

        schema.EncoderDescriptor.IsValue(v => JsonValue.FromString(v.ToString()));

        AssertEncodeAndEqual(schema, Guid.Parse(guid), expected);
    }

    [Test]
    public void EncodeValueWithSpecialCharacters()
    {
        var schema = Schema.CreateJson<string>();

        schema.EncoderDescriptor.IsValue(schema.NativeFrom.String);

        AssertEncodeAndEqual(schema, "\f\n\r\t\x99", "\"\\f\\n\\r\\t\\u0099\"");
    }

    [Test]
    [TestCase(false, "false")]
    [TestCase(true, "true")]
    [TestCase(3, "3")]
    [TestCase(3.5, "3.5")]
    [TestCase("I sense a soul in search of answers", "\"I sense a soul in search of answers\"")]
    [TestCase("\xFF \u0066 \uB3A8", "\"\\u00FF f \\uB3A8\"")]
    [TestCase("\"", "\"\\\"\"")]
    [TestCase("\\", "\"\\\\\"")]
    public void EncodeValueNative<T>(T value, string expected)
    {
        var schema = Schema.CreateJson<T>();
        var converter = SchemaHelper<JsonValue>.GetEncoderConverter<T>(schema.NativeFrom);

        schema.EncoderDescriptor.IsValue(converter);

        AssertEncodeAndEqual(schema, value, expected);
    }

    [Test]
    [TestCase(false, "null")]
    [TestCase(true, "null")]
    public void EncodeValueWithOmitNull(bool omitNull, string expected)
    {
        var schema = Schema.CreateJson<string>(new JsonConfiguration { OmitNull = omitNull });

        schema.EncoderDescriptor.IsValue(schema.NativeFrom.String);

        AssertEncodeAndEqual(schema, null, expected);
    }

    [Test]
    public void RoundTripCustomType()
    {
        var schema = Schema.CreateJson<Container<Guid>>();

        var decoderConverters = new Dictionary<Type, object>
        {
            {
                typeof(Guid),
                new Func<JsonValue, Guid>(source => Guid.TryParse(source.String, out var target) ? target : Guid.Empty)
            }
        };

        var decoder = Linker.CreateDecoder(schema, decoderConverters, BindingFlags.Public | BindingFlags.Instance);

        var encoderConverters = new Dictionary<Type, object>
        {
            {typeof(Guid), new Func<Guid, JsonValue>(g => JsonValue.FromString(g.ToString()))}
        };

        var encoder = Linker.CreateEncoder(schema, encoderConverters, BindingFlags.Public | BindingFlags.Instance);

        SchemaHelper<JsonValue>.AssertRoundTripWithCustom(decoder, encoder, new Container<Guid> { Value = Guid.NewGuid() });
    }

    protected override ISchema<JsonValue, TEntity> CreateSchema<TEntity>()
    {
        return Schema.CreateJson<TEntity>(new JsonConfiguration { OmitNull = true });
    }

    private static void AssertDecodeAndEqual<TEntity>(ISchema<JsonValue, TEntity> schema, string json,
        TEntity expected)
    {
        var decoder = schema.CreateDecoder();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        using var decoderStream = decoder.Open(stream);

        Assert.IsTrue(decoderStream.TryDecode(out var value));

        var compare = new CompareLogic();
        var result = compare.Compare(expected, value);

        Assert.That(result.AreEqual, Is.True, result.DifferencesString);
    }

    private static void AssertEncodeAndEqual<TEntity>(ISchema<JsonValue, TEntity> schema, TEntity value,
        string expected)
    {
        var encoder = schema.CreateEncoder();

        using var stream = new MemoryStream();

        using (var encoderStream = encoder.Open(stream))
            encoderStream.Encode(value);

        Assert.That(Encoding.UTF8.GetString(stream.ToArray()), Is.EqualTo(expected));
    }

    private class Container<T>
    {
        public T Value { get; set; }
    }

    private class RecursiveEntity
    {
        public RecursiveEntity Field;

        public int Value;
    }
}