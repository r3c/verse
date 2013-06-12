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

		private Func<Stream, Encoding, JSONPrinter>	generator;

		private JSONSettings						settings;

		#endregion
		
		#region Constructors

		public	JSONSchema (JSONSettings settings, Func<Stream, Encoding, JSONPrinter> generator, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			if (generator == null)
				throw new ArgumentNullException ("generator");

			this.encoding = encoding;
			this.generator = generator;
			this.settings = settings;
		}

		public	JSONSchema (JSONSettings settings, Func<Stream, Encoding, JSONPrinter> generator) :
			this (settings, generator, new UTF8Encoding (false))
		{
		}

		public	JSONSchema (JSONSettings settings) :
			this (settings, (s, e) => new JSONPrinter (s, e))
		{
		}

		public	JSONSchema () :
			this (0)
		{
		}
		
		#endregion
		
		#region Methods

		protected override AbstractDecoder<T>	CreateDecoder<T> (Func<T> constructor)
		{
			return new JSONDecoder<T> (this.decoderConverters, this.settings, this.encoding, constructor);
		}
		
		protected override AbstractEncoder<T>	CreateEncoder<T> ()
		{
			return new JSONEncoder<T> (this.encoderConverters, this.settings, this.encoding, this.generator);
		}

		#endregion
	}
}
