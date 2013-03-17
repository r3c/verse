using System;
using System.IO;

namespace Verse
{
	public interface IDecoder<T>
	{
		#region Methods
	
		bool		Decode (Stream stream, out T instance);

		IDecoder<U>	Define<U> (string name, Func<U> builder, DecoderFieldSetter<T, U> setter);

		IDecoder<U>	Define<U> (string name, DecoderFieldSetter<T, U> setter);

		IDecoder<U>	Define<U> (Func<U> builder, DecoderSubSetter<T, U> setter);

		IDecoder<U>	Define<U> (DecoderSubSetter<T, U> setter);

		void		Link (Func<T> builder);

		void		Link ();
		
		#endregion
	}
}
