using System;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Verse.Test
{
    public class SchemaTester
    {
        protected void AssertRoundtrip<T>(ISchema<T> schema, T instance, Func<T> constructor)
        {
            byte[] first;
            CompareLogic compare;
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

            compare = new CompareLogic();

            CollectionAssert.IsEmpty(compare.Compare(instance, other).Differences);

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(printer.Print(other, stream));

                second = stream.ToArray();
            }

            CollectionAssert.AreEqual(first, second);
        }
    }
}