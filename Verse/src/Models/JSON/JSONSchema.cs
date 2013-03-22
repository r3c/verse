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
			return new JSONDecoder<T> (constructor, this.encoding, this.decoderConverters);
		}
		
		public override IEncoder<T>	GetEncoder<T> ()
		{
			return new JSONEncoder<T> (this.encoding);
		}

		#endregion
	}
}
