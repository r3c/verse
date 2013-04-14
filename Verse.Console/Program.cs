using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

namespace Verse.Console
{
	class Program
	{
		public static void Main(string[] args)
		{
			RecursiveEncoding.TestContainsValue ();
			RecursiveEncoding.TestContainsArray ();
			SimpleEncoding.Test ();

			System.Console.ReadKey (true);
		}
	}
}
