#if false
using System;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace Verse.Test.Helpers
{
	class EncoderValidator
	{
		public static void	Validate<T> (IEncoder<T> encoder, T instance, string expected)
		{
			byte[]	buffer;

			using (MemoryStream stream = new MemoryStream ())
			{
				Assert.IsTrue (encoder.Encode (stream, instance));

				buffer = stream.ToArray ();
			}

			Assert.AreEqual (expected, Encoding.ASCII.GetString (buffer));
		}
	}
}
#endif