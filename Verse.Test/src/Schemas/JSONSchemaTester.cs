using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using Verse.Schemas;
using Verse.Schemas.JSON;

namespace Verse.Test.Schemas
{
	[TestFixture]
	public class JSONSchemaTester : BaseSchemaTester
	{
		[Test]
		[TestCase(false, "{\"value1\" : [ { \"value2\" : 123 } ]}", new[] {123d})]
		[TestCase(true, "{\"value1\" : [ { \"value2\" : 123 } ]}", new[] {123d})]
		[TestCase(false, "{\"value1\" : [ { \"value2\" : 123 }, { \"value2\" : 124 } ]}", new[] {123d, 124d})]
		[TestCase(true, "{\"value1\" : [ { \"value2\" : 123 }, { \"value2\" : 124 } ]}", new[] {123d, 124d})]
		[TestCase(false, "{\"value1\" : { \"value2\" : 123 } }", new double[0])]
		[TestCase(true, "{\"value1\" : { \"value2\" : 123 } }", new[] {123d})]
		public void DecodeObjectAsArray(bool acceptAsArray, string json, double[] expected)
		{
			var schema = new JSONSchema<double[]>(new JSONConfiguration {AcceptValueAsArray = acceptAsArray});

			schema.DecoderDescriptor
				.HasField("value1", () => default, (ref double[] entity, double[] value) => entity = value)
				.HasElements(() => 0d, (ref double[] t, IEnumerable<double> e) => t = e.ToArray())
				.HasField("value2", () => default, (ref double entity, double value) => entity = value)
				.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, Array.Empty<double>, json, expected);
		}

