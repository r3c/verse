using System;
using System.IO;

namespace Verse.DecoderDescriptors.Abstract
{
	class Decoder<TEntity, TState> : IDecoder<TEntity>
	{
		public event DecodeError Error;

		private readonly Func<TEntity> constructor;

		private readonly IReader<TEntity, TState> reader;

		private readonly IReaderSession<TState> session;

		public Decoder(Func<TEntity> constructor, IReaderSession<TState> session, IReader<TEntity, TState> reader)
		{
			this.constructor = constructor;
			this.reader = reader;
			this.session = session;
		}

		public bool Decode(Stream input, out TEntity output)
		{
			TState state;

			if (!this.session.Start(input, this.OnError, out state))
			{
				output = default(TEntity);

				return false;
			}

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

		private void OnError(int position, string message)
		{
			DecodeError error;

			error = this.Error;

			if (error != null)
				error(position, message);
		}
	}
}
