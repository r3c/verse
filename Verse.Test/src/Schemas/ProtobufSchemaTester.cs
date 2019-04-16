using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoBuf;
using Verse.Schemas;
using Verse.Schemas.Protobuf;

namespace Verse.Test.Schemas
{
	[TestFixture]
	internal class ProtobufSchemaTester
	{
		private const string LongString =
			"Verum ad istam omnem orationem brevis est defensio. " +
			"Nam quoad aetas M. Caeli dare potuit isti suspicioni locum, fuit primum ipsius pudore, " +
			"deinde etiam patris diligentia disciplinaque munita. " +
			"Qui ut huic virilem togam deditšnihil dicam hoc loco de me; " +
			"tantum sit, quantum vos existimatis; hoc dicam, hunc a patre continuo ad me esse deductum; " +
			"nemo hunc M. Caelium in illo aetatis flore vidit nisi aut cum patre aut mecum aut in M. Crassi castissima domo, cum artibus honestissimis erudiretur.";

		[Test]
		//[TestCase("res/Protobuf/Example2.proto", "outer")]
		[TestCase("res/Protobuf/Example3.proto", "outer")]
		[TestCase("res/Protobuf/Person.proto", "Person")]
		public void Decode(string path, string messageName)
		{
			var proto = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, path));
			var schema = new ProtobufSchema<int>(new StringReader(proto), messageName);

