using System;

namespace Verse.BuilderDescriptors.Recurse
{
	public interface IPointer<T, C, V>
	{
		void	Enter (T source, IWriter<C, V> writer, C context);
	}
}
