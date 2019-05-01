using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoBuf;
using Verse.Schemas;
using Verse.Schemas.Protobuf;
using Verse.Schemas.RawProtobuf;

namespace Verse.Test.Schemas
{
	[TestFixture]
	internal class RawProtobufSchemaTester
	{
		private const string LongString =
			"Verum ad istam omnem orationem brevis est defensio. " +
			"Nam quoad aetas M. Caeli dare potuit isti suspicioni locum, fuit primum ipsius pudore, " +
			"deinde etiam patris diligentia disciplinaque munita. " +
			"Qui ut huic virilem togam deditšnihil dicam hoc loco de me; " +
			"tantum sit, quantum vos existimatis; hoc dicam, hunc a patre continuo ad me esse deductum; " +
			"nemo hunc M. Caelium in illo aetatis flore vidit nisi aut cum patre aut mecum aut in M. Crassi castissima domo, cum artibus honestissimis erudiretur.";

		[Test]
		[TestCase(long.MinValue, long.MinValue, long.MinValue)]
		[TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
		[TestCase(float.MinValue, float.MinValue, float.MinValue)]
		[TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
		[TestCase(double.MinValue, double.MinValue, double.MinValue)]
		[TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
		[TestCase("", "", "")]
		[TestCase(RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString)]
		public void DecodeRepeatedScalarFromObject<T>(T a, T b, T c)
		{
			var schema = RawProtobufSchemaTester.CreateSchema<List<T>>();
			var testFieldClass = new TestFieldClass<T> {Items = new List<T> {a, b, c}};

			schema.DecoderDescriptor
				.HasField("_2", Array.Empty<T>, (ref List<T> target, T[] values) => target.AddRange(values))
				.HasElements(() => default, (ref T[] target, IEnumerable<T> elements) => target = elements.ToArray())
				.HasValue();

			var value = RawProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(() => new List<T>()),
				testFieldClass);

			CollectionAssert.AreEqual(testFieldClass.Items, value);
		}

		[Test]
		[TestCase(long.MinValue)]
		[TestCase(long.MaxValue)]
		[TestCase(float.MinValue)]
		[TestCase(float.MaxValue)]
		[TestCase(double.MinValue)]
		[TestCase(double.MaxValue)]
		[TestCase("")]
		[TestCase(RawProtobufSchemaTester.LongString)]
		public void DecodeScalarFromNestedObject<T>(T expectedValue)
		{
			var schema = RawProtobufSchemaTester.CreateSchema<T>();
			var testFieldClass = new TestFieldClass<T> {SubValue = new SubTestFieldClass<T> {Value = expectedValue}};

			schema.DecoderDescriptor
				.HasField("_3", () => default, (ref T t, T v) => t = v)
				.HasField("_4", () => default, (ref T t, T v) => t = v)
				.HasValue();

			var value = RawProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(() => default), testFieldClass);

			Assert.AreEqual(expectedValue, value);
		}

		[Test]
		[TestCase(long.MinValue, ProtobufType.Signed)]
		[TestCase(long.MaxValue, ProtobufType.Signed)]
		[TestCase(float.MinValue, ProtobufType.Float32)]
		[TestCase(float.MaxValue, ProtobufType.Float32)]
		[TestCase(0f, ProtobufType.Float32)]
		[TestCase(double.MinValue, ProtobufType.Float64)]
		[TestCase(double.MaxValue, ProtobufType.Float64)]
		[TestCase(0.0, ProtobufType.Float64)]
		[TestCase("", ProtobufType.String)]
		[TestCase(RawProtobufSchemaTester.LongString, ProtobufType.String)]
		public void DecodeScalarFromObject<T>(T value, ProtobufType type)
		{
			var schema = RawProtobufSchemaTester.CreateSchema<T>();
			var testFieldClass = new TestFieldClass<T> {Value = value};

			schema.DecoderDescriptor.HasField("_1", () => default, (ref T obj, T v) => obj = v).HasValue();

			var decodedValue = RawProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(() => default), testFieldClass);

			Assert.AreEqual(value, decodedValue);
		}

