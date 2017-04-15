using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Test.Schemas
{
    [TestFixture]
    public class QueryStringSchemaTester
    {
        [Test]
        [TestCase("?a&b&c")]
        [TestCase("?a&")]
        public void DecodeSuccess(string query)
        {
            IDecoder<string> decoder;
            QueryStringSchema<string> schema;
            string value;

            schema = new QueryStringSchema<string>();
            value = string.Empty;

            decoder = schema.CreateDecoder();

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
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
        public void DecodeFieldValue<T>(string name, string query, T expected)
        {
            IDecoder<T> decoder;
            QueryStringSchema<T> schema;
            T value;

            schema = new QueryStringSchema<T>();
            schema.DecoderDescriptor.HasField(name).IsValue();

            decoder = schema.CreateDecoder();
            value = default(T);

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
            Assert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("f0", "?f0=v0+v1", "v0 v1")]
        [TestCase("f0", "?f0=v0%3Dv1", "v0=v1")]
        [TestCase("f0", "?f0=v0%3dv1", "v0=v1")]
        [TestCase("f0", "?f0=http%3a%2f%2fwww.pokewiki.de%2fSchillern+de_Pok%c3%a9mon", "http://www.pokewiki.de/Schillern de_Pokémon")]
        public void DecodeSpecialCharacter<T>(string name, string query, T expected)
        {
            IDecoder<T> decoder;
            QueryStringSchema<T> schema;
            T value;

            schema = new QueryStringSchema<T>();
            schema.DecoderDescriptor.HasField(name).IsValue();

            decoder = schema.CreateDecoder();
            value = default(T);

            Assert.IsTrue(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
            Assert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("?=v0")]
        [TestCase("?&f0")]
        [TestCase("?f0==v0")]
        public void DecodeFail(string query)
        {
            IDecoder<string> decoder;
            QueryStringSchema<string> schema;
            string value;

            schema = new QueryStringSchema<string>();
            decoder = schema.CreateDecoder();
            value = string.Empty;

            Assert.IsFalse(decoder.Decode(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
        }
    }
}