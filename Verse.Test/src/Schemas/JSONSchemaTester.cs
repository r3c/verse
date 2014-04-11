using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Test.Schemas
{
	[TestFixture]
	public class JSONSchemaTester
	{
		[Test]
		[TestCase ("prizes", "glittering", "{\"glittering\": \"prizes\"}")]
		[TestCase ("pwic", "", "{\"\": \"pwic\"}")]
		public void BasicForField (string expected, string name, string json)
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
		[TestCase ("", "\"\"")]
		[TestCase ("Hello, World!", "\"Hello, World!\"")]
		public void BasicForValue (string expected, string json)
		{
			IParser<string>		parser;
			JSONSchema<string>	schema;
			string				value;

			schema = new JSONSchema<string> ();
			schema.ParserDescriptor.IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, value);
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
