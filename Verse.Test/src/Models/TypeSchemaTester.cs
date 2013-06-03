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

		private class	MyIdentifier
		{
			public Guid	guid { get; set; }
		}

		private class	MyValue
		{
			public float[]						floats;
			public short						int2;
			public MyEnum						myEnum;
			public Dictionary<string, string>	pairs;
			public string						str;
		}

		[Test]
		public void	WithGuid (ISchema schema)
		{
			IDecoder<MyIdentifier>	decoder;
			IEncoder<MyIdentifier>	encoder;

			encoder = schema.GetEncoder<MyIdentifier> ();
			encoder.Link ();

			decoder = schema.GetDecoder<MyIdentifier> ();
			decoder.Link ();

			this.Validate (decoder, encoder, new MyIdentifier
			{
				guid	= Guid.NewGuid ()
			});
		}

		[Test]
		public void	WithMixedTypes (ISchema schema)
		{
			IDecoder<MyValue>	decoder;
			IEncoder<MyValue>	encoder;

			encoder = schema.GetEncoder<MyValue> ();
			encoder.Link ();

			decoder = schema.GetDecoder<MyValue> ();
			decoder.Link ();

			this.Validate (decoder, encoder, new MyValue
			{
				floats		= new float[] {1.1f, 2.2f, 3.3f},
				int2		= 17,
				myEnum		= MyEnum.B,
				pairs		= new Dictionary<string, string>
				{
					{"a",	"aaa"},
					{"b",	"bbb"}
				},
				str			= "Hello, World!"
			});
		}

		[Test]
		public void	WithNullable (ISchema schema)
		{
			IDecoder<int?>	decoder;
			IEncoder<int?>	encoder;

			encoder = schema.GetEncoder<int?> ();
			encoder.Link ();

			decoder = schema.GetDecoder<int?> ();
			decoder.Link ();

			this.Validate (decoder, encoder, null);
			this.Validate (decoder, encoder, 42);
		}
	}
}
