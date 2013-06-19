using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

using Verse.Models.JSON;
using Verse.Test.Benches;
using Verse.Test.Models;

namespace Verse.Test
{
	class Program
	{
		public static void Main(string[] args)
		{
			NewtonsoftBench			newtonsoftBench;
			RecursiveSchemaTester	recursiveSchemaTester;
			SettingsTester			settingsTester;
			TypeSchemaTester		typeSchemaTester;

			recursiveSchemaTester = new RecursiveSchemaTester ();
			recursiveSchemaTester.NestedArray ();
			recursiveSchemaTester.NestedField ();

			settingsTester = new SettingsTester ();
			settingsTester.EncoderNoNullAttribute ();
			settingsTester.EncoderNoNullAttributeNoNullValue ();
			settingsTester.EncoderNoNullValue ();

			typeSchemaTester = new TypeSchemaTester ();
			typeSchemaTester.GuidProperty ();
			typeSchemaTester.MixedTypes ();
			typeSchemaTester.NullableDouble ();

			newtonsoftBench = new NewtonsoftBench ();
			newtonsoftBench.FlatStructureDecode ();
			newtonsoftBench.FlatStructureEncode ();
			newtonsoftBench.NestedArrayDecode ();
			newtonsoftBench.NestedArrayEncode ();

			Console.WriteLine ("OK");
			Console.ReadKey (true);
		}
	}
}
