using System;
using System.Collections.Generic;
using System.Globalization;
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
                .HasField("_3")
                .IsArray((ref List<SubTestFieldClass> target, IEnumerable<SubTestFieldClass> enumerable) => target.AddRange(enumerable))
                .HasField("_4", (ref SubTestFieldClass target, int value) => target.value = value)
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
        public void TestDecodeSubObject()
        {
            TestFieldClass<int> testFieldClass;
            testFieldClass = new TestFieldClass<int>();

            testFieldClass.subValue = new SubTestFieldClass();
            testFieldClass.subValue.value = 10;

            MemoryStream stream;
            stream = new MemoryStream();
            Serializer.Serialize(stream, testFieldClass);
            stream.Seek(0, SeekOrigin.Begin);

            ProtobufSchema<int> schema = new ProtobufSchema<int>();
            schema.ParserDescriptor.HasField("_3").HasField("_4").IsValue();

            int value = 0;

            IParser<int> parser = schema.CreateParser();

            Assert.IsTrue(parser.Parse(stream, ref value));
            Assert.AreEqual(10, value);
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
        public void TestDecodeSubObjectIndex(int? index0, int? index1, int expected)
        {
            IParser<int> parser;
            ProtobufSchema<int> schema;
            int value;

            TestFieldClass<TestFieldClass<SubTestFieldClass>> testFieldClass;
            testFieldClass = new TestFieldClass<TestFieldClass<SubTestFieldClass>>();
            testFieldClass.items = new List<TestFieldClass<SubTestFieldClass>>
            {
                new TestFieldClass<SubTestFieldClass>
                {
                    items = new List<SubTestFieldClass>
                    {
                        new SubTestFieldClass
                        {
                            value = 10
                        },
                        new SubTestFieldClass
                        {
                            value = 20
                        },
                        new SubTestFieldClass
                        {
                            value = 30
                        }
                    }
                },
                new TestFieldClass<SubTestFieldClass>
                {
                    items = new List<SubTestFieldClass>
                    {
                        new SubTestFieldClass
                        {
                            value = 40
                        },
                        new SubTestFieldClass
                        {
                            value = 50
                        },
                        new SubTestFieldClass
                        {
                            value = 60
                        }
                    }
                },
                new TestFieldClass<SubTestFieldClass>
                {
                    items = new List<SubTestFieldClass>
                    {
                        new SubTestFieldClass
                        {
                            value = 70
                        },
                        new SubTestFieldClass
                        {
                            value = 80
                        },
                        new SubTestFieldClass
                        {
                            value = 90
                        }
                    }
                },
            }.ToList();

            MemoryStream stream;
            stream = new MemoryStream();
            Serializer.Serialize(stream, testFieldClass);
            stream.Seek(0, SeekOrigin.Begin);

            schema = new ProtobufSchema<int>();

            IParserDescriptor<int> result = schema.ParserDescriptor.HasField("_2");

            if (index0.HasValue)
                result = result.HasField(index0.Value.ToString(CultureInfo.InvariantCulture));

            result = result.HasField("_2");

            if (index1.HasValue)
                result = result.HasField(index1.Value.ToString(CultureInfo.InvariantCulture));

            result = result.HasField("_4");
            result.IsValue();

            value = 0;
            parser = schema.CreateParser();
            Assert.IsTrue(parser.Parse(stream, ref value));
            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestEncodeSubListItem()
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
                .HasField("_3")
                .IsArray(source => new List<SubTestFieldClass> {source.subValue})
                .HasField("_4", target => target.value)
                .IsValue();

            stream = new MemoryStream();

            printer = schema.CreatePrinter();
            Assert.IsTrue(printer.Print(testFieldClass, stream));
            stream.Seek(0, SeekOrigin.Begin);

            decodedTestFieldClass = Serializer.Deserialize<TestFieldClass<long>>(stream);

            Assert.AreEqual(testFieldClass.subValue.value, decodedTestFieldClass.subValue.value);
        }

        [Test]
        public void TestEncodeSubItem()
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
                .HasField("_3", target => target.subValue)
                .HasField("_4", target => target.value)
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
            IParserDescriptor<List<T>> fieldParser = schema.ParserDescriptor.HasField("_2");
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
            IPrinterDescriptor<List<T>> printerDescriptor = schema.PrinterDescriptor.HasField("_2");
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
            schema.ParserDescriptor.HasField("_1").IsValue();
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
            schema.PrinterDescriptor.HasField("_1").IsValue();

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
