using System;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
    internal class TreeDecoder<TState, TNative, TEntity> : IDecoder<TEntity>
    {
        public event ErrorEvent Error;

        private readonly ReaderCallback<TState, TNative, TEntity> callback;

        private readonly Func<TEntity> constructor;

        private readonly IReader<TState, TNative> session;

        public TreeDecoder(IReader<TState, TNative> session, Func<TEntity> constructor, ReaderCallback<TState, TNative, TEntity> callback)
        {
            this.callback = callback;
            this.constructor = constructor;
            this.session = session;
        }

        public IDecoderStream<TEntity> Open(Stream input)
        {
            var state = this.session.Start(input, (p, m) => this.Error?.Invoke(p, m));

            return
                new TreeDecoderStream<TState, TNative, TEntity>(this.session, this.constructor, this.callback, state);
        }
    }
}
