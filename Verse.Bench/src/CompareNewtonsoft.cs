using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Bench
{
    public class CompareNewtonsoft
    {
        [Test]
        public void DecodeFlatStructure()
        {
            IDecoder<MyFlatStructure> decoder;
            string source;

            decoder = Linker.CreateDecoder(new JSONSchema<MyFlatStructure>());
            source = "{\"lorem\":0,\"ipsum\":65464658634633,\"sit\":1.1,\"amet\":\"Hello, World!\",\"consectetur\":255,\"adipiscing\":64,\"elit\":\"z\",\"sed\":53.25,\"pulvinar\":\"I sense a soul in search of answers\",\"fermentum\":6553,\"hendrerit\":-32768}";

            this.BenchDecode(decoder, () => new MyFlatStructure(), source, 10000);
        }

        [Test]
        [TestCase(10, 10000)]
        [TestCase(1000, 100)]
        [TestCase(10000, 10)]
        [TestCase(100000, 1)]
        public void DecodeLargeArray(int length, int count)
        {
            StringBuilder builder;
            IDecoder<long[]> decoder;
            Random random;
            ISchema<long[]> schema;

            builder = new StringBuilder();
            random = new Random();

            builder.Append("[");

            if (length > 0)
            {
                for (int i = 0; true;)
                {
                    builder.Append(random.Next().ToString(CultureInfo.InvariantCulture));

                    if (++i >= length)
                        break;

                    builder.Append(",");
                }
            }

            builder.Append("]");

            schema = new JSONSchema<long[]>();
            schema.DecoderDescriptor.IsArray((ref long[] target, IEnumerable<long> value) => target = value.ToArray()).IsValue();
            decoder = schema.CreateDecoder();

            this.BenchDecode(decoder, () => null, builder.ToString(), count);
        }

        [Test]
        public void DecodeNestedArray()
        {
            IDecoder<MyNestedArray> decoder;
            string source;

            decoder = Linker.CreateDecoder(new JSONSchema<MyNestedArray>());
            source = "{\"children\":[{\"children\":[],\"value\":\"a\"},{\"children\":[{\"children\":[],\"value\":\"b\"},{\"children\":[],\"value\":\"c\"}],\"value\":\"d\"},{\"children\":[],\"value\":\"e\"}],\"value\":\"f\"}";

            this.BenchDecode(decoder, () => new MyNestedArray(), source, 10000);
        }

        private void BenchDecode<T>(IDecoder<T> decoder, Func<T> constructor, string source, int count)
        {
            byte[] buffer;
            T instance;
            T reference;
            TimeSpan timeNewton;
            TimeSpan timeVerse;
            Stopwatch watch;

            reference = JsonConvert.DeserializeObject<T>(source);
            buffer = Encoding.UTF8.GetBytes(source);

            watch = Stopwatch.StartNew();

            for (int i = count; i-- > 0;)
                Assert.NotNull(JsonConvert.DeserializeObject<T>(source));

            timeNewton = watch.Elapsed;

            watch = Stopwatch.StartNew();

            for (int i = count; i-- > 0;)
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    instance = constructor();

                    Assert.IsTrue(decoder.Decode(stream, ref instance));
                }
            }

            timeVerse = watch.Elapsed;

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                instance = constructor();

                Assert.IsTrue(decoder.Decode(stream, ref instance));
            }

            Assert.AreEqual(instance, reference);

            try
            {
                Console.WriteLine("[{0}] NewtonSoft: {1}, Verse: {2}", TestContext.CurrentContext.Test.FullName, timeNewton, timeVerse);
            }
            catch (NullReferenceException)
            {
                // Test FullName throws when this method is executed out of a test
            }

#if DEBUG
            Assert.Inconclusive("Library should be compiled in Release mode before benching");
