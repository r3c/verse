using System;

namespace Verse.BuilderDescriptors.Recurse
{
	interface IEncoder<V>
	{
		#region Methods

		Converter<T, V>	Get<T> ();

		#endregion
	}
}
