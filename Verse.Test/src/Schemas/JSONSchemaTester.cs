using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;
using Verse.Schemas.JSON;

namespace Verse.Test.Schemas
{
	[TestFixture]
	public class JSONSchemaTester : BaseSchemaTester
	{
		[Test]
		[TestCase("[]", new double[0])]
		[TestCase("[-42.1]", new[] { -42.1 })]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
		[TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
		public void DecodeArrayOfIntegers(string json, double[] expected)
		{
			var schema = new JSONSchema<double[]>();

			schema.DecoderDescriptor.HasItems((ref double[] target, IEnumerable<double> items) => target = items.ToArray()).IsValue();

			var decoder = schema.CreateDecoder();

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
				Assert.IsTrue(decoderStream.Decode(out var value));

				CollectionAssert.AreEqual(expected, value);
			}
		}

		[Test]
		[TestCase("~", 1)]
		[TestCase("\"Unfinished", 13)]
		[TestCase("[1.1.1]", 5)]
		[TestCase("[0 0]", 4)]
		[TestCase("{0}", 2)]
		[TestCase("{\"\" 0}", 5)]
		[TestCase("fail", 3)]
		public void DecodeFailsWithInvalidStream(string json, int expected)
		{
			var schema = new JSONSchema<string>();

			schema.DecoderDescriptor.IsValue();

			var decoder = schema.CreateDecoder();
			var position = -1;

			decoder.Error += (p, m) => position = p;

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
				Assert.IsFalse(decoderStream.Decode(out var value));
				Assert.AreEqual(expected, position);
			}
		}

		[Test]
		public void DecodeFieldFailsWhenNameDoesNotMatch()
		{
			var schema = new JSONSchema<int>();

			schema.DecoderDescriptor.HasField("match").IsValue();

			this.AssertDecodeAndEqual(schema, "{\"match1\": 17}", 0);
			this.AssertDecodeAndEqual(schema, "{\"matc\": 17}", 0);
		}

		[Test]
		public void DecodeFieldFailsWhenScopeDoesNotMatch()
		{
			var schema = new JSONSchema<int>();

			schema.DecoderDescriptor.HasField("match").IsValue();

			this.AssertDecodeAndEqual(schema, "{\"unknown\": {\"match\": 17}}", 0);
		}

		[Test]
		[TestCase("b", "{\"a\": 50, \"b\": 43, \"c\": [1, 5, 9]}", 43)]
		[TestCase("b", "{\"a\": {\"x\": 1, \"y\": 2}, \"b\": \"OK\", \"c\": 21.6}", "OK")]
		[TestCase("glittering", "{\"glittering\": \"prizes\"}", "prizes")]
		[TestCase("", "{\"\": 5}", 5)]
		[TestCase("1", "[\"hey!\", \"take me!\", \"not me!\"]", "take me!")]
		public void DecodeFieldWithinIgnoredContents<T>(string name, string json, T expected)
		{
			var schema = new JSONSchema<T>();

			schema.DecoderDescriptor.HasField(name).IsValue();

			this.AssertDecodeAndEqual(schema, json, expected);
		}

