using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.UTest
{
    [TestFixture]
    public class LinkerTester
    {
        [Test]
        [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
        [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
        public void LinkPrinterArrayFromArray(double[] value, string expected)
        {
            IPrinter<double[]> printer;

            printer = Linker.CreatePrinter(new JSONSchema<double[]>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(value, stream));
                CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
        [TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
        public void LinkPrinterArrayFromList(double[] value, string expected)
        {
            IPrinter<List<double>> printer;

            printer = Linker.CreatePrinter(new JSONSchema<List<double>>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(new List<double>(value), stream));
                CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase(53, "{\"Field\":53}")]
        [TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
        public void LinkPrinterField<T>(T value, string expected)
        {
            IPrinter<FieldContainer<T>> printer;

            printer = Linker.CreatePrinter(new JSONSchema<FieldContainer<T>>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(new FieldContainer<T> { Field = value }, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase(53, "{\"Property\":53}")]
        [TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
        public void LinkPrinterProperty<T>(T value, string expected)
        {
            IPrinter<PropertyContainer<T>> printer;

            printer = Linker.CreatePrinter(new JSONSchema<PropertyContainer<T>>());

            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(new PropertyContainer<T> { Property = value }, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [Theory]
        public void LinkPrinterRecursive(bool ignoreNull)
        {
            string expected;
            IPrinter<Recursive> printer;
            Recursive value;

            expected = ignoreNull
                ? "{\"r\":{\"r\":{\"v\":42},\"v\":17},\"v\":3}"
                : "{\"r\":{\"r\":{\"r\":null,\"v\":42},\"v\":17},\"v\":3}";

            printer = Linker.CreatePrinter(new JSONSchema<Recursive>(new JSONSettings(new UTF8Encoding(false), ignoreNull)));

            using (var stream = new MemoryStream())
            {
                value = new Recursive { r = new Recursive { r = new Recursive { v = 42 }, v = 17 }, v = 3 };

                Assert.IsTrue(printer.Print(value, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        [Test]
        [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
        [TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
        public void LinkParserArrayFromArray(string json, double[] expected)
        {
            IParser<double[]> parser;
            double[] value;

            parser = Linker.CreateParser(new JSONSchema<double[]>());
            value = new double[0];

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
        [TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
        public void LinkParserArrayFromList(string json, double[] expected)
        {
            IParser<List<double>> parser;
            List<double> value;

            parser = Linker.CreateParser(new JSONSchema<List<double>>());
            value = new List<double>();

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            CollectionAssert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("{\"Field\": 53}", 53)]
        [TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
        public void LinkParserField<T>(string json, T expected)
        {
            IParser<FieldContainer<T>> parser;
            FieldContainer<T> value;

            parser = Linker.CreateParser(new JSONSchema<FieldContainer<T>>());
            value = new FieldContainer<T>();

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, value.Field);
        }

        [Test]
        [TestCase("{\"Property\": 53}", 53)]
        [TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
        public void LinkParserProperty<T>(string json, T expected)
        {
            IParser<PropertyContainer<T>> parser;
            PropertyContainer<T> value;

            parser = Linker.CreateParser(new JSONSchema<PropertyContainer<T>>());
            value = new PropertyContainer<T>();

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), ref value));
            Assert.AreEqual(expected, value.Property);
        }

        [Test]
        public void LinkParserRecursive()
        {
            IParser<Recursive> parser;
            Recursive value;

            parser = Linker.CreateParser(new JSONSchema<Recursive>());
            value = new Recursive();

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes("{\"r\": {\"r\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")), ref value));

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