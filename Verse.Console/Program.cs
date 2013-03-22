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
			IDecoder<Entity>	decoder;
			JSONSchema			schema;
			Entity				entity;

			schema = new JSONSchema ();
			schema.SetDecoderConverter<Guid> (Guid.TryParse);

			decoder = schema.GetDecoder<Entity> (() => new Entity ());
			decoder
				.Define ("pairs", (ref Entity e, Dictionary<string, string> v) => { e.pairs = v; })
				.Define ((ref Dictionary<string, string> pairs, string key, string value) => { pairs[key] = value; })
				.Link ();
			decoder
				.Define ("int2", (ref Entity e, short int2) => { e.int2 = int2; })
				.Link ();
			decoder
				.Define ("str", (ref Entity e, string str) => { e.str = str; })
				.Link ();
			decoder
				.Define ("guid", (ref Entity e, Guid guid) => { e.guid = guid; })
				.Link ();

			using (Stream stream = GetUTF8 ("{\"pairs\": {\"a\": \"aaa\", \"b\": \"bbb\"}, \"str\": \"Hello, World!\", \"int2\": 17.5, \"guid\": \"fc2706dd-ede6-4dbe-9014-9970c1931536\"}"))
			{
				if (decoder.Decode (stream, out entity))
					System.Console.WriteLine ("OK: pairs = [" + string.Join (", ", new List<KeyValuePair<string, string>> (entity.pairs).ConvertAll ((p) => p.Key + " = " + p.Value).ToArray ()) + "], str = " + entity.str + ", int2 = " + entity.int2 + ", guid = " + entity.guid);
				else
					System.Console.WriteLine ("KO");
			}

			System.Console.ReadKey (true);
		}

		private static Stream	GetUTF8 (string contents)
		{
			return new MemoryStream (Encoding.UTF8.GetBytes (contents));
		}
	}
}
