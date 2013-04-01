using System;
using System.IO;

namespace Verse
{
	public interface IDecoder<T>
	{
		#region Methods

		bool		Decode (Stream stream, out T instance);

		IDecoder<U>	HasField<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter);

		IDecoder<U>	HasField<U> (string name, DecoderValueSetter<T, U> setter);

		IDecoder<U>	HasItems<U> (Func<U> generator, DecoderArraySetter<T, U> setter);

		IDecoder<U>	HasItems<U> (DecoderArraySetter<T, U> setter);

		IDecoder<U>	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter);

		IDecoder<U>	HasPairs<U> (DecoderMapSetter<T, U> setter);

		void		Link ();
		
		#endregion
	}
}
