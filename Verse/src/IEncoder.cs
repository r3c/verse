using System;
using System.IO;

namespace Verse
{
	public interface IEncoder<T>
	{
		#region Methods

		bool		Encode (Stream stream, T instance);

		IEncoder<U>	HasAttribute<U> (string name, EncoderValueGetter<T, U> getter);

		void		HasAttribute<U> (string name, EncoderValueGetter<T, U> getter, IEncoder<U> encoder);

		IEncoder<U>	HasElements<U> (EncoderArrayGetter<T, U> getter);

		void		HasElements<U> (EncoderArrayGetter<T, U> getter, IEncoder<U> encoder);

		IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter);

		void		HasPairs<U> (EncoderMapGetter<T, U> getter, IEncoder<U> encoder);

		void		Link ();
		
		#endregion
	}
}
