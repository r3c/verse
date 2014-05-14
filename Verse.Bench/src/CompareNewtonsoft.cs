using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Bench
{
	public class CompareNewtonsoft
	{
		[Test]
		public void	BuildFlatStructure ()
		{
			IBuilder<MyFlatStructure>	builder;
			MyFlatStructure				instance;

			builder = Linker.CreateBuilder (new JSONSchema<MyFlatStructure> ());

			instance = new MyFlatStructure
			{
				adipiscing	= 64,
				amet		= "Hello, World!",
				consectetur	= 255,
				elit		= 'z',
				fermentum	= 6553,
				hendrerit	= -32768,
				ipsum		= 65464658634633,
				lorem		= 0,
				pulvinar	= "I sense a soul in search of answers",
				sed			= 53.25f,
				sit			= 1.1
			};

			this.BenchBuild (builder, instance, 10000);
		}

		[Test]
		public void	BuildNestedArray ()
		{
			IBuilder<MyNestedArray>	builder;
			MyNestedArray			instance;

			builder = Linker.CreateBuilder (new JSONSchema<MyNestedArray> ());

			instance = new MyNestedArray
			{
				children = new []
				{
					new MyNestedArray
					{
						children = null,
						value = "a"
					},
					new MyNestedArray
					{
						children = new []
						{
							new MyNestedArray
							{
								children = null,
								value = "b"
							},
							new MyNestedArray
							{
								children = null,
								value = "c"
							}
						},
						value = "d"
					},
					new MyNestedArray
					{
						children = new MyNestedArray[0],
						value = "e"
					}
				},
				value = "f"
			};

			this.BenchBuild (builder, instance, 10000);
		}

		[Test]
		public void	ParseFlatStructure ()
		{
			IParser<MyFlatStructure>	parser;
			string						source;

			parser = Linker.CreateParser (new JSONSchema<MyFlatStructure> ());
			source = "{\"lorem\":0,\"ipsum\":65464658634633,\"sit\":1.1,\"amet\":\"Hello, World!\",\"consectetur\":255,\"adipiscing\":64,\"elit\":\"z\",\"sed\":53.25,\"pulvinar\":\"I sense a soul in search of answers\",\"fermentum\":6553,\"hendrerit\":-32768}";

			this.BenchParse (parser, source, 10000);
		}

		[Test]
		[TestCase (10, 10000)]
		[TestCase (1000, 100)]
		[TestCase (10000, 10)]
		[TestCase (100000, 1)]
		public void ParseLargeArray (int length, int count)
		{
			StringBuilder	builder;
			IParser<long[]>	parser;
			Random			random;
			ISchema<long[]>	schema;

			builder = new StringBuilder ();
			random = new Random ();

			builder.Append ("[");

			if (length > 0)
			{
				for (int i = 0; true; )
				{
					builder.Append (random.Next ().ToString (CultureInfo.InvariantCulture));
	
					if (++i >= length)
						break;
	
					builder.Append (",");
				}
			}

			builder.Append ("]");

			schema = new JSONSchema<long[]> ();
			schema.ParserDescriptor.IsArray ((ref long[] target, IEnumerable<long> value) => target = value.ToArray ()).IsValue ();
			parser = schema.CreateParser ();

			this.BenchParse (parser, builder.ToString (), count);
		}

		[Test]
		public void	ParseNestedArray ()
		{
			IParser<MyNestedArray>	parser;
			string					source;

			parser = Linker.CreateParser (new JSONSchema<MyNestedArray> ());
			source = "{\"children\":[{\"children\":null,\"value\":\"a\"},{\"children\":[{\"children\":null,\"value\":\"b\"},{\"children\":null,\"value\":\"c\"}],\"value\":\"d\"},{\"children\":[],\"value\":\"e\"}],\"value\":\"f\"}";

			this.BenchParse (parser, source, 10000);
		}

		private void	BenchBuild<T> (IBuilder<T> builder, T instance, int count)
		{
			string			expected;
			TimeSpan		timeNewton;
			TimeSpan		timeVerse;
			Stopwatch		watch;

			expected = JsonConvert.SerializeObject (instance);
			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
				JsonConvert.SerializeObject (instance);

			timeNewton = watch.Elapsed;

			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
			{
				using (MemoryStream stream = new MemoryStream ())
				{
					Assert.IsTrue (builder.Build (instance, stream));
				}
			}

			timeVerse = watch.Elapsed;

			using (MemoryStream stream = new MemoryStream ())
			{
				Assert.IsTrue (builder.Build (instance, stream));
				Assert.AreEqual (expected, Encoding.UTF8.GetString (stream.ToArray ()));
			}

			Console.WriteLine ("NewtonSoft: {0}, Verse: {1}", timeNewton, timeVerse);
		}

		private void	BenchParse<T> (IParser<T> parser, string source, int count)
		{
			byte[]			buffer;
			CompareLogic	compare;
			T				instance;
			T				reference;
			TimeSpan		timeNewton;
			TimeSpan		timeVerse;
			Stopwatch		watch;

			reference = JsonConvert.DeserializeObject<T> (source);
			buffer = Encoding.UTF8.GetBytes (source);

			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
				Assert.NotNull (JsonConvert.DeserializeObject<T> (source));

			timeNewton = watch.Elapsed;

			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
			{
				using (MemoryStream stream = new MemoryStream (buffer))
				{
					Assert.IsTrue (parser.Parse (stream, out instance));
				}
			}

			timeVerse = watch.Elapsed;

			using (MemoryStream stream = new MemoryStream (buffer))
			{
				Assert.IsTrue (parser.Parse (stream, out instance));
			}

			compare = new CompareLogic ();
			CollectionAssert.IsEmpty(compare.Compare (instance, reference).Differences);

			Console.WriteLine ("NewtonSoft: {0}, Verse: {1}", timeNewton, timeVerse);
		}

		private struct	MyFlatStructure
		{
			public int		lorem;
			public long		ipsum;
			public double	sit;
			public string	amet;
			public byte		consectetur;
			public ushort	adipiscing;
			public char		elit;
			public float	sed;
			public string	pulvinar;
			public uint		fermentum;
			public short	hendrerit;
		}

		private class	MyNestedArray
		{
			public MyNestedArray[]	children;
			public string			value;
		}
	}
}
