using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

namespace Verse.Test
{
	class Program
	{
		public static void Main(string[] args)
		{
			RecursiveCoding.TestContainsValue ();
			RecursiveCoding.TestContainsArray ();
			TypeCoding.Test ();

			System.Console.ReadKey (true);
		}
	}
}
