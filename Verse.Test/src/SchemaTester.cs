using System;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Verse.Test
{
	public class SchemaTester
	{
		protected void AssertRoundtrip<T> (ISchema<T> schema, T instance, Func<T> constructor)
		{
			IBuilder<T> builder;
			byte[] first;
			CompareLogic compare;
			T other;
			IParser<T> parser;
			byte[] second;

			builder = Linker.CreateBuilder (schema);
			parser = Linker.CreateParser (schema);

			using (MemoryStream stream = new MemoryStream ())
			{
				Assert.IsTrue (builder.Build (instance, stream));

				first = stream.ToArray ();
			}

			using (MemoryStream stream = new MemoryStream (first))
			{
				other = constructor ();

				Assert.IsTrue (parser.Parse (stream, ref other));
			}

			compare = new CompareLogic ();

			CollectionAssert.IsEmpty (compare.Compare (instance, other).Differences);

			using (MemoryStream stream = new MemoryStream ())
			{
				Assert.IsTrue (builder.Build (other, stream));

				second = stream.ToArray ();
			}

			CollectionAssert.AreEqual (first, second);
		}
	}
}
