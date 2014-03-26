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
			schema.ForField (name).ForValue ((ref string t, string v) => t = v);

			parser = schema.GetParser ();

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
			schema.ForValue ((ref string t, string v) => t = v);

			parser = schema.GetParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, value);
		}
	}
}
