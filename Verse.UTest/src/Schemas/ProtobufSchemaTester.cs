using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoBuf;
using Verse.Schemas.Protobuf;
using Verse.Schemas;

namespace Verse.UTest.Schemas
{
    [TestFixture]
    class ProtobufSchemaTester : SchemaTester
    {
        [Test]
        [TestCase(2.3)]
        [TestCase(0.0)]
        [TestCase(-2.3)]
        [TestCase(1.11111111111)]
        public void DecodeNumericValue(double value)
        {
            Value decodedValue;

            decodedValue = ProtobufSchemaTester.TestDecode(value);

            Assert.AreEqual(ContentType.Double, decodedValue.Type);
            Assert.AreEqual(value, decodedValue.DoubleContent);
        }

        [TestCase(2.3f)]
        [TestCase(0.0f)]
        [TestCase(-2.3f)]
        public void DecodeNumericValue(float value)
        {
            Value decodedValue;

            decodedValue = ProtobufSchemaTester.TestDecode(value);

            Assert.AreEqual(ContentType.Float, decodedValue.Type);
            Assert.AreEqual(value, decodedValue.FloatContent);
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(150)]
        [TestCase(16777215)]
        [TestCase(9223372036854775807)]
        public void DecodeLongValue(long value)
        {
            Value decodedValue;

            decodedValue = ProtobufSchemaTester.TestDecode(value);

            Assert.AreEqual(ContentType.Long, decodedValue.Type);
            Assert.AreEqual(value, decodedValue.LongContent);
        }

        [TestCase("")]
        [TestCase("toto")]
        [TestCase("DecodingProtobuf")]
        public void DecodeStringValue(string value)
        {
            Value decodedValue;

            decodedValue = ProtobufSchemaTester.TestDecode(value);

            Assert.AreEqual(value, decodedValue.StringContent);
        }

        [Test]
        [TestCase(new[] { 1, 2, 3 })]
        [TestCase(new[] { 1, 1, 1 })]
        [TestCase(new int[0])]
        public void DecodeIntegers(int[] expectedItems)
        {
            TestDecodeItems(expectedItems);
        }

        [Test]
        [TestCase(new[] { 1.0, 2.0, 3.0 })]
        [TestCase(new[] { -1.0, 3.0, 2.0 })]
        [TestCase(new double[0])]
        public void DecodeDoubles(double[] expectedItems)
        {
            TestDecodeItems(expectedItems);
        }

        [Test]
        [TestCase("toto", "titi", "tutu")]
        [TestCase("A", "B", "D")]
        [TestCase("", "", "")]
        public void DecodeStrings(string a, string b, string c)
        {
            TestDecodeItems(new string[] { a, b, c });
        }

        [Test]
        [TestCase(new[] { 1f, 2f, 3f })]
        [TestCase(new[] { -1f, 3f, 2f })]
        [TestCase(new float[0])]
        public void DecodeFloats(float[] expectedItems)
        {
            TestDecodeItems(expectedItems);
        }

        [Test]
        public void DecodeSubItem()
        {
            List<SubTestFieldClass> decodedValue;
            IParser<List<SubTestFieldClass>> parser;
            ProtobufSchema<List<SubTestFieldClass>> schema;
            MemoryStream stream;
            TestFieldClass<long> testFieldClass;

            testFieldClass = new TestFieldClass<long>();
            testFieldClass.subValue = new SubTestFieldClass();
            testFieldClass.subValue.value = 10;

            stream = new MemoryStream();
            Serializer.Serialize(stream, testFieldClass);
            stream.Seek(0, SeekOrigin.Begin);

            schema = new ProtobufSchema<List<SubTestFieldClass>>();
            schema.ParserDescriptor
                .HasField("3")
                .IsArray((ref List<SubTestFieldClass> target, IEnumerable<SubTestFieldClass> enumerable) => target.AddRange(enumerable))
                .HasField<int>("4", (ref SubTestFieldClass target, int value) => target.value = value)
                .IsValue();

            parser = schema.CreateParser();

            decodedValue = new List<SubTestFieldClass>();
            Assert.IsTrue(parser.Parse(stream, ref decodedValue));

            Assert.AreEqual(1, decodedValue.Count);
            Assert.AreEqual(testFieldClass.subValue.value, decodedValue[0].value);
        }

