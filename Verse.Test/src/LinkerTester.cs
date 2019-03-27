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
		    var encoder = Linker.CreateEncoder(new JSONSchema<double[]>());

		    using (var stream = new MemoryStream())
			{
                Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
				Assert.IsTrue(encoderStream.Encode(value));

				CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		[Test]
		[TestCase(new[] { 0, 5, 90, 23, -9, 5.32 }, "[0,5,90,23,-9,5.32]")]
		[TestCase(new[] { 27.5, 19 }, "[27.5,19]")]
		public void LinkEncoderArrayFromList(double[] value, string expected)
		{
		    var encoder = Linker.CreateEncoder(new JSONSchema<List<double>>());

		    using (var stream = new MemoryStream())
			{
			    Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
                Assert.IsTrue(encoderStream.Encode(new List<double>(value)));

				CollectionAssert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		[Test]
		[TestCase(53, "{\"Field\":53}")]
		[TestCase("Black sheep wall", "{\"Field\":\"Black sheep wall\"}")]
		public void LinkEncoderField<T>(T value, string expected)
		{
		    var encoder = Linker.CreateEncoder(new JSONSchema<FieldContainer<T>>());

		    using (var stream = new MemoryStream())
			{
			    Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
                Assert.IsTrue(encoderStream.Encode(new FieldContainer<T> { Field = value }));

				Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		[Test]
		[TestCase(53, "{\"Property\":53}")]
		[TestCase("Black sheep wall", "{\"Property\":\"Black sheep wall\"}")]
		public void LinkEncoderProperty<T>(T value, string expected)
		{
		    var encoder = Linker.CreateEncoder(new JSONSchema<PropertyContainer<T>>());

		    using (var stream = new MemoryStream())
			{
			    Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
                Assert.IsTrue(encoderStream.Encode(new PropertyContainer<T> { Property = value }));

				Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		[Test]
		[Theory]
		public void LinkEncoderRecursive(bool ignoreNull)
		{
		    var expected = ignoreNull
			    ? "{\"r\":{\"r\":{\"v\":42},\"v\":17},\"v\":3}"
			    : "{\"r\":{\"r\":{\"r\":null,\"v\":42},\"v\":17},\"v\":3}";

			var encoder = Linker.CreateEncoder(new JSONSchema<Recursive>(new JSONConfiguration { OmitNull = ignoreNull }));

			using (var stream = new MemoryStream())
			{
				var value = new Recursive { r = new Recursive { r = new Recursive { v = 42 }, v = 17 }, v = 3 };

			    Assert.IsTrue(encoder.TryOpen(stream, out var encoderStream));
                Assert.IsTrue(encoderStream.Encode(value));

				Assert.AreEqual(expected, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		[Test]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
		[TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
		public void LinkDecoderArrayFromArray(string json, double[] expected)
		{
		    var decoder = Linker.CreateDecoder(new JSONSchema<double[]>());

		    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		    {
		        Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
		        Assert.IsTrue(decoderStream.Decode(out var value));

		        CollectionAssert.AreEqual(expected, value);
            }
		}

		[Test]
		[TestCase("[0, 5, 90, 23, -9, 5.32]", new[] { 0, 5, 90, 23, -9, 5.32 })]
		[TestCase("{\"key1\": 27.5, \"key2\": 19}", new[] { 27.5, 19 })]
		public void LinkDecoderArrayFromList(string json, double[] expected)
		{
		    var decoder = Linker.CreateDecoder(new JSONSchema<List<double>>());

		    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		    {
		        Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
		        Assert.IsTrue(decoderStream.Decode(out var value));

		        CollectionAssert.AreEqual(expected, value);
		    }
		}

		[Test]
		[TestCase("{\"Field\": 53}", 53)]
		[TestCase("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkDecoderField<T>(string json, T expected)
		{
		    var decoder = Linker.CreateDecoder(new JSONSchema<FieldContainer<T>>());

		    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		    {
		        Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
		        Assert.IsTrue(decoderStream.Decode(out var value));

		        Assert.AreEqual(expected, value.Field);
		    }
		}

		[Test]
		[TestCase("{\"Property\": 53}", 53)]
		[TestCase("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkDecoderProperty<T>(string json, T expected)
		{
		    var decoder = Linker.CreateDecoder(new JSONSchema<PropertyContainer<T>>());

		    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		    {
		        Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
		        Assert.IsTrue(decoderStream.Decode(out var value));

		        Assert.AreEqual(expected, value.Property);
		    }
		}

		[Test]
		public void LinkDecoderRecursive()
		{
		    var decoder = Linker.CreateDecoder(new JSONSchema<Recursive>());

		    using (var stream =
		        new MemoryStream(Encoding.UTF8.GetBytes("{\"r\": {\"r\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")))
		    {
		        Assert.IsTrue(decoder.TryOpen(stream, out var decoderStream));
                Assert.IsTrue(decoderStream.Decode(out var value));

		        Assert.AreEqual(42, value.r.r.v);
		        Assert.AreEqual(17, value.r.v);
		        Assert.AreEqual(3, value.v);
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