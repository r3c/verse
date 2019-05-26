using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Resolvers;
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

			Assert.That(AdapterResolver.TryGetDecoderConverter<string, T>(schema.DecoderAdapter, out var converter),
				Is.True);

			schema.DecoderDescriptor.HasField(name, () => default, (ref T t, T v) => t = v).HasValue(converter);

			var value = QueryStringSchemaTester.Decode(schema, query);

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

			Assert.That(AdapterResolver.TryGetDecoderConverter<string, T>(schema.DecoderAdapter, out var converter),
				Is.True);

			schema.DecoderDescriptor.HasField(name, () => default, (ref T t, T v) => t = v)
				.HasValue(converter);

			var value = QueryStringSchemaTester.Decode(schema, query);

			Assert.AreEqual(expected, value);
		}

		[Test]
		[TestCase("?=v0")]
		[TestCase("?&f0")]
		[TestCase("?f0==v0")]
		public void DecodeFail(string query)
		{
			var schema = new QueryStringSchema<string>();
			var decoder = schema.CreateDecoder(() => string.Empty);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(query)))
			{
				using (var decoderStream = decoder.Open(stream))
					Assert.IsFalse(decoderStream.TryDecode(out _));
			}
		}

		private static TEntity Decode<TEntity>(ISchema<string, TEntity> schema, string query)
		{
			var decoder = schema.CreateDecoder(() => default);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(query)))
			{
				using (var decoderStream = decoder.Open(stream))
				{
					Assert.IsTrue(decoderStream.TryDecode(out var value));

					return value;
				}
			}
		}
	}
}