		[Test]
		[TestCase(false, "{\"key1\": 27.5, \"key2\": 19}", new double[0])]
		[TestCase(true, "{\"key1\": 27.5, \"key2\": 19}", new[] {27.5, 19})]
		public void DecodeValueAsArray(bool acceptAsArray, string json, double[] expected)
		{
			var schema = new JSONSchema<double[]>(new JSONConfiguration {AcceptObjectAsArray = acceptAsArray});

			schema.DecoderDescriptor.HasElements(() => 0d, (ref double[] t, IEnumerable<double> e) => t = e.ToArray())
				.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, Array.Empty<double>, json, expected);
		}

		[Test]
		[TestCase("[]", new double[0])]
		[TestCase("[-42.1]", new[] {-42.1})]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] {0, 5, 90, 23, -9, 5.32})]
		public void DecodeArrayOfIntegers(string json, double[] expected)
		{
			var schema = new JSONSchema<double[]>();

			schema.DecoderDescriptor.HasElements(() => 0d, (ref double[] t, IEnumerable<double> e) => t = e.ToArray())
				.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, Array.Empty<double>, json, expected);
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

			schema.DecoderDescriptor.HasValue();

			var decoder = schema.CreateDecoder(() => string.Empty);
			var position = -1;

			decoder.Error += (p, m) => position = p;

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				using (var decoderStream = decoder.Open(stream))
					Assert.IsFalse(decoderStream.TryDecode(out _));

				Assert.AreEqual(expected, position);
			}
		}

		[Test]
		public void DecodeFieldFailsWhenNameDoesNotMatch()
		{
			var schema = new JSONSchema<int>();

			schema.DecoderDescriptor.HasField("match", () => default, (ref int target, int value) => target = value)
				.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => 0, "{\"match1\": 17}", 0);
			JSONSchemaTester.AssertDecodeAndEqual(schema, () => 0, "{\"matc\": 17}", 0);
		}

		[Test]
		public void DecodeFieldFailsWhenScopeDoesNotMatch()
		{
			var schema = new JSONSchema<int>();

			schema.DecoderDescriptor.HasField("match", () => default, (ref int o, int v) => o = v).HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => 0, "{\"unknown\": {\"match\": 17}}", 0);
		}

		[Test]
		public void DecodeFieldFromSameEntity()
		{
			var schema = new JSONSchema<int[]>();
			var root = schema.DecoderDescriptor;

			root.HasField("virtual0", () => 0, (ref int[] target, int source) => target[0] = source).HasValue();
			root.HasField("virtual1", () => 0, (ref int[] target, int source) => target[1] = source).HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => new[] {0, 0}, "{\"virtual0\": 42, \"virtual1\": 17}",
				new[] {42, 17});
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

			schema.DecoderDescriptor.HasField(name, () => default, (ref T o, T v) => o = v).HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => default, json, expected);
		}

		[Test]
		public void DecodeRecursiveSchema()
		{
			var schema = new JSONSchema<RecursiveEntity>();
			var descriptor = schema.DecoderDescriptor;

			descriptor.HasField("f", () => new RecursiveEntity(),
				(ref RecursiveEntity r, RecursiveEntity v) => r.field = v, descriptor);
			descriptor.HasField("v", () => 0, (ref RecursiveEntity r, int v) => r.value = v).HasValue();

			var decoder = schema.CreateDecoder(() => new RecursiveEntity());

			using (var stream =
				new MemoryStream(Encoding.UTF8.GetBytes("{\"f\": {\"f\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")))
			{
				using (var decoderStream = decoder.Open(stream))
				{
					Assert.IsTrue(decoderStream.TryDecode(out var value));

					Assert.AreEqual(42, value.field.field.value);
					Assert.AreEqual(17, value.field.value);
					Assert.AreEqual(3, value.value);
				}
			}
		}

		[Test]
		[TestCase(false, "19", 0)]
		[TestCase(true, "19", 19)]
		[TestCase(true, "\"test\"", "test")]
		public void DecodeValueAsArray<T>(bool acceptValueAsArray, string json, T expected)
		{
			var schema = new JSONSchema<T>(new JSONConfiguration {AcceptValueAsArray = acceptValueAsArray});

			schema.DecoderDescriptor.HasElements(() => default,
				(ref T target, IEnumerable<T> elements) => target = elements.FirstOrDefault()).HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => default, json, expected);
		}

		[Test]
		[TestCase("\"test\"")]
		[TestCase("53")]
		public void DecodeValueFailsWhenTypeDoesNotMatch(string json)
		{
			var schema = new JSONSchema<int>();

			schema.DecoderDescriptor.HasElements(() => string.Empty,
				(ref int target, IEnumerable<string> elements) => target = elements.Count()).HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => default, json, 0);
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
		public void DecodeValueFromNativeConstant<T>(string json, T expected)
		{
			var schema = new JSONSchema<T>();

			schema.DecoderDescriptor.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => default, json, expected);
		}

		[Test]
		public void DecodeValueFromNativeDecimal()
		{
			var schema = new JSONSchema<decimal>();

			schema.DecoderDescriptor.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => default, "1e-28", 1e-28m);
		}

		[Test]
		public void DecodeValueWithCustomConstructor()
		{
			const int fromConstructor = 17;

			var schema = new JSONSchema<Tuple<int, int, int>>();
			var descriptor = schema.DecoderDescriptor;

			var tuple = descriptor.HasField("tuple", () => Tuple.Create(0, 0),
				(ref Tuple<int, int, int> target, Tuple<int, int> field) => target =
					Tuple.Create(field.Item1, field.Item2, target.Item3));

			tuple.HasField("a", () => 0,
					(ref Tuple<int, int> target, int value) => target = Tuple.Create(value, target.Item2))
				.HasValue();

			tuple.HasField("b", () => 0,
					(ref Tuple<int, int> target, int value) => target = Tuple.Create(target.Item1, value))
				.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema,
				() => Tuple.Create(0, 0, fromConstructor),
				"{\"tuple\": {\"a\": 5, \"b\": 7}}", Tuple.Create(5, 7, fromConstructor));
		}

		[Test]
		[TestCase("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
		[TestCase("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
		public void DecodeValueWithCustomDecoder(string json, string expected)
		{
			var schema = new JSONSchema<Guid>();

			schema.SetDecoderConverter(v => Guid.Parse(v.String));
			schema.DecoderDescriptor.HasValue();

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => Guid.Empty, json, Guid.Parse(expected));
		}

		[Test]
		[TestCase("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
		[TestCase("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
		public void DecodeValueWithCustomSetter(string json, string expected)
		{
			var schema = new JSONSchema<Guid>();

			schema.DecoderDescriptor.HasValue((ref Guid target, JSONValue source) =>
				target = Guid.Parse(source.String));

			JSONSchemaTester.AssertDecodeAndEqual(schema, () => Guid.Empty, json, Guid.Parse(expected));
		}

		[Test]
		[TestCase(false, "{\"values\":[{\"value\":null},{\"value\":\"test\"}]}")]
		[TestCase(true, "{\"values\":[{},{\"value\":\"test\"}]}")]
		public void EncodeArrayOfEntities(bool omitNull, string expected)
		{
			var schema = new JSONSchema<JSONValue>(new JSONConfiguration {OmitNull = omitNull});

			schema.EncoderDescriptor
				.HasField("values", v => v)
				.HasElements(v => new[] {JSONValue.Void, v})
				.HasField("value", v => v)
				.HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, JSONValue.FromString("test"), expected);
		}

		[Test]
		[TestCase(new int[0], "[]")]
		[TestCase(new[] {21}, "[21]")]
		[TestCase(new[] {54, 90, -3, 34, 0, 49}, "[54,90,-3,34,0,49]")]
		public void EncodeArrayOfIntegers(int[] value, string expected)
		{
			var schema = new JSONSchema<int[]>();

			schema.EncoderDescriptor.HasElements(entity => entity).HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, value, expected);
		}

		[Test]
		[TestCase(false,
			"{\"firstnull\":null,\"value\":\"test\",\"secondnull\":null,\"item\":{\"values\":[null,\"val1\",null,\"val2\",null]},\"lastnull\":null}")]
		[TestCase(true, "{\"value\":\"test\",\"item\":{\"values\":[\"val1\",\"val2\"]}}")]
		public void EncodeEntityOmitNull(bool omitNull, string expected)
		{
			var schema = new JSONSchema<string>(new JSONConfiguration {OmitNull = omitNull});
			var descriptor = schema.EncoderDescriptor;

			descriptor.HasField("firstnull", s => JSONValue.Void).HasValue();
			descriptor.HasField("value", v => v).HasValue();
			descriptor.HasField("secondnull", s => JSONValue.Void).HasValue();
			descriptor.HasField("item", v => v).HasField("values", v => v)
				.HasElements(v => new[] {null, "val1", null, "val2", null}).HasValue();
			descriptor.HasField("lastnull", s => JSONValue.Void).HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, "test", expected);
		}

		[Test]
		[TestCase("glittering", "prizes", "{\"glittering\":\"prizes\"}")]
		[TestCase("", "pwic", "{\"\":\"pwic\"}")]
		public void EncodeField<T>(string name, T value, string expected)
		{
			var schema = new JSONSchema<T>();

			schema.EncoderDescriptor.HasField(name, v => v).HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, value, expected);
		}

		[Test]
		public void EncodeFieldFromSameEntity()
		{
			var schema = new JSONSchema<int[]>();
			var root = schema.EncoderDescriptor;

			root.HasField("virtual0", source => source[0]).HasValue();
			root.HasField("virtual1", source => source[1]).HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, new[] {42, 17}, "{\"virtual0\":42,\"virtual1\":17}");
		}

		[Test]
		public void EncodeRecursiveDescriptorDefinedAfterUse()
		{
			var schema = new JSONSchema<RecursiveEntity>();
			var descriptor = schema.EncoderDescriptor;

			descriptor.HasField("field", o => o.field, descriptor);
			descriptor.HasField("value", o => o.value).HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema,
				new RecursiveEntity {field = new RecursiveEntity {value = 1}, value = 2},
				"{\"field\":{\"field\":null,\"value\":1},\"value\":2}");
		}

		[Test]
		[TestCase("90f59097-d06a-4796-b8d5-87eb6af7ed8b", "\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"")]
		[TestCase("c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025", "\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"")]
		public void EncodeValueWithCustomEncoder(string guid, string expected)
		{
			var schema = new JSONSchema<Guid>();

			schema.SetEncoderConverter<Guid>(v => JSONValue.FromString(v.ToString()));
			schema.EncoderDescriptor.HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, Guid.Parse(guid), expected);
		}

		[Test]
		[TestCase("90f59097-d06a-4796-b8d5-87eb6af7ed8b", "\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"")]
		[TestCase("c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025", "\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"")]
		public void EncodeValueWithCustomGetter(string guid, string expected)
		{
			var schema = new JSONSchema<Guid>();

			schema.EncoderDescriptor.HasValue(v => JSONValue.FromString(v.ToString()));

			JSONSchemaTester.AssertEncodeAndEqual(schema, Guid.Parse(guid), expected);
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

			schema.EncoderDescriptor.HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, value, expected);
		}

		[Test]
		[TestCase(false, "null")]
		[TestCase(true, "")]
		public void EncodeValueNull(bool omitNull, string expected)
		{
			var schema = new JSONSchema<string>(new JSONConfiguration {OmitNull = omitNull});

			schema.EncoderDescriptor.HasValue();

			JSONSchemaTester.AssertEncodeAndEqual(schema, null, expected);
		}

		[Test]
		public void RoundTripCustomType()
		{
			var schema = new JSONSchema<GuidContainer>();

			schema.SetDecoderConverter(v => Guid.Parse(v.String));
			schema.SetEncoderConverter<Guid>(g => JSONValue.FromString(g.ToString()));

			BaseSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema),
				new GuidContainer
				{
					guid = Guid.NewGuid()
				});
		}

		protected override ISchema<T> CreateSchema<T>()
		{
			return new JSONSchema<T>();
		}

		private static void AssertDecodeAndEqual<T>(ISchema<T> schema, Func<T> constructor, string json, T expected)
		{
			var decoder = schema.CreateDecoder(constructor);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
			{
				using (var decoderStream = decoder.Open(stream))
				{
					Assert.IsTrue(decoderStream.TryDecode(out var value));

					var compare = new CompareLogic();
					var result = compare.Compare(expected, value);

					Assert.That(result.AreEqual, Is.True, result.DifferencesString);
				}
			}
		}

		private static void AssertEncodeAndEqual<T>(ISchema<T> schema, T value, string expected)
		{
			var encoder = schema.CreateEncoder();

			using (var stream = new MemoryStream())
			{
				using (var encoderStream = encoder.Open(stream))
					encoderStream.Encode(value);

				Assert.That(Encoding.UTF8.GetString(stream.ToArray()), Is.EqualTo(expected));
			}
		}

		private class GuidContainer
		{
			public Guid guid { get; set; }
		}

		private class RecursiveEntity
		{
			public RecursiveEntity field;

			public int value;
		}
	}
}
