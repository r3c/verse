
namespace Verse.EncoderDescriptors.Tree
{
	class TreeEncoderStream<TState, TNative, TEntity> : IEncoderStream<TEntity>
	{
		private readonly IWriterSession<TState, TNative> session;

	    private readonly TState state;

		private readonly WriterCallback<TState, TNative, TEntity> callback;

		public TreeEncoderStream(IWriterSession<TState, TNative> session, WriterCallback<TState, TNative, TEntity> callback, TState state)
		{
			this.callback = callback;
			this.session = session;
		    this.state = state;
		}

		public bool Encode(TEntity input)
		{
			try
			{
				this.callback(this.session, this.state, input);
			}
			finally
			{
				this.session.Stop(this.state);
			}

			return true;
		}
	}
}