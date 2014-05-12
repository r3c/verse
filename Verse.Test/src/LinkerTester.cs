using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Test
{
	[TestFixture]
	public class LinkerTester
	{
		[Test]
		[TestCase ("{\"Field\": 53}", 53)]
		[TestCase ("{\"Field\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkField<T> (string json, T expected)
		{
			IParser<FieldContainer<T>>	parser;
			FieldContainer<T>			value;

			parser = Linker.CreateParser (new JSONSchema<FieldContainer<T>> ());

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, value.Field);
		}

		[Test]
		[TestCase ("{\"Property\": 53}", 53)]
		[TestCase ("{\"Property\": \"Black sheep wall\"}", "Black sheep wall")]
		public void LinkProperty<T> (string json, T expected)
		{
			IParser<PropertyContainer<T>>	parser;
			PropertyContainer<T>			value;

			parser = Linker.CreateParser (new JSONSchema<PropertyContainer<T>> ());

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, value.Property);
		}

		[Test]
		public void LinkRecursive ()
		{
			IParser<Recursive>		parser;
			Recursive				value;

			parser = Linker.CreateParser (new JSONSchema<Recursive> ());

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes ("{\"r\": {\"r\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")), out value));

			Assert.AreEqual (42, value.r.r.v);
			Assert.AreEqual (17, value.r.v);
			Assert.AreEqual (3, value.v);
		}

		class FieldContainer<T>
		{
			public T	Field = default (T);
		}

		class PropertyContainer<T>
		{
			public T	Property
			{
				get;
				set;
			}
		}

		class Recursive
		{
			public Recursive	r = null;
			public int			v = 0;
		}
	}
}
