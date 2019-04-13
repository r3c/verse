using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using ProtoBuf;
using Verse.Schemas.Protobuf;
using Verse.Schemas;

namespace Verse.Test.Schemas
{
	[TestFixture]
	class ProtobufSchemaTester
	{
		private const long MIN_LONG = 0;
		private const long MAX_LONG = 9223372036854775807;
		private const float MIN_FLOAT = -3.40282347E+38f;
		private const float MAX_FLOAT = 3.40282347E+38f;
		private const double MIN_DOUBLE = -1.7976931348623157E+308;
		private const double MAX_DOUBLE = 1.7976931348623157E+308;
		private const string LONG_STRING = "Verum ad istam omnem orationem brevis est defensio. Nam quoad aetas M. Caeli dare potuit isti suspicioni locum, fuit primum ipsius pudore, deinde etiam patris diligentia disciplinaque munita. Qui ut huic virilem togam deditšnihil dicam hoc loco de me; tantum sit, quantum vos existimatis; hoc dicam, hunc a patre continuo ad me esse deductum; nemo hunc M. Caelium in illo aetatis flore vidit nisi aut cum patre aut mecum aut in M. Crassi castissima domo, cum artibus honestissimis erudiretur.";

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

		class Person
		{
			public string Email;
			public int Id;
			public string Name;
		}

		[Test]
		public void DecodeAssign()
		{
			var proto = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "res/Protobuf/Person.proto"));
			var schema = new ProtobufSchema<Person>(new StringReader(proto), "Person");

			schema.DecoderDescriptor.HasField("email", (ref Person p, string v) => p.Email = v).IsValue();
			schema.DecoderDescriptor.HasField("id", (ref Person p, int v) => p.Id = v).IsValue();
			schema.DecoderDescriptor.HasField("name", (ref Person p, string v) => p.Name = v).IsValue();

			var decoder = schema.CreateDecoder();

