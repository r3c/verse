using System;
using System.IO;

namespace Verse.DecoderDescriptors.Base
{
    class Decoder<TEntity, TState> : IDecoder<TEntity>
    {
        public event DecodeError Error;

        private readonly Func<TEntity> constructor;

        private readonly IReader<TState, TEntity> reader;

        private readonly IReaderSession<TState> session;

        public Decoder(Func<TEntity> constructor, IReaderSession<TState> session, IReader<TState, TEntity> reader)
        {
            this.constructor = constructor;
            this.reader = reader;
            this.session = session;
        }

        public bool TryOpen(Stream input, out IDecoderStream<TEntity> decoderStream)
        {
            if (!this.session.Start(input, (p, m) => this.Error?.Invoke(p, m), out var state))
            {
                decoderStream = default;

                return false;
            }

            decoderStream = new DecoderStream<TEntity, TState>(this.constructor, this.session, this.reader, state);

            return true;
        }
    }
}