		[Test]
		[TestCase(long.MinValue, long.MinValue, long.MinValue)]
		[TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
		[TestCase(float.MinValue, float.MinValue, float.MinValue)]
		[TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
		[TestCase(double.MinValue, double.MinValue, double.MinValue)]
		[TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
		[TestCase("", "", "")]
		[TestCase(RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString)]
		public void DecodeScalarFromRepeatedObject<T>(T a, T b, T c)
		{
			T[] expectedValues = { a, b, c };
			var schema = RawProtobufSchemaTester.CreateSchema<TestFieldClass<TestFieldClass<T>>>();
			var testFieldClass = new TestFieldClass<TestFieldClass<T>>();

			foreach (var value in expectedValues)
			{
				testFieldClass.Items.Add(new TestFieldClass<T>
				{
					SubValue = new SubTestFieldClass<T>
					{
						Value = value
					}
				});
			}

			schema.DecoderDescriptor
				.HasField("_2", () => default,
					(ref TestFieldClass<TestFieldClass<T>> target, TestFieldClass<T>[] value) =>
						target.Items.AddRange(value))
				.HasElements(() => new TestFieldClass<T>(),
					(ref TestFieldClass<T>[] target, IEnumerable<TestFieldClass<T>> elements) =>
						target = elements.ToArray())
				.HasField("_3", () => new SubTestFieldClass<T>(),
					(ref TestFieldClass<T> target, SubTestFieldClass<T> value) => target.SubValue = value)
				.HasField("_4", () => default, (ref SubTestFieldClass<T> target, T value) => target.Value = value)
				.HasValue();

			var decodedValue =
				RawProtobufSchemaTester.DecodeRoundTrip(
					schema.CreateDecoder(() => new TestFieldClass<TestFieldClass<T>>()), testFieldClass);

			Assert.AreEqual(expectedValues.Length, decodedValue.Items.Count);

			for (var i = 0; i < expectedValues.Length; ++i)
			{
				Assert.AreEqual(expectedValues[i], decodedValue.Items[i].SubValue.Value);
			}
		}

		[Test]
		[TestCase(0, 0, 10)]
		[TestCase(0, 1, 20)]
		[TestCase(0, 2, 30)]
		[TestCase(1, 0, 40)]
		[TestCase(1, 1, 50)]
		[TestCase(1, 2, 60)]
		[TestCase(2, 0, 70)]
		[TestCase(2, 1, 80)]
		[TestCase(2, 2, 90)]
		[TestCase(0, null, 10)]
		[TestCase(1, null, 40)]
		[TestCase(2, null, 70)]
		[TestCase(null, 0, 10)]
		[TestCase(null, 1, 20)]
		[TestCase(null, 2, 30)]
		[TestCase(null, null, 10)]
		[Ignore("This feature is currently not supported")]
		public void DecodeFromNestedFixedIndex(int? index0, int? index1, int expected)
		{
			var schema = RawProtobufSchemaTester.CreateSchema<int>();
			var testFieldClass = new TestFieldClass<TestFieldClass<SubTestFieldClass<int>>>
			{
				Items = new List<TestFieldClass<SubTestFieldClass<int>>>
				{
					new TestFieldClass<SubTestFieldClass<int>>
					{
						Items = new List<SubTestFieldClass<int>>
						{
							new SubTestFieldClass<int> {Value = 10},
							new SubTestFieldClass<int> {Value = 20},
							new SubTestFieldClass<int> {Value = 30}
						}
					},
					new TestFieldClass<SubTestFieldClass<int>>
					{
						Items = new List<SubTestFieldClass<int>>
						{
							new SubTestFieldClass<int> {Value = 40},
							new SubTestFieldClass<int> {Value = 50},
							new SubTestFieldClass<int> {Value = 60}
						}
					},
					new TestFieldClass<SubTestFieldClass<int>>
					{
						Items = new List<SubTestFieldClass<int>>
						{
							new SubTestFieldClass<int> {Value = 70},
							new SubTestFieldClass<int> {Value = 80},
							new SubTestFieldClass<int> {Value = 90}
						}
					}
				}
			};

			var descriptor = schema.DecoderDescriptor
				.HasField("_2", () => default, (ref int t, int v) => t = v);

			if (index0.HasValue)
				descriptor = descriptor.HasField(index0.Value.ToString(CultureInfo.InvariantCulture), () => default,
					(ref int t, int v) => t = v);

			descriptor = descriptor.HasField("_2", () => default, (ref int t, int v) => t = v);

			if (index1.HasValue)
				descriptor = descriptor.HasField(index1.Value.ToString(CultureInfo.InvariantCulture), () => default,
					(ref int t, int v) => t = v);

			descriptor
				.HasField("_4", () => default, (ref int t, int v) => t = v)
				.HasValue();

			var value = RawProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(() => 0), testFieldClass);

			Assert.AreEqual(expected, value);
		}

		[Test]
		[TestCase(long.MinValue, long.MinValue, long.MinValue)]
		[TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
		[TestCase(float.MinValue, float.MinValue, float.MinValue)]
		[TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
		[TestCase(double.MinValue, double.MinValue, double.MinValue)]
		[TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
		[TestCase("", "", "")]
		[TestCase(RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString)]
		public void EncodeRepeatedScalarToObject<T>(T a, T b, T c)
		{
			var expectedItems = new[] { a, b, c };
			var schema = RawProtobufSchemaTester.CreateSchema<List<T>>();

			schema.EncoderDescriptor
				.HasField("_2", v => v)
				.HasElements(source => source)
				.HasValue();

			var testFieldClass = RawProtobufSchemaTester.EncodeTranscode<List<T>, TestFieldClass<T>>(schema.CreateEncoder(), new List<T>(expectedItems));

			CollectionAssert.AreEqual(expectedItems, testFieldClass.Items);
		}

		[Test]
		[TestCase(long.MinValue)]
		[TestCase(long.MaxValue)]
		[TestCase(float.MinValue)]
		[TestCase(float.MaxValue)]
		[TestCase(double.MinValue)]
		[TestCase(double.MaxValue)]
		[TestCase("")]
		[TestCase(RawProtobufSchemaTester.LongString)]
		public void EncodeScalarToNestedObject<T>(T expectedValue)
		{
			var schema = RawProtobufSchemaTester.CreateSchema<TestFieldClass<T>>();
			var testFieldClass = new TestFieldClass<T> {SubValue = new SubTestFieldClass<T> {Value = expectedValue}};

			schema.EncoderDescriptor
				.HasField("_3", target => target.SubValue)
				.HasField("_4", target => target.Value)
				.HasValue();

			var decodedTestFieldClass = RawProtobufSchemaTester.EncodeRoundTrip(schema.CreateEncoder(), testFieldClass);

			Assert.AreEqual(expectedValue, decodedTestFieldClass.SubValue.Value);
		}

		[Test]
		[TestCase(int.MinValue)]
		[TestCase(int.MaxValue)]
		[TestCase(long.MinValue)]
		[TestCase(long.MaxValue)]
		[TestCase(float.MinValue)]
		[TestCase(float.MaxValue)]
		[TestCase(double.MinValue)]
		[TestCase(double.MaxValue)]
		[TestCase("")]
		[TestCase(RawProtobufSchemaTester.LongString)]
		public void EncodeScalarToObject<T>(T value)
		{
			var schema = RawProtobufSchemaTester.CreateSchema<T>();

			schema.EncoderDescriptor
				.HasField("_1", v => v)
				.HasValue();

			var testFieldClass =
				RawProtobufSchemaTester.EncodeTranscode<T, TestFieldClass<T>>(schema.CreateEncoder(),
					value);

			Assert.AreEqual(value, testFieldClass.Value);
		}

		[Test]
		[TestCase(long.MinValue, long.MinValue, long.MinValue)]
		[TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
		[TestCase(float.MinValue, float.MinValue, float.MinValue)]
		[TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
		[TestCase(double.MinValue, double.MinValue, double.MinValue)]
		[TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
		[TestCase("", "", "")]
		[TestCase(RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString, RawProtobufSchemaTester.LongString)]
		public void EncodeScalarToRepeatedObject<T>(T a, T b, T c)
		{
			var fieldClass = new TestFieldClass<TestFieldClass<T>>();
			var schema = RawProtobufSchemaTester.CreateSchema<TestFieldClass<TestFieldClass<T>>>();
			var expectedValues = new[] { a, b, c };

			foreach (var value in expectedValues)
			{
				fieldClass.Items.Add(new TestFieldClass<T>
				{
					SubValue = new SubTestFieldClass<T>
					{
						Value = value
					}
				});
			}

			schema.EncoderDescriptor
				.HasField("_2", v => v)
				.HasElements(source => source.Items)
				.HasField("_3", target => target.SubValue)
				.HasField("_4", target => target.Value)
				.HasValue();

			var decodedFieldClass = RawProtobufSchemaTester.EncodeRoundTrip(schema.CreateEncoder(), fieldClass);

			Assert.AreEqual(expectedValues.Length, decodedFieldClass.Items.Count);

			for (var i = 0; i < expectedValues.Length; ++i)
				Assert.AreEqual(expectedValues[i], decodedFieldClass.Items[i].SubValue.Value);
		}

		private static ISchema<TEntity> CreateSchema<TEntity>()
		{
			return new RawProtobufSchema<TEntity>(new RawProtobufConfiguration {NoZigZagEncoding = true});
		}

		private static T DecodeRoundTrip<T>(IDecoder<T> decoder, T input)
		{
			return RawProtobufSchemaTester.DecodeTranscode(decoder, input);
		}

		private static TOutput DecodeTranscode<TInput, TOutput>(IDecoder<TOutput> decoder, TInput input)
		{
			using (var stream = new MemoryStream())
			{
				Serializer.Serialize(stream, input);

				stream.Seek(0, SeekOrigin.Begin);

				using (var decoderStream = decoder.Open(stream))
				{
					Assert.IsTrue(decoderStream.TryDecode(out var output));

					return output;
				}
			}
		}

		private static T EncodeRoundTrip<T>(IEncoder<T> encoder, T input)
		{
			return RawProtobufSchemaTester.EncodeTranscode<T, T>(encoder, input);
		}

		private static TOutput EncodeTranscode<TInput, TOutput>(IEncoder<TInput> encoder, TInput input)
		{
			using (var stream = new MemoryStream())
			{
				using (var encoderStream = encoder.Open(stream))
					encoderStream.Encode(input);

				stream.Seek(0, SeekOrigin.Begin);

				return Serializer.Deserialize<TOutput>(stream);
			}
		}

		[Serializable, ProtoContract(Name = @"SubTestFieldClass")]
		public class SubTestFieldClass<T> : IExtensible
		{
			private T value;

			[ProtoMember(4, IsRequired = true)]
			public T Value
			{
				get => this.value;
				set => this.value = value;
			}

			private IExtension extensionObject;

			IExtension IExtensible.GetExtensionObject(bool createIfMissing)
			{
				return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
			}
		}

		[Serializable, ProtoContract(Name = @"TestFieldClass")]
		public class TestFieldClass<T> : IExtensible
		{
			private T value;

			[ProtoMember(1, IsRequired = true)]
			public T Value
			{
				get => this.value;
				set => this.value = value;
			}

			private List<T> items = new List<T>();

			[ProtoMember(2, IsRequired = true, DataFormat = DataFormat.Default)]
			public List<T> Items
			{
				get => this.items;
				set => this.items = value;
			}

			private SubTestFieldClass<T> subValue;

			[ProtoMember(3, IsRequired = true)]
			public SubTestFieldClass<T> SubValue
			{
				get => this.subValue;
				set => this.subValue = value;
			}

			private IExtension extensionObject;
			IExtension IExtensible.GetExtensionObject(bool createIfMissing)
			{
				return Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
			}
		}
	}
}
