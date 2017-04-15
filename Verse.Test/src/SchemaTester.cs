using System;
using System.IO;
using NUnit.Framework;

namespace Verse.Test
{
    public class SchemaTester
    {
        protected void AssertRoundtrip<T>(ISchema<T> schema, T instance, Func<T> constructor, Func<T, T, bool> equalityTester)
        {
            byte[] first;
            T other;
            IDecoder<T> decoder;
            IEncoder<T> encoder;
            byte[] second;

            decoder = Linker.CreateDecoder(schema);
            encoder = Linker.CreateEncoder(schema);

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(instance, stream));

                first = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream(first))
            {
                other = constructor();

                Assert.IsTrue(decoder.Decode(stream, ref other));
            }

            Assert.IsTrue(equalityTester(instance, other));

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(other, stream));

                second = stream.ToArray();
            }

            CollectionAssert.AreEqual(first, second);
        }
    }
}