		[Test]
		public void DecodeRecursiveSchema()
		{
			var schema = new JSONSchema<RecursiveEntity>();
			var descriptor = schema.DecoderDescriptor;

			descriptor.HasField("f", (ref RecursiveEntity r, RecursiveEntity v) => r.field = v, descriptor);
			descriptor.HasField("v", (ref RecursiveEntity r, int v) => r.value = v).IsValue();

			var decoder = schema.CreateDecoder();

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"f\": {\"f\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")))
			{
				Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
				Assert.IsTrue(decoderStream.Decode(out var value));

				Assert.AreEqual(42, value.field.field.value);
				Assert.AreEqual(17, value.field.value);
				Assert.AreEqual(3, value.value);
			}
		}

		[Test]
		[TestCase("\"test\"")]
		[TestCase("53")]
		public void DecodeValueFailsWhenTypeDoesNotMatch(string json)
		{
			var schema = new JSONSchema<int>();

			schema.DecoderDescriptor.HasItems<string>((ref int target, IEnumerable<string> elements) => target = elements.Count());
			schema.DecoderDescriptor.IsValue();

			this.AssertDecodeAndEqual(schema, json, 0);
		}

		[Test]
		public void DecodeValueWithCustomConstructor()
		{
			var schema = new JSONSchema<Tuple<Tuple<int, int>>>();

			schema.DecoderDescriptor.CanCreate(() => Tuple.Create(0, 0));

			var descriptor = schema.DecoderDescriptor.HasField("tuple", (ref Tuple<Tuple<int, int>> target, Tuple<int, int> value) => target = Tuple.Create(value));

			descriptor.HasField("a", (ref Tuple<int, int> target, int value) => target = Tuple.Create(value, target.Item2)).IsValue();
			descriptor.HasField("b", (ref Tuple<int, int> target, int value) => target = Tuple.Create(target.Item1, value)).IsValue();

			this.AssertDecodeAndEqual(schema, "{\"tuple\": {\"a\": 5, \"b\": 7}}", Tuple.Create(Tuple.Create(5, 7)));
		}

		[Test]
		[TestCase("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
		[TestCase("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
		public void DecodeValueWithCustomDecoder(string json, string expected)
		{
			var schema = new JSONSchema<Guid>();

			schema.SetDecoderConverter((v) => Guid.Parse(v.String));
			schema.DecoderDescriptor.IsValue();

			this.AssertDecodeAndEqual(schema, json, Guid.Parse(expected));
		}

		[Test]
		[TestCase("false", false)]
		[TestCase("true", true)]
		[TestCase("27", 27.0)]
		[TestCase("-15.5", -15.5)]
		[TestCase("-.5645", -.5645)]
		[TestCase("1.2e3", 1200.0)]
		[TestCase(".5e-2", 0.005)]
		[TestCase("2.945", 2.945)]
		[TestCase("1.1111111111111111111111", 1.1111111111111111111111)]
		[TestCase("8976435454354345437845468735", 8976435454354345437845468735d)]
		[TestCase("9007199254740992", 9007199254740992)] // 2^53
		[TestCase("-9007199254740992", -9007199254740992)] // -2^53
		[TestCase("\"\"", "")]
		[TestCase("\"Hello, World!\"", "Hello, World!")]
		[TestCase("\"\\u00FF \\u0066 \\uB3A8\"", "\xFF f \uB3A8")]
		[TestCase("\"\\\"\"", "\"")]
		[TestCase("\"\\\\\"", "\\")]
		[TestCase("9223372036854775807", 9223372036854775807.0)]
		public void DecodeValueNativeConstant<T>(string json, T expected)
		{
			var schema = new JSONSchema<T>();

			schema.DecoderDescriptor.IsValue();

			this.AssertDecodeAndEqual(schema, json, expected);
		}

		[Test]
		public void DecodeValueNativeDecimal()
		{
			var schema = new JSONSchema<decimal>();

			schema.DecoderDescriptor.IsValue();

			this.AssertDecodeAndEqual(schema, "1e-28", 1e-28m);
		}

		[Test]
		[TestCase(false, "{\"values\":[{\"value\":null},{\"value\":\"test\"}]}")]
		[TestCase(true, "{\"values\":[{},{\"value\":\"test\"}]}")]
		public void EncodeArrayOfEntities(bool omitNull, string expected)
		{
			JSONSchema<JSONValue> schema;

			schema = new JSONSchema<JSONValue>(new JSONConfiguration { OmitNull = omitNull });
			schema.EncoderDescriptor
				.HasField("values")
				.HasItems(v => new[] { JSONValue.Void, v, })
				.HasField("value")
				.IsValue();

			this.AssertEncodeAndEqual(schema, JSONValue.FromString("test"), expected);
		}

		[Test]
		[TestCase(new int[0], "[]")]
		[TestCase(new[] { 21 }, "[21]")]
		[TestCase(new[] { 54, 90, -3, 34, 0, 49 }, "[54,90,-3,34,0,49]")]
		public void EncodeArrayOfIntegers(int[] value, string expected)
		{
			var schema = new JSONSchema<int[]>();

			schema.EncoderDescriptor.HasItems((source) => source).IsValue();

			this.AssertEncodeAndEqual(schema, value, expected);
		}

		[Test]
		[TestCase(false, "{\"firstnull\":null,\"value\":\"test\",\"secondnull\":null,\"item\":{\"values\":[null,\"val1\",null,\"val2\",null]},\"lastnull\":null}")]
		[TestCase(true, "{\"value\":\"test\",\"item\":{\"values\":[\"val1\",\"val2\"]}}")]
		public void EncodeEntityOmitNull(bool omitNull, string expected)
		{
			var schema = new JSONSchema<string>(new JSONConfiguration { OmitNull = omitNull });

			schema.EncoderDescriptor.HasField("firstnull", s => JSONValue.Void).IsValue();
			schema.EncoderDescriptor.HasField("value").IsValue();
			schema.EncoderDescriptor.HasField("secondnull", s => JSONValue.Void).IsValue();
			schema.EncoderDescriptor.HasField("item").HasField("values").HasItems(v => new[] { null, "val1", null, "val2", null }).IsValue();
			schema.EncoderDescriptor.HasField("lastnull", s => JSONValue.Void).IsValue();

			this.AssertEncodeAndEqual(schema, "test", expected);
		}

		[Test]
		[TestCase("glittering", "prizes", "{\"glittering\":\"prizes\"}")]
		[TestCase("", "pwic", "{\"\":\"pwic\"}")]
		public void EncodeField<T>(string name, T value, string expected)
		{
			var schema = new JSONSchema<T>();

			schema.EncoderDescriptor.HasField(name).IsValue();

			this.AssertEncodeAndEqual(schema, value, expected);
		}

		[Test]
		[TestCase("90f59097-d06a-4796-b8d5-87eb6af7ed8b", "\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"")]
		[TestCase("c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025", "\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"")]
		public void EncodeValueCustomEncoder(string guid, string expected)
		{
			var schema = new JSONSchema<Guid>();

			schema.SetEncoderConverter<Guid>((v) => JSONValue.FromString(v.ToString()));
			schema.EncoderDescriptor.IsValue();

			this.AssertEncodeAndEqual(schema, Guid.Parse(guid), expected);
		}

		[Test]
		[TestCase(false, "false")]
		[TestCase(true, "true")]
		[TestCase(3, "3")]
		[TestCase(3.5, "3.5")]
		[TestCase("I sense a soul in search of answers", "\"I sense a soul in search of answers\"")]
		[TestCase("\xFF \u0066 \uB3A8", "\"\\u00FF f \\uB3A8\"")]
		[TestCase("\"", "\"\\\"\"")]
		[TestCase("\\", "\"\\\\\"")]
		public void EncodeValueNative<T>(T value, string expected)
		{
			var schema = new JSONSchema<T>();

			schema.EncoderDescriptor.IsValue();

			this.AssertEncodeAndEqual(schema, value, expected);
		}

		[Test]
		[TestCase(false, "null")]
		[TestCase(true, "")]
		public void EncodeValueNull(bool omitNull, string expected)
		{
			var schema = new JSONSchema<string>(new JSONConfiguration { OmitNull = omitNull });

			schema.EncoderDescriptor.IsValue();

			this.AssertEncodeAndEqual(schema, null, expected);
		}

		[Test]
		public void RoundTripCustomType()
		{
			var schema = new JSONSchema<GuidContainer>();

			schema.SetDecoderConverter<Guid>(v => Guid.Parse(v.String));
			schema.SetEncoderConverter<Guid>(g => JSONValue.FromString(g.ToString()));

			BaseSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema), new GuidContainer
			{
				guid = Guid.NewGuid()
			});
		}

		protected override ISchema<T> CreateSchema<T>()
		{
			return new JSONSchema<T>();
		}

		private void AssertDecodeAndEqual<T>(ISchema<T> schema, string json, T expected)
		{
			var decoder = schema.CreateDecoder();

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
				Assert.IsTrue(decoderStream.Decode(out var value));

				Assert.AreEqual(expected, value);
			}
		}

		private void AssertEncodeAndEqual<T>(ISchema<T> schema, T value, string expected)
		{
			var encoder = schema.CreateEncoder();

			using (var stream = new MemoryStream())
			{
				Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
				Assert.IsTrue(encoderStream.Encode(value));

				Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		private class GuidContainer
		{
			public Guid guid
			{
				get;
				set;
			}
		}

		private class RecursiveEntity
		{
			public RecursiveEntity field;

			public int value;
		}
	}
}