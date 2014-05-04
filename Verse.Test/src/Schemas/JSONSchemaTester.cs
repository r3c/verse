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
		[TestCase ("glittering", "prizes", "{\"glittering\":\"prizes\"}")]
		[TestCase ("", "pwic", "{\"\":\"pwic\"}")]
		public void BuildFieldValue<T> (string name, T value, string expected)
		{
			IBuilder<T>		builder;
			JSONSchema<T>	schema;

			schema = new JSONSchema<T> ();
			schema.BuilderDescriptor.HasField (name).IsValue ();

			builder = schema.GenerateBuilder ();

			using (var stream = new MemoryStream ())
			{
				Assert.IsTrue (builder.Build (value, stream));
				Assert.AreEqual (expected, Encoding.UTF8.GetString (stream.ToArray ()));
			}
		}

		[Test]
		[TestCase (new int[0], "[]")]
		[TestCase (new [] {21}, "[21]")]
		[TestCase (new [] {54, 90, -3, 34, 0, 49}, "[54,90,-3,34,0,49]")]
		public void BuildItems (int[] values, string expected)
		{
			IBuilder<int[]>		builder;
			JSONSchema<int[]>	schema;

			schema = new JSONSchema<int[]> ();
			schema.BuilderDescriptor.HasItems ((source) => source).IsValue ();

			builder = schema.GenerateBuilder ();

			using (var stream = new MemoryStream ())
			{
				Assert.IsTrue (builder.Build (values, stream));
				Assert.AreEqual (expected, Encoding.UTF8.GetString (stream.ToArray ()));
			}			
		}

		[Test]
		[TestCase (false, "false")]
		[TestCase (true, "true")]
		[TestCase (3, "3")]
		[TestCase (3.5, "3.5")]
		[TestCase ("I sense a soul in search of answers", "\"I sense a soul in search of answers\"")]
		[TestCase ("\xFF \u0066 \uB3A8", "\"\\u00FF f \\uB3A8\"")]
		public void BuildValueNative<T> (T value, string expected)
		{
 			IBuilder<T>		builder;
			JSONSchema<T>	schema;

			schema = new JSONSchema<T> ();
			schema.BuilderDescriptor.IsValue ();

			builder = schema.GenerateBuilder ();

			using (var stream = new MemoryStream ())
			{
				Assert.IsTrue (builder.Build (value, stream));
				Assert.AreEqual (expected, Encoding.UTF8.GetString (stream.ToArray ()));
			}
		}

		[Test]
		[TestCase ("glittering", "{\"glittering\": \"prizes\"}", "prizes")]
		[TestCase ("", "{\"\": 5}", 5)]
		[TestCase ("1", "[\"hey!\", \"take me!\", \"not me!\"]", "take me!")]
		public void ParseFieldValue<T> (string name, string json, T expected)
		{
			IParser<T>		parser;
			JSONSchema<T>	schema;
			T				value;

			schema = new JSONSchema<T> ();
			schema.ParserDescriptor.HasField (name).IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, value);
		}

		[Test]
		[TestCase ("~", 1)]
		[TestCase ("\"Unfinished", 12)]
		[TestCase ("[1.1.1]", 5)]
		[TestCase ("[0 0]", 4)]
		[TestCase ("{0}", 2)]
		[TestCase ("{\"\" 0}", 5)]
		[TestCase ("fail", 3)]  
		public void ParseInvalidStream (string json, int expected)
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
		[TestCase ("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
		[TestCase ("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
		public void ParseValueCustom (string json, string expected)
		{
			IParser<Guid>		parser;
			JSONSchema<Guid>	schema;
			Guid				value;

			schema = new JSONSchema<Guid> ();
			schema.SetDecoder ((v) => Guid.Parse (v.String));
			schema.ParserDescriptor.IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (Guid.Parse (expected), value);
		}

		[Test]
		[TestCase ("false", false)]
		[TestCase ("true", true)]
		[TestCase ("27", 27.0)]
		[TestCase ("-15.5", -15.5)]
		[TestCase ("1.2e3", 1200.0)]
		[TestCase (".5e-2", 0.005)]
		[TestCase ("\"\"", "")]
		[TestCase ("\"Hello, World!\"", "Hello, World!")]
		[TestCase ("\"\\u00FF \\u0066 \\uB3A8\"", "\xFF f \uB3A8")]
		public void ParseValueNative<T> (string json, T expected)
		{
			IParser<T>		parser;
			JSONSchema<T>	schema;
			T				value;

			schema = new JSONSchema<T> ();
			schema.ParserDescriptor.IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));

			Assert.AreEqual (expected, value);
		}

		[Test]
		public void ParseRecursiveSchema ()
		{
			IParserDescriptor<RecursiveEntity>	descriptor;
			IParser<RecursiveEntity>			parser;
			JSONSchema<RecursiveEntity>			schema;
			RecursiveEntity						value;

			schema = new JSONSchema<RecursiveEntity> ();

			descriptor = schema.ParserDescriptor;
			descriptor.HasField ("f", (ref RecursiveEntity r, RecursiveEntity v) => r.field = v, descriptor);
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
