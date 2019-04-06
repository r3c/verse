
namespace Verse.EncoderDescriptors.Base
{
	class EncoderStream<TEntity, TState> : IEncoderStream<TEntity>
	{
		private readonly IWriterSession<TState> session;

	    private readonly TState state;

		private readonly IWriter<TEntity, TState> writer;

		public EncoderStream(IWriterSession<TState> session, IWriter<TEntity, TState> writer, TState state)
		{
			this.session = session;
		    this.state = state;
			this.writer = writer;
		}

		public bool Encode(TEntity input)
		{
			try
			{
				this.writer.WriteEntity(input, this.state);
			}
			finally
			{
				this.session.Stop(this.state);
			}

			return true;
		}
	}
}