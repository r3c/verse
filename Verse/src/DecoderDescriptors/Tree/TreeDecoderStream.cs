using System;

namespace Verse.DecoderDescriptors.Tree
{
	internal class TreeDecoderStream<TState, TNative, TEntity> : IDecoderStream<TEntity>
	{
		private readonly ReaderCallback<TState, TNative, TEntity> callback;

		private readonly Func<TEntity> constructor;

		private readonly IReaderSession<TState, TNative> session;

		private readonly TState state;

		public TreeDecoderStream(IReaderSession<TState, TNative> session, Func<TEntity> constructor, ReaderCallback<TState, TNative, TEntity> callback, TState state)
		{
			this.callback = callback;
			this.constructor = constructor;
			this.session = session;
			this.state = state;
		}

		public void Dispose()
		{
			this.session.Stop(this.state);
		}

		public bool TryDecode(out TEntity entity)
		{
			entity = this.constructor();

			return this.callback(this.session, this.state, ref entity);
		}
	}
}
