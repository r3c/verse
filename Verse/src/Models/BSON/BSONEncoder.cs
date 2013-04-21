using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Verse.Models.BSON
{
	class BSONEncoder<T> : ConvertEncoder<byte[], T>
    {
		#region Attributes

        private Encoding	encoding;

        #endregion

		#region Constructors

		public	BSONEncoder (Dictionary<Type, object> converters, Encoding encoding) :
			base (converters)
		{
			this.encoding = encoding;
		}

		#endregion
		
		#region Methods / Public

		public override bool	Encode (Stream stream, T instance)
		{
			throw new NotImplementedException ();
		}

		public override void	HasPairs<U> (EncoderMapGetter<T, U> getter, IEncoder<U> encoder)
		{
			throw new NotImplementedException ();
		}
		
		protected override AbstractEncoder<U>	HasPairsAbstract<U> (EncoderMapGetter<T, U> getter)
		{
			throw new NotImplementedException ();
		}
		
		public override void	HasElements<U> (EncoderArrayGetter<T, U> getter, IEncoder<U> encoder)
		{
			throw new NotImplementedException ();
		}
		
		protected override AbstractEncoder<U>	HasElementsAbstract<U> (EncoderArrayGetter<T, U> getter)
		{
			throw new NotImplementedException ();
		}
		
		public override void	HasAttribute<U> (string name, EncoderValueGetter<T, U> getter, IEncoder<U> encoder)
		{
			throw new NotImplementedException ();
		}
		
		protected override AbstractEncoder<U>	HasAttributeAbstract<U> (string name, EncoderValueGetter<T, U> getter)
		{
			throw new NotImplementedException ();
		}
		
		#endregion
		
		#region Methods / Protected

		protected override bool	TryLinkNative ()
		{
			throw new NotImplementedException ();
		}
		
		protected override bool	TryLinkConvert (ConvertSchema<byte[]>.EncoderConverter<T> converter)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