#endif
        }

        [Test]
        public void EncodeFlatStructure()
        {
            IEncoder<MyFlatStructure> encoder;
            MyFlatStructure instance;

            encoder = Linker.CreateEncoder(new JSONSchema<MyFlatStructure>());

            instance = new MyFlatStructure
            {
                adipiscing = 64,
                amet = "Hello, World!",
                consectetur = 255,
                elit = 'z',
                fermentum = 6553,
                hendrerit = -32768,
                ipsum = 65464658634633,
                lorem = 0,
                pulvinar = "I sense a soul in search of answers",
                sed = 53.25f,
                sit = 1.1
            };

            this.BenchEncode(encoder, instance, 10000);
        }

        [Test]
        public void EncodeNestedArray()
        {
            IEncoder<MyNestedArray> encoder;
            MyNestedArray instance;

            encoder = Linker.CreateEncoder(new JSONSchema<MyNestedArray>());

            instance = new MyNestedArray
            {
                children = new[]
                {
                    new MyNestedArray
                    {
                        children = null,
                        value = "a"
                    },
                    new MyNestedArray
                    {
                        children = new[]
                        {
                            new MyNestedArray
                            {
                                children = null,
                                value = "b"
                            },
                            new MyNestedArray
                            {
                                children = null,
                                value = "c"
                            }
                        },
                        value = "d"
                    },
                    new MyNestedArray
                    {
                        children = new MyNestedArray[0],
                        value = "e"
                    }
                },
                value = "f"
            };

            this.BenchEncode(encoder, instance, 10000);
        }

        private void BenchEncode<T>(IEncoder<T> encoder, T instance, int count)
        {
            string expected;
            TimeSpan timeNewton;
            TimeSpan timeVerse;
            Stopwatch watch;

            expected = JsonConvert.SerializeObject(instance);
            watch = Stopwatch.StartNew();

            for (int i = count; i-- > 0;)
                JsonConvert.SerializeObject(instance);

            timeNewton = watch.Elapsed;

            watch = Stopwatch.StartNew();

            for (int i = count; i-- > 0;)
            {
                using (MemoryStream stream = new MemoryStream())
                    Assert.IsTrue(encoder.Encode(instance, stream));
            }

            timeVerse = watch.Elapsed;

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(encoder.Encode(instance, stream));
                Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
            }

            try
            {
                Console.WriteLine("[{0}] NewtonSoft: {1}, Verse: {2}", TestContext.CurrentContext.Test.FullName, timeNewton, timeVerse);
            }
            catch (NullReferenceException)
            {
                // Test FullName throws when this method is executed out of a test
            }

#if DEBUG
            Assert.Inconclusive("Library should be compiled in Release mode before benching");
#endif
        }

        private struct MyFlatStructure : IEquatable<MyFlatStructure>
        {
            public int lorem;

            public long ipsum;

            public double sit;

            public string amet;

            public byte consectetur;

            public ushort adipiscing;

            public char elit;

            public float sed;

            public string pulvinar;

            public uint fermentum;

            public short hendrerit;

            public bool Equals(MyFlatStructure other)
            {
                return
                    this.lorem == other.lorem &&
                    this.ipsum == other.ipsum &&
                    Math.Abs(this.sit - other.sit) < double.Epsilon &&
                    this.amet == other.amet &&
                    this.consectetur == other.consectetur &&
                    this.adipiscing == other.adipiscing &&
                    this.elit == other.elit &&
                    Math.Abs(this.sed - other.sed) < float.Epsilon &&
                    this.pulvinar == other.pulvinar &&
                    this.fermentum == other.fermentum &&
                    this.hendrerit == other.hendrerit;
            }
        }

        private class MyNestedArray : IEquatable<MyNestedArray>
        {
            public MyNestedArray[] children;

            public string value;

            public bool Equals(MyNestedArray other)
            {
                if (this.children.Length != other.children.Length)
                    return false;

                for (int i = 0; i < this.children.Length; ++i)
                {
                    if (!this.children[i].Equals(other.children[i]))
                        return false;
                }

                return this.value == other.value;
            }
        }
    }
}