using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Verse.Models.JSON;

namespace Verse.Console
{
	class Program
	{
		class	Entity
		{
			public Guid							guid;
			public short						int2;
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
			IDecoder<Entity>	decoder;
			IEncoder<Entity>	encoder;
			JSONSchema			schema;
			Entity				entity1;
			Entity				entity2;

			schema = new JSONSchema ();
			schema.OnStreamError += (position, message) => System.Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => System.Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);
			schema.SetDecoderConverter<Guid> (Guid.TryParse);
			schema.SetEncoderConverter<Guid> ((guid) => guid.ToString ());

			var enc = schema.GetEncoder<string> ();
			enc.Bind ();
			using (MemoryStream stream = new MemoryStream ())
			{
				enc.Encode (stream, "Hello,\r\nWorld!");

				System.Console.WriteLine (Encoding.UTF8.GetString (stream.ToArray ()));
			}

			encoder = schema.GetEncoder<Entity> ();
/*			encoder
				.HasField ("pairs", (entity) => entity.pairs)
				.HasMap ((Dictionary<string, string> pairs) => pairs)
				.Bind ();
			encoder
				.HasField ("int2", (entity) => entity.int2)
				.Bind ();
			encoder
				.HasField ("str", (entity) => entity.str)
				.Bind ();
			encoder
				.HasField ("guid", (entity) => entity.guid)
				.Bind ();
*/
			decoder = schema.GetDecoder<Entity> (() => new Entity ());
			decoder
				.HasField ("pairs", (ref Entity e, Dictionary<string, string> v) => { e.pairs = v; })
				.HasPairs ((ref Dictionary<string, string> pairs, IList<KeyValuePair<string, string>> input) => { foreach (var pair in input) pairs[pair.Key] = pair.Value; })
				.Bind ();
			decoder
				.HasField ("int2", (ref Entity e, short int2) => { e.int2 = int2; })
				.Bind ();
			decoder
				.HasField ("str", (ref Entity e, string str) => { e.str = str; })
				.Bind ();
			decoder
				.HasField ("guid", (ref Entity e, Guid guid) => { e.guid = guid; })
				.Bind ();

			using (MemoryStream stream = new MemoryStream ())
			{
				entity1 = new Entity
				{
					guid	= Guid.NewGuid (),
					int2	= 17,
					pairs	= new Dictionary<string, string>
					{
						{"a",	"aaa"},
						{"b",	"bbb"}
					},
					str		= "Hello, World!"
				};

/*				if (!encoder.Encode (stream, entity1))
					return false;*/
byte[] bytes = Encoding.UTF8.GetBytes ("{\"pairs\": {\"a\": \"aaa\", \"b\": \"bbb\"}, \"str\": \"Hello, World!\", \"int2\": 17.5, \"guid\": \"fc2706dd-ede6-4dbe-9014-9970c1931536\"}");
stream.Write (bytes, 0, bytes.Length);
				stream.Seek (0, SeekOrigin.Begin);

				if (!decoder.Decode (stream, out entity2))
					return false;

				Converter<IEnumerable<KeyValuePair<string, string>>, string>	converter = (pairs) => "[" + string.Join (", ", new List<KeyValuePair<string, string>> (pairs).ConvertAll ((p) => p.Key + " = " + p.Value).ToArray ()) + "]";

				System.Console.WriteLine (entity1.guid + " / " + entity2.guid);
				System.Console.WriteLine (entity1.int2 + " / " + entity2.int2);
				System.Console.WriteLine (converter (entity1.pairs) + " / " + converter (entity2.pairs));
				System.Console.WriteLine (entity1.str + " / " + entity2.str);
			}

			return true;
		}
	}
}
