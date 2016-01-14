using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;
using Verse.Schemas.JSON;

namespace Verse.UTest.Schemas
{
    [TestFixture]
    public class JSONSchemaTester : SchemaTester
    {
        [Test]
        [TestCase("glittering", "{\"glittering\": \"prizes\"}", "prizes")]
        [TestCase("", "{\"\": 5}", 5)]
        [TestCase("1", "[\"hey!\", \"take me!\", \"not me!\"]", "take me!")]
        public void ParseFieldValue<T>(string name, string json, T expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.ParserDescriptor.HasField(name).IsValue();

            this.AssertParseAndEqual(schema, json, expected);
        }

        [Test]
        [TestCase("b", "{\"a\": 50, \"b\": 43, \"c\": [1, 5, 9]}", 43)]
        [TestCase("b", "{\"a\": {\"x\": 1, \"y\": 2}, \"b\": \"OK\", \"c\": 21.6}", "OK")]
        public void ParseGarbage<T>(string name, string json, T expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.ParserDescriptor.HasField(name).IsValue();

            this.AssertParseAndEqual(schema, json, expected);
        }

        [Test]
        [TestCase("~", 1)]
        [TestCase("\"Unfinished", 13)]
        [TestCase("[1.1.1]", 5)]
        [TestCase("[0 0]", 4)]
        [TestCase("{0}", 2)]
        [TestCase("{\"\" 0}", 5)]
        [TestCase("fail", 3)]
        public void ParseInvalidStream(string json, int expected)
        {
            IParser<string> parser;
            int position;
            JSONSchema<string> schema;
            string value;

            schema = new JSONSchema<string>();
            schema.ParserDescriptor.IsValue();

            position = -1;

            parser = schema.CreateParser();
            parser.Error += (p, m) => position = p;

            value = string.Empty;

            Assert.IsFalse(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, position);
        }

