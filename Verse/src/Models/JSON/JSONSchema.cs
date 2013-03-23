using System;
using System.Collections.Generic;
using System.Text;

namespace Verse.Models.JSON
{
	public class JSONSchema : StringSchema
	{
		#region Attributes

		private Encoding	encoding;
		
		#endregion
		
		#region Constructors

		public	JSONSchema (Encoding encoding)
		{
			this.encoding = encoding;
		}

		public	JSONSchema () :
			this (Encoding.UTF8)
		{
		}
		
		#endregion
		
		#region Methods

		public override IDecoder<T>	GetDecoder<T> (Func<T> constructor)
		{
			AbstractDecoder<T>	decoder;

			decoder = new JSONDecoder<T> (constructor, this.encoding, this.decoderConverters);
			decoder.OnStreamError += this.EventStreamError;
			decoder.OnTypeError += this.EventTypeError;

			return decoder;
		}
		
		public override IEncoder<T>	GetEncoder<T> ()
		{
			AbstractEncoder<T>	encoder;

			encoder = new JSONEncoder<T> (this.encoding, this.encoderConverters);
			encoder.OnStreamError += this.EventStreamError;
			encoder.OnTypeError += this.EventTypeError;

			return encoder;
		}

		#endregion
	}
}
