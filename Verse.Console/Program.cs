using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

using Verse.Models.JSON;
using Verse.Models.JSON.Printers;

namespace Verse.Console
{
	class Program
	{
		enum	MyEnum
		{
			A,
			B,
			C
		}

		class	MyValue
		{
			public float[]						floats;
			public Guid							guid { get; set; }
			public short						int2;
			public MyEnum						myEnum;
			public Dictionary<string, string>	pairs;
			public string						str;
		}

		public static void Main(string[] args)
		{
			if (Test ())
				System.Console.WriteLine ("OK");
			else
				System.Console.WriteLine ("KO");

			System.Console.ReadKey (true);
		}
		
		private static bool	Test ()
		{
			byte[]				buffer;
			IDecoder<MyValue>	decoder;
			IEncoder<MyValue>	encoder;
			JSONSchema			schema;
			MyValue				value1;
			MyValue				value2;

			schema = new JSONSchema ((s, e) => new JSONIndentPrinter (s, e));
			schema.OnStreamError += (position, message) => System.Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => System.Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);
			schema.SetDecoderConverter<Guid> (Guid.TryParse);
			schema.SetEncoderConverter<Guid> ((Guid guid, out string s) => { s = guid.ToString (); return true; });

			encoder = schema.GetEncoder<MyValue> ();
			encoder.Link ();

			decoder = schema.GetDecoder<MyValue> ();
			decoder.Link ();

//buffer = null; value1 = new MyValue (); for (int i = 0; i < 100000; ++i)
			using (MemoryStream stream = new MemoryStream ())
			{
				value1 = new MyValue
				{
					floats	= new float[] {1.1f, 2.2f, 3.3f},
					guid	= Guid.NewGuid (),
					int2	= 17,
					myEnum	= MyEnum.B,
					pairs	= new Dictionary<string, string>
					{
						{"a",	"aaa"},
						{"b",	"bbb"}
					},
					str		= "Hello, World!"
				};

				if (!encoder.Encode (stream, value1))
					return false;

				buffer = stream.ToArray ();
			}

			System.Console.WriteLine (schema.Encoding.GetString (buffer));

			using (MemoryStream stream = new MemoryStream (buffer))
			{
				if (!decoder.Decode (stream, out value2))
					return false;

				Converter<IEnumerable<float>, string>							floatsConverter = (floats) => "[" + string.Join (", ", new List<float> (floats).ConvertAll ((f) => f.ToString ()).ToArray ()) + "]";
				Converter<IEnumerable<KeyValuePair<string, string>>, string>	pairsConverter = (pairs) => "{" + string.Join (", ", new List<KeyValuePair<string, string>> (pairs).ConvertAll ((p) => p.Key + ": " + p.Value).ToArray ()) + "}";

				System.Console.WriteLine (floatsConverter (value1.floats) + " / " + floatsConverter (value2.floats));
				System.Console.WriteLine (value1.guid + " / " + value2.guid);
				System.Console.WriteLine (value1.int2 + " / " + value2.int2);
				System.Console.WriteLine (value1.myEnum + " / " + value2.myEnum);
				System.Console.WriteLine (pairsConverter (value1.pairs) + " / " + pairsConverter (value2.pairs));
				System.Console.WriteLine (value1.str + " / " + value2.str);
			}

			return true;
		}
	}
}
