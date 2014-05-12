using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Bench
{
	class AlternativeIsValue
	{
		[Test]
		[TestCase (100000)]
		public void Bench(int repeat)
		{
			IParser<A>		parser1;
			IParser<A>		parser2;
			JSONSchema<A>	schema;
			A				value;

			schema = new JSONSchema<A> ();
			schema.ParserDescriptor.HasField ("b", (ref A a, int b) => a.b = b).IsValue ();

			parser1 = schema.CreateParser ();

			schema = new JSONSchema<A> ();
			schema.ParserDescriptor.HasField ("b").IsValue ((ref A a, int b) => a.b = b);

			parser2 = schema.CreateParser ();

			var j1 = Encoding.UTF8.GetBytes ("{\"b\": 5}");
			var j2 = Encoding.UTF8.GetBytes ("{\"b\": 7}");

			Assert.IsTrue (parser1.Parse (new MemoryStream (j1), out value));
			Assert.AreEqual (5, value.b);

			Assert.IsTrue (parser2.Parse (new MemoryStream (j2), out value));
			Assert.AreEqual (7, value.b);

			var s1 = System.Diagnostics.Stopwatch.StartNew ();
			for (int i = 0; i < repeat; ++i)
				parser1.Parse (new MemoryStream (j1), out value);
			Console.WriteLine ("p1: " + s1.Elapsed);
			var s2 = System.Diagnostics.Stopwatch.StartNew ();
			for (int i = 0; i < repeat; ++i)
				parser2.Parse (new MemoryStream (j2), out value);
			Console.WriteLine ("p2: " + s2.Elapsed);
		}

		struct A
		{
			public int b;
		}
	}
}
