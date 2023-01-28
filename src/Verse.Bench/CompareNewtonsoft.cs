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

        var decoder = Linker.CreateDecoder(Schema.CreateJson<MyFlatStructure>());

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

        var schema = Schema.CreateJson<long[]>();

        schema.DecoderDescriptor
            .IsArray<long>(elements => elements.ToArray())
            .IsValue(schema.NativeTo.Integer64S);

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

        var decoder = Linker.CreateDecoder(Schema.CreateJson<MyNestedArray>());

        BenchDecode(decoder, source, 10000);
    }

    [Test]
    public void EncodeFlatStructure()
    {
        var encoder = Linker.CreateEncoder(Schema.CreateJson<MyFlatStructure>());
        var instance = new MyFlatStructure
        {
            Adipiscing = 64,
            Amet = "Hello, World!",
            Consectetur = 255,
            Elit = 'z',
            Fermentum = 6553,
            Hendrerit = -32768,
            Ipsum = 65464658634633,
            Lorem = 0,
            Pulvinar = "I sense a soul in search of answers",
            Sed = 53.25f,
            Sit = 1.1
        };

        BenchEncode(encoder, instance, 10000);
    }

    [Test]
    public void EncodeNestedArray()
    {
        var encoder = Linker.CreateEncoder(Schema.CreateJson<MyNestedArray>());
        var instance = new MyNestedArray
        {
            Children = new[]
            {
                new MyNestedArray
                {
                    Children = null,
                    Value = "a"
                },
                new MyNestedArray
                {
                    Children = new[]
                    {
                        new MyNestedArray
                        {
                            Children = null,
                            Value = "b"
                        },
                        new MyNestedArray
                        {
                            Children = null,
                            Value = "c"
                        }
                    },
                    Value = "d"
                },
                new MyNestedArray
                {
                    Children = new MyNestedArray[0],
                    Value = "e"
                }
            },
            Value = "f"
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
        public int Lorem;

        public long Ipsum;

        public double Sit;

        public string Amet;

        public byte Consectetur;

        public ushort Adipiscing;

        public char Elit;

        public float Sed;

        public string Pulvinar;

        public uint Fermentum;

        public short Hendrerit;

        public bool Equals(MyFlatStructure other)
        {
            return
                Lorem == other.Lorem &&
                Ipsum == other.Ipsum &&
                Math.Abs(Sit - other.Sit) < double.Epsilon &&
                Amet == other.Amet &&
                Consectetur == other.Consectetur &&
                Adipiscing == other.Adipiscing &&
                Elit == other.Elit &&
                Math.Abs(Sed - other.Sed) < float.Epsilon &&
                Pulvinar == other.Pulvinar &&
                Fermentum == other.Fermentum &&
                Hendrerit == other.Hendrerit;
        }
    }

    private class MyNestedArray : IEquatable<MyNestedArray>
    {
        public MyNestedArray[] Children;

        public string Value;

        public bool Equals(MyNestedArray other)
        {
            if (Children.Length != other.Children.Length)
                return false;

            for (var i = 0; i < Children.Length; ++i)
            {
                if (!Children[i].Equals(other.Children[i]))
                    return false;
            }

            return Value == other.Value;
        }
    }
}