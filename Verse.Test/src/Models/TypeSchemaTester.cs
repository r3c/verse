using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

namespace Verse.Test.Models
{
	[TestFixture]
	class TypeSchemaTester : SchemaTester
	{
		private enum	MyEnum
		{
			A,
			B,
			C
		}

		private class	MyValue
		{
			public float[]						floats;
			public Guid							guid { get; set; }
			public short						int2;
			public MyEnum						myEnum;
			public Dictionary<string, string>	pairs;
			public string						str;
		}

		[Test]
		public void	WithGuid (ISchema schema)
		{
			IDecoder<MyValue>	decoder;
			IEncoder<MyValue>	encoder;

			encoder = schema.GetEncoder<MyValue> ();
			encoder.Link ();

			decoder = schema.GetDecoder<MyValue> ();
			decoder.Link ();

			this.Validate (decoder, encoder, new MyValue
			{
				floats	= new float[] {1.1f, 2.2f, 3.3f},
				guid	= Guid.NewGuid (),
				int2	= 17,
				myEnum	= MyEnum.B,
				pairs	= new Dictionary<string, string>
				{
					{"a",	"aaa"},
					{"b",	"bbb"}
				},
				str		= "Hello, World!"
			});
		}
	}
}
