using System;

namespace Verse.DecoderDescriptors.Abstract
{
	class DecoderStream<TEntity, TState> : IDecoderStream<TEntity>
	{
		private readonly Func<TEntity> constructor;

		private readonly IReader<TEntity, TState> reader;

		private readonly IReaderSession<TState> session;

	    private readonly TState state;

		public DecoderStream(Func<TEntity> constructor, IReaderSession<TState> session, IReader<TEntity, TState> reader, TState state)
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
		        output = this.constructor();

		        return this.reader.Read(ref output, state);
		    }
		    finally
		    {
		        this.session.Stop(state);
		    }
        }
	}
}
