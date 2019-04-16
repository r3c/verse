using System;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
    class TreeDecoder<TState, TNative, TEntity> : IDecoder<TEntity>
    {
        public event DecodeError Error;

        private readonly ReaderCallback<TState, TNative, TEntity> callback;

        private readonly IReaderSession<TState, TNative> session;

        public TreeDecoder(IReaderSession<TState, TNative> session, ReaderCallback<TState, TNative, TEntity> callback)
        {
            this.callback = callback;
            this.session = session;
        }

        public bool TryOpen(Stream input, out IDecoderStream<TEntity> decoderStream)
        {
            if (!this.session.Start(input, (p, m) => this.Error?.Invoke(p, m), out var state))
            {
                decoderStream = default;

                return false;
            }

            decoderStream = new TreeDecoderStream<TState, TNative, TEntity>(this.session, this.callback, state);

            return true;
        }
    }
}
