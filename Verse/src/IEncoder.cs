using System;
using System.IO;

namespace Verse
{
	public interface IEncoder<T>
	{
		#region Methods

		bool		Encode (Stream stream, T instance);

		IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter);

		void		HasField<U> (string name, EncoderValueGetter<T, U> getter, IEncoder<U> encoder);

		IEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter);

		void		HasItems<U> (EncoderArrayGetter<T, U> getter, IEncoder<U> encoder);

		IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter);

		void		HasPairs<U> (EncoderMapGetter<T, U> getter, IEncoder<U> encoder);

		void		Link ();
		
		#endregion
	}
}
