using System;

namespace Verse
{
	public interface ISchema
	{
		#region Methods

		IDecoder<T>	GetDecoder<T> (Func<T> constructor);

		IEncoder<T>	GetEncoder<T> ();

		#endregion
	}
}
