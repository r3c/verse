using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Verse.Test.Schemas
{
	public abstract class SchemaTester<TNative>
	{
		[Test]
		public void RoundTripFieldFlatten()
		{
			var schema = this.CreateSchema<int>();

			schema.DecoderDescriptor.HasField("virtual").HasField("value", () => 0,
				(ref int target, int source) => target = source).HasValue(schema.DecoderAdapter.ToInteger32S);
			schema.EncoderDescriptor.HasField("virtual").HasField("value", source => source)
				.HasValue(schema.EncoderAdapter.FromInteger32S);

			SchemaTester<TNative>.AssertRoundTrip(schema.CreateDecoder(() => 0), schema.CreateEncoder(), 17);
		}

		[Test]
		public void RoundTripNestedArray()
		{
			var schema = this.CreateSchema<NestedArray>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, new NestedArray
			{
				children = new[]
				{
					new NestedArray
					{
						children = Array.Empty<NestedArray>(),
						value = "a"
					},
					new NestedArray
					{
						children = new[]
						{
							new NestedArray
							{
								children = Array.Empty<NestedArray>(),
								value = "b"
							},
							new NestedArray
							{
								children = Array.Empty<NestedArray>(),
								value = "c"
							}
						},
						value = "d"
					},
					new NestedArray
					{
						children = Array.Empty<NestedArray>(),
						value = "e"
					}
				},
				value = "f"
			});
		}

		[Test]
		[Ignore("Enum types are not handled by linker yet.")]
		public void RoundTripMixedTypes()
		{
			var schema = this.CreateSchema<MixedContainer>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, new MixedContainer
			{
				floats = new[] {1.1f, 2.2f, 3.3f},
				integer = 17,
				option = SomeEnum.B,
				pairs = new Dictionary<string, string>
				{
					{"a", "aaa"},
					{"b", "bbb"}
				},
				text = "Hello, World!"
			});
		}

		[Test]
		public void RoundTripNestedValue()
		{
			var schema = this.CreateSchema<NestedValue>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, new NestedValue
			{
				child = new NestedValue
				{
					child = new NestedValue
					{
						child = null,
						value = 64
					},
					value = 42
				},
				value = 17
			});
		}

		[Test]
		[TestCase("Hello")]
		[TestCase(25.5)]
		public void RoundTripValueNative<T>(T instance)
		{
			var schema = this.CreateSchema<T>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, instance);
		}

		[Test]
		public void RoundTripValueNullableField()
		{
			var schema = this.CreateSchema<Container<double?>>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, new Container<double?>());
			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, new Container<double?> {Value = 42});
		}

		[Test]
		public void RoundTripValueNullableValue()
		{
			var schema = this.CreateSchema<double?>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, null);
			SchemaTester<TNative>.AssertRoundTrip(decoder, encoder, 42);
		}

		protected static void AssertRoundTrip<T>(IDecoder<T> decoder, IEncoder<T> encoder, T instance)
		{
			T decoded;
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

		protected abstract ISchema<TNative, TEntity> CreateSchema<TEntity>();

		private struct Container<T>
		{
			public T Value;
		}

		private class MixedContainer
		{
			public float[] floats;
			public short integer;
			public SomeEnum option;
			public Dictionary<string, string> pairs;
			public string text;
		}

		private class NestedArray
		{
			public NestedArray[] children;
			public string value;
		}

		private class NestedValue
		{
			public NestedValue child;
			public int value;
		}

		private enum SomeEnum
		{
			A,
			B,
			C
		}
	}
}