			Assert.NotNull(schema);
		}

		private class Person
		{
			public string Email;
			public int Id;
			public string Name;
		}

		[Test]
		[Ignore("Proto messages are not supported yet")]
		public void DecodeAssign()
		{
			var proto = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "res/Protobuf/Person.proto"));
			var schema = new ProtobufSchema<Person>(new StringReader(proto), "Person");

			var person = schema.DecoderDescriptor.IsObject(() => new Person());
			person.HasField("email", (ref Person p, string v) => p.Email = v).IsValue();
			person.HasField("id", (ref Person p, int v) => p.Id = v).IsValue();
			person.HasField("name", (ref Person p, string v) => p.Name = v).IsValue();

			var decoder = schema.CreateDecoder();

			using (var stream = new MemoryStream(new byte[] { 16, 17, 0, 0, 0 }))
			{
				Assert.True(decoder.TryOpen(stream, out var decoderStream));
				Assert.True(decoderStream.Decode(out var entity));

				Assert.AreEqual(17, entity.Id);
			}
		}

		[Test]
		[TestCase(long.MinValue, long.MinValue, long.MinValue)]
		[TestCase(long.MaxValue, long.MaxValue, long.MaxValue)]
		[TestCase(float.MinValue, float.MinValue, float.MinValue)]
		[TestCase(float.MaxValue, float.MaxValue, float.MaxValue)]
		[TestCase(double.MinValue, double.MinValue, double.MinValue)]
		[TestCase(double.MaxValue, double.MaxValue, double.MaxValue)]
		[TestCase("", "", "")]
		[TestCase(ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString)]
		public void DecodeRepeatedScalarFromObject<T>(T a, T b, T c)
		{
			var schema = new ProtobufSchema<List<T>>();
			var testFieldClass = new TestFieldClass<T> {Items = new List<T> {a, b, c}};

			schema.DecoderDescriptor
				.IsObject(() => new List<T>())
				.HasField("_2", (ref List<T> target, T[] values) => target.AddRange(values))
				.IsArray<T>(elements => elements.ToArray())
				.IsValue();

			var value = ProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(), testFieldClass);

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
		[TestCase(ProtobufSchemaTester.LongString)]
		public void DecodeScalarFromNestedObject<T>(T expectedValue)
		{
			var schema = new ProtobufSchema<T>();
			var testFieldClass = new TestFieldClass<T> {SubValue = new SubTestFieldClass<T> {Value = expectedValue}};

			schema.DecoderDescriptor
				.IsObject(() => default)
				.HasField("_3", (ref T t, T v) => t = v)
				.IsObject(() => default)
				.HasField("_4", (ref T t, T v) => t = v)
				.IsValue();

			var value = ProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(), testFieldClass);

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
		[TestCase(ProtobufSchemaTester.LongString, ProtobufType.String)]
		public void DecodeScalarFromObject<T>(T value, ProtobufType type)
		{
			var schema = new ProtobufSchema<T>();
			var testFieldClass = new TestFieldClass<T> {Value = value};

			schema.DecoderDescriptor.IsObject(() => default).HasField("_1", (ref T obj, T v) => obj = v).IsValue();

			var decodedValue = ProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(), testFieldClass);

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
		[TestCase(ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString)]
		public void DecodeScalarFromRepeatedObject<T>(T a, T b, T c)
		{
			T[] expectedValues = { a, b, c };
			var schema = new ProtobufSchema<TestFieldClass<TestFieldClass<T>>>();
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
				.IsObject(() => new TestFieldClass<TestFieldClass<T>>())
				.HasField("_2", (ref TestFieldClass<TestFieldClass<T>> target, TestFieldClass<T>[] value) => target.Items.AddRange(value))
				.IsArray<TestFieldClass<T>>(elements => elements.ToArray())
				.IsObject(() => new TestFieldClass<T>())
				.HasField("_3", (ref TestFieldClass<T> target, SubTestFieldClass<T> value) => target.SubValue = value)
				.IsObject(() => new SubTestFieldClass<T>())
				.HasField("_4", (ref SubTestFieldClass<T> target, T value) => target.Value = value)
				.IsValue();

			var decodedValue = ProtobufSchemaTester.DecodeRoundTrip(schema.CreateDecoder(), testFieldClass);

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
			var schema = new ProtobufSchema<int>();
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
				.IsObject(() => 0)
				.HasField("_2", (ref int t, int v) => t = v);

			if (index0.HasValue)
				descriptor = descriptor.IsObject(() => 0).HasField(index0.Value.ToString(CultureInfo.InvariantCulture),
					(ref int t, int v) => t = v);

			descriptor = descriptor
				.IsObject(() => 0)
				.HasField("_2", (ref int t, int v) => t = v);

			if (index1.HasValue)
				descriptor = descriptor.IsObject(() => 0).HasField(index1.Value.ToString(CultureInfo.InvariantCulture),
					(ref int t, int v) => t = v);

			descriptor
				.IsObject(() => 0)
				.HasField("_4", (ref int t, int v) => t = v)
				.IsValue();

			var value = ProtobufSchemaTester.DecodeTranscode(schema.CreateDecoder(), testFieldClass);

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
		[TestCase(ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString)]
		public void EncodeRepeatedScalarToObject<T>(T a, T b, T c)
		{
			var expectedItems = new[] { a, b, c };
			var schema = new ProtobufSchema<List<T>>();

			schema.EncoderDescriptor
				.IsObject()
				.HasField("_2", v => v)
				.IsArray(source => source)
				.IsValue();

			var testFieldClass = ProtobufSchemaTester.EncodeTranscode<List<T>, TestFieldClass<T>>(schema.CreateEncoder(), new List<T>(expectedItems));

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
		[TestCase(ProtobufSchemaTester.LongString)]
		public void EncodeScalarToNestedObject<T>(T expectedValue)
		{
			var schema = new ProtobufSchema<TestFieldClass<T>>();
			var testFieldClass = new TestFieldClass<T> {SubValue = new SubTestFieldClass<T> {Value = expectedValue}};

			schema.EncoderDescriptor
				.IsObject()
				.HasField("_3", target => target.SubValue)
				.IsObject()
				.HasField("_4", target => target.Value)
				.IsValue();

			var decodedTestFieldClass = ProtobufSchemaTester.EncodeRoundTrip(schema.CreateEncoder(), testFieldClass);

			Assert.AreEqual(expectedValue, decodedTestFieldClass.SubValue.Value);
		}

		[Test]
		[TestCase(long.MinValue, ProtobufType.Signed)]
		[TestCase(long.MaxValue, ProtobufType.Signed)]
		[TestCase(float.MinValue, ProtobufType.Float32)]
		[TestCase(float.MaxValue, ProtobufType.Float32)]
		[TestCase(double.MinValue, ProtobufType.Float64)]
		[TestCase(double.MaxValue, ProtobufType.Float64)]
		[TestCase("", ProtobufType.String)]
		[TestCase(ProtobufSchemaTester.LongString, ProtobufType.String)]
		public void EncodeScalarToObject<T>(T value, ProtobufType type)
		{
			ProtobufValue protoValue;

			ISchema<ProtobufValue> schema = new ProtobufSchema<ProtobufValue>();
			schema.EncoderDescriptor
				.IsObject()
				.HasField("_1", v => v)
				.IsValue();

			switch (type)
			{
				case ProtobufType.Float32:
					protoValue = new ProtobufValue((float)(object)value);
					break;

				case ProtobufType.Float64:
					protoValue = new ProtobufValue((double)(object)value);
					break;

				case ProtobufType.Signed:
					protoValue = new ProtobufValue((long)(object)value);
					break;

				case ProtobufType.String:
					protoValue = new ProtobufValue((string)(object)value);
					break;

				default:
					Assert.Fail("Invalid content type");
					return;
			}

			var testFieldClass =
				ProtobufSchemaTester.EncodeTranscode<ProtobufValue, TestFieldClass<T>>(schema.CreateEncoder(),
					protoValue);

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
		[TestCase(ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString, ProtobufSchemaTester.LongString)]
		public void EncodeScalarToRepeatedObject<T>(T a, T b, T c)
		{
			var fieldClass = new TestFieldClass<TestFieldClass<T>>();
			var schema = new ProtobufSchema<TestFieldClass<TestFieldClass<T>>>();
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
				.IsObject()
				.HasField("_2", v => v)
				.IsArray(source => source.Items)
				.IsObject()
				.HasField("_3", target => target.SubValue)
				.IsObject()
				.HasField("_4", target => target.Value)
				.IsValue();

			var decodedFieldClass = ProtobufSchemaTester.EncodeRoundTrip(schema.CreateEncoder(), fieldClass);

			Assert.AreEqual(expectedValues.Length, decodedFieldClass.Items.Count);

			for (var i = 0; i < expectedValues.Length; ++i)
				Assert.AreEqual(expectedValues[i], decodedFieldClass.Items[i].SubValue.Value);
		}

		private static T DecodeRoundTrip<T>(IDecoder<T> decoder, T input)
		{
			return ProtobufSchemaTester.DecodeTranscode(decoder, input);
		}

		private static U DecodeTranscode<T, U>(IDecoder<U> decoder, T input)
		{
			using (var stream = new MemoryStream())
			{
				Serializer.Serialize(stream, input);

				stream.Seek(0, SeekOrigin.Begin);

				Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
				Assert.IsTrue(decoderStream.Decode(out var output));

				return output;
			}
		}

		private static T EncodeRoundTrip<T>(IEncoder<T> encoder, T input)
		{
			return ProtobufSchemaTester.EncodeTranscode<T, T>(encoder, input);
		}

		private static U EncodeTranscode<T, U>(IEncoder<T> encoder, T input)
		{
			using (var stream = new MemoryStream())
			{
				Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
				Assert.IsTrue(encoderStream.Encode(input));

				stream.Seek(0, SeekOrigin.Begin);

				return Serializer.Deserialize<U>(stream);
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
				return Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
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
