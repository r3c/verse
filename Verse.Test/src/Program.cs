using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

using Verse.Models.JSON;
using Verse.Test.Models;

namespace Verse.Test
{
	class Program
	{
		public static void Main(string[] args)
		{
			RecursiveSchemaTester	recursiveSchemaTester;
			TypeSchemaTester		typeSchemaTester;

			JSONSchema	schema;

			schema = new JSONSchema (0, (s, e) => new JSONPrinter (s, e));
			schema.OnStreamError += (position, message) => Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);

			schema.SetDecoderConverter<Guid> (Guid.TryParse);
			schema.SetEncoderConverter<Guid> ((g) => g.ToString ());

			recursiveSchemaTester = new RecursiveSchemaTester ();
			recursiveSchemaTester.WithNestedArray (schema);
			recursiveSchemaTester.WithNestedValue (schema);

			typeSchemaTester = new TypeSchemaTester ();
			typeSchemaTester.WithGuid (schema);
			typeSchemaTester.WithMixedTypes (schema);
			//typeSchemaTester.WithNullable (schema);

			Console.WriteLine ("OK");
			Console.ReadKey (true);
		}
	}
}
