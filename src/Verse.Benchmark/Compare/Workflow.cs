using System;
using System.IO;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using Verse.Schemas.Json;

namespace Verse.Benchmark.Compare;

internal static class Workflow
{
    public static (Action Decode, Action Encode, string Source) InitializeJson<T>(T instance)
    {
        var compareLogic = new CompareLogic();
        var source = JsonConvert.SerializeObject(instance);

        var linker = Linker.CreateReflection<JsonValue>();
        var schema = Schema.CreateJson<T>();
        var decoder = linker.CreateDecoder(schema);
        var decoderMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(source));
        var encoder = linker.CreateEncoder(schema);
        var encoderMemoryStream = new MemoryStream(1024);

        using (var decoderStream = decoder.Open(decoderMemoryStream))
        {
            if (!decoderStream.TryDecode(out var decoded1))
                throw new InvalidOperationException("failed decoding");

            var compare1 = compareLogic.Compare(instance, decoded1);

            if (!compare1.AreEqual)
                throw new InvalidOperationException("unexpected decoded differences: " + compare1.DifferencesString);
        }

        using (var encoderStream = encoder.Open(encoderMemoryStream))
        {
            encoderStream.Encode(instance);
        }

        var decoded2 = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(encoderMemoryStream.ToArray()));
        var compare2 = compareLogic.Compare(instance, decoded2);

        if (!compare2.AreEqual)
            throw new InvalidOperationException("unexpected encoded differences: " + compare2.DifferencesString);

        return (
            () => DecodeJson(decoder, decoderMemoryStream),
            () => EncodeJson(encoder, encoderMemoryStream, instance),
            source);
    }

    private static void DecodeJson<T>(IDecoder<T> decoder, Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var decoderStream = decoder.Open(stream);

        decoderStream.TryDecode(out _);
    }

    private static void EncodeJson<T>(IEncoder<T> encoder, Stream stream, T instance)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var encoderStream = encoder.Open(stream);

        encoderStream.Encode(instance);
    }
}