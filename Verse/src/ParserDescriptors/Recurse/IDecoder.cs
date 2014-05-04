using System;

namespace Verse.ParserDescriptors.Recurse
{
	public interface IDecoder<V>
	{
		#region Methods

		Converter<V, T>	Get<T> ();

		#endregion
	}
}
