using System;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
    internal class TreeDecoder<TState, TNative, TKey, TEntity> : IDecoder<TEntity>
    {
        public event ErrorEvent Error;

        private readonly ReaderCallback<TState, TNative, TKey, TEntity> callback;

        private readonly Func<TEntity> constructor;

        private readonly IReader<TState, TNative, TKey> reader;

        public TreeDecoder(IReader<TState, TNative, TKey> reader, Func<TEntity> constructor,
            ReaderCallback<TState, TNative, TKey, TEntity> callback)
        {
            this.callback = callback;
            this.constructor = constructor;
            this.reader = reader;
        }

        public IDecoderStream<TEntity> Open(Stream input)
        {
            var state = this.reader.Start(input, (p, m) => this.Error?.Invoke(p, m));

            return new TreeDecoderStream<TState, TNative, TKey, TEntity>(this.reader, this.constructor, this.callback,
                state);
        }
    }
}
