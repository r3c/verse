using System;
using System.IO;

namespace Verse
{
	public interface IDecoder<T>
	{
		#region Methods

		void		Bind (Func<T> builder);

		void		Bind ();
	
		bool		Decode (Stream stream, out T instance);

		IDecoder<U>	Field<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter);

		IDecoder<U>	Field<U> (string name, DecoderValueSetter<T, U> setter);

		IDecoder<U>	Field<U> (Func<U> builder, DecoderKeyValueSetter<T, U> setter);

		IDecoder<U>	Field<U> (DecoderKeyValueSetter<T, U> setter);
		
		IDecoder<U>	Field<U> (Func<U> builder, DecoderValueSetter<T, U> setter);

		IDecoder<U>	Field<U> (DecoderValueSetter<T, U> setter);
		
		#endregion
	}
}
