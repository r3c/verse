using System;

namespace Verse.Bench
{
	public class Program
	{
		public static void Main()
		{
			CompareNewtonsoft compare = new CompareNewtonsoft ();

			compare.BuildFlatStructure ();
			compare.ParseFlatStructure ();
		}
	}
}