			using (var stream = new MemoryStream(new byte[] { 16, 17, 0, 0, 0 }))
			{
				Assert.True(decoder.TryOpen(stream, out var decoderStream));
				Assert.True(decoderStream.Decode(out var person));

				Assert.AreEqual(17, person.Id);
			}
		}

		[Test]
		[TestCase(MIN_LONG, ProtobufType.Signed)]
		[TestCase(MAX_LONG, ProtobufType.Signed)]
		[TestCase(MIN_FLOAT, ProtobufType.Float32)]
		[TestCase(MAX_FLOAT, ProtobufType.Float32)]
		[TestCase(0f, ProtobufType.Float32)]
		[TestCase(MIN_DOUBLE, ProtobufType.Float64)]
		[TestCase(MAX_DOUBLE, ProtobufType.Float64)]
		[TestCase(0.0, ProtobufType.Float64)]
		[TestCase("", ProtobufType.String)]
		[TestCase(LONG_STRING, ProtobufType.String)]
		public void TestDecodeValue<T>(T value, ProtobufType type)
		{
			ProtobufValue decodedValue;
			var schema = new ProtobufSchema<ProtobufValue>();
			var testFieldClass = new TestFieldClass<T>();

			testFieldClass.value = value;

			schema.DecoderDescriptor.HasField("_1").IsValue();

			decodedValue = ProtobufSchemaTester.DecodeTranscode<TestFieldClass<T>, ProtobufValue>(schema.CreateDecoder(), testFieldClass);

			Assert.AreEqual(type, decodedValue.Type);

			switch (type)
			{
				case ProtobufType.Float32:
					Assert.AreEqual(value, decodedValue.Float32);
					break;

				case ProtobufType.Float64:
					Assert.AreEqual(value, decodedValue.Float64);
					break;

				case ProtobufType.Signed:
					Assert.AreEqual(value, decodedValue.Signed);
					break;

				case ProtobufType.String:
					Assert.AreEqual(value, decodedValue.String);
					break;

				default:
					Assert.Fail("Invalid content type");
					break;
			}
		}

		[Test]
		[TestCase(MIN_LONG, MIN_LONG, MIN_LONG)]
		[TestCase(MAX_LONG, MAX_LONG, MAX_LONG)]
		[TestCase(MIN_FLOAT, MIN_FLOAT, MIN_FLOAT)]
		[TestCase(MAX_FLOAT, MAX_FLOAT, MAX_FLOAT)]
		[TestCase(MIN_DOUBLE, MIN_DOUBLE, MIN_DOUBLE)]
		[TestCase(MAX_DOUBLE, MAX_DOUBLE, MAX_DOUBLE)]
		[TestCase("", "", "")]
		[TestCase(LONG_STRING, LONG_STRING, LONG_STRING)]
		public void TestDecodeValues<T>(T a, T b, T c)
		{
			var schema = new ProtobufSchema<List<T>>();
			var testFieldClass = new TestFieldClass<T>();
			List<T> value;

			testFieldClass.items = new List<T> { a, b, c };

			schema.DecoderDescriptor
				.HasField("_2", (ref List<T> target, List<T> values) => target.AddRange(values))
				.HasItems((ref List<T> target, IEnumerable<T> enumerable) => target.AddRange(enumerable))
				.IsValue();

			value = ProtobufSchemaTester.DecodeTranscode<TestFieldClass<T>, List<T>>(schema.CreateDecoder(), testFieldClass);

			CollectionAssert.AreEqual(testFieldClass.items, value);
		}

		[Test]
		[TestCase(MIN_LONG)]
		[TestCase(MAX_LONG)]
		[TestCase(MIN_FLOAT)]
		[TestCase(MAX_FLOAT)]
		[TestCase(MIN_DOUBLE)]
		[TestCase(MAX_DOUBLE)]
		[TestCase("")]
		[TestCase(LONG_STRING)]
		public void TestDecodeSubValue<T>(T expectedValue)
		{
			var schema = new ProtobufSchema<T>();
			var testFieldClass = new TestFieldClass<T>();
			T value;

			testFieldClass.subValue = new SubTestFieldClass<T>();
			testFieldClass.subValue.value = expectedValue;

			schema.DecoderDescriptor
				.HasField("_3")
				.HasField("_4")
				.IsValue();

			value = ProtobufSchemaTester.DecodeTranscode<TestFieldClass<T>, T>(schema.CreateDecoder(), testFieldClass);

			Assert.AreEqual(expectedValue, value);
		}

		[Test]
		[TestCase(MIN_LONG, MIN_LONG, MIN_LONG)]
		[TestCase(MAX_LONG, MAX_LONG, MAX_LONG)]
		[TestCase(MIN_FLOAT, MIN_FLOAT, MIN_FLOAT)]
		[TestCase(MAX_FLOAT, MAX_FLOAT, MAX_FLOAT)]
		[TestCase(MIN_DOUBLE, MIN_DOUBLE, MIN_DOUBLE)]
		[TestCase(MAX_DOUBLE, MAX_DOUBLE, MAX_DOUBLE)]
		[TestCase("", "", "")]
		[TestCase(LONG_STRING, LONG_STRING, LONG_STRING)]
		public void TestDecodeSubValues<T>(T a, T b, T c)
		{
			TestFieldClass<TestFieldClass<T>> decodedValue;
			T[] expectedValues = { a, b, c };
			var schema = new ProtobufSchema<TestFieldClass<TestFieldClass<T>>>();
			var testFieldClass = new TestFieldClass<TestFieldClass<T>>();

			foreach (var value in expectedValues)
			{
				testFieldClass.items.Add(new TestFieldClass<T>
				{
					subValue = new SubTestFieldClass<T>
					{
						value = value
					}
				});
			}

			schema.DecoderDescriptor
				.HasField("_2", (ref TestFieldClass<TestFieldClass<T>> target, List<TestFieldClass<T>> value) => target.items.AddRange(value))
				.HasItems((ref List<TestFieldClass<T>> target, IEnumerable<TestFieldClass<T>> value) => target.AddRange(value))
				.HasField("_3", (ref TestFieldClass<T> target, SubTestFieldClass<T> value) => target.subValue = value)
				.HasField("_4", (ref SubTestFieldClass<T> target, T value) => target.value = value)
				.IsValue();

			decodedValue = ProtobufSchemaTester.DecodeRoundTrip(schema.CreateDecoder(), testFieldClass);

			Assert.AreEqual(expectedValues.Length, decodedValue.items.Count);

			for (int i = 0; i < expectedValues.Length; ++i)
			{
				Assert.AreEqual(expectedValues[i], decodedValue.items[i].subValue.value);
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
		public void TestDecodeSubObjectIndex(int? index0, int? index1, int expected)
		{
			IDecoderDescriptor<int> descriptor;
			var schema = new ProtobufSchema<int>();
			var testFieldClass = new TestFieldClass<TestFieldClass<SubTestFieldClass<int>>>();
			int value;

			testFieldClass.items = new List<TestFieldClass<SubTestFieldClass<int>>>
			{
				new TestFieldClass<SubTestFieldClass<int>>
				{
					items = new List<SubTestFieldClass<int>>
					{
						new SubTestFieldClass<int>
						{
							value = 10
						},
						new SubTestFieldClass<int>
						{
							value = 20
						},
						new SubTestFieldClass<int>
						{
							value = 30
						}
					}
				},
				new TestFieldClass<SubTestFieldClass<int>>
				{
					items = new List<SubTestFieldClass<int>>
					{
						new SubTestFieldClass<int>
						{
							value = 40
						},
						new SubTestFieldClass<int>
						{
							value = 50
						},
						new SubTestFieldClass<int>
						{
							value = 60
						}
					}
				},
				new TestFieldClass<SubTestFieldClass<int>>
				{
					items = new List<SubTestFieldClass<int>>
					{
						new SubTestFieldClass<int>
						{
							value = 70
						},
						new SubTestFieldClass<int>
						{
							value = 80
						},
						new SubTestFieldClass<int>
						{
							value = 90
						}
					}
				},
			};

			descriptor = schema.DecoderDescriptor
				.HasField("_2");

			if (index0.HasValue)
				descriptor = descriptor.HasField(index0.Value.ToString(CultureInfo.InvariantCulture));

			descriptor = descriptor
				.HasField("_2");

			if (index1.HasValue)
				descriptor = descriptor.HasField(index1.Value.ToString(CultureInfo.InvariantCulture));

			descriptor
				.HasField("_4")
				.IsValue();

			value = ProtobufSchemaTester.DecodeTranscode<TestFieldClass<TestFieldClass<SubTestFieldClass<int>>>, int>(schema.CreateDecoder(), testFieldClass);

			Assert.AreEqual(expected, value);
		}

		[Test]
		[TestCase(MIN_LONG, ProtobufType.Signed)]
		[TestCase(MAX_LONG, ProtobufType.Signed)]
		[TestCase(MIN_FLOAT, ProtobufType.Float32)]
		[TestCase(MAX_FLOAT, ProtobufType.Float32)]
		[TestCase(MIN_DOUBLE, ProtobufType.Float64)]
		[TestCase(MAX_DOUBLE, ProtobufType.Float64)]
		[TestCase("", ProtobufType.String)]
		[TestCase(LONG_STRING, ProtobufType.String)]
		public void TestEncodeValue<T>(T value, ProtobufType type)
		{
			ProtobufValue protoValue;
			ISchema<ProtobufValue> schema;
			TestFieldClass<T> testFieldClass;

			schema = new ProtobufSchema<ProtobufValue>();
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

			testFieldClass = ProtobufSchemaTester.EncodeTranscode<ProtobufValue, TestFieldClass<T>>(schema.CreateEncoder(), protoValue);

			Assert.AreEqual(value, testFieldClass.value);
		}

		[Test]
		[TestCase(MIN_LONG, MIN_LONG, MIN_LONG)]
		[TestCase(MAX_LONG, MAX_LONG, MAX_LONG)]
		[TestCase(MIN_FLOAT, MIN_FLOAT, MIN_FLOAT)]
		[TestCase(MAX_FLOAT, MAX_FLOAT, MAX_FLOAT)]
		[TestCase(MIN_DOUBLE, MIN_DOUBLE, MIN_DOUBLE)]
		[TestCase(MAX_DOUBLE, MAX_DOUBLE, MAX_DOUBLE)]
		[TestCase("", "", "")]
		[TestCase(LONG_STRING, LONG_STRING, LONG_STRING)]
		public void TestEncodeValues<T>(T a, T b, T c)
		{
			var expectedItems = new[] { a, b, c };
			var schema = new ProtobufSchema<List<T>>();
			TestFieldClass<T> testFieldClass;

			schema.EncoderDescriptor
				.IsObject()
				.HasField("_2", v => v)
				.IsArray(source => source)
				.IsValue();

			testFieldClass = ProtobufSchemaTester.EncodeTranscode<List<T>, TestFieldClass<T>>(schema.CreateEncoder(), new List<T>(expectedItems));

			CollectionAssert.AreEqual(expectedItems, testFieldClass.items);
		}

		[Test]
		[TestCase(MIN_LONG)]
		[TestCase(MAX_LONG)]
		[TestCase(MIN_FLOAT)]
		[TestCase(MAX_FLOAT)]
		[TestCase(MIN_DOUBLE)]
		[TestCase(MAX_DOUBLE)]
		[TestCase("")]
		[TestCase(LONG_STRING)]
		public void TestEncodeSubValue<T>(T expectedValue)
		{
			TestFieldClass<T> decodedTestFieldClass;
			var schema = new ProtobufSchema<TestFieldClass<T>>();
			var testFieldClass = new TestFieldClass<T>();

			testFieldClass.subValue = new SubTestFieldClass<T>();
			testFieldClass.subValue.value = expectedValue;

			schema.EncoderDescriptor
				.IsObject()
				.HasField("_3", target => target.subValue)
				.IsObject()
				.HasField("_4", target => target.value)
				.IsValue();

			decodedTestFieldClass = ProtobufSchemaTester.EncodeRoundTrip(schema.CreateEncoder(), testFieldClass);

			Assert.AreEqual(expectedValue, decodedTestFieldClass.subValue.value);
		}

		[Test]
		[TestCase(MIN_LONG, MIN_LONG, MIN_LONG)]
		[TestCase(MAX_LONG, MAX_LONG, MAX_LONG)]
		[TestCase(MIN_FLOAT, MIN_FLOAT, MIN_FLOAT)]
		[TestCase(MAX_FLOAT, MAX_FLOAT, MAX_FLOAT)]
		[TestCase(MIN_DOUBLE, MIN_DOUBLE, MIN_DOUBLE)]
		[TestCase(MAX_DOUBLE, MAX_DOUBLE, MAX_DOUBLE)]
		[TestCase("", "", "")]
		[TestCase(LONG_STRING, LONG_STRING, LONG_STRING)]
		public void TestEncodeSubValues<T>(T a, T b, T c)
		{
			TestFieldClass<TestFieldClass<T>> decodedFieldClass;
			var fieldClass = new TestFieldClass<TestFieldClass<T>>();
			var schema = new ProtobufSchema<TestFieldClass<TestFieldClass<T>>>();
			var expectedValues = new[] { a, b, c };

			foreach (var value in expectedValues)
			{
				fieldClass.items.Add(new TestFieldClass<T>
				{
					subValue = new SubTestFieldClass<T>
					{
						value = value
					}
				});
			}

			schema.EncoderDescriptor
				.IsObject()
				.HasField("_2", v => v)
				.IsArray(source => source.items)
				.IsObject()
				.HasField("_3", target => target.subValue)
				.IsObject()
				.HasField("_4", target => target.value)
				.IsValue();

			decodedFieldClass = ProtobufSchemaTester.EncodeRoundTrip(schema.CreateEncoder(), fieldClass);

			Assert.AreEqual(expectedValues.Length, decodedFieldClass.items.Count);

			for (int i = 0; i < expectedValues.Length; ++i)
				Assert.AreEqual(expectedValues[i], decodedFieldClass.items[i].subValue.value);
		}

		private static T DecodeRoundTrip<T>(IDecoder<T> decoder, T input)
		{
			return ProtobufSchemaTester.DecodeTranscode<T, T>(decoder, input);
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

		[global::System.Serializable, global::ProtoBuf.ProtoContract(Name = @"SubTestFieldClass")]
		public class SubTestFieldClass<T> : global::ProtoBuf.IExtensible
		{
			private T _value;
			[global::ProtoBuf.ProtoMember(4, IsRequired = true)]
			public T value
			{
				get { return _value; }
				set { _value = value; }
			}

			private global::ProtoBuf.IExtension extensionObject;
			global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
			{
				return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
			}
		}

		[global::System.Serializable, global::ProtoBuf.ProtoContract(Name = @"TestFieldClass")]
		public class TestFieldClass<T> : global::ProtoBuf.IExtensible
		{
			private T _value;
			[global::ProtoBuf.ProtoMember(1, IsRequired = true)]
			public T value
			{
				get { return _value; }
				set { _value = value; }
			}

			private global::System.Collections.Generic.List<T> _items = new global::System.Collections.Generic.List<T>();
			[global::ProtoBuf.ProtoMember(2, IsRequired = true, DataFormat = global::ProtoBuf.DataFormat.Default)]
			public global::System.Collections.Generic.List<T> items
			{
				get { return _items; }
				set { _items = value; }
			}

			private SubTestFieldClass<T> _subValue;
			[global::ProtoBuf.ProtoMember(3, IsRequired = true)]
			public SubTestFieldClass<T> subValue
			{
				get { return _subValue; }
				set { _subValue = value; }
			}

			private global::ProtoBuf.IExtension extensionObject;
			global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
			{
				return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
			}
		}
	}
}
