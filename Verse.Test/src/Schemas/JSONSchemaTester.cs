using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;
using Verse.Schemas.JSON;

namespace Verse.Test.Schemas
{
    [TestFixture]
    public class JSONSchemaTester : SchemaTester
    {
        [Test]
        [TestCase("[]", new double[0])]
        [TestCase("[-42.1]", new[] { -42.1 })]
        [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
        [TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
        public void DecodeArray(string json, double[] expected)
        {
            IDecoder<double[]> decoder;
            JSONSchema<double[]> schema;
            double[] value;

            schema = new JSONSchema<double[]>();
            schema.DecoderDescriptor.IsArray((ref double[] target, IEnumerable<double> items) => target = items.ToArray()).IsValue();

            decoder = schema.CreateDecoder();
            value = new double[0];

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("glittering", "{\"glittering\": \"prizes\"}", "prizes")]
        [TestCase("", "{\"\": 5}", 5)]
        [TestCase("1", "[\"hey!\", \"take me!\", \"not me!\"]", "take me!")]
        public void DecodeField<T>(string name, string json, T expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.DecoderDescriptor.HasField(name).IsValue();

            this.AssertDecodeAndEqual(schema, json, expected);
        }

        [Test]
        public void DecodeFieldMismatchName()
        {
            var schema = new JSONSchema<int>();

            schema.DecoderDescriptor.HasField("match").IsValue();

            this.AssertDecodeAndEqual(schema, "{\"match1\": 17}", 0);
            this.AssertDecodeAndEqual(schema, "{\"matc\": 17}", 0);
        }

        [Test]
        public void DecodeFieldMismatchScope()
        {
            var schema = new JSONSchema<int>();

            schema.DecoderDescriptor.HasField("match").IsValue();

            this.AssertDecodeAndEqual(schema, "{\"unknown\": {\"match\": 17}}", 0);
        }

        [Test]
        [TestCase("b", "{\"a\": 50, \"b\": 43, \"c\": [1, 5, 9]}", 43)]
        [TestCase("b", "{\"a\": {\"x\": 1, \"y\": 2}, \"b\": \"OK\", \"c\": 21.6}", "OK")]
        public void DecodeGarbage<T>(string name, string json, T expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.DecoderDescriptor.HasField(name).IsValue();

            this.AssertDecodeAndEqual(schema, json, expected);
        }

        [Test]
        [TestCase("~", 1)]
        [TestCase("\"Unfinished", 13)]
        [TestCase("[1.1.1]", 5)]
        [TestCase("[0 0]", 4)]
        [TestCase("{0}", 2)]
        [TestCase("{\"\" 0}", 5)]
        [TestCase("fail", 3)]
        public void DecodeInvalidStream(string json, int expected)
        {
            IDecoder<string> decoder;
            int position;
            JSONSchema<string> schema;
            string value;

            schema = new JSONSchema<string>();
            schema.DecoderDescriptor.IsValue();

            position = -1;

            decoder = schema.CreateDecoder();
            decoder.Error += (p, m) => position = p;

            value = string.Empty;

            Assert.IsFalse(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, position);
        }

        [Test]
        public void DecodeRecursiveSchema()
        {
            IDecoder<RecursiveEntity> decoder;
            IDecoderDescriptor<RecursiveEntity> descriptor;
            JSONSchema<RecursiveEntity> schema;
            RecursiveEntity value;

            schema = new JSONSchema<RecursiveEntity>();

            descriptor = schema.DecoderDescriptor;
            descriptor.HasField("f", (ref RecursiveEntity r, RecursiveEntity v) => r.field = v, descriptor);
            descriptor.HasField("v", (ref RecursiveEntity r, int v) => r.value = v).IsValue();

            decoder = schema.CreateDecoder();
            value = new RecursiveEntity();

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes("{\"f\": {\"f\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")), ref value));

            Assert.AreEqual(42, value.field.field.value);
            Assert.AreEqual(17, value.field.value);
            Assert.AreEqual(3, value.value);
        }

        [Test]
        public void DecodeValueWithCustomConstructor()
        {
            IDecoderDescriptor<Tuple<int, int>> descriptor;
            JSONSchema<Tuple<Tuple<int, int>>> schema;

            schema = new JSONSchema<Tuple<Tuple<int, int>>>();
            schema.DecoderDescriptor.CanCreate((v) => Tuple.Create(0, 0));

            descriptor = schema.DecoderDescriptor.HasField("tuple", (ref Tuple<Tuple<int, int>> target, Tuple<int, int> value) => target = Tuple.Create(value));
            descriptor.HasField("a", (ref Tuple<int, int> target, int value) => target = Tuple.Create(value, target.Item2)).IsValue();
            descriptor.HasField("b", (ref Tuple<int, int> target, int value) => target = Tuple.Create(target.Item1, value)).IsValue();

            this.AssertDecodeAndEqual(schema, "{\"tuple\": {\"a\": 5, \"b\": 7}}", Tuple.Create(Tuple.Create(5, 7)));
        }

