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

		protected override AbstractDecoder<T>	CreateDecoder<T> (Func<T> constructor)
		{
			return new BSONDecoder<T> (this.decoderConverters, this.encoding, constructor);
		}
		
		protected override AbstractEncoder<T>	CreateEncoder<T> ()
		{
			return new BSONEncoder<T> (this.encoderConverters, this.encoding);
		}

		#endregion
	}
}
