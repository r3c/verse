using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoBuf;
using Verse.Formats.Protobuf;
using Verse.Formats.RawProtobuf;
using Verse.Schemas;
using Verse.Schemas.RawProtobuf;

namespace Verse.Test.Schemas;

[TestFixture]
internal class RawProtobufSchemaTester
{
    private const string LongString =
        "Verum ad istam omnem orationem brevis est defensio. " +
        "Nam quoad aetas M. Caeli dare potuit isti suspicioni locum, fuit primum ipsius pudore, " +
        "deinde etiam patris diligentia disciplinaque munita. " +
        "Qui ut huic virilem togam deditšnihil dicam hoc loco de me; " +
        "tantum sit, quantum vos existimatis; hoc dicam, hunc a patre continuo ad me esse deductum; " +
        "nemo hunc M. Caelium in illo aetatis flore vidit nisi aut cum patre aut mecum aut in M. Crassi castissima domo, cum artibus honestissimis erudiretur.";

    [Test]
    [TestCase(long.MinValue, long.MinValue, long.MinValue)]
    [TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
    [TestCase(float.MinValue, float.MinValue, float.MinValue)]
    [TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
    [TestCase(double.MinValue, double.MinValue, double.MinValue)]
    [TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
    [TestCase("", "", "")]
    [TestCase(LongString, LongString, LongString)]
    public void DecodeRepeatedScalarFromObject<T>(T a, T b, T c)
    {
        var schema = CreateSchema<List<T>>();
        var converter = SchemaHelper<RawProtobufValue>.GetDecoderConverter<T>(Format.RawProtobuf.To);
        var testFieldClass = new TestFieldClass<T> { Items = [a, b, c] };

        schema.DecoderDescriptor
            .IsObject(() => [])
            .HasField("_2", SetterHelper.Mutation((List<T> target, T[] values) => target.AddRange(values)))
            .IsArray<T>(elements => elements.ToArray())
            .IsValue(converter);

        var value = DecodeTranscode(schema.CreateDecoder(), testFieldClass);

        Assert.That(testFieldClass.Items, Is.EqualTo(value).AsCollection);
    }

    [Test]
    [TestCase(long.MinValue)]
    [TestCase(long.MaxValue)]
    [TestCase(float.MinValue)]
    [TestCase(float.MaxValue)]
    [TestCase(double.MinValue)]
    [TestCase(double.MaxValue)]
    [TestCase("")]
    [TestCase(LongString)]
    public void DecodeScalarFromNestedObject<T>(T expectedValue)
    {
        var schema = CreateSchema<T>();
        var converter = SchemaHelper<RawProtobufValue>.GetDecoderConverter<T>(Format.RawProtobuf.To);
        var testFieldClass = new TestFieldClass<T> { SubValue = new SubTestFieldClass<T> { Value = expectedValue } };

        schema.DecoderDescriptor
            .IsObject(() => default!)
            .HasField<T>("_3", (_, v) => v)
            .IsObject(() => default!)
            .HasField<T>("_4", (_, v) => v)
            .IsValue(converter);

        var value = DecodeTranscode(schema.CreateDecoder(), testFieldClass);

        Assert.That(expectedValue, Is.EqualTo(value));
    }

    [Test]
    [TestCase(long.MinValue, ProtobufType.Signed)]
    [TestCase(long.MaxValue, ProtobufType.Signed)]
    [TestCase(float.MinValue, ProtobufType.Float32)]
    [TestCase(float.MaxValue, ProtobufType.Float32)]
    [TestCase(0f, ProtobufType.Float32)]
    [TestCase(double.MinValue, ProtobufType.Float64)]
    [TestCase(double.MaxValue, ProtobufType.Float64)]
    [TestCase(0.0, ProtobufType.Float64)]
    [TestCase("", ProtobufType.String)]
    [TestCase(LongString, ProtobufType.String)]
    public void DecodeScalarFromObject<T>(T value, ProtobufType type)
    {
        var schema = CreateSchema<T>();
        var testFieldClass = new TestFieldClass<T> { Value = value };
        var converter = SchemaHelper<RawProtobufValue>.GetDecoderConverter<T>(Format.RawProtobuf.To);

        schema.DecoderDescriptor
            .IsObject(() => default!)
            .HasField<T>("_1", (_, v) => v)
            .IsValue(converter);

        var decodedValue = DecodeTranscode(schema.CreateDecoder(), testFieldClass);

        Assert.That(value, Is.EqualTo(decodedValue));
    }

    [Test]
    [TestCase(long.MinValue, long.MinValue, long.MinValue)]
    [TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
    [TestCase(float.MinValue, float.MinValue, float.MinValue)]
    [TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
    [TestCase(double.MinValue, double.MinValue, double.MinValue)]
    [TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
    [TestCase("", "", "")]
    [TestCase(LongString, LongString, LongString)]
    public void DecodeScalarFromRepeatedObject<T>(T a, T b, T c)
    {
        T[] expectedValues = [a, b, c];
        var schema = CreateSchema<TestFieldClass<TestFieldClass<T>>>();
        var testFieldClass = new TestFieldClass<TestFieldClass<T>>();

        foreach (var value in expectedValues)
        {
            testFieldClass.Items.Add(new TestFieldClass<T>
            {
                SubValue = new SubTestFieldClass<T>
                {
                    Value = value
                }
            });
        }

        var converter = SchemaHelper<RawProtobufValue>.GetDecoderConverter<T>(Format.RawProtobuf.To);

        schema.DecoderDescriptor
            .IsObject(() => new TestFieldClass<TestFieldClass<T>>())
            .HasField("_2",
                SetterHelper.Mutation((TestFieldClass<TestFieldClass<T>> t, TestFieldClass<T>[] v) =>
                    t.Items.AddRange(v)))
            .IsArray<TestFieldClass<T>>(elements => elements.ToArray())
            .IsObject(() => new TestFieldClass<T>())
            .HasField("_3",
                SetterHelper.Mutation((TestFieldClass<T> target, SubTestFieldClass<T> value) =>
                    target.SubValue = value))
            .IsObject(() => new SubTestFieldClass<T>())
            .HasField("_4", SetterHelper.Mutation((SubTestFieldClass<T> target, T value) => target.Value = value))
            .IsValue(converter);

        var decodedValue = DecodeRoundTrip(schema.CreateDecoder(), testFieldClass);

        Assert.That(expectedValues.Length, Is.EqualTo(decodedValue.Items.Count));

        for (var i = 0; i < expectedValues.Length; ++i)
        {
            Assert.That(expectedValues[i], Is.EqualTo(decodedValue.Items[i].SubValue.Value));
        }
    }

    [Test]
    [TestCase(0, 0, 10)]
    [TestCase(0, 1, 20)]
    [TestCase(0, 2, 30)]
    [TestCase(1, 0, 40)]
    [TestCase(1, 1, 50)]
    [TestCase(1, 2, 60)]
    [TestCase(2, 0, 70)]
    [TestCase(2, 1, 80)]
    [TestCase(2, 2, 90)]
    [TestCase(0, null, 10)]
    [TestCase(1, null, 40)]
    [TestCase(2, null, 70)]
    [TestCase(null, 0, 10)]
    [TestCase(null, 1, 20)]
    [TestCase(null, 2, 30)]
    [TestCase(null, null, 10)]
    [Ignore("This feature is currently not supported")]
    public void DecodeFromNestedFixedIndex(int? index0, int? index1, int expected)
    {
        var schema = CreateSchema<int>();
        var testFieldClass = new TestFieldClass<TestFieldClass<SubTestFieldClass<int>>>
        {
            Items =
            [
                new()
                {
                    Items =
                    [
                        new() { Value = 10 },
                        new() { Value = 20 },
                        new() { Value = 30 }
                    ]
                },

                new()
                {
                    Items =
                    [
                        new() { Value = 40 },
                        new() { Value = 50 },
                        new() { Value = 60 }
                    ]
                },

                new()
                {
                    Items =
                    [
                        new() { Value = 70 },
                        new() { Value = 80 },
                        new() { Value = 90 }
                    ]
                }
            ]
        };

        var descriptor = schema.DecoderDescriptor
            .IsObject(() => 0!)
            .HasField<int>("_2", (_, v) => v);

        if (index0.HasValue)
        {
            descriptor = descriptor
                .IsObject(() => 0!)
                .HasField<int>(index0.Value.ToString(CultureInfo.InvariantCulture), (_, v) => v);
        }

        descriptor = descriptor
            .IsObject(() => 0!)
            .HasField<int>("_2", (_, v) => v);

        if (index1.HasValue)
        {
            descriptor = descriptor
                .IsObject(() => 0!)
                .HasField<int>(index1.Value.ToString(CultureInfo.InvariantCulture), (_, v) => v);
        }

        descriptor
            .IsObject(() => 0)
            .HasField<int>("_4", (_, v) => v)
            .IsValue(Format.RawProtobuf.To.Integer32S);

        var value = DecodeTranscode(schema.CreateDecoder(), testFieldClass);

        Assert.That(expected, Is.EqualTo(value));
    }

    [Test]
    [TestCase(long.MinValue, long.MinValue, long.MinValue)]
    [TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
    [TestCase(float.MinValue, float.MinValue, float.MinValue)]
    [TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
    [TestCase(double.MinValue, double.MinValue, double.MinValue)]
    [TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
    [TestCase("", "", "")]
    [TestCase(LongString, LongString, LongString)]
    public void EncodeRepeatedScalarToObject<T>(T a, T b, T c)
    {
        var expectedItems = new[] { a, b, c };
        var schema = CreateSchema<List<T>>();
        var converter = SchemaHelper<RawProtobufValue>.GetEncoderConverter<T>(Format.RawProtobuf.From);

        schema.EncoderDescriptor
            .IsObject()
            .HasField("_2", v => v)
            .IsArray(source => source)
            .IsValue(converter);

        var testFieldClass = EncodeTranscode<List<T>, TestFieldClass<T>>(schema.CreateEncoder(), [.. expectedItems]);

        Assert.That(expectedItems, Is.EqualTo(testFieldClass.Items).AsCollection);
    }

    [Test]
    [TestCase(long.MinValue)]
    [TestCase(long.MaxValue)]
    [TestCase(float.MinValue)]
    [TestCase(float.MaxValue)]
    [TestCase(double.MinValue)]
    [TestCase(double.MaxValue)]
    [TestCase("")]
    [TestCase(LongString)]
    public void EncodeScalarToNestedObject<T>(T expectedValue)
    {
        var schema = CreateSchema<TestFieldClass<T>>();
        var testFieldClass = new TestFieldClass<T> { SubValue = new SubTestFieldClass<T> { Value = expectedValue } };
        var converter = SchemaHelper<RawProtobufValue>.GetEncoderConverter<T>(Format.RawProtobuf.From);

        schema.EncoderDescriptor
            .IsObject()
            .HasField("_3", target => target.SubValue)
            .IsObject()
            .HasField("_4", target => target.Value)
            .IsValue(converter);

        var decodedTestFieldClass = EncodeRoundTrip(schema.CreateEncoder(), testFieldClass);

        Assert.That(expectedValue, Is.EqualTo(decodedTestFieldClass.SubValue.Value));
    }

    [Test]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    [TestCase(long.MinValue)]
    [TestCase(long.MaxValue)]
    [TestCase(float.MinValue)]
    [TestCase(float.MaxValue)]
    [TestCase(double.MinValue)]
    [TestCase(double.MaxValue)]
    [TestCase("")]
    [TestCase(LongString)]
    public void EncodeScalarToObject<T>(T value)
    {
        var schema = CreateSchema<T>();
        var converter = SchemaHelper<RawProtobufValue>.GetEncoderConverter<T>(Format.RawProtobuf.From);

        schema.EncoderDescriptor
            .IsObject()
            .HasField("_1", v => v)
            .IsValue(converter);

        var testFieldClass =
            EncodeTranscode<T, TestFieldClass<T>>(schema.CreateEncoder(),
                value);

        Assert.That(value, Is.EqualTo(testFieldClass.Value));
    }

    [Test]
    [TestCase(long.MinValue, long.MinValue, long.MinValue)]
    [TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
    [TestCase(float.MinValue, float.MinValue, float.MinValue)]
    [TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
    [TestCase(double.MinValue, double.MinValue, double.MinValue)]
    [TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
    [TestCase("", "", "")]
    [TestCase(LongString, LongString, LongString)]
    public void EncodeScalarToRepeatedObject<T>(T a, T b, T c)
    {
        var fieldClass = new TestFieldClass<TestFieldClass<T>>();
        var schema = CreateSchema<TestFieldClass<TestFieldClass<T>>>();
        var converter = SchemaHelper<RawProtobufValue>.GetEncoderConverter<T>(Format.RawProtobuf.From);
        var expectedValues = new[] { a, b, c };

        foreach (var value in expectedValues)
        {
            fieldClass.Items.Add(new TestFieldClass<T>
            {
                SubValue = new SubTestFieldClass<T>
                {
                    Value = value
                }
            });
        }

        schema.EncoderDescriptor
            .IsObject()
            .HasField("_2", v => v)
            .IsArray(source => source.Items)
            .IsObject()
            .HasField("_3", target => target.SubValue)
            .IsObject()
            .HasField("_4", target => target.Value)
            .IsValue(converter);

        var decodedFieldClass = EncodeRoundTrip(schema.CreateEncoder(), fieldClass);

        Assert.That(expectedValues.Length, Is.EqualTo(decodedFieldClass.Items.Count));

        for (var i = 0; i < expectedValues.Length; ++i)
            Assert.That(expectedValues[i], Is.EqualTo(decodedFieldClass.Items[i].SubValue.Value));
    }

    private static ISchema<RawProtobufValue, TEntity> CreateSchema<TEntity>()
    {
        return new RawProtobufSchema<TEntity>(new RawProtobufConfiguration { NoZigZagEncoding = true });
    }

    private static T DecodeRoundTrip<T>(IDecoder<T> decoder, T input)
    {
        return DecodeTranscode(decoder, input);
    }

    private static TOutput DecodeTranscode<TInput, TOutput>(IDecoder<TOutput> decoder, TInput input)
    {
        using var stream = new MemoryStream();

        Serializer.Serialize(stream, input);

        stream.Seek(0, SeekOrigin.Begin);

        using var decoderStream = decoder.Open(stream);

        Assert.That(decoderStream.TryDecode(out var decoded), Is.True);

        return decoded!;
    }

    private static T EncodeRoundTrip<T>(IEncoder<T> encoder, T input)
    {
        return EncodeTranscode<T, T>(encoder, input);
    }

    private static TOutput EncodeTranscode<TInput, TOutput>(IEncoder<TInput> encoder, TInput input)
    {
        using var stream = new MemoryStream();

        using (var encoderStream = encoder.Open(stream))
            encoderStream.Encode(input);

        stream.Seek(0, SeekOrigin.Begin);

        return Serializer.Deserialize<TOutput>(stream);
    }

    [Serializable, ProtoContract(Name = @"SubTestFieldClass")]
    public class SubTestFieldClass<T> : IExtensible
    {
        private T _value = default!;

        [ProtoMember(4, IsRequired = true)]
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        private IExtension? _extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }

    [Serializable, ProtoContract(Name = @"TestFieldClass")]
    public class TestFieldClass<T> : IExtensible
    {
        private T _value = default!;

        [ProtoMember(1, IsRequired = true)]
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        private List<T> _items = [];

        [ProtoMember(2, IsRequired = true, DataFormat = DataFormat.Default)]
        public List<T> Items
        {
            get => _items;
            set => _items = value;
        }

        private SubTestFieldClass<T> _subValue = null!;

        [ProtoMember(3, IsRequired = true)]
        public SubTestFieldClass<T> SubValue
        {
            get => _subValue;
            set => _subValue = value;
        }

        private IExtension? _extensionObject;

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}