        [Test]
        [TestCase("[]", new double[0])]
        [TestCase("[-42.1]", new[] { -42.1 })]
        [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
        [TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
        public void ParseItems(string json, double[] expected)
        {
            IParser<double[]> parser;
            JSONSchema<double[]> schema;
            double[] value;

            schema = new JSONSchema<double[]>();
            schema.ParserDescriptor.IsArray((ref double[] target, IEnumerable<double> items) => target = items.ToArray()).IsValue();

            parser = schema.CreateParser();
            value = new double[0];

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        public void ParseValueCustom()
        {
            IParserDescriptor<Tuple<int, int>> descriptor;
            JSONSchema<Tuple<Tuple<int, int>>> schema;

            schema = new JSONSchema<Tuple<Tuple<int, int>>>();
            schema.ParserDescriptor.CanCreate((v) => Tuple.Create(0, 0));

            descriptor = schema.ParserDescriptor.HasField("tuple", (ref Tuple<Tuple<int, int>> target, Tuple<int, int> value) => target = Tuple.Create(value));
            descriptor.HasField("a", (ref Tuple<int, int> target, int value) => target = Tuple.Create(value, target.Item2)).IsValue();
            descriptor.HasField("b", (ref Tuple<int, int> target, int value) => target = Tuple.Create(target.Item1, value)).IsValue();

            this.AssertParseAndEqual(schema, "{\"tuple\": {\"a\": 5, \"b\": 7}}", Tuple.Create(Tuple.Create(5, 7)));
        }

        [Test]
        [TestCase("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
        [TestCase("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
        public void ParseValueDecoder(string json, string expected)
        {
            JSONSchema<Guid> schema;

            schema = new JSONSchema<Guid>();
            schema.SetDecoder((v) => Guid.Parse(v.String));
            schema.ParserDescriptor.IsValue();

            this.AssertParseAndEqual(schema, json, Guid.Parse(expected));
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
        public void ParseValueNative<T>(string json, T expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.ParserDescriptor.IsValue();

            this.AssertParseAndEqual(schema, json, expected);
        }

        [Test]
        public void ParseLittleValueNativeDecimal()
        {
            JSONSchema<decimal> schema;

            schema = new JSONSchema<decimal>();
            schema.ParserDescriptor.IsValue();

            this.AssertParseAndEqual(schema, "1e-28", 1e-28m);
        }

        [Test]
        public void ParseRecursiveSchema()
        {
            IParserDescriptor<RecursiveEntity> descriptor;
            IParser<RecursiveEntity> parser;
            JSONSchema<RecursiveEntity> schema;
            RecursiveEntity value;

            schema = new JSONSchema<RecursiveEntity>();

            descriptor = schema.ParserDescriptor;
            descriptor.HasField("f", (ref RecursiveEntity r, RecursiveEntity v) => r.field = v, descriptor);
            descriptor.HasField("v", (ref RecursiveEntity r, int v) => r.value = v).IsValue();

            parser = schema.CreateParser();
            value = new RecursiveEntity();

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes("{\"f\": {\"f\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")), ref value));

            Assert.AreEqual(42, value.field.field.value);
            Assert.AreEqual(17, value.field.value);
            Assert.AreEqual(3, value.value);
        }

        [Test]
        [TestCase("glittering", "prizes", "{\"glittering\":\"prizes\"}")]
        [TestCase("", "pwic", "{\"\":\"pwic\"}")]
        public void PrintFieldValue<T>(string name, T value, string expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.PrinterDescriptor.HasField(name).IsValue();

            this.AssertPrintAndEqual(schema, value, expected);
        }

        [Test]
        [TestCase("num", 9223372036854775807, "{\"num\":9223372036854775807}")]
        public void PrintLongValue(string name, long value, string expected)
        {
            JSONSchema<long> schema;

            schema = new JSONSchema<long>();
            schema.PrinterDescriptor.HasField(name).IsValue();

            this.AssertPrintAndEqual(schema, value, expected);
        }

        [Test]
        [TestCase(new int[0], "[]")]
        [TestCase(new[] { 21 }, "[21]")]
        [TestCase(new[] { 54, 90, -3, 34, 0, 49 }, "[54,90,-3,34,0,49]")]
        public void PrintItems(int[] value, string expected)
        {
            JSONSchema<int[]> schema;

            schema = new JSONSchema<int[]>();
            schema.PrinterDescriptor.IsArray((source) => source).IsValue();

            this.AssertPrintAndEqual(schema, value, expected);
        }

        [Test]
        [TestCase("90f59097-d06a-4796-b8d5-87eb6af7ed8b", "\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"")]
        [TestCase("c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025", "\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"")]
        public void PrintValueEncoder(string guid, string expected)
        {
            JSONSchema<Guid> schema;

            schema = new JSONSchema<Guid>();
            schema.SetEncoder<Guid>((v) => Value.FromString(v.ToString()));
            schema.PrinterDescriptor.IsValue();

            this.AssertPrintAndEqual(schema, Guid.Parse(guid), expected);
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
        public void PrintValueNative<T>(T value, string expected)
        {
            JSONSchema<T> schema;

            schema = new JSONSchema<T>();
            schema.PrinterDescriptor.IsValue();

            this.AssertPrintAndEqual(schema, value, expected);
        }

        [Test]
        [Theory]
        public void PrintNullValue(bool ignoreNull)
        {
            string expected;
            JSONSchema<string> schema;

            expected = ignoreNull ? string.Empty : "null";
            schema = new JSONSchema<string>(new JSONSettings(new UTF8Encoding(false), ignoreNull));
            schema.PrinterDescriptor.IsValue();

            this.AssertPrintAndEqual(schema, null, expected);
        }

        [Test]
        [Theory]
        public void PrintItem(bool ignoreNull)
        {
            string expected;
            JSONSchema<string> schema;

            expected = ignoreNull
                ? "{\"value\":\"test\",\"item\":{\"values\":[\"val1\",\"val2\"]}}"
                : "{\"firstnull\":null,\"value\":\"test\",\"secondnull\":null,\"item\":{\"values\":[null,\"val1\",null,\"val2\",null]},\"lastnull\":null}";

            schema = new JSONSchema<string>(new JSONSettings(new UTF8Encoding(false), ignoreNull));
            schema.PrinterDescriptor.HasField("firstnull").IsValue(v => Value.Void);
            schema.PrinterDescriptor.HasField("value").IsValue();
            schema.PrinterDescriptor.HasField("secondnull").IsValue(v => Value.Void);
            schema.PrinterDescriptor.HasField("item").HasField("values").IsArray(v => new[] { null, "val1", null, "val2", null }).IsValue();
            schema.PrinterDescriptor.HasField("lastnull").IsValue(v => Value.Void);

            this.AssertPrintAndEqual(schema, "test", expected);
        }

        [Test]
        [Theory]
        public void PrintArrayOfItems(bool ignoreNull)
        {
            string expected;
            JSONSchema<Value> schema;

            expected = ignoreNull
                ? "{\"values\":[{},{\"value\":\"test\"}]}"
                : "{\"values\":[{\"value\":null},{\"value\":\"test\"}]}";

            schema = new JSONSchema<Value>(new JSONSettings(new UTF8Encoding(false), ignoreNull));
            schema.PrinterDescriptor
                .HasField("values")
                .IsArray(v => new[] { Value.Void, v, })
                .HasField("value")
                .IsValue();

            this.AssertPrintAndEqual(schema, Value.FromString("test"), expected);
        }

        [Test]
        public void Roundtrip()
        {
            this.AssertRoundtrip(new JSONSchema<string>(), "Hello", () => string.Empty, (a, b) => a == b);
            this.AssertRoundtrip(new JSONSchema<double>(), 25.5, () => 0, (a, b) => Math.Abs(a - b) < double.Epsilon);
        }

        private void AssertParseAndEqual<T>(ISchema<T> schema, string json, T expected)
        {
            IParser<T> parser;
            T value;

            parser = schema.CreateParser();
            value = default(T);

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, value);
        }

        private void AssertPrintAndEqual<T>(ISchema<T> schema, T value, string expected)
        {
            IPrinter<T> printer;

            printer = schema.CreatePrinter();

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(value, stream));
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