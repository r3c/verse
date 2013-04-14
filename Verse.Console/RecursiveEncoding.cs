using System;
using System.IO;
using System.Text;

using Verse.Models.JSON;
using Verse.Models.JSON.Writers;

namespace Verse.Console
{
	public class RecursiveEncoding
	{
		class	MyContainsValue
		{
			public MyContainsValue	child;
			public int				value;
		}

		class	MyContainsArray
		{
			public MyContainsArray[]	children;
			public string				value;
		}

		public static void	TestContainsValue ()
		{
			IEncoder<MyContainsValue>	encoder;
			MyContainsValue				instance;
			ISchema						schema;

			schema = RecursiveEncoding.Schema ();

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

			System.Console.WriteLine (Encoding.UTF8.GetString (Encode (encoder, instance)));
		}

		public static void	TestContainsArray ()
		{
			IEncoder<MyContainsArray>	encoder;
			MyContainsArray				instance;
			ISchema						schema;

			schema = RecursiveEncoding.Schema ();

			encoder = schema.GetEncoder<MyContainsArray> ();
			encoder.HasField ("children", (n) => n.children).HasItems ((c) => c, encoder);
			encoder.HasField ("value", (n) => n.value).Link ();

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

			System.Console.WriteLine (Encoding.UTF8.GetString (Encode (encoder, instance)));
		}

		private static byte[]	Encode<T> (IEncoder<T> encoder, T instance)
		{
			using (MemoryStream stream = new MemoryStream ())
			{
				if (!encoder.Encode (stream, instance))
					throw new NotImplementedException ("test failed");

				return stream.ToArray ();
			}
		}

		private static ISchema	Schema ()
		{
			JSONSchema	schema;

			schema = new JSONSchema ((s, e) => new JSONIndentWriter (s, e, "  "));
			schema.OnStreamError += (position, message) => System.Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => System.Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);
			schema.SetDecoderConverter<Guid> (Guid.TryParse);
			schema.SetEncoderConverter<Guid> ((Guid guid, out string s) => { s = guid.ToString (); return true; });

			return schema;
		}
	}
}
