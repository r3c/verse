using System;

namespace Verse.EncoderDescriptors.Abstract
{
	public interface IWriter<TEntity, TState>
	{
		void WriteEntity(TEntity source, TState state);
	}
}
