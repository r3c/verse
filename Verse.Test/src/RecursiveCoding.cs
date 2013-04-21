using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Verse.Models.JSON;
using Verse.Models.JSON.Writers;

namespace Verse.Test
{
	public class RecursiveCoding
	{
		private class	MyContainsArray
		{
			public MyContainsArray[]	children;
			public string				value;
		}

		public static void	TestContainsArray ()
		{
			IDecoder<MyContainsArray>	decoder;
			IEncoder<MyContainsArray>	encoder;
			MyContainsArray				instance;
			ISchema						schema;

			schema = RecursiveCoding.Schema ();

			decoder = schema.GetDecoder<MyContainsArray> ();
			decoder.Link ();

			encoder = schema.GetEncoder<MyContainsArray> ();
			encoder.Link ();

			instance = new MyContainsArray
			{
				children = new MyContainsArray[]
				{
					new MyContainsArray
					{
						children = null,
						value = "a"
					},
					new MyContainsArray
					{
						children = new MyContainsArray[]
						{
							new MyContainsArray
							{
								children = null,
								value = "b"
							},
							new MyContainsArray
							{
								children = null,
								value = "c"
							}
						},
						value = "d"
					},
					new MyContainsArray
					{
						children = new MyContainsArray[0],
						value = "e"
					}
				},
				value = "f"
			};

			Verify (decoder, encoder, instance, Encoding.UTF8.GetBytes ("{\"children\":[{\"children\":null,\"value\":\"a\"},{\"children\":[{\"children\":null,\"value\":\"b\"},{\"children\":null,\"value\":\"c\"}],\"value\":\"d\"},{\"children\":[],\"value\":\"e\"}],\"value\":\"f\"}"));
		}

		private class	MyContainsValue
		{
			public MyContainsValue	child;
			public int				value;
		}

		public static void	TestContainsValue ()
		{
			IDecoder<MyContainsValue>	decoder;
			IEncoder<MyContainsValue>	encoder;
			MyContainsValue				instance;
			ISchema						schema;

			schema = RecursiveCoding.Schema ();

			decoder = schema.GetDecoder<MyContainsValue> ();
			decoder.HasField ("child", (ref MyContainsValue c, MyContainsValue v) => c.child = v, decoder);
			decoder.HasField ("value", (ref MyContainsValue c, int v) => c.value = v).Link ();

			encoder = schema.GetEncoder<MyContainsValue> ();
			encoder.HasField ("child", (n) => n.child, encoder);
			encoder.HasField ("value", (n) => n.value).Link ();

			instance = new MyContainsValue
			{
				child	= new MyContainsValue
				{
					child	= new MyContainsValue
					{
						child	= null,
						value	= 64
					},
					value	= 42
				},
				value	= 17
			};

			Verify (decoder, encoder, instance, Encoding.UTF8.GetBytes ("{\"child\":{\"child\":{\"child\":null,\"value\":64},\"value\":42},\"value\":17}"));
		}

		private static ISchema	Schema ()
		{
			JSONSchema	schema;

			schema = new JSONSchema (0, (s, e) => new JSONWriter (s, e));
			schema.OnStreamError += (position, message) => System.Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => System.Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);

			return schema;
		}

		private static void	Verify<T> (IDecoder<T> decoder, IEncoder<T> encoder, T instance, byte[] compare)
		{
			byte[]	buffer;
			T		other;
			using (MemoryStream stream = new MemoryStream ())
			{
				if (!encoder.Encode (stream, instance))
					throw new NotImplementedException ("test failed");

				buffer = stream.ToArray ();
			}

			using (MemoryStream stream = new MemoryStream (buffer))
			{
				if (!decoder.Decode (stream, out other))
					throw new NotImplementedException ("test failed");
			}

			using (MemoryStream stream = new MemoryStream ())
			{
				if (!encoder.Encode (stream, other))
					throw new NotImplementedException ("test failed");

				buffer = stream.ToArray ();
			}

			if (buffer.Length != compare.Length)
				throw new NotImplementedException ("test failed");

			for (int i = 0; i < buffer.Length; ++i)
			{
				if (buffer[i] != compare[i])
					throw new NotImplementedException ("test failed");
			}
		}
	}
}
