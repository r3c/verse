using System;

namespace Verse.Bench
{
	public class Program
	{
		public static void Main()
		{
			CompareNewtonsoft compare = new CompareNewtonsoft();

			compare.DecodeFlatStructure();
			compare.EncodeFlatStructure();
		}
	}
}