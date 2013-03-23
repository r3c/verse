using System;
using System.IO;

namespace Verse
{
	public interface IEncoder<T>
	{
		#region Methods

		void		Bind (Func<T> builder);

		void		Bind ();

		bool		Encode (Stream stream, T instance);

		IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter);

		IEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter);

		IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter);
		
		#endregion
	}
}
