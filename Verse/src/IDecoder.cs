using System;
using System.IO;

namespace Verse
{
	public interface IDecoder<T>
	{
		#region Methods

		bool		Decode (Stream stream, out T instance);

		IDecoder<U>	HasAttribute<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter);

		IDecoder<U>	HasAttribute<U> (string name, DecoderValueSetter<T, U> setter);

		void		HasAttribute<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter, IDecoder<U> decoder);

		void		HasAttribute<U> (string name, DecoderValueSetter<T, U> setter, IDecoder<U> decoder);

		IDecoder<U>	HasElements<U> (Func<U> generator, DecoderArraySetter<T, U> setter);

		IDecoder<U>	HasElements<U> (DecoderArraySetter<T, U> setter);

		void		HasElements<U> (Func<U> generator, DecoderArraySetter<T, U> setter, IDecoder<U> decoder);

		void		HasElements<U> (DecoderArraySetter<T, U> setter, IDecoder<U> decoder);

		IDecoder<U>	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter);

		IDecoder<U>	HasPairs<U> (DecoderMapSetter<T, U> setter);

		void		HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter, IDecoder<U> decoder);

		void		HasPairs<U> (DecoderMapSetter<T, U> setter, IDecoder<U> decoder);

		void		Link ();
		
		#endregion
	}
}