        [Test]
        [TestCase(2.3)]
        [TestCase(0.0)]
        [TestCase(-2.3)]
        [TestCase(1.11111111111)]
        public void EncodeNumericValue(double value)
        {
            double decodedValue;

            decodedValue = ProtobufSchemaTester.TestEncode<double>(new Value(value));

            Assert.AreEqual(value, decodedValue);
        }

        [TestCase(2.3f)]
        [TestCase(0.0f)]
        [TestCase(-2.3f)]
        public void EncodeNumericValue(float value)
        {
            float decodedValue;

            decodedValue = ProtobufSchemaTester.TestEncode<float>(new Value(value));

            Assert.AreEqual(value, decodedValue);
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(150)]
        [TestCase(16777215)]
        [TestCase(9223372036854775807)]
        public void EncodeLongValue(long value)
        {
            long decodedValue;

            decodedValue = ProtobufSchemaTester.TestEncode<long>(new Value(value));

            Assert.AreEqual(value, decodedValue);
        }


        [TestCase("")]
        [TestCase("toto")]
        [TestCase("DecodingProtobuf")]
        public void EncodeStringValue(string value)
        {
            string decodedValue;

            decodedValue = ProtobufSchemaTester.TestEncode<string>(new Value(value));

            Assert.AreEqual(value, decodedValue);
        }

        [Test]
        [TestCase(new[] { 1, 2, 3 })]
        [TestCase(new[] { 1, 1, 1 })]
        [TestCase(new int[0])]
        public void EncodeIntegers(int[] expectedItems)
        {
            TestEncodeItems(expectedItems);
        }

        [Test]
        [TestCase(new[] { 1.0, 2.0, 3.0 })]
        [TestCase(new[] { -1.0, 3.0, 2.0 })]
        [TestCase(new double[0])]
        public void EncodeDoubles(double[] expectedItems)
        {
            TestEncodeItems(expectedItems);
        }

        [Test]
        [TestCase("toto", "titi", "tutu")]
        [TestCase("A", "B", "D")]
        [TestCase("", "", "")]
        public void EncodeStrings(string a, string b, string c)
        {
            TestEncodeItems(new string[] { a, b, c });
        }

        [Test]
        [TestCase(new[] { 1f, 2f, 3f })]
        [TestCase(new[] { -1f, 3f, 2f })]
        [TestCase(new float[0])]
        public void EncodeFloats(float[] expectedItems)
        {
            TestEncodeItems(expectedItems);
        }

        [Test]
        public void EncodeSubItem()
        {
            TestFieldClass<long> decodedTestFieldClass;
            IPrinter<TestFieldClass<long>> printer;
            ProtobufSchema<TestFieldClass<long>> schema;
            MemoryStream stream;
            TestFieldClass<long> testFieldClass;

            testFieldClass = new TestFieldClass<long>();
            testFieldClass.subValue = new SubTestFieldClass();
            testFieldClass.subValue.value = 10;

            schema = new ProtobufSchema<TestFieldClass<long>>();
            schema.PrinterDescriptor
                .HasField("3")
                .IsArray(source => new List<SubTestFieldClass> {source.subValue})
                .HasField("4", target => target.value)
                .IsValue();

            stream = new MemoryStream();

            printer = schema.CreatePrinter();
            Assert.IsTrue(printer.Print(testFieldClass, stream));
            stream.Seek(0, SeekOrigin.Begin);

            decodedTestFieldClass = Serializer.Deserialize<TestFieldClass<long>>(stream);

            Assert.AreEqual(testFieldClass.subValue.value, decodedTestFieldClass.subValue.value);
        }

