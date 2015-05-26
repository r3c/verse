using System;
using System.IO;
using NUnit.Framework;

namespace Verse.UTest
{
    public class SchemaTester
    {
        protected void AssertRoundtrip<T>(ISchema<T> schema, T instance, Func<T> constructor, Func<T, T, bool> equalityTester)
        {
            byte[] first;
            T other;
            IParser<T> parser;
            IPrinter<T> printer;
            byte[] second;

            parser = Linker.CreateParser(schema);
            printer = Linker.CreatePrinter(schema);

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(instance, stream));

                first = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream(first))
            {
                other = constructor();

                Assert.IsTrue(parser.Parse(stream, ref other));
            }

            Assert.IsTrue(equalityTester(instance, other));

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(other, stream));

                second = stream.ToArray();
            }

            CollectionAssert.AreEqual(first, second);
        }
    }
}