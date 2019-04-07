using System;

namespace Verse.DecoderDescriptors.Base
{
	class DecoderStream<TEntity, TState> : IDecoderStream<TEntity>
	{
		private readonly Func<TEntity> constructor;

		private readonly IReader<TState, TEntity> reader;

		private readonly IReaderSession<TState> session;

	    private readonly TState state;

		public DecoderStream(Func<TEntity> constructor, IReaderSession<TState> session, IReader<TState, TEntity> reader, TState state)
		{
			this.constructor = constructor;
			this.reader = reader;
			this.session = session;
		    this.state = state;
		}

		public bool Decode(out TEntity output)
		{
		    try
		    {
		        return this.reader.Read(state, this.constructor, out output);
		    }
		    finally
		    {
		        this.session.Stop(state);
		    }
        }
	}
}
