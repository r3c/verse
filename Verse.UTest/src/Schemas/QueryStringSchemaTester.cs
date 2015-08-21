using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.UTest.Schemas
{
    [TestFixture]
    public class QueryStringSchemaTester : SchemaTester
    {
        [Test]
        [TestCase("?a&b&c")]
        [TestCase("?a&")]
        public void ParseSuccess(string query)
        {
            IParser<string> parser;
            QueryStringSchema<string> schema;
            string value;

            schema = new QueryStringSchema<string>();
            value = string.Empty;

            parser = schema.CreateParser();

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
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
        public void ParseFieldValue<T>(string name, string query, T expected)
        {
            IParser<T> parser;
            QueryStringSchema<T> schema;
            T value;

            schema = new QueryStringSchema<T>();
            schema.ParserDescriptor.HasField(name).IsValue();

            parser = schema.CreateParser();
            value = default(T);

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
            Assert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("f0", "?f0=v0+v1", "v0 v1")]
        [TestCase("f0", "?f0=v0%3Dv1", "v0=v1")]
        [TestCase("f0", "?f0=v0%3dv1", "v0=v1")]
        [TestCase("f0", "?f0=http%3a%2f%2fwww.pokewiki.de%2fSchillern+de_Pok%c3%a9mon", "http://www.pokewiki.de/Schillern de_Pokémon")]
        public void ParseSpecialCharacter<T>(string name, string query, T expected)
        {
            IParser<T> parser;
            QueryStringSchema<T> schema;
            T value;

            schema = new QueryStringSchema<T>();
            schema.ParserDescriptor.HasField(name).IsValue();

            parser = schema.CreateParser();
            value = default(T);

            Assert.IsTrue(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
            Assert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("?=v0")]
        [TestCase("?&f0")]
        [TestCase("?f0==v0")]
        public void ParseFail(string query)
        {
            IParser<string> parser;
            QueryStringSchema<string> schema;
            string value;

            schema = new QueryStringSchema<string>();
            parser = schema.CreateParser();
            value = string.Empty;

            Assert.IsFalse(parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(query)), ref value));
        }
    }
}