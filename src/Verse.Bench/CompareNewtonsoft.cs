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

namespace Verse.Bench;

public class CompareNewtonsoft
{
    [OneTimeSetUp]
    public void Setup()
    {
#if DEBUG
        Assert.Fail("Library should be compiled in Release mode before benching");
#endif

        Trace.Listeners.Add(new TextWriterTraceListener("Verse.Bench.log"));
        Trace.AutoFlush = true;
    }

    [Test]
    public void DecodeFlatStructure()
    {
        const string source = "{" +
                              "\"lorem\":0," +
                              "\"ipsum\":65464658634633," +
                              "\"sit\":1.1," +
                              "\"amet\":\"Hello, World!\"," +
                              "\"consectetur\":255," +
                              "\"adipiscing\":64," +
                              "\"elit\":\"z\"," + "\"sed\":53.25," +
                              "\"pulvinar\":\"I sense a soul in search of answers\"," +
                              "\"fermentum\":6553," +
                              "\"hendrerit\":-32768" +
                              "}";

        var decoder = Linker.CreateDecoder(new JsonSchema<MyFlatStructure>());

        BenchDecode(decoder, source, 10000);
    }

    [Test]
    [TestCase(10, 10000)]
    [TestCase(1000, 100)]
    [TestCase(10000, 10)]
    [TestCase(100000, 1)]
    public void DecodeLargeArray(int length, int count)
    {
        var builder = new StringBuilder();
        var random = new Random();

        builder.Append("[");

        if (length > 0)
        {
            for (var i = 0;;)
            {
                builder.Append(random.Next().ToString(CultureInfo.InvariantCulture));

                if (++i >= length)
                    break;

                builder.Append(",");
            }
        }

        builder.Append("]");

        var schema = new JsonSchema<long[]>();

        schema.DecoderDescriptor
            .IsArray<long>(elements => elements.ToArray())
            .IsValue(schema.DecoderAdapter.ToInteger64S);

        var decoder = schema.CreateDecoder();

        BenchDecode(decoder, builder.ToString(), count);
    }

    [Test]
    public void DecodeNestedArray()
    {
        const string source =
            "{" +
            "\"children\":[{"+
            "\"children\":[],\"value\":\"a\""+
            "},{"+
            "\"children\":[{\"children\":[],\"value\":\"b\"},{\"children\":[],\"value\":\"c\"}]," +
            "\"value\":\"d\""+
            "},{"+
            "\"children\":[],\"value\":\"e\""+
            "}],"+
            "\"value\":\"f\"" +
            "}";

        var decoder = Linker.CreateDecoder(new JsonSchema<MyNestedArray>());

        BenchDecode(decoder, source, 10000);
    }

    [Test]
    public void EncodeFlatStructure()
    {
        var encoder = Linker.CreateEncoder(new JsonSchema<MyFlatStructure>());
        var instance = new MyFlatStructure
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

        BenchEncode(encoder, instance, 10000);
    }

    [Test]
    public void EncodeNestedArray()
    {
        var encoder = Linker.CreateEncoder(new JsonSchema<MyNestedArray>());
        var instance = new MyNestedArray
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

        BenchEncode(encoder, instance, 10000);
    }

    private static void BenchDecode<T>(IDecoder<T> decoder, string source, int count)
    {
        var expected = JsonConvert.DeserializeObject<T>(source);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(source));

        using (var decoderStream = decoder.Open(stream))
        {
            Assert.That(decoderStream.TryDecode(out var candidate), Is.True);
            Assert.That(candidate, Is.EqualTo(expected));
        }

        Bench(new (string, Action)[]
        {
            ("Newtonsoft", () => { JsonConvert.DeserializeObject<T>(source); }),
            ("Verse", () =>
            {
                stream.Seek(0, SeekOrigin.Begin);

                using (var decoderStream = decoder.Open(stream))
                    decoderStream.TryDecode(out _);
            })
        }, count);
    }

    private static void BenchEncode<T>(IEncoder<T> encoder, T instance, int count)
    {
        var expected = JsonConvert.SerializeObject(instance);
        var stream = new MemoryStream(1024);

        using (var encoderStream = encoder.Open(stream))
            encoderStream.Encode(instance);

        var candidate = Encoding.UTF8.GetString(stream.ToArray());

        Assert.That(expected, Is.EqualTo(candidate));

        Bench(new (string, Action)[]
        {
            ("Newtonsoft", () => { JsonConvert.SerializeObject(instance); }),
            ("Verse", () =>
            {
                stream.Seek(0, SeekOrigin.Begin);

                using (var encoderStream = encoder.Open(stream))
                    encoderStream.Encode(instance);
            })
        }, count);
    }

    private static void Bench(IEnumerable<(string, Action)> variants, int repeat)
    {
        var timings = new List<(string, TimeSpan)>();

        foreach (var (name, action) in variants)
        {
            action();

            var timer = Stopwatch.StartNew();

            for (var i = 0; i < repeat; ++i)
                action();

            timings.Add((name, timer.Elapsed));
        }

        Trace.WriteLine($"[{TestContext.CurrentContext.Test.FullName}]");

        foreach (var (item1, timeSpan) in timings)
            Trace.WriteLine($"  - {item1}: {timeSpan}");
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
                lorem == other.lorem &&
                ipsum == other.ipsum &&
                Math.Abs(sit - other.sit) < double.Epsilon &&
                amet == other.amet &&
                consectetur == other.consectetur &&
                adipiscing == other.adipiscing &&
                elit == other.elit &&
                Math.Abs(sed - other.sed) < float.Epsilon &&
                pulvinar == other.pulvinar &&
                fermentum == other.fermentum &&
                hendrerit == other.hendrerit;
        }
    }

    private class MyNestedArray : IEquatable<MyNestedArray>
    {
        public MyNestedArray[] children;

        public string value;

        public bool Equals(MyNestedArray other)
        {
            if (children.Length != other.children.Length)
                return false;

            for (var i = 0; i < children.Length; ++i)
            {
                if (!children[i].Equals(other.children[i]))
                    return false;
            }

            return value == other.value;
        }
    }
}