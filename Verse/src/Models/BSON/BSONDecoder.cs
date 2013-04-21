using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Verse.Models.BSON
{
	class BSONDecoder<T> : ConvertDecoder<byte[], T>
    {
		#region Attributes
		
		private Func<T>		constructor;

		private Encoding	encoding;

        #endregion

		#region Constructors

		public	BSONDecoder (Dictionary<Type, object> converters, Encoding encoding, Func<T> constructor) :
			base (converters)
		{
			this.constructor = constructor;
			this.encoding = encoding;
		}

		#endregion

		#region Methods / Public

		public override bool	Decode (Stream stream, out T instance)
		{
			throw new NotImplementedException ();
		}

		public override void	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter, IDecoder<U> decoder)
		{
			throw new NotImplementedException ();
		}
		
		protected override AbstractDecoder<U>	HasPairsAbstract<U> (Func<U> generator, DecoderMapSetter<T, U> setter)
		{
			throw new NotImplementedException();
		}
		
		public override void	HasItems<U> (Func<U> generator, DecoderArraySetter<T, U> setter, IDecoder<U> decoder)
		{
			throw new NotImplementedException ();
		}
		
		protected override AbstractDecoder<U>	HasItemsAbstract<U> (Func<U> generator, DecoderArraySetter<T, U> setter)
		{
			throw new NotImplementedException ();
		}
		
		public override void	HasField<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter, IDecoder<U> decoder)
		{
			throw new NotImplementedException ();
		}
		
		protected override AbstractDecoder<U>	HasFieldAbstract<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Methods / Protected
		
		protected override bool	TryLinkNative ()
		{
			throw new NotImplementedException ();
		}
		
		protected override bool	TryLinkConvert (ConvertSchema<byte[]>.DecoderConverter<T> converter)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
