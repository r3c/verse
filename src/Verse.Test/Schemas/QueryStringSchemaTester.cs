using System.IO;
using System.Text;
using NUnit.Framework;

namespace Verse.Test.Schemas;

[TestFixture]
public class QueryStringSchemaTester
{
    [Test]
    [TestCase("?a&b&c")]
    [TestCase("?a&")]
    public void DecodeSuccess(string query)
    {
        var schema = Schema.CreateQueryString<string>();

        schema.DecoderDescriptor.IsObject(() => null);

        Decode(schema, query);
    }

    [Test]
    [TestCase("f0", "?f0=v0", "v0")]
    [TestCase("f0", "?f0=", "")]
    [TestCase("f0", "?f0=v0&f1=v1", "v0")]
    [TestCase("f1", "?f0=v0&f1=v1", "v1")]
    [TestCase("f0", "?f0=v0(v1)", "v0(v1)")]
    [TestCase("f0", "?f0=-v0", "-v0")]
    [TestCase("f0", "?f0=v0_v1", "v0_v1")]
    [TestCase("f0", "?f0=v0.v1", "v0.v1")]
    [TestCase("f0", "?f0=v0!v1", "v0!v1")]
    [TestCase("f0", "?f0=v0~v1", "v0~v1")]
    [TestCase("f0", "?f0=v0*v1", "v0*v1")]
    [TestCase("f0", "?f0=v0'v1", "v0'v1")]
    [TestCase("f0", "?f0=a,b&f1=c", "a,b")]
    [TestCase("f1", "?f0=a,b&f1=c", "c")]
    [TestCase("f0", "?f0&f1", "")]
    [TestCase("f1", "?f0&f1", "")]
    public void DecodeFieldValue<T>(string name, string query, T expected)
    {
        var schema = Schema.CreateQueryString<T>();
        var converter = SchemaHelper<string>.GetDecoderConverter<T>(Format.String.To);

        schema.DecoderDescriptor
            .IsObject(() => default!)
            .HasField<T>(name, (_, v) => v)
            .IsValue(converter);

        var value = Decode(schema, query);

        Assert.That(expected, Is.EqualTo(value));
    }

    [Test]
    [TestCase("f0", "?f0=v0+v1", "v0 v1")]
    [TestCase("f0", "?f0=v0%3Dv1", "v0=v1")]
    [TestCase("f0", "?f0=v0%3dv1", "v0=v1")]
    [TestCase("f0", "?f0=http%3a%2f%2fwww.pokewiki.de%2fSchillern+de_Pok%c3%a9mon",
        "http://www.pokewiki.de/Schillern de_Pokémon")]
    [TestCase("percent", "?percent=%F0%9F%91%8D", "👍")]
    public void DecodeSpecialCharacter<T>(string name, string query, T expected)
    {
        var schema = Schema.CreateQueryString<T>();
        var converter = SchemaHelper<string>.GetDecoderConverter<T>(Format.String.To);

        schema.DecoderDescriptor
            .IsObject(() => default!)
            .HasField<T>(name, (_, v) => v)
            .IsValue(converter);

        var value = Decode(schema, query);

        Assert.That(expected, Is.EqualTo(value));
    }

    [Test]
    [TestCase("?=v0")]
    [TestCase("?&f0")]
    [TestCase("?f0==v0")]
    public void DecodeFail(string query)
    {
        var schema = Schema.CreateQueryString<string>();
        var decoder = schema.CreateDecoder();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(query));
        using var decoderStream = decoder.Open(stream);

        Assert.That(decoderStream.TryDecode(out _), Is.False);
    }

    private static TEntity Decode<TEntity>(ISchema<string, TEntity> schema, string query)
    {
        var decoder = schema.CreateDecoder();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(query));
        using var decoderStream = decoder.Open(stream);

        Assert.That(decoderStream.TryDecode(out var value), Is.True);

        return value;
    }
}