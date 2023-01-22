using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Verse.Exceptions;
using Verse.Schemas;
using Verse.Schemas.JSON;

namespace Verse.Test
{
	[TestFixture]
	public class LinkerTester
	{
		[Test]
		[TestCase("[]", new double[0])]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] {0, 5, 90, 23, -9, 5.32})]
		public void LinkDecoderArrayFromArray(string json, double[] expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.Decode(Linker.CreateDecoder(new JSONSchema<double[]>()), encoded);

			CollectionAssert.AreEqual(expected, decoded);
		}

		[Test]
		[TestCase("[]", new double[0])]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
		public void LinkDecoderArrayFromList(string json, double[] expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.Decode(Linker.CreateDecoder(new JSONSchema<List<double>>()), encoded);

			CollectionAssert.AreEqual(expected, decoded);
		}

		[Test]
		[TestCase("{\"Field\": 53}", 53)]
		[TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkDecoderField<T>(string json, T expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.Decode(Linker.CreateDecoder(new JSONSchema<FieldContainer<T>>()), encoded);

			Assert.AreEqual(expected, decoded.Field);
		}

		[Test]
		[TestCase("{\"Property\": 53}", 53)]
		[TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkDecoderProperty<T>(string json, T expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.Decode(Linker.CreateDecoder(new JSONSchema<PropertyContainer<T>>()), encoded);

			Assert.AreEqual(expected, decoded.Property);
		}

		[Test]
		public void LinkDecoderRecursive()
		{
			var encoded = Encoding.UTF8.GetBytes("{\"r\": {\"r\": {\"v\": 42}, \"v\": 17}, \"v\": 3}");
			var decoded = LinkerTester.Decode(Linker.CreateDecoder(new JSONSchema<Recursive>()), encoded);

			Assert.AreEqual(42, decoded.r.r.v);
			Assert.AreEqual(17, decoded.r.v);
			Assert.AreEqual(3, decoded.v);
		}

		[Test]
		[TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"isPublic\":1}", "0:0:1")]
		[TestCase(BindingFlags.Instance | BindingFlags.NonPublic, "{\"isProtected\":2,\"isPrivate\":3}", "3:2:0")]
		public void LinkDecoderVisibility(BindingFlags bindings, string json, string expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.Decode(Linker.CreateDecoder(new JSONSchema<Visibility>(), bindings), encoded);

			Assert.AreEqual(expected, decoded.ToString());
		}

		[Test]
		public void CreateDecoder_ShouldThrowWhenNoParameterlessConstructor()
		{
			var schema = new JSONSchema<Uri>();

			Assert.That(() => Linker.CreateDecoder(schema), Throws.InstanceOf<ConstructorNotFoundException>());
		}

		[Test]
		[TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
		[TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
		public void LinkEncoderArrayFromArray(double[] value, string expected)
		{
			var encoded = LinkerTester.Encode(Linker.CreateEncoder(new JSONSchema<double[]>()), value);

			CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
		[TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
		public void LinkEncoderArrayFromList(double[] value, string expected)
		{
			var decoded = new List<double>(value);
			var encoded = LinkerTester.Encode(Linker.CreateEncoder(new JSONSchema<List<double>>()), decoded);

			CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(53, "{\"Field\":53}")]
		[TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
		public void LinkEncoderField<T>(T value, string expected)
		{
			var decoded = new FieldContainer<T> { Field = value };
			var encoded = LinkerTester.Encode(Linker.CreateEncoder(new JSONSchema<FieldContainer<T>>()), decoded);

			Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(53, "{\"Property\":53}")]
		[TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
		public void LinkEncoderProperty<T>(T value, string expected)
		{
			var decoded = new PropertyContainer<T> { Property = value };
			var encoded = LinkerTester.Encode(Linker.CreateEncoder(new JSONSchema<PropertyContainer<T>>()), decoded);

			Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(false, "{\"r\":{\"r\":{\"r\":null,\"v\":42},\"v\":17},\"v\":3}")]
		[TestCase(true, "{\"r\":{\"r\":{\"v\":42},\"v\":17},\"v\":3}")]
		public void LinkEncoderRecursive(bool omitNull, string expected)
		{
			var decoded = new Recursive { r = new Recursive { r = new Recursive { v = 42 }, v = 17 }, v = 3 };
			var encoded = LinkerTester.Encode(Linker.CreateEncoder(new JSONSchema<Recursive>(new JSONConfiguration { OmitNull = omitNull })), decoded);

			Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(BindingFlags.Instance | BindingFlags.Public, "{\"isPublic\":0}")]
		[TestCase(BindingFlags.Instance | BindingFlags.NonPublic, "{\"isProtected\":0,\"isPrivate\":0}")]
		public void LinkEncoderVisibility(BindingFlags bindings, string expected)
		{
			var decoded = new Visibility();
			var encoded = LinkerTester.Encode(Linker.CreateEncoder(new JSONSchema<Visibility>(), bindings), decoded);

			Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		private static T Decode<T>(IDecoder<T> decoder, byte[] encoded)
		{
			using (var stream = new MemoryStream(encoded))
			{
				using (var decoderStream = decoder.Open(stream))
				{
					Assert.IsTrue(decoderStream.TryDecode(out var decoded));

					return decoded;
				}
			}
		}

		private static byte[] Encode<T>(IEncoder<T> encoder, T decoded)
		{
			using (var stream = new MemoryStream())
			{
				using (var encoderStream = encoder.Open(stream))
					encoderStream.Encode(decoded);

				return stream.ToArray();
			}
		}

		private class FieldContainer<T>
		{
			public T Field;
		}

		private class PropertyContainer<T>
		{
			public T Property
			{
				get;
				set;
			}
		}

		private class Recursive
		{
			public Recursive r = null;

			public int v = 0;
		}

		private class Visibility
		{
			public int isPublic = 0;
			protected int isProtected = 0;
			private int isPrivate = 0;

			public override string ToString()
			{
				return $"{isPrivate}:{isProtected}:{isPublic}";
			}
		}
	}
}
