using System;

namespace Verse.BuilderDescriptors.Recurse
{
	public interface IEncoder<V>
	{
		#region Methods

		Converter<T, V>	Get<T> ();

		#endregion
	}
}
