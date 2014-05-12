using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			JSONSchema<T>	schema;

			schema = new JSONSchema<T> ();
			schema.ParserDescriptor.HasField (name).IsValue ();

			this.AssertParseAndEqual (schema, json, expected);
		}

		[Test]
		[TestCase ("b", "{\"a\": 50, \"b\": 43, \"c\": [1, 5, 9]}", 43)]
		[TestCase ("b", "{\"a\": {\"x\": 1, \"y\": 2}, \"b\": \"OK\", \"c\": 21.6}", "OK")]
		public void ParseGarbage<T> (string name, string json, T expected)
		{
			JSONSchema<T>	schema;

			schema = new JSONSchema<T> ();
			schema.ParserDescriptor.HasField (name).IsValue ();

			this.AssertParseAndEqual (schema, json, expected);
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
		[TestCase ("[]", new double[0])]
		[TestCase ("[-42.1]", new [] {-42.1})]
		[TestCase ("[0, 5, 90, 23, -9, 5.3]", new [] {0, 5, 90, 23, -9, 5.3})]
		[TestCase ("{\"key1\": 27.5, \"key2\": 19}", new [] {27.5, 19})]
		public void ParseItems (string json, double[] expected)
		{
			IParser<double[]>		parser;
			double[]				result;
			JSONSchema<double[]>	schema;

			schema = new JSONSchema<double[]> ();
			schema.ParserDescriptor.HasItems ((ref double[] target, IEnumerable<double> value) => target = value.ToArray ()).IsValue ();

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out result));
			CollectionAssert.AreEqual (expected, result);
		}

		[Test]
		[TestCase ("\"90f59097-d06a-4796-b8d5-87eb6af7ed8b\"", "90f59097-d06a-4796-b8d5-87eb6af7ed8b")]
		[TestCase ("\"c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025\"", "c566a1c7-d89e-4e3f-8c5f-d4bcd0b82025")]
		public void ParseValueCustom (string json, string expected)
		{
			JSONSchema<Guid>	schema;

			schema = new JSONSchema<Guid> ();
			schema.SetDecoder ((v) => Guid.Parse (v.String));
			schema.ParserDescriptor.IsValue ();

			this.AssertParseAndEqual (schema, json, Guid.Parse (expected));
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
			JSONSchema<T>	schema;

			schema = new JSONSchema<T> ();
			schema.ParserDescriptor.IsValue ();

			this.AssertParseAndEqual (schema, json, expected);
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

		private void AssertParseAndEqual<T> (ISchema<T> schema, string json, T expected)
		{
			IParser<T>	parser;
			T			value;

			parser = schema.GenerateParser ();

			Assert.IsTrue (parser.Parse (new MemoryStream (Encoding.UTF8.GetBytes (json)), out value));
			Assert.AreEqual (expected, value);
		}

		private class RecursiveEntity
		{
			public RecursiveEntity	field;
			public int				value;
		}
	}
}
