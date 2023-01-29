using System;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace Verse.Benchmark.Compare;

public abstract class JsonCompare<T>
{
    protected abstract T Instance { get; }

    private Action _decode = () => { };
    private Action _encode = () => { };
    private T _instance = default!;
    private string _source = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        var (decode, encode, source) = Workflow.InitializeJson(Instance);

        _decode = decode;
        _encode = encode;
        _instance = Instance;
        _source = source;
    }

    [Benchmark]
    public void DecodeWithNewtonsoft()
    {
        JsonConvert.DeserializeObject<T>(_source);
    }

    [Benchmark]
    public void DecodeWithVerse()
    {
        _decode();
    }

    [Benchmark]
    public void EncodeWithNewtonsoft()
    {
        JsonConvert.SerializeObject(_instance);
    }

    [Benchmark]
    public void EncodeWithVerse()
    {
        _encode();
    }
}