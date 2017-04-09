using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Bench
{
    class AlternativeIsValue
    {
        [Test]
        [TestCase(100000)]
        public void Bench(int repeat)
        {
            IDecoder<A> decoder1;
            IDecoder<A> decoder2;
            JSONSchema<A> schema;
            A value;

            schema = new JSONSchema<A>();
            schema.DecoderDescriptor.HasField("b", (ref A a, int b) => a.b = b).IsValue();

            decoder1 = schema.CreateDecoder();

            schema = new JSONSchema<A>();
            schema.DecoderDescriptor.HasField("b").IsValue((ref A a, int b) => a.b = b);

            decoder2 = schema.CreateDecoder();

            var j1 = Encoding.UTF8.GetBytes("{\"b\": 5}");
            var j2 = Encoding.UTF8.GetBytes("{\"b\": 7}");

            value = new A();
            Assert.IsTrue(decoder1.Decode(new MemoryStream(j1), ref value));
            Assert.AreEqual(5, value.b);

            value = new A();
            Assert.IsTrue(decoder2.Decode(new MemoryStream(j2), ref value));
            Assert.AreEqual(7, value.b);

            var m1 = new MemoryStream(j1);
            var s1 = Stopwatch.StartNew();

            for (int i = 0; i < repeat; ++i)
            {
                decoder1.Decode(m1, ref value);
                m1.Seek(0, SeekOrigin.Begin);
            }

            s1.Stop();

            var m2 = new MemoryStream(j2);
            var s2 = Stopwatch.StartNew();

            for (int i = 0; i < repeat; ++i)
            {
                decoder2.Decode(m2, ref value);
                m2.Seek(0, SeekOrigin.Begin);
            }

            s2.Stop();

            Console.WriteLine("p1: " + s1.Elapsed);
            Console.WriteLine("p2: " + s2.Elapsed);
        }

        private struct A
        {
            public int b;
        }
    }
}