        [Test]
        [TestCase("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
        [TestCase("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
        public void DecodeValueWithCustomDecoder(string json, string expected)
        {
            JSONSchema<Guid> schema;

            schema = new JSONSchema<Guid>();
            schema.SetDecoder((v) => Guid.Parse(v.String));
            schema.DecoderDescriptor.IsValue();

            this.AssertDecodeAndEqual(schema, json, Guid.Parse(expected));
        }

        [Test]
        public void DecodeValueMismatchType()
        {
            var schema = new JSONSchema<int>();

            schema.DecoderDescriptor.IsArray<string>((ref int target, IEnumerable<string> elements) => target = elements.Count());
            schema.DecoderDescriptor.IsValue();

            this.AssertDecodeAndEqual(schema, "\"test\"", 0);
            this.AssertDecodeAndEqual(schema, "53", 0);
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
        [TestCase("9223372036854775807", long.MaxValue)]
        [TestCase("\"\"", "")]
        [TestCase("\"Hello, World!\"", "Hello, World!")]
        [TestCase("\"\\u00FF \\u0066 \\uB3A8\"", "\xFF f \uB3A8")]
        [TestCase("\"\\\"\"", "\"")]
        [TestCase("\"\\\\\"", "\\")]
        [TestCase("9223372036854775807", 9223372036854775807)]
        public void DecodeValueNative<T>(string json, T expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.DecoderDescriptor.IsValue();

            this.AssertDecodeAndEqual(schema, json, expected);
        }

        [Test]
        public void DecodeValueSmallNativeDecimal()
        {
            JSONSchema<decimal> schema;

            schema = new JSONSchema<decimal>();
            schema.DecoderDescriptor.IsValue();

            this.AssertDecodeAndEqual(schema, "1e-28", 1e-28m);
        }

        [Test]
        [TestCase(new int[0], "[]")]
        [TestCase(new[] { 21 }, "[21]")]
        [TestCase(new[] { 54, 90, -3, 34, 0, 49 }, "[54,90,-3,34,0,49]")]
        public void EncodeArray(int[] value, string expected)
        {
            JSONSchema<int[]> schema;

            schema = new JSONSchema<int[]>();
            schema.EncoderDescriptor.IsArray((source) => source).IsValue();

            this.AssertEncodeAndEqual(schema, value, expected);
        }

        [Test]
        [TestCase("glittering", "prizes", "{\"glittering\":\"prizes\"}")]
        [TestCase("", "pwic", "{\"\":\"pwic\"}")]
        public void EncodeField<T>(string name, T value, string expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.EncoderDescriptor.HasField(name).IsValue();

            this.AssertEncodeAndEqual(schema, value, expected);
        }

        [Test]
        [TestCase("90f59097-d06a-4796-b8d5-87eb6af7ed8b", "\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"")]
        [TestCase("c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025", "\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"")]
        public void EncodeValueEncoder(string guid, string expected)
        {
            JSONSchema<Guid> schema;

            schema = new JSONSchema<Guid>();
            schema.SetEncoder<Guid>((v) => Value.FromString(v.ToString()));
            schema.EncoderDescriptor.IsValue();

            this.AssertEncodeAndEqual(schema, Guid.Parse(guid), expected);
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
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.EncoderDescriptor.IsValue();

            this.AssertEncodeAndEqual(schema, value, expected);
        }

        [Test]
        [Theory]
        public void EncodeNullValue(bool ignoreNull)
        {
            string expected;
            JSONSchema<string> schema;

            expected = ignoreNull ? string.Empty : "null";
            schema = new JSONSchema<string>(new JSONSettings(new UTF8Encoding(false), ignoreNull));
            schema.EncoderDescriptor.IsValue();

            this.AssertEncodeAndEqual(schema, null, expected);
        }

        [Test]
        [Theory]
        public void EncodeItem(bool ignoreNull)
        {
            string expected;
            JSONSchema<string> schema;

            expected = ignoreNull
                ? "{\"value\":\"test\",\"item\":{\"values\":[\"val1\",\"val2\"]}}"
                : "{\"firstnull\":null,\"value\":\"test\",\"secondnull\":null,\"item\":{\"values\":[null,\"val1\",null,\"val2\",null]},\"lastnull\":null}";

            schema = new JSONSchema<string>(new JSONSettings(new UTF8Encoding(false), ignoreNull));
            schema.EncoderDescriptor.HasField("firstnull").IsValue(v => Value.Void);
            schema.EncoderDescriptor.HasField("value").IsValue();
            schema.EncoderDescriptor.HasField("secondnull").IsValue(v => Value.Void);
            schema.EncoderDescriptor.HasField("item").HasField("values").IsArray(v => new[] { null, "val1", null, "val2", null }).IsValue();
            schema.EncoderDescriptor.HasField("lastnull").IsValue(v => Value.Void);

            this.AssertEncodeAndEqual(schema, "test", expected);
        }

        [Test]
        [Theory]
        public void EncodeArrayOfItems(bool ignoreNull)
        {
            string expected;
            JSONSchema<Value> schema;

            expected = ignoreNull
                ? "{\"values\":[{},{\"value\":\"test\"}]}"
                : "{\"values\":[{\"value\":null},{\"value\":\"test\"}]}";

            schema = new JSONSchema<Value>(new JSONSettings(new UTF8Encoding(false), ignoreNull));
            schema.EncoderDescriptor
                .HasField("values")
                .IsArray(v => new[] { Value.Void, v, })
                .HasField("value")
                .IsValue();

            this.AssertEncodeAndEqual(schema, Value.FromString("test"), expected);
        }

        [Test]
        public void Roundtrip()
        {
            this.AssertRoundtrip(new JSONSchema<string>(), "Hello", () => string.Empty, (a, b) => a == b);
            this.AssertRoundtrip(new JSONSchema<double>(), 25.5, () => 0, (a, b) => Math.Abs(a - b) < double.Epsilon);
        }

        private void AssertDecodeAndEqual<T>(ISchema<T> schema, string json, T expected)
        {
            IDecoder<T> decoder;
            T value;

            decoder = schema.CreateDecoder();
            value = default(T);

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, value);
        }

        private void AssertEncodeAndEqual<T>(ISchema<T> schema, T value, string expected)
        {
            IEncoder<T> encoder;

            encoder = schema.CreateEncoder();

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(value, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        private class RecursiveEntity
        {
            public RecursiveEntity field;

            public int value;
        }
    }
}