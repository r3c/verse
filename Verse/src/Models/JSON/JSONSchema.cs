using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Verse.Models.JSON.Formatters;

namespace Verse.Models.JSON
{
	public class JSONSchema : StringSchema
	{
		#region Attributes

		private Encoding	encoding;

		private IFormatter	formatter;
		
		#endregion
		
		#region Constructors

		public	JSONSchema (Encoding encoding, IFormatter formatter)
		{
			this.encoding = encoding;
			this.formatter = formatter;
		}

		public	JSONSchema (Encoding encoding) :
			this (encoding, new CompactFormatter ())
		{
		}

		public	JSONSchema () :
			this (new UTF8Encoding (false), new CompactFormatter ())
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

			encoder = new JSONEncoder<T> (this.encoding, this.encoderConverters, this.formatter);
			encoder.OnStreamError += this.EventStreamError;
			encoder.OnTypeError += this.EventTypeError;

			return encoder;
		}

		#endregion
	}
}
