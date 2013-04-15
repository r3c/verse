using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Verse.Models.BSON
{
	public class BSONSchema : ConvertSchema<byte[]>
	{
		#region Properties

		public Encoding	Encoding
		{
			get
			{
				return this.encoding;
			}
		}

		#endregion

		#region Attributes

		private Encoding	encoding;

		#endregion
		
		#region Constructors

		public	BSONSchema (Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			this.encoding = encoding;
		}

		public	BSONSchema () :
			this (new UTF8Encoding (false))
		{
		}
		
		#endregion
		
		#region Methods

		public override IDecoder<T>	GetDecoder<T> (Func<T> constructor)
		{
			AbstractDecoder<T>	decoder;

			if (constructor == null)
				throw new ArgumentNullException ("constructor");

			decoder = new BSONDecoder<T> (this.decoderConverters, this.encoding, constructor);
			decoder.OnStreamError += this.EventStreamError;
			decoder.OnTypeError += this.EventTypeError;

			return decoder;
		}
		
		public override IEncoder<T>	GetEncoder<T> ()
		{
			AbstractEncoder<T>	encoder;

			encoder = new BSONEncoder<T> (this.encoderConverters, this.encoding);
			encoder.OnStreamError += this.EventStreamError;
			encoder.OnTypeError += this.EventTypeError;

			return encoder;
		}

		#endregion
	}
}
