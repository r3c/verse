using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Verse.Exceptions;
using Verse.Schemas.Json;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Verse.Test;

[TestFixture]
public class LinkerTester
{
    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void CreateDecoder_ArrayFromArray(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Schema.CreateJson<double[]>(), encoded);

        Assert.That(decoded, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void CreateDecoder_ArrayFromList(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Schema.CreateJson<List<double>>(), encoded);

        CollectionAssert.AreEqual(expected, decoded);
    }

    [Test]
    [TestCase("{\"Field\": 53}", 53)]
    [TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
    public void CreateDecoder_ObjectField<T>(string json, T expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Schema.CreateJson<FieldContainer<T>>(), encoded);

        Assert.AreEqual(expected, decoded.Field);
    }

    [Test]
    [TestCase("{\"Property\": 53}", 53)]
    [TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
    public void CreateDecoder_ObjectProperty<T>(string json, T expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Schema.CreateJson<PropertyContainer<T>>(), encoded);

        Assert.AreEqual(expected, decoded.Property);
    }

    [Test]
    [TestCase("{\"R\": {\"R\": {\"V\": 42}, \"V\": 17}, \"V\": 3}")]
    public void CreateDecoder_ObjectRecursive(string json)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Schema.CreateJson<Recursive>(), encoded);

        Assert.AreEqual(42, decoded.R.R.V);
        Assert.AreEqual(17, decoded.R.V);
        Assert.AreEqual(3, decoded.V);
    }

    [Test]
    [TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"IsPublic\":1}", "0:0:1")]
    [TestCase(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        "{\"IsPublic\":1,\"IsProtected\":2,\"_isPrivate\":3}", "3:2:1")]
    public void CreateDecoder_WithBindingFlags(BindingFlags bindings, string json, string expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoder = Linker.CreateReflection<JsonValue>()
            .SetBindingFlags(bindings)
            .CreateDecoder(Schema.CreateJson<Visibility>());
        var decoded = Decode(decoder, encoded);

        Assert.AreEqual(expected, decoded.ToString());
    }

    [Test]
    public void CreateDecoder_ObjectWithParameterlessConstructor()
    {
        var decoded = CreateDecoderAndDecode(Schema.CreateJson<ReferenceType>(), "{}"u8.ToArray());

        Assert.That(decoded, Is.InstanceOf<ReferenceType>());
    }

    [Test]
    public void CreateDecoder_ShouldThrowWhenNoParameterlessConstructor()
    {
        var schema = Schema.CreateJson<Uri>();
        var linker = Linker.CreateReflection<JsonValue>();

        Assert.That(() => linker.CreateDecoder(schema), Throws.InstanceOf<ConstructorNotFoundException>());
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void CreateEncoder_ArrayFromArray(double[] value, string expected)
    {
        var encoded = CreateEncoderAndEncode(Schema.CreateJson<double[]>(), value);

        Assert.That(Encoding.UTF8.GetString(encoded), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void CreateEncoder_ArrayFromList(double[] value, string expected)
    {
        var decoded = new List<double>(value);
        var encoded = CreateEncoderAndEncode(Schema.CreateJson<List<double>>(), decoded);

        Assert.That(Encoding.UTF8.GetString(encoded), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(53, "{\"Field\":53}")]
    [TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
    public void CreateEncoder_ObjectField<T>(T value, string expected)
    {
        var decoded = new FieldContainer<T> { Field = value };
        var encoded = CreateEncoderAndEncode(Schema.CreateJson<FieldContainer<T>>(), decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(53, "{\"Property\":53}")]
    [TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
    public void CreateEncoder_ObjectProperty<T>(T value, string expected)
    {
        var decoded = new PropertyContainer<T> { Property = value };
        var encoded = CreateEncoderAndEncode(Schema.CreateJson<PropertyContainer<T>>(), decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(false, "{\"R\":{\"R\":{\"R\":null,\"V\":42},\"V\":17},\"V\":3}")]
    [TestCase(true, "{\"R\":{\"R\":{\"V\":42},\"V\":17},\"V\":3}")]
    public void CreateEncoder_ObjectRecursive(bool omitNull, string expected)
    {
        var decoded = new Recursive { R = new Recursive { R = new Recursive { V = 42 }, V = 17 }, V = 3 };
        var schema = Schema.CreateJson<Recursive>(new JsonConfiguration { OmitNull = omitNull });
        var encoded = CreateEncoderAndEncode(schema, decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"IsPublic\":0}")]
    [TestCase(BindingFlags.Instance | BindingFlags.NonPublic, "{\"IsProtected\":0,\"_isPrivate\":0}")]
    public void CreateEncoder_WithBindingFlags(BindingFlags bindings, string expected)
    {
        var decoded = new Visibility();
        var encoder = Linker.CreateReflection<JsonValue>()
            .SetBindingFlags(bindings)
            .CreateEncoder(Schema.CreateJson<Visibility>());
        var encoded = Encode(encoder, decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase("[1, 3, 5]", new[] { 2, 6, 10 })]
    public void SetDecoderDescriptor_OverrideSystemType(string json, int[] expected)
    {
        var decoder = Linker
            .CreateReflection<JsonValue>()
            .SetDecoderDescriptor<int>(descriptor => descriptor.IsValue(v => (int)(v.Number * 2)))
            .CreateDecoder(Schema.CreateJson<int[]>());

        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = Decode(decoder, encoded);

        CollectionAssert.AreEqual(expected, decoded);
    }

    private static TEntity CreateDecoderAndDecode<TNative, TEntity>(ISchema<TNative, TEntity> schema, byte[] encoded)
    {
        var linker = Linker.CreateReflection<TNative>();
        var decoder = linker.CreateDecoder(schema);

        return Decode(decoder, encoded);
    }

    private static byte[] CreateEncoderAndEncode<TNative, TEntity>(ISchema<TNative, TEntity> schema, TEntity decoded)
    {
        var linker = Linker.CreateReflection<TNative>();
        var encoder = linker.CreateEncoder(schema);

        return Encode(encoder, decoded);
    }

    private static T Decode<T>(IDecoder<T> decoder, byte[] encoded)
    {
        using var stream = new MemoryStream(encoded);
        using var decoderStream = decoder.Open(stream);

        Assert.IsTrue(decoderStream.TryDecode(out var decoded));

        return decoded;
    }

    private static byte[] Encode<T>(IEncoder<T> encoder, T decoded)
    {
        using var stream = new MemoryStream();

        using (var encoderStream = encoder.Open(stream))
            encoderStream.Encode(decoded);

        return stream.ToArray();
    }

    public class ReferenceType
    {
    }

    private class FieldContainer<T>
    {
        public T Field;
    }

    private class PropertyContainer<T>
    {
        public T Property
        {
            get;
            set;
        }
    }

    private class Recursive
    {
        public Recursive R = null;

        public int V = 0;
    }

    public class Visibility
    {
        public int IsPublic = 0;
        protected int IsProtected = 0;
        private int _isPrivate = 0;

        public override string ToString()
        {
            return $"{_isPrivate}:{IsProtected}:{IsPublic}";
        }
    }
}