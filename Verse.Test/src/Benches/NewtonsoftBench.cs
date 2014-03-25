using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using Verse.Models.JSON;

namespace Verse.Test.Benches
{
	public class NewtonsoftBench
	{
		private ISchema	GetSchema ()
		{
			JSONSchema	schema;

			schema = new JSONSchema (0);
			schema.OnStreamError += (position, message) => Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);

			return schema;
		}

		private void	BenchDecode<T> (IDecoder<T> decoder, string source, int count)
		{
			byte[]			buffer;
			CompareObjects	compare;
			T				instance;
			T				reference;
			MemoryStream	stream;
			TimeSpan		timeNewton;
			TimeSpan		timeVerse;
			Stopwatch		watch;

			reference = JsonConvert.DeserializeObject<T> (source);
			buffer = Encoding.UTF8.GetBytes (source);

			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
				JsonConvert.DeserializeObject<T> (source);

			timeNewton = watch.Elapsed;

			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
			{
				stream = new MemoryStream (buffer);
				decoder.Decode (stream, out instance);
			}

			timeVerse = watch.Elapsed;

			stream = new MemoryStream (buffer);
			Assert.IsTrue (decoder.Decode (stream, out instance));
			compare = new CompareObjects ();
			Assert.IsTrue (compare.Compare (instance, reference));

			Console.WriteLine ("NewtonSoft: {0}, Verse: {1}", timeNewton, timeVerse);
		}

		private void	BenchEncode<T> (IEncoder<T> encoder, T instance, int count)
		{
			string			reference;
			MemoryStream	stream;
			TimeSpan		timeNewton;
			TimeSpan		timeVerse;
			Stopwatch		watch;

			reference = JsonConvert.SerializeObject (instance);
			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
				JsonConvert.SerializeObject (instance);

			timeNewton = watch.Elapsed;

			watch = Stopwatch.StartNew ();

			for (int i = count; i-- > 0; )
			{
				stream = new MemoryStream ();
				encoder.Encode (stream, instance);
			}

			timeVerse = watch.Elapsed;

			stream = new MemoryStream ();
			Assert.IsTrue (encoder.Encode (stream, instance));
			Assert.AreEqual (reference, Encoding.UTF8.GetString (stream.ToArray ()));

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

		[Test]
		public void	FlatStructureDecode ()
		{
			IDecoder<MyFlatStructure>	decoder;
			ISchema						schema;
			string						source;

			schema = this.GetSchema ();

			decoder = schema.GetDecoder<MyFlatStructure> ();
			decoder.Link ();

			source = "{\"lorem\":0,\"ipsum\":65464658634633,\"sit\":1.1,\"amet\":\"Hello, World!\",\"consectetur\":255,\"adipiscing\":64,\"elit\":\"z\",\"sed\":53.25,\"pulvinar\":\"I sense a soul in search of answers\",\"fermentum\":6553,\"hendrerit\":-32768}";

			this.BenchDecode (decoder, source, 10000);
		}

		[Test]
		public void	FlatStructureEncode ()
		{
			IEncoder<MyFlatStructure>	encoder;
			MyFlatStructure				instance;
			ISchema						schema;

			schema = this.GetSchema ();

			encoder = schema.GetEncoder<MyFlatStructure> ();
			encoder.Link ();

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

			this.BenchEncode (encoder, instance, 10000);
		}

		private class	MyNestedArray
		{
			public MyNestedArray[]	children;
			public string			value;
		}

		[Test]
		public void	NestedArrayDecode ()
		{
			IDecoder<MyNestedArray>	decoder;
			ISchema					schema;
			string					source;

			schema = this.GetSchema ();

			decoder = schema.GetDecoder<MyNestedArray> ();
			decoder.Link ();

			source = "{\"children\":[{\"children\":null,\"value\":\"a\"},{\"children\":[{\"children\":null,\"value\":\"b\"},{\"children\":null,\"value\":\"c\"}],\"value\":\"d\"},{\"children\":[],\"value\":\"e\"}],\"value\":\"f\"}";

			this.BenchDecode (decoder, source, 10000);
		}

		[Test]
		public void	NestedArrayEncode ()
		{
			IEncoder<MyNestedArray>	encoder;
			MyNestedArray			instance;
			ISchema					schema;

			schema = this.GetSchema ();

			encoder = schema.GetEncoder<MyNestedArray> ();
			encoder.Link ();

			instance = new MyNestedArray
			{
				children = new MyNestedArray[]
				{
					new MyNestedArray
					{
						children = null,
						value = "a"
					},
					new MyNestedArray
					{
						children = new MyNestedArray[]
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

			this.BenchEncode (encoder, instance, 10000);
		}
	}
}
