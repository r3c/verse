using System;

namespace Verse.ParserDescriptors.Recurse
{
	interface IDecoder<V>
	{
		#region Methods

		Converter<V, T>	Get<T> ();

		#endregion
	}
}
