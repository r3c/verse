using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;
using Verse.Schemas.JSON;

namespace Verse.Test
{
	[TestFixture]
	public class LinkerTester
	{
		[Test]
		[TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
		[TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
		public void LinkEncoderArrayFromArray(double[] value, string expected)
		{
			var encoded = LinkAndEncode(new JSONSchema<double[]>(), value);

			CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
		[TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
		public void LinkEncoderArrayFromList(double[] value, string expected)
		{
			var decoded = new List<double>(value);
			var encoded = LinkAndEncode(new JSONSchema<List<double>>(), decoded);

			CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(53, "{\"Field\":53}")]
		[TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
		public void LinkEncoderField<T>(T value, string expected)
		{
			var decoded = new FieldContainer<T> { Field = value };
			var encoded = LinkAndEncode(new JSONSchema<FieldContainer<T>>(), decoded);

			Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(53, "{\"Property\":53}")]
		[TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
		public void LinkEncoderProperty<T>(T value, string expected)
		{
			var decoded = new PropertyContainer<T> { Property = value };
			var encoded = LinkAndEncode(new JSONSchema<PropertyContainer<T>>(), decoded);

			Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase(false, "{\"r\":{\"r\":{\"r\":null,\"v\":42},\"v\":17},\"v\":3}")]
		[TestCase(true, "{\"r\":{\"r\":{\"v\":42},\"v\":17},\"v\":3}")]
		public void LinkEncoderRecursive(bool omitNull, string expected)
		{
			var decoded = new Recursive { r = new Recursive { r = new Recursive { v = 42 }, v = 17 }, v = 3 };
			var encoded = LinkAndEncode(new JSONSchema<Recursive>(new JSONConfiguration { OmitNull = omitNull }), decoded);

			Assert.AreEqual(expected, Encoding.UTF8.GetString(encoded));
		}

		[Test]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
		[TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
		public void LinkDecoderArrayFromArray(string json, double[] expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.LinkAndDecode(new JSONSchema<double[]>(), encoded);

			CollectionAssert.AreEqual(expected, decoded);
		}

		[Test]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
		[TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
		public void LinkDecoderArrayFromList(string json, double[] expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.LinkAndDecode(new JSONSchema<List<double>>(), encoded);

			CollectionAssert.AreEqual(expected, decoded);
		}

		[Test]
		[TestCase("{\"Field\": 53}", 53)]
		[TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkDecoderField<T>(string json, T expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.LinkAndDecode(new JSONSchema<FieldContainer<T>>(), encoded);

			Assert.AreEqual(expected, decoded.Field);
		}

		[Test]
		[TestCase("{\"Property\": 53}", 53)]
		[TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkDecoderProperty<T>(string json, T expected)
		{
			var encoded = Encoding.UTF8.GetBytes(json);
			var decoded = LinkerTester.LinkAndDecode(new JSONSchema<PropertyContainer<T>>(), encoded);

			Assert.AreEqual(expected, decoded.Property);
		}

		[Test]
		public void LinkDecoderRecursive()
		{
			var encoded = Encoding.UTF8.GetBytes("{\"r\": {\"r\": {\"v\": 42}, \"v\": 17}, \"v\": 3}");
			var decoded = LinkerTester.LinkAndDecode(new JSONSchema<Recursive>(), encoded);

			Assert.AreEqual(42, decoded.r.r.v);
			Assert.AreEqual(17, decoded.r.v);
			Assert.AreEqual(3, decoded.v);
		}

		private static T LinkAndDecode<T>(ISchema<T> schema, byte[] encoded)
		{
			using (var stream = new MemoryStream(encoded))
			{
				var decoder = Linker.CreateDecoder(schema);

				Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
				Assert.IsTrue(decoderStream.Decode(out var decoded));

				return decoded;
			}
		}

		private static byte[] LinkAndEncode<T>(ISchema<T> schema, T decoded)
		{
			var encoder = Linker.CreateEncoder(schema);

			using (var stream = new MemoryStream())
			{
				Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
				Assert.IsTrue(encoderStream.Encode(decoded));

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
	}
}