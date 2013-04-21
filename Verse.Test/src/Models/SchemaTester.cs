using System;
using System.IO;

using KellermanSoftware.CompareNetObjects;

using NUnit.Framework;

namespace Verse.Test.Models
{
	class SchemaTester
	{
		protected void	Validate<T> (IDecoder<T> decoder, IEncoder<T> encoder, T instance)
		{
			byte[]			first;
			CompareObjects	compare;
			T				other;
			byte[]			second;

			using (MemoryStream stream = new MemoryStream ())
			{
				Assert.IsTrue (encoder.Encode (stream, instance));

				first = stream.ToArray ();
			}

			using (MemoryStream stream = new MemoryStream (first))
			{
				Assert.IsTrue (decoder.Decode (stream, out other));
			}

			compare = new CompareObjects ();
			compare.Compare (instance, other);

			using (MemoryStream stream = new MemoryStream ())
			{
				Assert.IsTrue (encoder.Encode (stream, other));

				second = stream.ToArray ();
			}

			Assert.AreEqual (first.Length, second.Length);

			for (int i = 0; i < first.Length; ++i)
				Assert.AreEqual (first[i], second[i]);
		}
	}
}
