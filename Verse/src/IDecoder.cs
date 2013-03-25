using System;
using System.IO;

namespace Verse
{
	public interface IDecoder<T>
	{
		#region Methods

		void		Bind ();
	
		bool		Decode (Stream stream, out T instance);

		void		Fake (Func<T> builder);

		IDecoder<U>	HasField<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter);

		IDecoder<U>	HasField<U> (string name, DecoderValueSetter<T, U> setter);

		IDecoder<U>	HasItems<U> (Func<U> builder, DecoderArraySetter<T, U> setter);

		IDecoder<U>	HasItems<U> (DecoderArraySetter<T, U> setter);

		IDecoder<U>	HasPairs<U> (Func<U> builder, DecoderMapSetter<T, U> setter);

		IDecoder<U>	HasPairs<U> (DecoderMapSetter<T, U> setter);
		
		#endregion
	}
}
