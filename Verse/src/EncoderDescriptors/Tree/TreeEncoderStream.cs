
namespace Verse.EncoderDescriptors.Tree
{
	internal class TreeEncoderStream<TState, TNative, TEntity> : IEncoderStream<TEntity>
	{
		private readonly IWriter<TState, TNative> session;

	    private readonly TState state;

		private readonly WriterCallback<TState, TNative, TEntity> callback;

		public TreeEncoderStream(IWriter<TState, TNative> session, WriterCallback<TState, TNative, TEntity> callback, TState state)
		{
			this.callback = callback;
			this.session = session;
		    this.state = state;
		}

		public void Dispose()
		{
			this.session.Stop(this.state);
		}

		public void Encode(TEntity input)
		{
			this.callback(this.session, this.state, input);
		}
	}
}