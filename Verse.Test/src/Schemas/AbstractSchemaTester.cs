using System;
using System.Collections.Generic;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Verse.Test.Schemas
{
    public abstract class AbstractSchemaTester
    {
		[Test]
		[Ignore("Arrays are always built before reading, so decoding 'null' would still create an empty array")]
		public virtual void RoundTripNestedArray()
		{
			ISchema<NestedArray> schema = this.CreateSchema<NestedArray>();

			AbstractSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema), new NestedArray
			{
				children = new []
				{
					new NestedArray
					{
						children = null,
						value = "a"
					},
					new NestedArray
					{
						children = new []
						{
							new NestedArray
							{
								children = null,
								value = "b"
							},
							new NestedArray
							{
								children = null,
								value = "c"
							}
						},
						value = "d"
					},
					new NestedArray
					{
						children = new NestedArray[0],
						value = "e"
					}
				},
				value = "f"
			});
		}

		[Test]
		[Ignore("Enum types are not handled by linker yet.")]
		public void	RoundTripMixedTypes()
		{
			ISchema<MixedContainer> schema = this.CreateSchema<MixedContainer>();

			AbstractSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema), new MixedContainer
			{
				floats		= new float[] {1.1f, 2.2f, 3.3f},
				integer		= 17,
				option		= SomeEnum.B,
				pairs		= new Dictionary<string, string>
				{
					{"a", "aaa"},
					{"b", "bbb"}
				},
				text		= "Hello, World!"
			});
		}

		[Test]
		public virtual void RoundTripNestedValue()
		{
			ISchema<NestedValue> schema = this.CreateSchema<NestedValue>();

			AbstractSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema), new NestedValue
			{
				child	= new NestedValue
				{
					child	= new NestedValue
					{
						child	= null,
						value	= 64
					},
					value	= 42
				},
				value	= 17
			});
		}

		[Test]
		[TestCase("Hello")]
		[TestCase(25.5)]
        public virtual void RoundTripValueNative<T>(T instance)
        {
            ISchema<T> schema = this.CreateSchema<T>();

            AbstractSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema), instance);
        }

		[Test]
		[Ignore("Nullable types are not handled by linker yet.")]
		public virtual void RoundTripValueNullable()
		{
			ISchema<double?> schema = this.CreateSchema<double?>();

			AbstractSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema), null);
			AbstractSchemaTester.AssertRoundTrip(Linker.CreateDecoder(schema), Linker.CreateEncoder(schema), 42);
		}

		protected static void AssertRoundTrip<T>(IDecoder<T> decoder, IEncoder<T> encoder, T instance)
		{
			T decoded;
			byte[] first;
			byte[] second;
			Type type = typeof(T);

			using (var stream = new MemoryStream())
			{
				Assert.IsTrue(encoder.Encode(instance, stream));

				first = stream.ToArray();
			}

			using (var stream = new MemoryStream(first))
			{
				decoded = type.IsValueType || type == typeof(string) ? default(T) : (T)Activator.CreateInstance(type);

				Assert.IsTrue(decoder.Decode(stream, out decoded));
			}

			CollectionAssert.IsEmpty(new CompareLogic().Compare(instance, decoded).Differences);

			using (var stream = new MemoryStream())
			{
				Assert.IsTrue(encoder.Encode(decoded, stream));

				second = stream.ToArray();
			}

			CollectionAssert.AreEqual(first, second);
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