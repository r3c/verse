using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Verse.Models.JSON
{
	public class JSONSchema : ConvertSchema<string>
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

		private Encoding							encoding;

		private Func<Stream, Encoding, JSONWriter>	generator;
		
		#endregion
		
		#region Constructors

		public	JSONSchema (Func<Stream, Encoding, JSONWriter> generator, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			if (generator == null)
				throw new ArgumentNullException ("generator");

			this.encoding = encoding;
			this.generator = generator;
		}

		public	JSONSchema (Func<Stream, Encoding, JSONWriter> generator) :
			this (generator, new UTF8Encoding (false))
		{
		}

		public	JSONSchema () :
			this ((s, e) => new JSONWriter (s, e))
		{
		}
		
		#endregion
		
		#region Methods

		public override IDecoder<T>	GetDecoder<T> (Func<T> constructor)
		{
			AbstractDecoder<T>	decoder;

			if (constructor == null)
				throw new ArgumentNullException ("constructor");

			decoder = new JSONDecoder<T> (this.decoderConverters, this.encoding, constructor);
			decoder.OnStreamError += this.EventStreamError;
			decoder.OnTypeError += this.EventTypeError;

			return decoder;
		}
		
		public override IEncoder<T>	GetEncoder<T> ()
		{
			AbstractEncoder<T>	encoder;

			encoder = new JSONEncoder<T> (this.encoderConverters, this.encoding, this.generator);
			encoder.OnStreamError += this.EventStreamError;
			encoder.OnTypeError += this.EventTypeError;

			return encoder;
		}

		#endregion
	}
}
