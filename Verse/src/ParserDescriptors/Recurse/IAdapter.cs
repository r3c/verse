using System;

namespace Verse.ParserDescriptors.Recurse
{
    public interface IAdapter<V>
    {
    	#region Methods

    	Converter<V, T>	Get<T> ();

    	#endregion
    }
}
