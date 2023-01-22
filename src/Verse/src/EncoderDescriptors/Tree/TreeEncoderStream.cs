
namespace Verse.EncoderDescriptors.Tree
{
	internal class TreeEncoderStream<TState, TNative, TEntity> : IEncoderStream<TEntity>
	{
		private readonly IWriter<TState, TNative> reader;

	    private readonly TState state;

		private readonly WriterCallback<TState, TNative, TEntity> callback;

		public TreeEncoderStream(IWriter<TState, TNative> reader, WriterCallback<TState, TNative, TEntity> callback,
			TState state)
		{
			this.callback = callback;
			this.reader = reader;
			this.state = state;
		}

		public void Dispose()
		{
			reader.Stop(state);
		}

		public void Encode(TEntity input)
		{
			callback(reader, state, input);
			reader.Flush(state);
		}
	}
}