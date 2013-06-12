using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Verse.Test.Helpers;

namespace Verse.Test.Models
{
	[TestFixture]
	class TypeSchemaTester
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

			SchemaValidator.Validate (decoder, encoder, new MyIdentifier
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

			SchemaValidator.Validate (decoder, encoder, new MyValue
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
			IDecoder<double?>	decoder;
			IEncoder<double?>	encoder;

			encoder = schema.GetEncoder<double?> ();
			encoder.Link ();

			decoder = schema.GetDecoder<double?> ();
			decoder.Link ();

			SchemaValidator.Validate (decoder, encoder, null);
			SchemaValidator.Validate (decoder, encoder, 42);
		}
	}
}
