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
			QueryStringSchemaTester.Decode(new QueryStringSchema<string>(), query);
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
			var schema = new QueryStringSchema<T>();

			schema.DecoderDescriptor.HasField(name).IsValue();

			T value = QueryStringSchemaTester.Decode(schema, query);

			Assert.AreEqual(expected, value);
		}

		[Test]
		[TestCase("f0", "?f0=v0+v1", "v0 v1")]
		[TestCase("f0", "?f0=v0%3Dv1", "v0=v1")]
		[TestCase("f0", "?f0=v0%3dv1", "v0=v1")]
		[TestCase("f0", "?f0=http%3a%2f%2fwww.pokewiki.de%2fSchillern+de_Pok%c3%a9mon", "http://www.pokewiki.de/Schillern de_Pokémon")]
		[TestCase("percent", "?percent=%F0%9F%91%8D", "👍")]
		public void DecodeSpecialCharacter<T>(string name, string query, T expected)
		{
			var schema = new QueryStringSchema<T>();

			schema.DecoderDescriptor.HasField(name).IsValue();

			T value = QueryStringSchemaTester.Decode(schema, query);

			Assert.AreEqual(expected, value);
		}

	    [Test]
	    [TestCase("?=v0")]
	    [TestCase("?&f0")]
	    [TestCase("?f0==v0")]
	    public void DecodeFail(string query)
	    {
	        var schema = new QueryStringSchema<string>();
	        var decoder = schema.CreateDecoder();

	        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(query)))
	        {
	            Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
	            Assert.IsFalse(decoderStream.Decode(out _));
	        }
	    }

	    private static T Decode<T>(ISchema<T> schema, string query)
		{
		    var decoder = schema.CreateDecoder();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(query)))
		    {
		        Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
                Assert.IsTrue(decoderStream.Decode(out var value));

		        return value;
            }
		}
	}
}