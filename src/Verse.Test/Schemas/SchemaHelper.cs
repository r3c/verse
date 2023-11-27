using System;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using Verse.Resolvers;

namespace Verse.Test.Schemas;

internal static class SchemaHelper<TNative>
{
    public static void AssertRoundTripWithCustom<TEntity>(IDecoder<TEntity> decoder, IEncoder<TEntity> encoder,
        TEntity instance)
    {
        TEntity decoded;
        byte[] encoded1;
        byte[] encoded2;

        using (var stream = new MemoryStream())
        {
            using (var encoderStream = encoder.Open(stream))
                encoderStream.Encode(instance);

            encoded1 = stream.ToArray();
        }

        using (var stream = new MemoryStream(encoded1))
        {
            using (var decoderStream = decoder.Open(stream))
                Assert.That(decoderStream.TryDecode(out decoded), Is.True);
        }

        var comparisonResult = new CompareLogic().Compare(instance, decoded);

        Assert.That(comparisonResult.Differences, Is.Empty,
            $"differences found after decoding entity: {comparisonResult.DifferencesString}");

        using (var stream = new MemoryStream())
        {
            using (var encoderStream = encoder.Open(stream))
                encoderStream.Encode(decoded);

            encoded2 = stream.ToArray();
        }

        Assert.That(encoded1, Is.EqualTo(encoded2).AsCollection);
    }

    public static void AssertRoundTripWithLinker<TEntity>(IFormat<TNative> format, ISchema<TNative, TEntity> schema,
        TEntity instance)
    {
        var linker = Linker.CreateReflection<TNative>();

        AssertRoundTripWithCustom(linker.CreateDecoder(format, schema), linker.CreateEncoder(format, schema), instance);
    }

    public static Func<TNative, TEntity> GetDecoderConverter<TEntity>(IDecoderAdapter<TNative> decoderAdapter)
    {
        var result = AdapterResolver.TryGetDecoderConverter<TNative, TEntity>(decoderAdapter, out var converter);

        Assert.That(result, Is.True);

        return converter;
    }

    public static Func<TEntity, TNative> GetEncoderConverter<TEntity>(IEncoderAdapter<TNative> encoderAdapter)
    {
        var result = AdapterResolver.TryGetEncoderConverter<TNative, TEntity>(encoderAdapter, out var converter);

        Assert.That(result, Is.True);

        return converter;
    }
}