using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Verse.Exceptions;
using Verse.Schemas;
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
    public void LinkDecoderArrayFromArray(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = Decode(Linker.CreateDecoder(Schema.CreateJson<double[]>()), encoded);

        CollectionAssert.AreEqual(expected, decoded);
    }

    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void LinkDecoderArrayFromList(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = Decode(Linker.CreateDecoder(Schema.CreateJson<List<double>>()), encoded);

        CollectionAssert.AreEqual(expected, decoded);
    }

    [Test]
    [TestCase("{\"Field\": 53}", 53)]
    [TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
    public void LinkDecoderField<T>(string json, T expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = Decode(Linker.CreateDecoder(Schema.CreateJson<FieldContainer<T>>()), encoded);

        Assert.AreEqual(expected, decoded.Field);
    }

    [Test]
    [TestCase("{\"Property\": 53}", 53)]
    [TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
    public void LinkDecoderProperty<T>(string json, T expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = Decode(Linker.CreateDecoder(Schema.CreateJson<PropertyContainer<T>>()), encoded);

        Assert.AreEqual(expected, decoded.Property);
    }

    [Test]
    public void LinkDecoderRecursive()
    {
        var encoded = Encoding.UTF8.GetBytes("{\"R\": {\"R\": {\"V\": 42}, \"V\": 17}, \"V\": 3}");
        var decoded = Decode(Linker.CreateDecoder(Schema.CreateJson<Recursive>()), encoded);

        Assert.AreEqual(42, decoded.R.R.V);
        Assert.AreEqual(17, decoded.R.V);
        Assert.AreEqual(3, decoded.V);
    }

    [Test]
    [TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"IsPublic\":1}", "0:0:1")]
    [TestCase(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, "{\"IsPublic\":1,\"IsProtected\":2,\"_isPrivate\":3}", "3:2:1")]
    public void LinkDecoderVisibility(BindingFlags bindings, string json, string expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded =
            Decode(
                Linker.CreateDecoder(Schema.CreateJson<Visibility>(), new Dictionary<Type, object>(), bindings),
                encoded);

        Assert.AreEqual(expected, decoded.ToString());
    }

    [Test]
    public void CreateDecoder_ShouldFindParameterlessConstructorFromReference()
    {
        var schema = Schema.CreateJson<ReferenceType>();
        var decoder = Linker.CreateDecoder(schema);
        var decoded = Decode(decoder, "{}"u8.ToArray());

        Assert.That(decoded, Is.InstanceOf<ReferenceType>());
    }

    [Test]
    public void CreateDecoder_ShouldThrowWhenNoParameterlessConstructor()
    {
        var schema = Schema.CreateJson<Uri>();

        Assert.That(() => Linker.CreateDecoder(schema), Throws.InstanceOf<ConstructorNotFoundException>());
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void LinkEncoderArrayFromArray(double[] value, string expected)
    {
        var encoded = Encode(Linker.CreateEncoder(Schema.CreateJson<double[]>()), value);

        CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void LinkEncoderArrayFromList(double[] value, string expected)
    {
        var decoded = new List<double>(value);
        var encoded = Encode(Linker.CreateEncoder(Schema.CreateJson<List<double>>()), decoded);

        CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(53, "{\"Field\":53}")]
    [TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
    public void LinkEncoderField<T>(T value, string expected)
    {
        var decoded = new FieldContainer<T> { Field = value };
        var encoded = Encode(Linker.CreateEncoder(Schema.CreateJson<FieldContainer<T>>()), decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(53, "{\"Property\":53}")]
    [TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
    public void LinkEncoderProperty<T>(T value, string expected)
    {
        var decoded = new PropertyContainer<T> { Property = value };
        var encoded = Encode(Linker.CreateEncoder(Schema.CreateJson<PropertyContainer<T>>()), decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(false, "{\"R\":{\"R\":{\"R\":null,\"V\":42},\"V\":17},\"V\":3}")]
    [TestCase(true, "{\"R\":{\"R\":{\"V\":42},\"V\":17},\"V\":3}")]
    public void LinkEncoderRecursive(bool omitNull, string expected)
    {
        var decoded = new Recursive { R = new Recursive { R = new Recursive { V = 42 }, V = 17 }, V = 3 };
        var encoded = Encode(Linker.CreateEncoder(Schema.CreateJson<Recursive>(new JsonConfiguration { OmitNull = omitNull })), decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
    }

    [Test]
    [TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"IsPublic\":0}")]
    [TestCase(BindingFlags.Instance | BindingFlags.NonPublic, "{\"IsProtected\":0,\"_isPrivate\":0}")]
    public void LinkEncoderVisibility(BindingFlags bindings, string expected)
    {
        var decoded = new Visibility();
        var encoded =
            Encode(
                Linker.CreateEncoder(Schema.CreateJson<Visibility>(), new Dictionary<Type, object>(), bindings),
                decoded);

        Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
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