﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Verse.Exceptions;
using Verse.Formats.Json;
using Verse.Schemas.Json;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Verse.Test.Linkers;

[TestFixture]
public class ReflectionLinkerTester
{
    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void CreateDecoder_ArrayFromArray(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Format.Json, Schema.CreateJson<double[]>(), encoded);

        Assert.That(decoded, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void CreateDecoder_ArrayFromCustom(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Format.Json, Schema.CreateJson<CustomList<double>>(), encoded);

        Assert.That(expected, Is.EqualTo(decoded).AsCollection);
    }

    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void CreateDecoder_ArrayFromInterface(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decodedResults = new[]
        {
            CreateDecoderAndDecode(Format.Json, Schema.CreateJson<ISet<double>>(), encoded),
#if NET6_0_OR_GREATER
            CreateDecoderAndDecode(Format.Json, Schema.CreateJson<IReadOnlySet<double>>(), encoded),
#endif
            CreateDecoderAndDecode(Format.Json, Schema.CreateJson<IList<double>>(), encoded),
            CreateDecoderAndDecode(Format.Json, Schema.CreateJson<IReadOnlyList<double>>(), encoded),
            CreateDecoderAndDecode(Format.Json, Schema.CreateJson<ICollection<double>>(), encoded),
            CreateDecoderAndDecode(Format.Json, Schema.CreateJson<IReadOnlyCollection<double>>(), encoded),
            CreateDecoderAndDecode(Format.Json, Schema.CreateJson<IEnumerable<double>>(), encoded)
        };

        foreach (var decoded in decodedResults)
            Assert.That(expected, Is.EqualTo(decoded).AsCollection);
    }

    [Test]
    [TestCase("[]", new double[0])]
    [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
    public void CreateDecoder_ArrayFromList(string json, double[] expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Format.Json, Schema.CreateJson<List<double>>(), encoded);

        Assert.That(expected, Is.EqualTo(decoded).AsCollection);
    }

    [Test]
    [TestCase("{\"Field\": 53}", 53)]
    [TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
    public void CreateDecoder_ObjectField<T>(string json, T expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Format.Json, Schema.CreateJson<FieldContainer<T>>(), encoded);

        Assert.That(expected, Is.EqualTo(decoded.Field));
    }

    [Test]
    [TestCase("{\"Property\": 53}", 53)]
    [TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
    public void CreateDecoder_ObjectProperty<T>(string json, T expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Format.Json, Schema.CreateJson<PropertyContainer<T>>(), encoded);

        Assert.That(expected, Is.EqualTo(decoded.Property));
    }

    [Test]
    [TestCase("{\"R\": {\"R\": {\"V\": 42}, \"V\": 17}, \"V\": 3}")]
    public void CreateDecoder_ObjectRecursive(string json)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = CreateDecoderAndDecode(Format.Json, Schema.CreateJson<Recursive>(), encoded);

