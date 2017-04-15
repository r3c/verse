using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Test
{
    [TestFixture]
    public class LinkerTester
    {
        [Test]
        [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
        [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
        public void LinkEncoderArrayFromArray(double[] value, string expected)
        {
            IEncoder<double[]> encoder;

            encoder = Linker.CreateEncoder(new JSONSchema<double[]>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(value, stream));
                CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
        [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
        public void LinkEncoderArrayFromList(double[] value, string expected)
        {
            IEncoder<List<double>> encoder;

            encoder = Linker.CreateEncoder(new JSONSchema<List<double>>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(new List<double>(value), stream));
                CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase(53, "{\"Field\":53}")]
        [TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
        public void LinkEncoderField<T>(T value, string expected)
        {
            IEncoder<FieldContainer<T>> encoder;

            encoder = Linker.CreateEncoder(new JSONSchema<FieldContainer<T>>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(new FieldContainer<T> { Field = value }, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase(53, "{\"Property\":53}")]
        [TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
        public void LinkEncoderProperty<T>(T value, string expected)
        {
            IEncoder<PropertyContainer<T>> encoder;

            encoder = Linker.CreateEncoder(new JSONSchema<PropertyContainer<T>>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(new PropertyContainer<T> { Property = value }, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [Theory]
        public void LinkEncoderRecursive(bool ignoreNull)
        {
            string expected;
            IEncoder<Recursive> encoder;
            Recursive value;

            expected = ignoreNull
                ? "{\"r\":{\"r\":{\"v\":42},\"v\":17},\"v\":3}"
                : "{\"r\":{\"r\":{\"r\":null,\"v\":42},\"v\":17},\"v\":3}";

            encoder = Linker.CreateEncoder(new JSONSchema<Recursive>(new JSONSettings(new UTF8Encoding(false), ignoreNull)));

            using (var stream = new MemoryStream())
            {
                value = new Recursive { r = new Recursive { r = new Recursive { v = 42 }, v = 17 }, v = 3 };

                Assert.IsTrue(encoder.Encode(value, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
        [TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
        public void LinkDecoderArrayFromArray(string json, double[] expected)
        {
            IDecoder<double[]> decoder;
            double[] value;

            decoder = Linker.CreateDecoder(new JSONSchema<double[]>());
            value = new double[0];

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
        [TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
        public void LinkDecoderArrayFromList(string json, double[] expected)
        {
            IDecoder<List<double>> decoder;
            List<double> value;

            decoder = Linker.CreateDecoder(new JSONSchema<List<double>>());
            value = new List<double>();

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("{\"Field\": 53}", 53)]
        [TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
        public void LinkDecoderField<T>(string json, T expected)
        {
            IDecoder<FieldContainer<T>> decoder;
            FieldContainer<T> value;

            decoder = Linker.CreateDecoder(new JSONSchema<FieldContainer<T>>());
            value = new FieldContainer<T>();

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, value.Field);
        }

        [Test]
        [TestCase("{\"Property\": 53}", 53)]
        [TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
        public void LinkDecoderProperty<T>(string json, T expected)
        {
            IDecoder<PropertyContainer<T>> decoder;
            PropertyContainer<T> value;

            decoder = Linker.CreateDecoder(new JSONSchema<PropertyContainer<T>>());
            value = new PropertyContainer<T>();

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, value.Property);
        }

        [Test]
        public void LinkDecoderRecursive()
        {
            IDecoder<Recursive> decoder;
            Recursive value;

            decoder = Linker.CreateDecoder(new JSONSchema<Recursive>());
            value = new Recursive();

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes("{\"r\": {\"r\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")), ref value));

            Assert.AreEqual(42, value.r.r.v);
            Assert.AreEqual(17, value.r.v);
            Assert.AreEqual(3, value.v);
        }

        private class FieldContainer<T>
        {
            public T Field = default(T);
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
            public Recursive r = null;

            public int v = 0;
        }
    }
}