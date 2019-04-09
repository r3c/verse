using System;
using System.Collections.Generic;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Verse.Test.Schemas
{
	public abstract class BaseSchemaTester
	{
		[Test]
		public virtual void RoundTripNestedArray()
		{
			var schema = this.CreateSchema<NestedArray>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			BaseSchemaTester.AssertRoundTrip(decoder, encoder, new NestedArray
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
						children = new []
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

			BaseSchemaTester.AssertRoundTrip(decoder, encoder, new MixedContainer
			{
				floats = new float[] { 1.1f, 2.2f, 3.3f },
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
		public virtual void RoundTripNestedValue()
		{
			var schema = this.CreateSchema<NestedValue>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			BaseSchemaTester.AssertRoundTrip(decoder, encoder, new NestedValue
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
		public virtual void RoundTripValueNative<T>(T instance)
		{
			var schema = this.CreateSchema<T>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			BaseSchemaTester.AssertRoundTrip(decoder, encoder, instance);
		}

		[Test]
		[Ignore("Nullable types are not handled by linker yet.")]
		public virtual void RoundTripValueNullable()
		{
			var schema = this.CreateSchema<double?>();
			var decoder = Linker.CreateDecoder(schema);
			var encoder = Linker.CreateEncoder(schema);

			BaseSchemaTester.AssertRoundTrip(decoder, encoder, null);
			BaseSchemaTester.AssertRoundTrip(decoder, encoder, 42);
		}

		protected static void AssertRoundTrip<T>(IDecoder<T> decoder, IEncoder<T> encoder, T instance)
		{
			T decoded;
			byte[] encoded1;
			byte[] encoded2;

			using (var stream = new MemoryStream())
			{
				Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
				Assert.IsTrue(encoderStream.Encode(instance));

				encoded1 = stream.ToArray();
			}

			using (var stream = new MemoryStream(encoded1))
			{
				Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
				Assert.IsTrue(decoderStream.Decode(out decoded));
			}

			var comparisonResult = new CompareLogic().Compare(instance, decoded);

			CollectionAssert.IsEmpty(comparisonResult.Differences, $"differences found after decoding entity: {comparisonResult.DifferencesString}");

			using (var stream = new MemoryStream())
			{
				Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
				Assert.IsTrue(encoderStream.Encode(decoded));

				encoded2 = stream.ToArray();
			}

			CollectionAssert.AreEqual(encoded1, encoded2);
		}

		protected abstract ISchema<T> CreateSchema<T>();

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