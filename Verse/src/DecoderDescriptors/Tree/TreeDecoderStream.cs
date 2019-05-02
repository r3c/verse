using System;

namespace Verse.DecoderDescriptors.Tree
{
	internal class TreeDecoderStream<TState, TNative, TKey, TEntity> : IDecoderStream<TEntity>
	{
		private readonly ReaderCallback<TState, TNative, TKey, TEntity> callback;

		private readonly Func<TEntity> constructor;

		private readonly IReader<TState, TNative, TKey> reader;

		private readonly TState state;

		public TreeDecoderStream(IReader<TState, TNative, TKey> reader, Func<TEntity> constructor,
			ReaderCallback<TState, TNative, TKey, TEntity> callback, TState state)
		{
			this.callback = callback;
			this.constructor = constructor;
			this.reader = reader;
			this.state = state;
		}

		public void Dispose()
		{
			this.reader.Stop(this.state);
		}

		public bool TryDecode(out TEntity entity)
		{
			entity = this.constructor();

			return this.callback(this.reader, this.state, ref entity);
		}
	}
}
