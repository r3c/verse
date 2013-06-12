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

			recursiveSchemaTester = new RecursiveSchemaTester ();
			recursiveSchemaTester.NestedArray ();
			recursiveSchemaTester.NestedField ();

			typeSchemaTester = new TypeSchemaTester ();
			typeSchemaTester.GuidProperty ();
			typeSchemaTester.MixedTypes ();
			typeSchemaTester.NullableDouble ();

			Console.WriteLine ("OK");
			Console.ReadKey (true);
		}
	}
}