        private static void TestDecodeItems<T>(T[] expectedItems)
        {
            IParser<List<T>> parser;
            ProtobufSchema<List<T>> schema;
            List<T> value;

            TestFieldClass<T> testFieldClass;
            testFieldClass = new TestFieldClass<T>();
            testFieldClass.items = expectedItems.ToList();

            MemoryStream stream;
            stream = new MemoryStream();
            Serializer.Serialize(stream, testFieldClass);
            stream.Seek(0, SeekOrigin.Begin);

            schema = new ProtobufSchema<List<T>>();
            IParserDescriptor<List<T>> fieldParser = schema.ParserDescriptor.HasField("2");
            fieldParser.IsArray((ref List<T> target, IEnumerable<T> enumerable) => target.AddRange(enumerable)).IsValue();

            value = new List<T>();
            parser = schema.CreateParser();
            Assert.IsTrue(parser.Parse(stream, ref value));
            CollectionAssert.AreEqual(expectedItems, value);
        }

        private static void TestEncodeItems<T>(T[] expectedItems)
        {
            ProtobufSchema<List<T>> schema;
            schema = new ProtobufSchema<List<T>>();
            IPrinterDescriptor<List<T>> printerDescriptor = schema.PrinterDescriptor.HasField("2");
            printerDescriptor.IsArray(source => source).IsValue();

            MemoryStream stream;
            stream = new MemoryStream();
            Assert.IsTrue(schema.CreatePrinter().Print(new List<T>(expectedItems), stream));
            stream.Seek(0, SeekOrigin.Begin);

            TestFieldClass<T> testFieldClass = Serializer.Deserialize<TestFieldClass<T>>(stream);
            CollectionAssert.AreEqual(expectedItems, testFieldClass.items);
        }

        private static Value TestDecode<T>(T value)
        {
            Value decodedValue;
            IParser<Value> parser;
            ISchema<Value> schema;
            MemoryStream stream;
            TestFieldClass<T> testFieldClass;

            testFieldClass = new TestFieldClass<T>();
            testFieldClass.value = value;

            stream = new MemoryStream();
            Serializer.Serialize(stream, testFieldClass);
            stream.Seek(0, SeekOrigin.Begin);

            decodedValue = new Value();

            schema = new ProtobufSchema<Value>();
            schema.ParserDescriptor.HasField("1").IsValue();
            parser = schema.CreateParser();
            Assert.IsTrue(parser.Parse(stream, ref decodedValue));

            return decodedValue;
        }

        private static T TestEncode<T>(Value value)
        {
            IPrinter<Value> printer;
            ISchema<Value> schema;
            MemoryStream stream;
            TestFieldClass<T> testFieldClass;

            schema = new ProtobufSchema<Value>();
            schema.PrinterDescriptor.HasField("1").IsValue();

            stream = new MemoryStream();

            printer = schema.CreatePrinter();
            Assert.IsTrue(printer.Print(value, stream));

            stream.Seek(0, SeekOrigin.Begin);

            testFieldClass = Serializer.Deserialize<TestFieldClass<T>>(stream);

            return testFieldClass.value;
        }

        [global::System.Serializable, global::ProtoBuf.ProtoContract(Name = @"SubTestFieldClass")]
        public class SubTestFieldClass : global::ProtoBuf.IExtensible
        {
            private int _value;
            [global::ProtoBuf.ProtoMember(4, IsRequired = true)]
            public int value
            {
                get { return _value; }
                set { _value = value; }
            }

            private global::ProtoBuf.IExtension extensionObject;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            {
                return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
            }
        }

        [global::System.Serializable, global::ProtoBuf.ProtoContract(Name = @"TestFieldClass")]
        public class TestFieldClass<T> : global::ProtoBuf.IExtensible
        {
            private T _value;
            [global::ProtoBuf.ProtoMember(1, IsRequired = true)]
            public T value
            {
                get { return _value; }
                set { _value = value; }
            }

            private global::System.Collections.Generic.List<T> _items = new global::System.Collections.Generic.List<T>();
            [global::ProtoBuf.ProtoMember(2, IsRequired = true, DataFormat = global::ProtoBuf.DataFormat.Default)]
            public global::System.Collections.Generic.List<T> items
            {
                get { return _items; }
                set { _items = value; }
            }

            private SubTestFieldClass _subValue;
            [global::ProtoBuf.ProtoMember(3, IsRequired = true)]
            public SubTestFieldClass subValue
            {
                get { return _subValue; }
                set { _subValue = value; }
            }

            private global::ProtoBuf.IExtension extensionObject;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            {
                return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
            }
        }
    }
}
