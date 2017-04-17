using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
		[TestCase(MIN_LONG, ProtobufType.Long)]
		[TestCase(MAX_LONG, ProtobufType.Long)]
		[TestCase(MIN_FLOAT, ProtobufType.Float)]
		[TestCase(MAX_FLOAT, ProtobufType.Float)]
		[TestCase(0f, ProtobufType.Float)]
		[TestCase(MIN_DOUBLE, ProtobufType.Double)]
		[TestCase(MAX_DOUBLE, ProtobufType.Double)]
		[TestCase(0.0, ProtobufType.Double)]
		[TestCase("", ProtobufType.String)]
		[TestCase(LONG_STRING, ProtobufType.String)]
		public void TestDecodeValue<T>(T value, ProtobufType type)
		{
			ProtobufValue decodedValue;
			IDecoder<ProtobufValue> decoder;
			ISchema<ProtobufValue> schema;
			MemoryStream stream;
			TestFieldClass<T> testFieldClass;

			testFieldClass = new TestFieldClass<T>();
			testFieldClass.value = value;

			stream = new MemoryStream();
			Serializer.Serialize(stream, testFieldClass);
			stream.Seek(0, SeekOrigin.Begin);

			decodedValue = new ProtobufValue();

			schema = new ProtobufSchema<ProtobufValue>();
			schema.DecoderDescriptor.HasField("_1", Identity<ProtobufValue>.Assign).IsValue();
			decoder = schema.CreateDecoder();

			Assert.IsTrue(decoder.Decode(stream, out decodedValue));
			Assert.AreEqual(type, decodedValue.Type);

			switch (type)
			{
				case ProtobufType.Double:
					Assert.AreEqual(value, decodedValue.DoubleContent);
					break;

				case ProtobufType.Float:
					Assert.AreEqual(value, decodedValue.FloatContent);
					break;

				case ProtobufType.Long:
					Assert.AreEqual(value, decodedValue.LongContent);
					break;

				case ProtobufType.String:
					Assert.AreEqual(value, decodedValue.StringContent);
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
			IDecoder<List<T>> decoder;
			ProtobufSchema<List<T>> schema;
			List<T> value;

			TestFieldClass<T> testFieldClass;
			testFieldClass = new TestFieldClass<T>();
			testFieldClass.items = new List<T> {a, b, c};

			MemoryStream stream;
			stream = new MemoryStream();
			Serializer.Serialize(stream, testFieldClass);
			stream.Seek(0, SeekOrigin.Begin);

			schema = new ProtobufSchema<List<T>>();
			schema.DecoderDescriptor
				.HasField("_2", (ref List<T> target, List<T> values) => target.AddRange(values))
				.IsArray((ref List<T> target, IEnumerable<T> enumerable) => target.AddRange(enumerable))
				.IsValue();

			value = new List<T>();
			decoder = schema.CreateDecoder();
			Assert.IsTrue(decoder.Decode(stream, out value));
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
			TestFieldClass<T> testFieldClass;
			testFieldClass = new TestFieldClass<T>();

			testFieldClass.subValue = new SubTestFieldClass<T>();
			testFieldClass.subValue.value = expectedValue;

			MemoryStream stream;
			stream = new MemoryStream();
			Serializer.Serialize(stream, testFieldClass);
			stream.Seek(0, SeekOrigin.Begin);

			ProtobufSchema<T> schema = new ProtobufSchema<T>();
			schema.DecoderDescriptor.HasField("_3", Identity<T>.Assign).HasField("_4", Identity<T>.Assign).IsValue();

			IDecoder<T> decoder = schema.CreateDecoder();

			T value = default(T);
			Assert.IsTrue(decoder.Decode(stream, out value));
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
			IDecoder<TestFieldClass<TestFieldClass<T>>> decoder;
			ProtobufSchema<TestFieldClass<TestFieldClass<T>>> schema;
			MemoryStream stream;
			TestFieldClass<TestFieldClass<T>> testFieldClass;
			T[] expectedValues = {a, b, c};

			testFieldClass = new TestFieldClass<TestFieldClass<T>>();
			foreach (var value in expectedValues)
			{
				testFieldClass.items.Add(
					new TestFieldClass<T>
					{
						subValue = new SubTestFieldClass<T>
						{
							value = value
						}
					});
			}

			stream = new MemoryStream();
			Serializer.Serialize(stream, testFieldClass);
			stream.Seek(0, SeekOrigin.Begin);

			schema = new ProtobufSchema<TestFieldClass<TestFieldClass<T>>>();
			schema.DecoderDescriptor
				.HasField("_2", (ref TestFieldClass<TestFieldClass<T>> target, List<TestFieldClass<T>> value) => target.items.AddRange(value))
				.IsArray((ref List<TestFieldClass<T>> target, IEnumerable<TestFieldClass<T>> value) => target.AddRange(value))
				.HasField("_3", (ref TestFieldClass<T> target, SubTestFieldClass<T> value) => target.subValue  = value)
				.HasField("_4", (ref SubTestFieldClass<T> target, T value) => target.value = value)
				.IsValue();

			decoder = schema.CreateDecoder();

			decodedValue = new TestFieldClass<TestFieldClass<T>>();
			Assert.IsTrue(decoder.Decode(stream, out decodedValue));

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
			IDecoder<int> decoder;
			ProtobufSchema<int> schema;
			int value;

			TestFieldClass<TestFieldClass<SubTestFieldClass<int>>> testFieldClass;
			testFieldClass = new TestFieldClass<TestFieldClass<SubTestFieldClass<int>>>();
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
			}.ToList();

			MemoryStream stream;
			stream = new MemoryStream();
			Serializer.Serialize(stream, testFieldClass);
			stream.Seek(0, SeekOrigin.Begin);

			schema = new ProtobufSchema<int>();

			IDecoderDescriptor<int?> result = schema.DecoderDescriptor
				.HasField("_2", (ref int target, int? v) => target = v.GetValueOrDefault(target));

			if (index0.HasValue)
				result = result.HasField(index0.Value.ToString(CultureInfo.InvariantCulture), (ref int? target, int? v) => target = target ?? v);

			result = result
				.HasField("_2", (ref int? target, int? v) => target = target ?? v);

			if (index1.HasValue)
				result = result.HasField(index1.Value.ToString(CultureInfo.InvariantCulture), (ref int? target, int? v) => target = target ?? v);

			result
				.HasField("_4", (ref int? target, int v) => target = v)
				.IsValue();

			decoder = schema.CreateDecoder();
			Assert.IsTrue(decoder.Decode(stream, out value));
			Assert.AreEqual(expected, value);
		}

		[Test]
		[TestCase(MIN_LONG, ProtobufType.Long)]
		[TestCase(MAX_LONG, ProtobufType.Long)]
		[TestCase(MIN_FLOAT, ProtobufType.Float)]
		[TestCase(MAX_FLOAT, ProtobufType.Float)]
		[TestCase(MIN_DOUBLE, ProtobufType.Double)]
		[TestCase(MAX_DOUBLE, ProtobufType.Double)]
		[TestCase("", ProtobufType.String)]
		[TestCase(LONG_STRING, ProtobufType.String)]
		public void TestEncodeValue<T>(T value, ProtobufType type)
		{
			IEncoder<ProtobufValue> encoder;
			ISchema<ProtobufValue> schema;
			MemoryStream stream;
			TestFieldClass<T> testFieldClass;

			schema = new ProtobufSchema<ProtobufValue>();
			schema.EncoderDescriptor.HasField("_1", Identity<ProtobufValue>.Access).IsValue();

			stream = new MemoryStream();

			ProtobufValue protoValue;
			switch (type)
			{
				case ProtobufType.Float:
					protoValue = new ProtobufValue((float)(object)value);
					break;

				case ProtobufType.Double:
					protoValue = new ProtobufValue((double)(object)value);
					break;

				case ProtobufType.Long:
					protoValue = new ProtobufValue((long)(object)value);
					break;

				case ProtobufType.String:
					protoValue = new ProtobufValue((string)(object)value);
					break;

				default:
					Assert.Fail("Invalid content type");
					return;
			}

			encoder = schema.CreateEncoder();
			Assert.IsTrue(encoder.Encode(protoValue, stream));

			stream.Seek(0, SeekOrigin.Begin);

			testFieldClass = Serializer.Deserialize<TestFieldClass<T>>(stream);

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
			ProtobufSchema<List<T>> schema;
			schema = new ProtobufSchema<List<T>>();
			IEncoderDescriptor<List<T>> encoderDescriptor = schema.EncoderDescriptor.HasField("_2", Identity<List<T>>.Access);
			encoderDescriptor.IsArray(source => source).IsValue();
			T[] expectedItems = {a, b, c};

			MemoryStream stream;
			stream = new MemoryStream();
			Assert.IsTrue(schema.CreateEncoder().Encode(new List<T>(expectedItems), stream));
			stream.Seek(0, SeekOrigin.Begin);

			TestFieldClass<T> testFieldClass = Serializer.Deserialize<TestFieldClass<T>>(stream);
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
			IEncoder<TestFieldClass<T>> encoder;
			ProtobufSchema<TestFieldClass<T>> schema;
			MemoryStream stream;
			TestFieldClass<T> testFieldClass;

			testFieldClass = new TestFieldClass<T>();
			testFieldClass.subValue = new SubTestFieldClass<T>();
			testFieldClass.subValue.value = expectedValue;

			schema = new ProtobufSchema<TestFieldClass<T>>();
			schema.EncoderDescriptor
				.HasField("_3", target => target.subValue)
				.HasField("_4", target => target.value)
				.IsValue();

			stream = new MemoryStream();

			encoder = schema.CreateEncoder();
			Assert.IsTrue(encoder.Encode(testFieldClass, stream));
			stream.Seek(0, SeekOrigin.Begin);

			decodedTestFieldClass = Serializer.Deserialize<TestFieldClass<T>>(stream);

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
			IEncoder<TestFieldClass<TestFieldClass<T>>> encoder;
			TestFieldClass<TestFieldClass<T>> fieldClass;
			ProtobufSchema<TestFieldClass<TestFieldClass<T>>> schema;
			MemoryStream stream;
			T[] expectedValues = {a, b, c};

			fieldClass = new TestFieldClass<TestFieldClass<T>>();
			foreach (var value in expectedValues)
			{
				fieldClass.items.Add(
					new TestFieldClass<T>
					{
						subValue = new SubTestFieldClass<T>
						{
							value = value
						}
					});
			}

			schema = new ProtobufSchema<TestFieldClass<TestFieldClass<T>>>();
			schema.EncoderDescriptor
				.HasField("_2", Identity<TestFieldClass<TestFieldClass<T>>>.Access)
				.IsArray(source => source.items)
				.HasField("_3", target => target.subValue)
				.HasField("_4", target => target.value)
				.IsValue();

			stream = new MemoryStream();

			encoder = schema.CreateEncoder();
			Assert.IsTrue(encoder.Encode(fieldClass, stream));

			stream.Seek(0, SeekOrigin.Begin);

			TestFieldClass<TestFieldClass<T>> decodedFieldClass = Serializer.Deserialize<TestFieldClass<TestFieldClass<T>>>(stream);

			Assert.AreEqual(expectedValues.Length, decodedFieldClass.items.Count);

			for (int i = 0; i < expectedValues.Length; ++i)
			{
				Assert.AreEqual(expectedValues[i], decodedFieldClass.items[i].subValue.value);
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
