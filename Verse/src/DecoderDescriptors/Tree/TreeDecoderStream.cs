
namespace Verse.DecoderDescriptors.Tree
{
	internal class TreeDecoderStream<TState, TNative, TEntity> : IDecoderStream<TEntity>
	{
		private readonly ReaderCallback<TState, TNative, TEntity> callback;

		private readonly IReaderSession<TState, TNative> session;

		private readonly TState state;

		public TreeDecoderStream(IReaderSession<TState, TNative> session, ReaderCallback<TState, TNative, TEntity> callback, TState state)
		{
			this.callback = callback;
			this.session = session;
			this.state = state;
		}

		public bool Decode(out TEntity entity)
		{
			try
			{
				return this.callback(this.session, this.state, out entity);
			}
			finally
			{
				this.session.Stop(this.state);
			}
		}
	}
}
