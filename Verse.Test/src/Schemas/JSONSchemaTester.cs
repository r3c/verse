using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;
using Verse.Schemas.JSON;

namespace Verse.Test.Schemas
{
	[TestFixture]
	public class JSONSchemaTester
	{
		[Test]
		[TestCase ("glittering", "{\"glittering\": \"prizes\"}", "prizes")]
		[TestCase ("", "{\"\": \"pwic\"}", "pwic")]
		[TestCase ("1", "[\"hey!\", \"take me!\", \"not me!\"]", "take me!")]
		public void BasicForField (string name, string json, string expected)
		{
			IParser<string>		parser;
			JSONSchema<string>	schema;
			string				value;

			schema = new JSONSchema<string> ();
			schema.ParserDescriptor.HasField (name).IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, value);
		}

		[Test]
		[TestCase ("false", false)]
		[TestCase ("true", true)]
		[TestCase ("null", null)]
		[TestCase ("27", 27.0)]
		[TestCase ("-15.5", -15.5)]
		[TestCase ("1.2e3", 1200.0)]
		[TestCase (".5e-2", 0.005)]
		[TestCase ("\"\"", "")]
		[TestCase ("\"Hello, World!\"", "Hello, World!")]
		public void BasicForValue (string json, object expected)
		{
			IParser<Value>		parser;
			JSONSchema<Value>	schema;
			Value				value;

			schema = new JSONSchema<Value> ();
			schema.ParserDescriptor.IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));

			if (expected is bool)
			{
				Assert.AreEqual (Content.Boolean, value.Type);
				Assert.AreEqual ((bool)expected, value.Boolean);
			}
			else if (expected is double)
			{
				Assert.AreEqual (Content.Number, value.Type);
				Assert.AreEqual ((double)expected, value.Number, Math.Abs ((double)expected) * 0.001f);
			}
			else if (expected is string)
			{
				Assert.AreEqual (Content.String, value.Type);
				Assert.AreEqual ((string)expected, value.String);
			}
			else if (expected == null)
				Assert.AreEqual (Content.Void, value.Type);
			else
				Assert.Fail ("unknown expected type");
		}

		[Test]
		[TestCase ("~", 0)]
		[TestCase ("\"Unfinished", 11)]
		[TestCase ("[1.1.1]", 4)]
		[TestCase ("[0 0]", 3)]
		[TestCase ("{0}", 1)]
		[TestCase ("{\"\" 0}", 4)]
		[TestCase ("fail", 2)]  
		public void InvalidStream (string json, int expected)
		{
			IParser<string>		parser;
			int					position;
			JSONSchema<string>	schema;
			string				value;

			schema = new JSONSchema<string> ();
			schema.ParserDescriptor.IsValue ();

			position = -1;

			parser = schema.GenerateParser ();
			parser.Error += (p, m) => position = p;

			Assert.IsFalse (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, position);
		}

		[Test]
		public void RecursiveSchema ()
		{
			IParserDescriptor<RecursiveEntity>	descriptor;
			IParser<RecursiveEntity>			parser;
			JSONSchema<RecursiveEntity>			schema;
			RecursiveEntity						value;

			schema = new JSONSchema<RecursiveEntity> ();

			descriptor = schema.ParserDescriptor;
			descriptor.HasField ("f", (ref RecursiveEntity r, RecursiveEntity v) => r.field = v, (ref RecursiveEntity r) => new RecursiveEntity (), descriptor);
			descriptor.HasField ("v", (ref RecursiveEntity r, int v) => r.value = v).IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes ("{\"f\": {\"f\": {\"v\": 42}, \"v\": 17}, \"v\": 3}")), out value));

			Assert.AreEqual (42, value.field.field.value);
			Assert.AreEqual (17, value.field.value);
			Assert.AreEqual (3, value.value);
		}

		private class RecursiveEntity
		{
			public RecursiveEntity	field;
			public int				value;
		}
	}
}
