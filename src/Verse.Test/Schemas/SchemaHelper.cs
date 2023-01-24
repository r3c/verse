using System;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using Verse.Resolvers;

namespace Verse.Test.Schemas;

internal static class SchemaHelper<TNative>
{
    public static void AssertRoundTrip<TEntity>(IDecoder<TEntity> decoder, IEncoder<TEntity> encoder, TEntity instance)
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
                Assert.IsTrue(decoderStream.TryDecode(out decoded));
        }

        var comparisonResult = new CompareLogic().Compare(instance, decoded);

        CollectionAssert.IsEmpty(comparisonResult.Differences,
            $"differences found after decoding entity: {comparisonResult.DifferencesString}");

        using (var stream = new MemoryStream())
        {
            using (var encoderStream = encoder.Open(stream))
                encoderStream.Encode(decoded);

            encoded2 = stream.ToArray();
        }

        CollectionAssert.AreEqual(encoded1, encoded2);
    }

    public static Setter<TEntity, TNative> GetDecoderConverter<TEntity>(IDecoderAdapter<TNative> decoderAdapter)
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