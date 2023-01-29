using System;
using BenchmarkDotNet.Attributes;

namespace Verse.Benchmark.Compare;

[MemoryDiagnoser]
public class JsonCompareFlatStructure : JsonCompare<JsonCompareFlatStructure.MyFlatStructure>
{
    protected override MyFlatStructure Instance => new()
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

    public struct MyFlatStructure : IEquatable<MyFlatStructure>
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
}