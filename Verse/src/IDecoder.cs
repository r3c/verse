using System;
using System.IO;

namespace Verse
{
	public interface IDecoder<T>
	{
		#region Methods
	
		bool		Decode (Stream stream, out T instance);

		IDecoder<U>	Define<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter);

		IDecoder<U>	Define<U> (string name, DecoderValueSetter<T, U> setter);

		IDecoder<U>	Define<U> (Func<U> builder, DecoderKeyValueSetter<T, U> setter);

		IDecoder<U>	Define<U> (DecoderKeyValueSetter<T, U> setter);
		
		IDecoder<U>	Define<U> (Func<U> builder, DecoderValueSetter<T, U> setter);

		IDecoder<U>	Define<U> (DecoderValueSetter<T, U> setter);

		void		Link (Func<T> builder);

		void		Link ();
		
		#endregion
	}
}
