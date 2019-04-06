
namespace Verse.EncoderDescriptors.Base
{
	class EncoderStream<TEntity, TState> : IEncoderStream<TEntity>
	{
		private readonly IWriterSession<TState> session;

	    private readonly TState state;

		private readonly IWriter<TState, TEntity> writer;

		public EncoderStream(IWriterSession<TState> session, IWriter<TState, TEntity> writer, TState state)
		{
			this.session = session;
		    this.state = state;
			this.writer = writer;
		}

		public bool Encode(TEntity input)
		{
			try
			{
				this.writer.Write(this.state, input);
			}
			finally
			{
				this.session.Stop(this.state);
			}

			return true;
		}
	}
}