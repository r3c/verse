using System;
using System.IO;
using System.Text;

using KellermanSoftware.CompareNetObjects;

using NUnit.Framework;

namespace Verse.Test.Helpers
{
	class DecoderValidator
	{
		public static void	Validate<T> (IDecoder<T> decoder, string source, T expected)
		{
			CompareObjects	compare;
			T				instance;

			using (MemoryStream stream = new MemoryStream (Encoding.ASCII.GetBytes (source)))
			{
				Assert.IsTrue (decoder.Decode (stream, out instance));
			}

			compare = new CompareObjects ();

			Assert.IsTrue (compare.Compare (instance, expected));
		}
	}
}
