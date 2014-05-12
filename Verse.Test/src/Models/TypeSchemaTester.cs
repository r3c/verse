#if false
using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Verse.Models.JSON;
using Verse.Test.Helpers;

namespace Verse.Test.Models
{
	[TestFixture]
	class TypeSchemaTester
	{
		private ISchema	GetSchema ()
		{
			JSONSchema	schema;

			schema = new JSONSchema (0);
			schema.OnStreamError += (position, message) => Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);

			schema.SetDecoderConverter<Guid> (Guid.TryParse);
			schema.SetEncoderConverter<Guid> ((g) => g.ToString ());

			return schema;
		}

		private class	MyGuidContainer
		{
			public Guid	guid
			{
				get;
				set;
			}
		}

		[Test]
		public void	GuidProperty ()
		{
			IDecoder<MyGuidContainer>	decoder;
			IEncoder<MyGuidContainer>	encoder;
			ISchema						schema;

			schema = this.GetSchema ();

			encoder = schema.GetEncoder<MyGuidContainer> ();
			encoder.Link ();

			decoder = schema.GetDecoder<MyGuidContainer> ();
			decoder.Link ();

			SchemaValidator.Validate (decoder, encoder, new MyGuidContainer
			{
				guid	= Guid.NewGuid ()
			});
		}

		private enum	MyOptionEnum
		{
			A,
			B,
			C
		}

		private class	MyMixedTypes
		{
			public float[]						floats;
			public short						integer;
			public MyOptionEnum					option;
			public Dictionary<string, string>	pairs;
			public string						text;
		}

		[Test]
		public void	MixedTypes ()
		{
			IDecoder<MyMixedTypes>	decoder;
			IEncoder<MyMixedTypes>	encoder;
			ISchema					schema;

			schema = this.GetSchema ();

			encoder = schema.GetEncoder<MyMixedTypes> ();
			encoder.Link ();

			decoder = schema.GetDecoder<MyMixedTypes> ();
			decoder.Link ();

			SchemaValidator.Validate (decoder, encoder, new MyMixedTypes
			{
				floats		= new float[] {1.1f, 2.2f, 3.3f},
				integer		= 17,
				option		= MyOptionEnum.B,
				pairs		= new Dictionary<string, string>
				{
					{"a",	"aaa"},
					{"b",	"bbb"}
				},
				text			= "Hello, World!"
			});
		}

		[Test]
		public void	NullableDouble ()
		{
			IDecoder<double?>	decoder;
			IEncoder<double?>	encoder;
			ISchema				schema;

			schema = this.GetSchema ();

			encoder = schema.GetEncoder<double?> ();
			encoder.Link ();

			decoder = schema.GetDecoder<double?> ();
			decoder.Link ();

			SchemaValidator.Validate (decoder, encoder, null);
			SchemaValidator.Validate (decoder, encoder, 42);
		}
	}
}
#endif