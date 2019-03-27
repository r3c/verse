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

        public bool TryOpen(Stream input, out IDecoderStream<TEntity> decoderStream)
        {
            if (!this.session.Start(input, this.OnError, out var state))
            {
                decoderStream = default;

                return false;
            }

            decoderStream = new DecoderStream<TEntity,TState>(this.constructor, this.session, this.reader, state);

            return true;
        }

        private void OnError(int position, string message)
        {
            this.Error?.Invoke(position, message);
        }
    }
}