        Assert.That(42, Is.EqualTo(decoded.R?.R?.V));
        Assert.That(17, Is.EqualTo(decoded.R?.V));
        Assert.That(3, Is.EqualTo(decoded.V));
    }

    [Test]
    [TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"IsPublic\":1}", "0:0:1")]
    [TestCase(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        "{\"IsPublic\":1,\"IsProtected\":2,\"_isPrivate\":3}", "3:2:1")]
    public void CreateDecoder_WithBindingFlags(BindingFlags bindingFlags, string json, string expected)
    {
        var encoded = Encoding.UTF8.GetBytes(json);
        var decoder = Linker.CreateReflection<JsonValue>(bindingFlags)
            .CreateDecoder(Format.Json, Schema.CreateJson<Visibility>());
        var decoded = Decode(decoder, encoded);

        Assert.That(expected, Is.EqualTo(decoded.ToString()));
    }

    [Test]
    public void CreateDecoder_ObjectWithParameterlessConstructor()
    {
        var decoded = CreateDecoderAndDecode(Format.Json, Schema.CreateJson<ReferenceType>(), "{}"u8.ToArray());

        Assert.That(decoded, Is.InstanceOf<ReferenceType>());
    }

    [Test]
    public void CreateDecoder_ShouldThrowWhenNoParameterlessConstructor()
    {
        var schema = Schema.CreateJson<Uri>();
        var linker = Linker.CreateReflection<JsonValue>(BindingFlags.Instance | BindingFlags.Public);

        Assert.That(() => linker.CreateDecoder(Format.Json, schema), Throws.InstanceOf<ConstructorNotFoundException>());
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void CreateEncoder_ArrayFromArray(double[] value, string expected)
    {
        var encoded = CreateEncoderAndEncode(Format.Json, Schema.CreateJson<double[]>(), value);

        Assert.That(Encoding.UTF8.GetString(encoded), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void CreateEncoder_ArrayFromCustom(double[] value, string expected)
    {
        var encoded = CreateEncoderAndEncode(Format.Json, Schema.CreateJson<CustomList<double>>(),
            new CustomList<double>(value));

        Assert.That(Encoding.UTF8.GetString(encoded), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void CreateEncoder_ArrayFromInterface(double[] value, string expected)
    {
        var encodedResults = new[]
        {
            CreateEncoderAndEncode(Format.Json, Schema.CreateJson<IList<double>>(), value),
            CreateEncoderAndEncode(Format.Json, Schema.CreateJson<IReadOnlyList<double>>(), value),
            CreateEncoderAndEncode(Format.Json, Schema.CreateJson<ICollection<double>>(), value),
            CreateEncoderAndEncode(Format.Json, Schema.CreateJson<IReadOnlyCollection<double>>(), value),
            CreateEncoderAndEncode(Format.Json, Schema.CreateJson<IEnumerable<double>>(), value),
        };

        foreach (var encoded in encodedResults)
            Assert.That(Encoding.UTF8.GetString(encoded), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
    [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
    public void CreateEncoder_ArrayFromList(double[] value, string expected)
    {
        var decoded = new List<double>(value);
        var encoded = CreateEncoderAndEncode(Format.Json, Schema.CreateJson<List<double>>(), decoded);

        Assert.That(Encoding.UTF8.GetString(encoded), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(53, "{\"Field\":53}")]
    [TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
    public void CreateEncoder_ObjectField<T>(T value, string expected)
    {
        var decoded = new FieldContainer<T> { Field = value };
        var encoded = CreateEncoderAndEncode(Format.Json, Schema.CreateJson<FieldContainer<T>>(), decoded);

        Assert.That(expected, Is.EqualTo(Encoding.UTF8.GetString(encoded)));
    }

    [Test]
    [TestCase(53, "{\"Property\":53}")]
    [TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
    public void CreateEncoder_ObjectProperty<T>(T value, string expected)
    {
        var decoded = new PropertyContainer<T> { Property = value };
        var encoded = CreateEncoderAndEncode(Format.Json, Schema.CreateJson<PropertyContainer<T>>(), decoded);

        Assert.That(expected, Is.EqualTo(Encoding.UTF8.GetString(encoded)));
    }

    [Test]
    [TestCase(false, "{\"R\":{\"R\":{\"R\":null,\"V\":42},\"V\":17},\"V\":3}")]
    [TestCase(true, "{\"R\":{\"R\":{\"V\":42},\"V\":17},\"V\":3}")]
    public void CreateEncoder_ObjectRecursive(bool omitNull, string expected)
    {
        var decoded = new Recursive { R = new Recursive { R = new Recursive { V = 42 }, V = 17 }, V = 3 };
        var schema = Schema.CreateJson<Recursive>(new JsonConfiguration { OmitNull = omitNull });
        var encoded = CreateEncoderAndEncode(Format.Json, schema, decoded);

        Assert.That(expected, Is.EqualTo(Encoding.UTF8.GetString(encoded)));
    }

    [Test]
    [TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"IsPublic\":0}")]
    [TestCase(BindingFlags.Instance | BindingFlags.NonPublic, "{\"IsProtected\":0,\"_isPrivate\":0}")]
    public void CreateEncoder_WithBindingFlags(BindingFlags bindingFlags, string expected)
    {
        var decoded = new Visibility();
        var encoder = Linker.CreateReflection<JsonValue>(bindingFlags)
            .CreateEncoder(Format.Json, Schema.CreateJson<Visibility>());
        var encoded = Encode(encoder, decoded);

        Assert.That(expected, Is.EqualTo(Encoding.UTF8.GetString(encoded)));
    }

    [Test]
    [TestCase("[1, 3, 5]", new[] { 2, 6, 10 })]
    public void SetDecoderDescriptor_OverrideSystemType(string json, int[] expected)
    {
        var decoder = Linker
            .CreateReflection<JsonValue>(BindingFlags.Instance | BindingFlags.Public)
            .SetDecoderDescriptor<int>(descriptor => descriptor.IsValue(v => (int)(v.Number * 2)))
            .CreateDecoder(Format.Json, Schema.CreateJson<int[]>());

        var encoded = Encoding.UTF8.GetBytes(json);
        var decoded = Decode(decoder, encoded);

        Assert.That(expected, Is.EqualTo(decoded).AsCollection);
    }

    private static TEntity CreateDecoderAndDecode<TNative, TEntity>(IFormat<TNative> format,
        ISchema<TNative, TEntity> schema, byte[] encoded)
    {
        var linker = Linker.CreateReflection<TNative>(BindingFlags.Instance | BindingFlags.Public);
        var decoder = linker.CreateDecoder(format, schema);

        return Decode(decoder, encoded);
    }

    private static byte[] CreateEncoderAndEncode<TNative, TEntity>(IFormat<TNative> format,
        ISchema<TNative, TEntity> schema, TEntity decoded)
    {
        var linker = Linker.CreateReflection<TNative>(BindingFlags.Instance | BindingFlags.Public);
        var encoder = linker.CreateEncoder(format, schema);

        return Encode(encoder, decoded);
    }

    private static T Decode<T>(IDecoder<T> decoder, byte[] encoded)
    {
        using var stream = new MemoryStream(encoded);
        using var decoderStream = decoder.Open(stream);

        Assert.That(decoderStream.TryDecode(out var decoded), Is.True);

        return decoded!;
    }

    private static byte[] Encode<T>(IEncoder<T> encoder, T decoded)
    {
        using var stream = new MemoryStream();

        using (var encoderStream = encoder.Open(stream))
            encoderStream.Encode(decoded);

        return stream.ToArray();
    }

    public class CustomList<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _list;

        public CustomList(IEnumerable<T> enumerable)
        {
            _list = enumerable.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int Count => _list.Count;

        public T this[int index] => _list[index];
    }

    public class ReferenceType
    {
    }

    private class FieldContainer<T>
    {
        public T Field = default!;
    }

    private class PropertyContainer<T>
    {
        public T Property
        {
            get;
            set;
        } = default!;
    }

    private class Recursive
    {
        public Recursive? R;

        public int V;
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